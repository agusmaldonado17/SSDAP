using System.Data;
using Microsoft.EntityFrameworkCore;
using SSDAP.Api.Data;
using SSDAP.Shared.DTOs.Alertas;
using SSDAP.Shared.DTOs.Calidad;

namespace SSDAP.Api.Services;

public sealed class AlertasService(AppDbContext db, AccesoLocalidadService acceso)
{
    private const int TamanoPagina = 50;

    public async Task<PaginaAlertasDto<FuenteAlertasDto>> FuentesAsync(
        int localidadId, int? maxDiasAntiguedad, int pagina, CancellationToken ct)
    {
        await acceso.VerificarAsync(localidadId, ct);
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, ct);
        var hoy = FechaLocal();
        var consulta = db.FuentesAgua.AsNoTracking().Where(f => f.IdLocalidad == localidadId && f.Estado == "Activa");
        var total = await consulta.CountAsync(ct);
        var fuentes = await consulta.OrderBy(f => f.IdFuente).Skip((pagina - 1) * TamanoPagina)
            .Take(TamanoPagina).ToListAsync(ct);
        var ids = fuentes.Select(f => f.IdFuente).ToArray();
        var analisis = db.AnalisisCalidad.AsNoTracking();
        // Uno por fuente, sin cargar todo el histórico ni mezclar resultados de distintas fechas.
        var ultimos = await analisis.Where(a => ids.Contains(a.IdFuente) && a.Fecha <= hoy &&
                !analisis.Any(b => b.IdFuente == a.IdFuente && b.Fecha <= hoy &&
                    (b.Fecha > a.Fecha || (b.Fecha == a.Fecha && b.IdAnalisis > a.IdAnalisis))))
            .ToDictionaryAsync(a => a.IdFuente, ct);
        var futuras = await analisis.Where(a => ids.Contains(a.IdFuente) && a.Fecha > hoy)
            .Select(a => a.IdFuente).Distinct().ToListAsync(ct);
        var limites = (await db.ParametrosLey.AsNoTracking().Where(p => p.Codigo != null)
            .ToListAsync(ct)).Select(ParametroCalidadService.Detalle).ToList();
        var resultados = new List<FuenteAlertasDto>();
        foreach (var fuente in fuentes)
        {
            var alertas = new List<string>();
            if (fuente.Caudal <= 0) alertas.Add("CaudalNoPositivo");
            if (futuras.Contains(fuente.IdFuente)) alertas.Add("AnalisisConFechaFutura");
            ultimos.TryGetValue(fuente.IdFuente, out var ultimo);
            int? edad = ultimo is null ? null : hoy.DayNumber - ultimo.Fecha.DayNumber;
            List<ComparacionParametroDto> calidad = [];
            if (ultimo is null) alertas.Add("SinAnalisis");
            else
            {
                if (maxDiasAntiguedad.HasValue && edad > maxDiasAntiguedad)
                    alertas.Add("AnalisisAntiguo");
                calidad = EvaluadorCalidad.Comparar(AnalisisCalidadService.Detalle(ultimo).Mediciones, limites);
                if (calidad.Any(p => p.Estado == "FueraDeRango")) alertas.Add("CalidadFueraDeRango");
                if (calidad.Count == 0 || calidad.Any(p => p.Estado != "DentroDeRango" && p.Estado != "FueraDeRango"))
                    alertas.Add("CalidadNoEvaluadaCompletamente");
            }
            resultados.Add(new(fuente.IdFuente, fuente.Nombre, ultimo?.IdAnalisis, ultimo?.Fecha,
                edad, alertas, calidad));
        }
        await transaction.CommitAsync(ct);
        return new(hoy, maxDiasAntiguedad, pagina, total, (long)pagina * TamanoPagina < total, resultados);
    }

    public async Task<PaginaAlertasDto<BateriaAlertasDto>> BateriasAsync(int localidadId, int pagina, CancellationToken ct)
    {
        await acceso.VerificarAsync(localidadId, ct);
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, ct);
        var hoy = FechaLocal();
        var consulta = db.Baterias.AsNoTracking().Where(b => b.IdLocalidad == localidadId && b.Estado == "Activa");
        var total = await consulta.CountAsync(ct);
        var baterias = await consulta.OrderBy(b => b.IdBateria).Skip((pagina - 1) * TamanoPagina)
            .Take(TamanoPagina).ToListAsync(ct);
        var ids = baterias.Select(b => b.IdBateria).ToArray();
        var pozos = await db.BateriasPozos.AsNoTracking().Where(p => ids.Contains(p.IdBateria))
            .Select(p => new { p.IdBateria, p.IdPozo, p.Pozo.FuenteAgua.Estado,
                p.Pozo.FuenteAgua.Caudal, p.Pozo.FuenteAgua.IdLocalidad }).ToListAsync(ct);
        var resultados = baterias.Select(b =>
        {
            var asociados = pozos.Where(p => p.IdBateria == b.IdBateria).ToList();
            var problemas = asociados.Where(p => p.Estado != "Activa" || p.Caudal <= 0 || p.IdLocalidad != localidadId)
                .Select(p => p.IdPozo).Order().ToList();
            List<string> alertas = [];
            if (asociados.Count == 0) alertas.Add("BateriaSinPozos");
            if (problemas.Count > 0) alertas.Add("PozosNoDisponibles");
            return new BateriaAlertasDto(b.IdBateria, b.Nombre, alertas, problemas);
        }).ToList();
        await transaction.CommitAsync(ct);
        return new(hoy, null, pagina, total, (long)pagina * TamanoPagina < total, resultados);
    }

    private static DateOnly FechaLocal()
    {
        var zona = HorarioSistema.ZonaHoraria;
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(zona)).DateTime);
    }
}

