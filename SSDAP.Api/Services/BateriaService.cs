using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using SSDAP.Api.Data;
using SSDAP.Api.Models;
using SSDAP.Shared.DTOs.Baterias;

namespace SSDAP.Api.Services;

public sealed class BateriaService(AppDbContext db, AccesoLocalidadService acceso)
{
    public async Task<List<BateriaResumenDto>> ListarAsync(int localidadId, string? estado, int pagina, CancellationToken ct)
    {
        await acceso.VerificarAsync(localidadId, ct);
        var consulta = db.Baterias.AsNoTracking().Where(b => b.IdLocalidad == localidadId);
        if (estado is not null) consulta = consulta.Where(b => b.Estado == estado);
        var baterias = await consulta.OrderBy(b => b.Nombre).ThenBy(b => b.IdBateria)
            .Skip((pagina - 1) * 50).Take(50).ToListAsync(ct);
        return (await ConstruirDetallesAsync(baterias, ct)).Select(b => b.Bateria).ToList();
    }

    public async Task<BateriaDto> ObtenerAsync(int localidadId, int id, CancellationToken ct)
    {
        await acceso.VerificarAsync(localidadId, ct);
        var bateria = await BuscarAsync(localidadId, id, ct);
        return (await ConstruirDetallesAsync([bateria], ct)).Single();
    }

    public async Task<BateriaDto> CrearAsync(int localidadId, GuardarBateriaDto datos, CancellationToken ct)
    {
        await acceso.VerificarAsync(localidadId, ct);
        var bateria = new Bateria { IdLocalidad = localidadId };
        ActualizarDatos(bateria, datos);
        db.Baterias.Add(bateria);
        await db.SaveChangesAsync(ct);
        return (await ConstruirDetallesAsync([bateria], ct)).Single();
    }

    public async Task<BateriaDto> ActualizarAsync(int localidadId, int id, GuardarBateriaDto datos, CancellationToken ct)
    {
        await acceso.VerificarAsync(localidadId, ct);
        var bateria = await BuscarAsync(localidadId, id, ct);
        // Comprobar las relaciones antes de escribir si hubo cargas ajenas a la API.
        await ConstruirDetallesAsync([bateria], ct);
        ActualizarDatos(bateria, datos);
        await db.SaveChangesAsync(ct);
        return (await ConstruirDetallesAsync([bateria], ct)).Single();
    }

    public async Task AsociarPozoAsync(int localidadId, int id, int pozoId, CancellationToken ct)
    {
        await acceso.VerificarAsync(localidadId, ct);
        await BuscarAsync(localidadId, id, ct);
        var pozo = await db.Pozos.AsNoTracking().Include(p => p.FuenteAgua)
            .SingleOrDefaultAsync(p => p.IdPozo == pozoId && p.FuenteAgua.IdLocalidad == localidadId, ct)
            ?? throw new ReglaNegocioException(404, "Pozo no encontrado en esta localidad.");
        if (pozo.FuenteAgua.TipoFuente != "Pozo" ||
            await db.Acueductos.AnyAsync(a => a.IdFuente == pozo.IdFuente, ct))
            throw new ReglaNegocioException(409, "El pozo tiene una fuente inconsistente.");
        if (await db.BateriasPozos.AnyAsync(x => x.IdBateria == id && x.IdPozo == pozoId, ct)) return;

        var relacion = new BateriaPozo { IdBateria = id, IdPozo = pozoId };
        db.BateriasPozos.Add(relacion);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is MySqlException { Number: 1062 })
        {
            // Dos PUT simultáneos deben terminar con una sola asociación y el mismo resultado.
            db.Entry(relacion).State = EntityState.Detached;
            if (!await db.BateriasPozos.AnyAsync(x => x.IdBateria == id && x.IdPozo == pozoId, ct)) throw;
        }
    }

    public async Task DesasociarPozoAsync(int localidadId, int id, int pozoId, CancellationToken ct)
    {
        await acceso.VerificarAsync(localidadId, ct);
        await BuscarAsync(localidadId, id, ct);
        // DELETE repetido es idempotente, incluso si el vínculo ya no existe.
        await db.BateriasPozos.Where(x => x.IdBateria == id && x.IdPozo == pozoId).ExecuteDeleteAsync(ct);
    }

    private async Task<Bateria> BuscarAsync(int localidadId, int id, CancellationToken ct) =>
        await db.Baterias.SingleOrDefaultAsync(b => b.IdBateria == id && b.IdLocalidad == localidadId, ct)
        ?? throw new ReglaNegocioException(404, "Batería no encontrada en esta localidad.");

    private async Task<List<BateriaDto>> ConstruirDetallesAsync(List<Bateria> baterias, CancellationToken ct)
    {
        if (baterias.Count == 0) return [];
        var ids = baterias.Select(b => b.IdBateria).ToArray();
        var relaciones = await db.BateriasPozos.AsNoTracking().Where(x => ids.Contains(x.IdBateria))
            .Select(x => new
            {
                x.IdBateria, x.Pozo.FuenteAgua.IdLocalidad, x.Pozo.FuenteAgua.TipoFuente,
                TieneAcueducto = db.Acueductos.Any(a => a.IdFuente == x.Pozo.IdFuente),
                Pozo = new PozoBateriaDto(x.IdPozo, x.Pozo.IdFuente, x.Pozo.Codigo,
                    x.Pozo.FuenteAgua.Nombre, x.Pozo.FuenteAgua.Estado, x.Pozo.FuenteAgua.Caudal)
            }).ToListAsync(ct);
        var grupos = relaciones.ToLookup(x => x.IdBateria);
        var resultado = new List<BateriaDto>();
        foreach (var bateria in baterias)
        {
            var grupo = grupos[bateria.IdBateria].ToList();
            if (grupo.Any(x => x.IdLocalidad != bateria.IdLocalidad || x.TipoFuente != "Pozo" || x.TieneAcueducto))
                throw new ReglaNegocioException(409, "La batería tiene asociaciones inconsistentes. Revisar sus datos.");
            var pozos = grupo.Select(x => x.Pozo).OrderBy(p => p.Codigo).ThenBy(p => p.IdPozo).ToList();
            var total = pozos.Sum(p => p.CaudalM3H);
            var disponible = bateria.Estado == "Activa"
                ? pozos.Where(p => p.Estado == "Activa").Sum(p => p.CaudalM3H) : 0m;
            resultado.Add(new BateriaDto(new BateriaResumenDto(bateria.IdBateria, bateria.IdLocalidad,
                bateria.Nombre, bateria.Descripcion, bateria.Estado, pozos.Count, total, disponible), pozos));
        }
        return resultado;
    }

    private static void ActualizarDatos(Bateria bateria, GuardarBateriaDto datos)
    {
        bateria.Nombre = datos.Nombre.Trim();
        bateria.Descripcion = string.IsNullOrWhiteSpace(datos.Descripcion) ? null : datos.Descripcion.Trim();
        bateria.Estado = datos.Estado;
    }
}
