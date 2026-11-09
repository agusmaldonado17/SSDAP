using System.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SSDAP.Api.Data;
using SSDAP.Api.Models;
using SSDAP.Shared.DTOs.Calendario;

namespace SSDAP.Api.Services;

public sealed class CalendarioService(AppDbContext db, AccesoLocalidadService acceso, IHttpContextAccessor context)
{
    public async Task<List<ProgramacionDto>> ListarAsync(int localidadId, int anio, int mes, int pagina, CancellationToken ct)
    {
        await acceso.VerificarAsync(localidadId, ct);

        var (inicio, fin) = IntervalosCalendario.Mes(anio, mes, HorarioSistema.ZonaHoraria);
        var fecha = new DateOnly(anio, mes, 1);
        var fechaFin = fecha.AddMonths(1);
        var eventos = await Consulta(localidadId)
            .Where(p => (p.InicioUtc < fin && p.FinUtc > inicio) ||
                (p.InicioUtc == null && p.Fecha >= fecha && p.Fecha < fechaFin))
            .OrderBy(p => p.InicioUtc).ThenBy(p => p.IdProgramacion)
            .Skip((pagina - 1) * 50).Take(50).Include(p => p.Pozos).ToListAsync(ct);
        return eventos.Select(Convertir).ToList();
    }

    public async Task<ProgramacionDto> ObtenerAsync(int localidadId, int id, CancellationToken ct)
    {
        await acceso.VerificarAsync(localidadId, ct);
        var evento = await Consulta(localidadId).Include(p => p.Pozos).SingleOrDefaultAsync(p => p.IdProgramacion == id, ct)
            ?? throw new ReglaNegocioException(404, "Programación no encontrada en esta localidad.");
        return Convertir(evento);
    }

    public async Task<ProgramacionDto> GuardarAsync(int localidadId, int? id, GuardarProgramacionDto datos, CancellationToken ct)
    {
        await acceso.VerificarAsync(localidadId, ct);
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        await BloquearLocalidadAsync(localidadId, ct);
        var resultado = await GuardarEnTransaccionAsync(localidadId, id, datos, ct);
        await transaction.CommitAsync(ct);
        return resultado;
    }

    public async Task<List<ProgramacionDto>> CrearLoteAsync(int localidadId, CrearTurnosDto datos, CancellationToken ct)
    {
        await acceso.VerificarAsync(localidadId, ct);
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        await BloquearLocalidadAsync(localidadId, ct);
        var resultado = new List<ProgramacionDto>();
        foreach (var turno in datos.Turnos)
        {
            try { resultado.Add(await GuardarEnTransaccionAsync(localidadId, null, turno, ct)); }
            catch (ReglaNegocioException ex)
            { throw new ReglaNegocioException(ex.Estado, $"Turno {resultado.Count + 1} ({turno.Inicio}): {ex.Message} No se guardó ningún turno del grupo."); }
        }
        await transaction.CommitAsync(ct);
        return resultado;
    }

