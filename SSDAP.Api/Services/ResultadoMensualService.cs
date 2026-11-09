using System.Data;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SSDAP.Api.Data;
using SSDAP.Api.Models;
using SSDAP.Shared.DTOs.Reportes;

namespace SSDAP.Api.Services;

public sealed class ResultadoMensualService(AppDbContext db, AccesoLocalidadService acceso,
    ProduccionMensualService produccion, IHttpContextAccessor context, CalidadMensualService calidad)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private sealed record CapturaMensual(ProduccionMensualDto Produccion, CalidadMensualDto? Calidad = null);

    public async Task<ResultadoMensualDetalleDto> GuardarAsync(int localidadId, GuardarResultadoMensualDto datos, CancellationToken ct)
    {
        await acceso.VerificarAsync(localidadId, ct);
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, ct);
        // Serializa las revisiones por localidad y evita competir con modificaciones del calendario.
        await db.Localidades.FromSqlInterpolated($"SELECT * FROM localidad WHERE id_localidad = {localidadId} FOR UPDATE")
            .AsNoTracking().ToListAsync(ct);
        var revisionAnterior = await db.ResultadosMensuales
            .Where(r => r.IdLocalidad == localidadId && r.Anio == datos.Produccion.Anio && r.Mes == datos.Produccion.Mes)
            .MaxAsync(r => r.Revision, ct) ?? 0;
        if (revisionAnterior == int.MaxValue)
            throw new ReglaNegocioException(409, "Se alcanzó el máximo de revisiones del período.");
        var reporte = await produccion.ConsultarEnLecturaAsync(localidadId, datos.Produccion, ct);
        var ahora = DateTime.UtcNow;
        // La captura sustituye únicamente la advertencia de vista previa; conserva las demás limitaciones.
        reporte = reporte with
        {
            Advertencias = reporte.Advertencias.Where(a => !a.StartsWith("Consulta recalculada", StringComparison.Ordinal))
                .Append("Resumen estimado guardado; no acredita ejecución real ni certifica potabilidad.").ToList()
        };
        var mezcla = await calidad.CalcularEnLecturaAsync(reporte, datos.Calidad, ct);
        var resultado = new ResultadoMensual
        {
            IdLocalidad = localidadId, Anio = datos.Produccion.Anio, Mes = datos.Produccion.Mes,
            Revision = revisionAnterior + 1, VersionFormato = 4,
            ReporteJson = JsonSerializer.Serialize(new CapturaMensual(reporte, mezcla), Json),
            CreadoEnUtc = ahora, CreadoPorId = int.Parse(context.HttpContext!.User.FindFirstValue("sub")!),
            FechaGeneracion = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(ahora, TimeZoneInfo.FindSystemTimeZoneById(reporte.ZonaHoraria))),
            EstadoCalidad = mezcla.Estado,
            Observaciones = string.IsNullOrWhiteSpace(datos.Observaciones) ? null : datos.Observaciones.Trim()
        };
        // Los campos anteriores de caudal/dosis quedan null: el reporte conserva magnitudes y unidades inequívocas.
        db.ResultadosMensuales.Add(resultado);
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return new(Resumen(resultado), 4, reporte, null) { Calidad = mezcla };
    }

    public async Task<List<ResultadoMensualResumenDto>> ListarAsync(
        int localidadId, int? anio, int? mes, int pagina, CancellationToken ct)
    {
        await acceso.VerificarAsync(localidadId, ct);
        var consulta = db.ResultadosMensuales.AsNoTracking().Where(r => r.IdLocalidad == localidadId);
        if (anio.HasValue) consulta = consulta.Where(r => r.Anio == anio.Value);
        if (mes.HasValue) consulta = consulta.Where(r => r.Mes == mes.Value);
        // No traer el JSON en el listado paginado.
        return await consulta.OrderByDescending(r => r.Anio).ThenByDescending(r => r.Mes)
            .ThenByDescending(r => r.Revision).ThenByDescending(r => r.IdResultado)
            .Skip((pagina - 1) * 50).Take(50)
            .Select(r => new ResultadoMensualResumenDto(r.IdResultado, r.IdLocalidad, r.Anio, r.Mes,
                r.Revision, r.ReporteJson == null ? "SinCapturaHistorica" : "Guardado", r.FechaGeneracion,
                r.CreadoEnUtc == null ? null : new DateTimeOffset(DateTime.SpecifyKind(r.CreadoEnUtc.Value, DateTimeKind.Utc)),
                r.CreadoPorId, r.Observaciones)).ToListAsync(ct);
    }

    public async Task<ResultadoMensualDetalleDto> ObtenerAsync(int localidadId, int id, CancellationToken ct)
    {
        await acceso.VerificarAsync(localidadId, ct);
        var r = await db.ResultadosMensuales.AsNoTracking().SingleOrDefaultAsync(r => r.IdLocalidad == localidadId && r.IdResultado == id, ct)
            ?? throw new ReglaNegocioException(404, "Resultado mensual no encontrado en esta localidad.");
        if (r.ReporteJson is null)
            return new(Resumen(r), r.VersionFormato, null, new(r.CaudalTotal, r.DosisCloro, r.EstadoCalidad));
        if (r.VersionFormato is not (1 or 2 or 3 or 4))
            throw new ReglaNegocioException(409, "Versión de reporte guardado no soportada.");
        ProduccionMensualDto? reporte;
        CalidadMensualDto? mezcla = null;
        try
        {
            if (r.VersionFormato == 1) reporte = JsonSerializer.Deserialize<ProduccionMensualDto>(r.ReporteJson, Json);
            else
            {
                var captura = JsonSerializer.Deserialize<CapturaMensual>(r.ReporteJson, Json);
                reporte = captura?.Produccion;
                mezcla = captura?.Calidad;
            }
        }
        catch (JsonException) { throw new ReglaNegocioException(409, "La captura del resultado mensual no se puede leer."); }
        if (reporte is null || reporte.IdLocalidad != r.IdLocalidad || reporte.Anio != r.Anio || reporte.Mes != r.Mes)
            throw new ReglaNegocioException(409, "La captura no coincide con el resultado mensual.");
        if (r.VersionFormato >= 3 && (mezcla is null || mezcla.IdLocalidad != r.IdLocalidad || mezcla.Anio != r.Anio || mezcla.Mes != r.Mes))
            throw new ReglaNegocioException(409, "La captura de calidad no coincide con el resultado mensual.");
        return new(Resumen(r), r.VersionFormato, reporte, null) { Calidad = mezcla };
    }

    private static ResultadoMensualResumenDto Resumen(ResultadoMensual r) =>
        new(r.IdResultado, r.IdLocalidad, r.Anio, r.Mes, r.Revision,
            r.ReporteJson is null ? "SinCapturaHistorica" : "Guardado", r.FechaGeneracion,
            r.CreadoEnUtc.HasValue ? new DateTimeOffset(DateTime.SpecifyKind(r.CreadoEnUtc.Value, DateTimeKind.Utc)) : null,
            r.CreadoPorId, r.Observaciones);
}
