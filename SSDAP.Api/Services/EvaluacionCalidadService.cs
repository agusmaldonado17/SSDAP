using Microsoft.EntityFrameworkCore;
using SSDAP.Api.Data;
using SSDAP.Shared.DTOs.Calidad;

namespace SSDAP.Api.Services;

public sealed class EvaluacionCalidadService(AppDbContext db, AnalisisCalidadService analisis)
{
    public async Task<EvaluacionCalidadDto> EvaluarAsync(int localidadId, int fuenteId, int id, CancellationToken ct)
    {
        var datos = await analisis.ObtenerAsync(localidadId, fuenteId, id, ct);
        var limites = (await db.ParametrosLey.AsNoTracking()
            .Where(p => p.Codigo != null).ToListAsync(ct))
            .Select(ParametroCalidadService.Detalle).ToList();
        var resultados = EvaluadorCalidad.Comparar(datos.Mediciones, limites);
        var fuera = resultados.Count(p => p.Estado == "FueraDeRango");
        var dentro = resultados.Count(p => p.Estado == "DentroDeRango");
        return new(datos.IdAnalisis, datos.IdFuente, datos.Fecha, DateTimeOffset.UtcNow,
            fuera, dentro, resultados.Count - fuera - dentro, resultados);
    }
}