    private async Task<ProgramacionDto> GuardarEnTransaccionAsync(int localidadId, int? id, GuardarProgramacionDto datos, CancellationToken ct)
    {
        var (inicio, fin) = IntervalosCalendario.Leer(datos.Inicio, datos.Fin);
        var evento = id.HasValue
            ? await db.ProgramacionesBateria.SingleOrDefaultAsync(p => p.IdProgramacion == id && p.Bateria.IdLocalidad == localidadId, ct)
                ?? throw new ReglaNegocioException(404, "Programación no encontrada en esta localidad.")
            : new ProgramacionBateria();
        if (id.HasValue)
        {
            if (datos is not EditarProgramacionDto edicion || evento.Version != edicion.Version)
                throw new ReglaNegocioException(409, "La programación cambió. Volvé a consultarla antes de editar.");
            if (evento.Estado == "Cancelada")
                throw new ReglaNegocioException(409, "Una programación cancelada no puede editarse. Creá un turno nuevo.");
            if (evento.IdBateria != datos.IdBateria)
                throw new ReglaNegocioException(409, "No se puede cambiar la batería de un turno existente.");
        }
        if (await Consulta(localidadId).AnyAsync(p => p.Estado == "PendienteRevision" && p.IdProgramacion != (id ?? 0), ct))
            throw new ReglaNegocioException(409, "Hay turnos anteriores pendientes de revisión. Editalos o cancelalos antes de programar.");
        var bateria = await db.Baterias.AsNoTracking().SingleOrDefaultAsync(
            b => b.IdBateria == datos.IdBateria && b.IdLocalidad == localidadId, ct)
            ?? throw new ReglaNegocioException(404, "Batería no encontrada en esta localidad.");
        if (bateria.Estado != "Activa") throw new ReglaNegocioException(409, "La batería debe estar activa.");
        var pozos = await db.BateriasPozos.AsNoTracking().Where(x => x.IdBateria == bateria.IdBateria)
            .Select(x => new
            {
                x.IdPozo, x.Pozo.Codigo, x.Pozo.FuenteAgua.Caudal, x.Pozo.FuenteAgua.IdLocalidad,
                x.Pozo.FuenteAgua.TipoFuente, x.Pozo.FuenteAgua.Estado,
                TieneAcueducto = db.Acueductos.Any(a => a.IdFuente == x.Pozo.IdFuente)
            }).ToListAsync(ct);
        if (pozos.Count == 0) throw new ReglaNegocioException(409, "La batería no tiene pozos asociados.");
        foreach (var pozo in pozos.OrderBy(p => p.IdPozo))
        {
            if (pozo.IdLocalidad != localidadId || pozo.TipoFuente != "Pozo" || pozo.TieneAcueducto)
                throw new ReglaNegocioException(409, $"El pozo {pozo.Codigo} tiene una asociación inconsistente. Revisá los pozos de la batería.");
            if (pozo.Estado != "Activa")
                throw new ReglaNegocioException(409, $"No se puede programar: el pozo {pozo.Codigo} está en estado {pozo.Estado}. Todos los pozos de la batería deben estar activos.");
            if (pozo.Caudal <= 0)
                throw new ReglaNegocioException(409, $"No se puede programar: el pozo {pozo.Codigo} debe tener un caudal mayor que cero.");
        }
        var idsPozos = pozos.Select(p => p.IdPozo).ToArray();
        // El bloqueo por localidad serializa altas, ediciones y cancelaciones del calendario.
        var conflicto = await db.ProgramacionesPozos.AsNoTracking()
            .Where(p => idsPozos.Contains(p.IdPozo) && p.IdProgramacion != (id ?? 0) &&
                p.Programacion.Estado == "Programada" && p.Programacion.InicioUtc < fin && p.Programacion.FinUtc > inicio)
            .OrderBy(p => p.IdProgramacion).Select(p => new { p.IdPozo, p.IdProgramacion }).FirstOrDefaultAsync(ct);
        if (conflicto is not null)
            throw new ReglaNegocioException(409, $"El pozo {conflicto.IdPozo} se superpone con el turno {conflicto.IdProgramacion}.");

        var usuarioId = int.Parse(context.HttpContext!.User.FindFirstValue("sub")!);
        if (!id.HasValue)
        {
            evento.CreadoEnUtc = DateTime.UtcNow;
            evento.CreadoPorId = usuarioId;
            db.ProgramacionesBateria.Add(evento);
        }
        else
        {
            await db.ProgramacionesPozos.Where(p => p.IdProgramacion == id).ExecuteDeleteAsync(ct);
        }
        evento.IdBateria = bateria.IdBateria;
        evento.NombreBateria = bateria.Nombre;
        evento.InicioUtc = inicio;
        evento.FinUtc = fin;
        evento.Estado = "Programada";
        evento.Observaciones = string.IsNullOrWhiteSpace(datos.Observaciones) ? null : datos.Observaciones.Trim();
        evento.Version++;
        evento.ModificadoEnUtc = DateTime.UtcNow;
        evento.ModificadoPorId = usuarioId;
        // Compatibilidad con columnas anteriores; InicioUtc/FinUtc son la fuente de verdad.
        var zona = TimeZoneInfo.FindSystemTimeZoneById(HorarioSistema.ZonaHoraria);
        var inicioLocal = TimeZoneInfo.ConvertTimeFromUtc(inicio, zona);
        evento.Fecha = DateOnly.FromDateTime(inicioLocal);
        evento.HoraInicio = TimeOnly.FromDateTime(inicioLocal);
        evento.HoraFin = TimeOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(fin, zona));
        evento.Pozos = pozos.Select(p => new ProgramacionPozo
        { IdPozo = p.IdPozo, Codigo = p.Codigo, CaudalM3H = p.Caudal }).ToList();
        await db.SaveChangesAsync(ct);
        return Convertir(evento);
    }

    public async Task<ProgramacionDto> CancelarAsync(int localidadId, int id, int version, CancellationToken ct)
    {
        await acceso.VerificarAsync(localidadId, ct);
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        await BloquearLocalidadAsync(localidadId, ct);
        var evento = await db.ProgramacionesBateria.Include(p => p.Pozos)
            .SingleOrDefaultAsync(p => p.IdProgramacion == id && p.Bateria.IdLocalidad == localidadId, ct)
            ?? throw new ReglaNegocioException(404, "Programación no encontrada en esta localidad.");
        if (evento.Version != version) throw new ReglaNegocioException(409, "La programación cambió. Volvé a consultarla.");
        if (evento.Estado != "Cancelada")
        {
            evento.Estado = "Cancelada";
            evento.Version++;
            evento.ModificadoEnUtc = DateTime.UtcNow;
            evento.ModificadoPorId = int.Parse(context.HttpContext!.User.FindFirstValue("sub")!);
            await db.SaveChangesAsync(ct);
        }
        await transaction.CommitAsync(ct);
        return Convertir(evento);
    }

    public async Task<ProduccionProgramadaDto> ProduccionAsync(int localidadId, int anio, int mes, CancellationToken ct)
    {
        await acceso.VerificarAsync(localidadId, ct);
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, ct);
        var resultado = await ProduccionEnLecturaAsync(localidadId, anio, mes, ct);
        await transaction.CommitAsync(ct);
        return resultado;
    }

    // El llamador verifica acceso y mantiene una transacción consistente para todas las lecturas del reporte.
    internal async Task<ProduccionProgramadaDto> ProduccionEnLecturaAsync(int localidadId, int anio, int mes, CancellationToken ct)
    {

        var (inicio, fin) = IntervalosCalendario.Mes(anio, mes, HorarioSistema.ZonaHoraria);
        if (await Consulta(localidadId).AnyAsync(p => p.Estado == "PendienteRevision", ct))
            throw new ReglaNegocioException(409, "Revisá los turnos anteriores antes de estimar producción.");
        var eventos = await Consulta(localidadId).Where(p => p.Estado == "Programada" && p.InicioUtc < fin && p.FinUtc > inicio)
            .Include(p => p.Pozos).ToListAsync(ct);
        var filas = eventos.SelectMany(e => e.Pozos.Select(p => new
        {
            p.IdPozo, Horas = IntervalosCalendario.Horas(e.InicioUtc!.Value, e.FinUtc!.Value, inicio, fin), p.CaudalM3H
        }));
        var resultado = filas.GroupBy(p => p.IdPozo).OrderBy(g => g.Key)
            .Select(g => new ProduccionPozoDto(g.Key, g.Sum(p => p.Horas), g.Sum(p => p.Horas * p.CaudalM3H))).ToList();
        return new ProduccionProgramadaDto(anio, mes, HorarioSistema.ZonaHoraria, resultado.Sum(p => p.VolumenEstimadoM3), resultado);
    }

    private IQueryable<ProgramacionBateria> Consulta(int localidadId) =>
        db.ProgramacionesBateria.AsNoTracking().Where(p => p.Bateria.IdLocalidad == localidadId);

    private async Task<Localidad> BloquearLocalidadAsync(int localidadId, CancellationToken ct) =>
        (await db.Localidades.FromSqlInterpolated($"SELECT * FROM localidad WHERE id_localidad = {localidadId} FOR UPDATE")
            .AsNoTracking().ToListAsync(ct)).Single();

    private static ProgramacionDto Convertir(ProgramacionBateria p) => new(p.IdProgramacion, p.IdBateria,
        p.NombreBateria, Utc(p.InicioUtc), Utc(p.FinUtc), p.Estado, p.Version, p.Observaciones,
        p.Pozos.OrderBy(x => x.IdPozo).Select(x => new PozoProgramadoDto(x.IdPozo, x.Codigo, x.CaudalM3H)).ToList());

    private static DateTimeOffset? Utc(DateTime? valor) =>
        valor.HasValue ? new DateTimeOffset(DateTime.SpecifyKind(valor.Value, DateTimeKind.Utc)) : null;
}


