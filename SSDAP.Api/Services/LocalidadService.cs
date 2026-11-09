using Microsoft.EntityFrameworkCore;
using SSDAP.Api.Data;
using SSDAP.Api.Models;
using SSDAP.Shared.DTOs.Localidades;
namespace SSDAP.Api.Services;
public sealed class LocalidadService(AppDbContext db)
{
    public Task<List<LocalidadDto>> ListarAsync(int? usuarioId, int pagina, CancellationToken ct)
    {
        var consulta = db.Localidades.AsNoTracking();
        if (usuarioId.HasValue)
            consulta = consulta.Where(l => db.UsuariosLocalidades.Any(x => x.IdUsuario == usuarioId.Value && x.IdLocalidad == l.IdLocalidad));
        return consulta.OrderBy(l => l.Nombre).ThenBy(l => l.IdLocalidad).Skip((pagina - 1) * 50).Take(50)
            .Select(l => new LocalidadDto(l.IdLocalidad, l.Nombre, l.Provincia, l.Estado)).ToListAsync(ct);
    }
    public async Task<LocalidadDto> ObtenerAsync(int id, CancellationToken ct)
    {
        var l = await db.Localidades.AsNoTracking().SingleOrDefaultAsync(l => l.IdLocalidad == id, ct)
            ?? throw new ReglaNegocioException(404, "Localidad no encontrada.");
        return new(l.IdLocalidad, l.Nombre, l.Provincia, l.Estado);
    }
    public async Task<LocalidadDto> CrearAsync(CrearLocalidadDto datos, CancellationToken ct)
    {
        var l = new Localidad { Nombre = datos.Nombre.Trim(), Provincia = datos.Provincia.Trim(), Estado = "Activa" };
        db.Localidades.Add(l);
        await db.SaveChangesAsync(ct);
        return new(l.IdLocalidad, l.Nombre, l.Provincia, l.Estado);
    }

    public async Task<LocalidadDto> ActualizarAsync(int id, ActualizarLocalidadDto datos, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var localidad = (await db.Localidades.FromSqlInterpolated($"SELECT * FROM localidad WHERE id_localidad = {id} FOR UPDATE")
            .ToListAsync(ct)).SingleOrDefault() ?? throw new ReglaNegocioException(404, "Localidad no encontrada.");
        localidad.Nombre = datos.Nombre.Trim();
        localidad.Provincia = datos.Provincia.Trim();
        localidad.Estado = datos.Estado;
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return new(localidad.IdLocalidad, localidad.Nombre, localidad.Provincia, localidad.Estado);
    }
}
