using System.Data;
using Microsoft.EntityFrameworkCore;
using SSDAP.Api.Data;
using SSDAP.Shared.DTOs.Reportes;

namespace SSDAP.Api.Services;

public sealed class CalidadMensualService(AppDbContext db, AccesoLocalidadService acceso, ProduccionMensualService produccion)
{
    public async Task<CalidadMensualDto> ConsultarAsync(int localidadId, ConsultarCalidadMensualDto datos, CancellationToken ct)
    {
        await acceso.VerificarAsync(localidadId, ct);
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, ct);
        var reporte = await produccion.ConsultarEnLecturaAsync(localidadId, datos.Produccion, ct);
        var calidad = await CalcularEnLecturaAsync(reporte, datos.Calidad, ct);
        await tx.CommitAsync(ct);
        return calidad;
    }

    internal async Task<CalidadMensualDto> CalcularEnLecturaAsync(ProduccionMensualDto reporte, SeleccionCalidadMensualDto seleccion, CancellationToken ct)
    {
        var idsPozos = reporte.Pozos.Where(p => p.VolumenEstimadoM3 > 0).Select(p => p.IdPozo).ToArray();
        var pozos = await db.Pozos.AsNoTracking().Where(p => idsPozos.Contains(p.IdPozo) && p.FuenteAgua.IdLocalidad == reporte.IdLocalidad)
            .Select(p => new { p.IdPozo, p.IdFuente, p.FuenteAgua.Nombre }).ToListAsync(ct);
        if (pozos.Count != idsPozos.Length) throw new ReglaNegocioException(409, "No se pueden identificar todos los pozos programados.");
        var aportes = pozos.Select(p => new AporteCalidadMensualDto(p.IdFuente, p.Nombre,
            reporte.Pozos.Single(x => x.IdPozo == p.IdPozo).VolumenEstimadoM3, null))
            .Concat(reporte.Acueductos.Where(a => a.VolumenEstimadoM3 > 0).Select(a => new AporteCalidadMensualDto(a.IdFuente, a.Nombre, a.VolumenEstimadoM3, null)))
            .OrderBy(a => a.IdFuente).ToList();
        if (aportes.Select(a => a.IdFuente).Distinct().Count() != aportes.Count)
            throw new ReglaNegocioException(409, "Hay fuentes con especialización inconsistente.");
        if (seleccion.Analisis.Any(a => !aportes.Any(p => p.IdFuente == a.IdFuente)))
            throw new ReglaNegocioException(409, "Seleccionar análisis únicamente de fuentes con volumen positivo en este reporte.");
        var ids = seleccion.Analisis.Select(a => a.IdAnalisis).ToArray();
        var fuentes = aportes.Select(a => a.IdFuente).ToArray();
        var muestras = await db.AnalisisCalidad.AsNoTracking().Where(a => ids.Contains(a.IdAnalisis) && fuentes.Contains(a.IdFuente)).ToListAsync(ct);
        var zona = TimeZoneInfo.FindSystemTimeZoneById(reporte.ZonaHoraria);
        var hoy = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zona).DateTime);
        var finMes = new DateOnly(reporte.Anio, reporte.Mes, DateTime.DaysInMonth(reporte.Anio, reporte.Mes));
        for (var i = 0; i < aportes.Count; i++)
        {
            var elegido = seleccion.Analisis.SingleOrDefault(a => a.IdFuente == aportes[i].IdFuente);
            if (elegido is null) continue;
            var muestra = muestras.SingleOrDefault(a => a.IdFuente == elegido.IdFuente && a.IdAnalisis == elegido.IdAnalisis)
                ?? throw new ReglaNegocioException(404, "El análisis no pertenece a la fuente seleccionada.");
            if (muestra.Fecha > hoy || muestra.Fecha > finMes)
                throw new ReglaNegocioException(409, "No usar análisis futuros ni posteriores al mes estimado.");
            aportes[i] = aportes[i] with { Analisis = AnalisisCalidadService.Detalle(muestra) };
        }
        var limites = (await db.ParametrosLey.AsNoTracking().Where(p => p.Codigo != null).ToListAsync(ct))
            .Select(ParametroCalidadService.Detalle).ToList();
        var parametros = aportes.Count == 0 ? [] : CalculadorMezcla.CalcularPonderado(aportes.Select(a =>
            new CalculadorMezcla.AporteMezcla(a.IdFuente, a.VolumenM3, a.Analisis?.Mediciones)).ToList(), limites);
        var estado = aportes.Count == 0 ? "SinProduccion"
            : parametros.Any(p => p.EstadoEvaluacion == "FueraDeRango") ? "FueraDeRango"
            : parametros.Count == 0 || parametros.Any(p => p.EstadoCalculo != "Calculado" || p.EstadoEvaluacion != "DentroDeRango") ? "EvaluacionParcial" : "DentroDeRango";
        List<string> advertencias =
        [
            "Mezcla mensual estimada por suma(volumen × concentración)/volumen total, sin reacciones ni tratamiento.",
            "Cada análisis elegido se supone representativo de su fuente durante todo el mes; no se interpolan muestras.",
            "Es una media ponderada por volumen, no describe variaciones horarias ni acredita calidad real o potabilidad.",
            "pH y conductividad no se promedian; alcalinidad y dureza siguen pendientes de base analítica. Cloro medido se conserva separado."
        ];
        if (aportes.Any(a => a.Analisis is null)) advertencias.Add("Faltan análisis seleccionados; no se omiten sus volúmenes ni se reemplazan mediciones por cero.");
        if (aportes.Any(a => a.Analisis?.Fecha < new DateOnly(reporte.Anio, reporte.Mes, 1)))
            advertencias.Add("Se usaron análisis anteriores al mes; revisar su representatividad.");
        return new(reporte.IdLocalidad, reporte.Anio, reporte.Mes, "BalanceVolumenMensualSinReacciones-v1", estado,
            reporte.VolumenTotalEstimadoM3, aportes, parametros, advertencias);
    }
}
