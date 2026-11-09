using Microsoft.EntityFrameworkCore;
using SSDAP.Api.Data;
using SSDAP.Api.Models;
using SSDAP.Shared.DTOs.Fuentes;

namespace SSDAP.Api.Services;

public sealed class FuenteService(AppDbContext db, AccesoLocalidadService acceso)
{
    private const string TipoPozo = "Pozo";
    private const string TipoAcueducto = "Acueducto";

    public async Task<List<FuenteResumenDto>> ListarAsync(
        int localidadId, string? tipo, string? estado, int pagina, CancellationToken ct)
    {
        await acceso.VerificarAsync(localidadId, ct);
        var consulta = db.FuentesAgua.AsNoTracking().Where(f => f.IdLocalidad == localidadId);
        if (tipo is not null) consulta = consulta.Where(f => f.TipoFuente == tipo);
        if (estado is not null) consulta = consulta.Where(f => f.Estado == estado);

        return await consulta.OrderBy(f => f.Nombre).ThenBy(f => f.IdFuente)
            .Skip((pagina - 1) * 50).Take(50)
            .Select(f => new FuenteResumenDto(f.IdFuente, f.IdLocalidad, f.Nombre,
                f.TipoFuente, f.Caudal, f.Estado, f.Observaciones)).ToListAsync(ct);
    }

    public async Task<FuenteDto> ObtenerAsync(int localidadId, int id, CancellationToken ct)
    {
        await acceso.VerificarAsync(localidadId, ct);
        var fuente = await BuscarAsync(localidadId, id, ct);
        return await DetalleAsync(fuente, ct);
    }

    public async Task<FuenteDto> CrearPozoAsync(int localidadId, GuardarPozoDto datos, CancellationToken ct)
    {
        await acceso.VerificarAsync(localidadId, ct);
        ValidarPrecision(datos);
        var fuente = NuevaFuente(localidadId, TipoPozo, datos);
        var pozo = new Pozo { FuenteAgua = fuente };
        ActualizarPozo(pozo, datos);
        db.Pozos.Add(pozo);
        // SaveChanges guarda fuente y especialización en una misma transacción.
        await db.SaveChangesAsync(ct);
        return new FuenteDto(Resumen(fuente), DetallePozo(pozo), null);
    }

    public async Task<FuenteDto> CrearAcueductoAsync(int localidadId, GuardarFuenteDto datos, CancellationToken ct)
    {
        await acceso.VerificarAsync(localidadId, ct);
        ValidarPrecision(datos);
        var fuente = NuevaFuente(localidadId, TipoAcueducto, datos);
        var acueducto = new Acueducto { FuenteAgua = fuente };
        db.Acueductos.Add(acueducto);
        await db.SaveChangesAsync(ct);
        return new FuenteDto(Resumen(fuente), null, acueducto.IdAcueducto);
    }

    public async Task<FuenteDto> ActualizarPozoAsync(
        int localidadId, int id, GuardarPozoDto datos, CancellationToken ct)
    {
        await acceso.VerificarAsync(localidadId, ct);
        ValidarPrecision(datos);
        var fuente = await BuscarAsync(localidadId, id, ct);
        var detalle = await DetalleAsync(fuente, ct);
        if (detalle.Pozo is null)
            throw new ReglaNegocioException(409, "La fuente no es un pozo. El tipo no se puede cambiar.");

        var pozo = await db.Pozos.SingleAsync(p => p.IdFuente == id, ct);
        ActualizarFuente(fuente, datos);
        ActualizarPozo(pozo, datos);
        await db.SaveChangesAsync(ct);
        return new FuenteDto(Resumen(fuente), DetallePozo(pozo), null);
    }

    public async Task<FuenteDto> ActualizarAcueductoAsync(
        int localidadId, int id, GuardarFuenteDto datos, CancellationToken ct)
    {
        await acceso.VerificarAsync(localidadId, ct);
        ValidarPrecision(datos);
        var fuente = await BuscarAsync(localidadId, id, ct);
        var detalle = await DetalleAsync(fuente, ct);
        if (detalle.IdAcueducto is null)
            throw new ReglaNegocioException(409, "La fuente no es un acueducto. El tipo no se puede cambiar.");

        ActualizarFuente(fuente, datos);
        await db.SaveChangesAsync(ct);
        return detalle with { Fuente = Resumen(fuente) };
    }

