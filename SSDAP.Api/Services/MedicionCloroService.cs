using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SSDAP.Api.Data;
using SSDAP.Api.Models;
using SSDAP.Shared.DTOs.Cloro;

namespace SSDAP.Api.Services;

public sealed class MedicionCloroService(AppDbContext db, AccesoLocalidadService acceso, IHttpContextAccessor context)
{
    public async Task<MedicionCloroDto> CrearAsync(int localidadId, CrearMedicionCloroDto datos, CancellationToken ct)
    {
        await acceso.VerificarAsync(localidadId, ct);
        var fecha = FechasApi.LeerInstante(datos.FechaHora);
        var ahora = DateTime.UtcNow;
        if (fecha > ahora)
            throw new ReglaNegocioException(400, "Una medición realizada no puede tener fecha futura.");
        var medicion = new MedicionCloro
        {
            IdLocalidad = localidadId, FechaHoraUtc = fecha, ValorMgL = datos.ValorMgL!.Value,
            TipoCloro = datos.TipoCloro,
            Observaciones = string.IsNullOrWhiteSpace(datos.Observaciones) ? null : datos.Observaciones.Trim(),
            CreadoEnUtc = ahora, CreadoPorId = int.Parse(context.HttpContext!.User.FindFirstValue("sub")!)
        };
        db.MedicionesCloro.Add(medicion);
        await db.SaveChangesAsync(ct);
        return Detalle(medicion);
    }

    public async Task<List<MedicionCloroDto>> ListarAsync(int localidadId, string? desde, string? hasta,
        string? tipoCloro, int pagina, CancellationToken ct)
    {
        await acceso.VerificarAsync(localidadId, ct);
        DateTime? inicio = desde is null ? null : FechasApi.LeerInstante(desde);
        DateTime? fin = hasta is null ? null : FechasApi.LeerInstante(hasta);
        if (inicio.HasValue && fin.HasValue && inicio >= fin)
            throw new ReglaNegocioException(400, "Desde debe ser anterior a hasta.");
        var consulta = Consulta(localidadId, tipoCloro);
        if (inicio.HasValue) consulta = consulta.Where(m => m.FechaHoraUtc >= inicio.Value);
        if (fin.HasValue) consulta = consulta.Where(m => m.FechaHoraUtc < fin.Value);
        var filas = await consulta.OrderByDescending(m => m.FechaHoraUtc).ThenByDescending(m => m.IdMedicionCloro)
            .Skip((pagina - 1) * 50).Take(50).ToListAsync(ct);
        return filas.Select(Detalle).ToList();
    }

    public async Task<MedicionCloroDto> ObtenerAsync(int localidadId, int? id, string? tipoCloro, CancellationToken ct)
    {
        await acceso.VerificarAsync(localidadId, ct);
        var consulta = Consulta(localidadId, tipoCloro);
        if (id.HasValue) consulta = consulta.Where(m => m.IdMedicionCloro == id.Value);
        var medicion = await consulta.OrderByDescending(m => m.FechaHoraUtc).ThenByDescending(m => m.IdMedicionCloro)
            .FirstOrDefaultAsync(ct)
            ?? throw new ReglaNegocioException(404, "Medición no encontrada en esta localidad.");
        return Detalle(medicion);
    }

    private IQueryable<MedicionCloro> Consulta(int localidadId, string? tipoCloro)
    {
        var consulta = db.MedicionesCloro.AsNoTracking().Where(m => m.IdLocalidad == localidadId);
        return tipoCloro is null ? consulta : consulta.Where(m => m.TipoCloro == tipoCloro);
    }

    internal static MedicionCloroDto Detalle(MedicionCloro m) =>
        new(m.IdMedicionCloro, m.IdLocalidad, Utc(m.FechaHoraUtc), m.ValorMgL, m.TipoCloro,
            m.Observaciones, Utc(m.CreadoEnUtc), m.CreadoPorId);

    private static DateTimeOffset Utc(DateTime fecha) => new(DateTime.SpecifyKind(fecha, DateTimeKind.Utc));
}
