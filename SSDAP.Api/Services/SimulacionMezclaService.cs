using System.Data;
using Microsoft.EntityFrameworkCore;
using SSDAP.Api.Data;
using SSDAP.Shared.DTOs.Simulaciones;

namespace SSDAP.Api.Services;

public sealed class SimulacionMezclaService(AppDbContext db, AccesoLocalidadService acceso)
{
    public async Task<SimulacionMezclaDto> SimularAsync(int localidadId, SimularMezclaDto datos, CancellationToken ct)
    {
        await acceso.VerificarAsync(localidadId, ct);
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, ct);
        var ahora = DateTimeOffset.UtcNow;
        var zona = HorarioSistema.ZonaHoraria;
        var hoy = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(ahora, TimeZoneInfo.FindSystemTimeZoneById(zona)).DateTime);
        var ids = datos.Fuentes.Select(f => f.IdFuente).ToArray();
        var fuentes = await db.FuentesAgua.AsNoTracking()
            .Where(f => ids.Contains(f.IdFuente) && f.IdLocalidad == localidadId).ToListAsync(ct);
        if (fuentes.Count != ids.Length)
            throw new ReglaNegocioException(404, "Alguna fuente no existe en esta localidad.");
        if (fuentes.Any(f => f.Estado != "Activa" || f.Caudal <= 0))
            throw new ReglaNegocioException(409, "Todas las fuentes deben estar activas y tener caudal positivo.");
        var idsAnalisis = datos.Fuentes.Select(f => f.IdAnalisis).ToArray();
        var analisis = await db.AnalisisCalidad.AsNoTracking()
            .Where(a => idsAnalisis.Contains(a.IdAnalisis) && ids.Contains(a.IdFuente)).ToListAsync(ct);
        var entradas = new List<FuenteSimulacionDto>();
        foreach (var seleccion in datos.Fuentes)
        {
            var fuente = fuentes.Single(f => f.IdFuente == seleccion.IdFuente);
            var muestra = analisis.SingleOrDefault(a => a.IdAnalisis == seleccion.IdAnalisis && a.IdFuente == fuente.IdFuente)
                ?? throw new ReglaNegocioException(404, "El análisis seleccionado no pertenece a la fuente.");
            if (muestra.Fecha > hoy)
                throw new ReglaNegocioException(409, "No se puede usar un análisis con fecha futura.");
            entradas.Add(new(fuente.IdFuente, fuente.Nombre, fuente.TipoFuente, fuente.Caudal,
                AnalisisCalidadService.Detalle(muestra)));
        }
        var limites = (await db.ParametrosLey.AsNoTracking().Where(p => p.Codigo != null)
            .ToListAsync(ct)).Select(ParametroCalidadService.Detalle).ToList();
        var parametros = CalculadorMezcla.Calcular(entradas, limites);
        List<string> advertencias = ["Mezcla instantánea sin reacciones, precipitación ni tratamiento; caudales constantes y simultáneos."];
        if (entradas.Select(f => f.Analisis.Fecha).Distinct().Count() > 1)
            advertencias.Add("Se utilizaron análisis de distintas fechas.");
        if (datos.MaxDiasAntiguedad is int maximo &&
            entradas.Any(f => hoy.DayNumber - f.Analisis.Fecha.DayNumber > maximo))
            advertencias.Add("Hay análisis que superan la antigüedad solicitada.");
        if (parametros.Any(p => p.EstadoCalculo != "Calculado"))
            advertencias.Add("Hay parámetros sin calcular; revisar estados y fuentes sin medición.");
        advertencias.Add("El resultado no certifica potabilidad. El cloro se consulta como medición final independiente.");
        await transaction.CommitAsync(ct);
        return new(localidadId, hoy, ahora, "BalanceInstantaneoSinReacciones-v1",
            entradas.Sum(f => f.CaudalM3H), entradas, parametros, advertencias);
    }
}