    public async Task<FuenteDto> CambiarEstadoAsync(
        int localidadId, int id, CambiarEstadoFuenteDto datos, CancellationToken ct)
    {
        await acceso.VerificarAsync(localidadId, ct);
        var fuente = await BuscarAsync(localidadId, id, ct);
        var detalle = await DetalleAsync(fuente, ct);
        fuente.Estado = datos.Estado;
        await db.SaveChangesAsync(ct);
        return detalle with { Fuente = Resumen(fuente) };
    }

    private async Task<FuenteAgua> BuscarAsync(int localidadId, int id, CancellationToken ct) =>
        await db.FuentesAgua.SingleOrDefaultAsync(f => f.IdLocalidad == localidadId && f.IdFuente == id, ct)
        ?? throw new ReglaNegocioException(404, "Fuente no encontrada en esta localidad.");

    private async Task<FuenteDto> DetalleAsync(FuenteAgua fuente, CancellationToken ct)
    {
        var pozo = await db.Pozos.AsNoTracking().SingleOrDefaultAsync(p => p.IdFuente == fuente.IdFuente, ct);
        var acueducto = await db.Acueductos.AsNoTracking().SingleOrDefaultAsync(a => a.IdFuente == fuente.IdFuente, ct);
        var esPozo = fuente.TipoFuente == TipoPozo && pozo is not null && acueducto is null;
        var esAcueducto = fuente.TipoFuente == TipoAcueducto && acueducto is not null && pozo is null;
        if (!esPozo && !esAcueducto)
            throw new ReglaNegocioException(409, "La fuente tiene un tipo o una especialización inconsistente. Revisar sus datos.");

        return new FuenteDto(Resumen(fuente), pozo is null ? null : DetallePozo(pozo), acueducto?.IdAcueducto);
    }

    private static FuenteAgua NuevaFuente(int localidadId, string tipo, GuardarFuenteDto datos)
    {
        var fuente = new FuenteAgua { IdLocalidad = localidadId, TipoFuente = tipo };
        ActualizarFuente(fuente, datos);
        return fuente;
    }

    private static void ActualizarFuente(FuenteAgua fuente, GuardarFuenteDto datos)
    {
        fuente.Nombre = datos.Nombre.Trim();
        fuente.Caudal = datos.CaudalM3H!.Value;
        fuente.Estado = datos.Estado;
        fuente.Observaciones = string.IsNullOrWhiteSpace(datos.Observaciones) ? null : datos.Observaciones.Trim();
    }

    private static void ActualizarPozo(Pozo pozo, GuardarPozoDto datos)
    {
        pozo.Codigo = datos.Codigo.Trim();
        pozo.Latitud = datos.Latitud!.Value;
        pozo.Longitud = datos.Longitud!.Value;
    }

    private static void ValidarPrecision(GuardarFuenteDto datos)
    {
        if (datos.CaudalM3H != decimal.Round(datos.CaudalM3H!.Value, 2))
            throw new ReglaNegocioException(400, "El caudal admite hasta 2 decimales en m³/h.");
        if (datos is GuardarPozoDto pozo &&
            (pozo.Latitud != decimal.Round(pozo.Latitud!.Value, 7) ||
             pozo.Longitud != decimal.Round(pozo.Longitud!.Value, 7)))
            throw new ReglaNegocioException(400, "Las coordenadas admiten hasta 7 decimales.");
    }

    private static FuenteResumenDto Resumen(FuenteAgua fuente) => new(
        fuente.IdFuente, fuente.IdLocalidad, fuente.Nombre, fuente.TipoFuente,
        fuente.Caudal, fuente.Estado, fuente.Observaciones);

    private static PozoDetalleDto DetallePozo(Pozo pozo) =>
        new(pozo.IdPozo, pozo.Codigo, pozo.Latitud, pozo.Longitud);
}
