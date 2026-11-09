using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SSDAP.Api.Data;
using SSDAP.Api.Models;
using SSDAP.Shared.DTOs.Calidad;

namespace SSDAP.Api.Services;

public sealed class AnalisisCalidadService(AppDbContext db, AccesoLocalidadService acceso, IHttpContextAccessor context)
{
    public async Task<List<AnalisisCalidadDto>> ListarAsync(
        int localidadId, int fuenteId, DateOnly? desde, DateOnly? hasta, int pagina, CancellationToken ct)
    {
        await VerificarFuenteAsync(localidadId, fuenteId, ct);
        if (desde > hasta)
            throw new ReglaNegocioException(400, "Desde no puede ser posterior a hasta.");
        var consulta = db.AnalisisCalidad.AsNoTracking().Where(a => a.IdFuente == fuenteId);
        if (desde.HasValue) consulta = consulta.Where(a => a.Fecha >= desde.Value);
        if (hasta.HasValue) consulta = consulta.Where(a => a.Fecha <= hasta.Value);
        var resultados = await consulta.OrderByDescending(a => a.Fecha).ThenByDescending(a => a.IdAnalisis)
            .Skip((pagina - 1) * 50).Take(50).ToListAsync(ct);
        return resultados.Select(Detalle).ToList();
    }

    public async Task<AnalisisCalidadDto> ObtenerAsync(
        int localidadId, int fuenteId, int? id, CancellationToken ct)
    {
        await VerificarFuenteAsync(localidadId, fuenteId, ct);
        var consulta = db.AnalisisCalidad.AsNoTracking().Where(a => a.IdFuente == fuenteId);
        if (id.HasValue) consulta = consulta.Where(a => a.IdAnalisis == id.Value);
        var analisis = await consulta.OrderByDescending(a => a.Fecha).ThenByDescending(a => a.IdAnalisis)
            .FirstOrDefaultAsync(ct)
            ?? throw new ReglaNegocioException(404, "Análisis no encontrado para esta fuente.");
        return Detalle(analisis);
    }

    public async Task<AnalisisCalidadDto> CrearAsync(
        int localidadId, int fuenteId, CrearAnalisisDto datos, CancellationToken ct)
    {
        await VerificarFuenteAsync(localidadId, fuenteId, ct);
        var analisis = new AnalisisCalidad { IdFuente = fuenteId };
        Aplicar(analisis, datos);
        db.AnalisisCalidad.Add(analisis);
        await db.SaveChangesAsync(ct);
        return Detalle(analisis);
    }

    public async Task<AnalisisCalidadDto> EditarAsync(int localidadId, int fuenteId, int id, EditarAnalisisDto datos, CancellationToken ct)
    {
        await VerificarFuenteAsync(localidadId, fuenteId, ct);
        var analisis = await db.AnalisisCalidad.SingleOrDefaultAsync(a => a.IdFuente == fuenteId && a.IdAnalisis == id, ct)
            ?? throw new ReglaNegocioException(404, "Análisis no encontrado para esta fuente.");
        if (analisis.Version != datos.Version || analisis.Version == int.MaxValue)
            throw new ReglaNegocioException(409, "El análisis cambió. Volvé a abrirlo antes de editar.");
        Aplicar(analisis, datos);
        analisis.Version++;
        analisis.ModificadoEnUtc = DateTime.UtcNow;
        analisis.ModificadoPorId = int.Parse(context.HttpContext!.User.FindFirstValue("sub")!);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException) { throw new ReglaNegocioException(409, "El análisis cambió. Volvé a abrirlo antes de editar."); }
        return Detalle(analisis);
    }

    private static void Aplicar(AnalisisCalidad analisis, CrearAnalisisDto datos)
    {
        analisis.Fecha = datos.Fecha!.Value;
        analisis.ResiduoSeco = datos.Mediciones.ResiduoSeco;
        analisis.Conductividad = datos.Mediciones.Conductividad;
        analisis.Ph = datos.Mediciones.Ph;
        analisis.Cloruro = datos.Mediciones.Cloruro;
        analisis.Sulfato = datos.Mediciones.Sulfato;
        analisis.AlcalinidadTotal = datos.Mediciones.AlcalinidadTotal;
        analisis.Carbonato = datos.Mediciones.Carbonato;
        analisis.Bicarbonato = datos.Mediciones.Bicarbonato;
        analisis.DurezaTotal = datos.Mediciones.DurezaTotal;
        analisis.Calcio = datos.Mediciones.Calcio;
        analisis.Magnesio = datos.Mediciones.Magnesio;
        analisis.Hierro = datos.Mediciones.Hierro;
        analisis.Nitrato = datos.Mediciones.Nitrato;
        analisis.Nitrito = datos.Mediciones.Nitrito;
        analisis.Amonio = datos.Mediciones.Amonio;
        analisis.Fluor = datos.Mediciones.Fluor;
        analisis.Arsenico = datos.Mediciones.Arsenico;
        analisis.Sodio = datos.Mediciones.Sodio;
        analisis.Potasio = datos.Mediciones.Potasio;
        analisis.Observaciones = string.IsNullOrWhiteSpace(datos.Observaciones) ? null : datos.Observaciones.Trim();
    }

    private async Task VerificarFuenteAsync(int localidadId, int fuenteId, CancellationToken ct)
    {
        await acceso.VerificarAsync(localidadId, ct);
        if (!await db.FuentesAgua.AnyAsync(f => f.IdFuente == fuenteId && f.IdLocalidad == localidadId, ct))
            throw new ReglaNegocioException(404, "Fuente no encontrada en esta localidad.");
    }

    internal static AnalisisCalidadDto Detalle(AnalisisCalidad analisis) =>
        new(analisis.IdAnalisis, analisis.IdFuente, analisis.Fecha, new MedicionesCalidadDto
        {
            ResiduoSeco = analisis.ResiduoSeco,
            Conductividad = analisis.Conductividad,
            Ph = analisis.Ph,
            Cloruro = analisis.Cloruro,
            Sulfato = analisis.Sulfato,
            AlcalinidadTotal = analisis.AlcalinidadTotal,
            Carbonato = analisis.Carbonato,
            Bicarbonato = analisis.Bicarbonato,
            DurezaTotal = analisis.DurezaTotal,
            Calcio = analisis.Calcio,
            Magnesio = analisis.Magnesio,
            Hierro = analisis.Hierro,
            Nitrato = analisis.Nitrato,
            Nitrito = analisis.Nitrito,
            Amonio = analisis.Amonio,
            Fluor = analisis.Fluor,
            Arsenico = analisis.Arsenico,
            Sodio = analisis.Sodio,
            Potasio = analisis.Potasio,
        }, analisis.Observaciones) { Version = analisis.Version };
}

