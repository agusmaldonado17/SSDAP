using System.Data;
using Microsoft.EntityFrameworkCore;
using SSDAP.Api.Data;
using SSDAP.Shared.DTOs.Reportes;

namespace SSDAP.Api.Services;

public sealed class ProduccionMensualService(AppDbContext db, AccesoLocalidadService acceso, CalendarioService calendario)
{
    public async Task<ProduccionMensualDto> ConsultarAsync(
        int localidadId, ConsultarProduccionMensualDto datos, CancellationToken ct)
    {
        await acceso.VerificarAsync(localidadId, ct);
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, ct);
        var resultado = await ConsultarEnLecturaAsync(localidadId, datos, ct);
        await transaction.CommitAsync(ct);
        return resultado;
    }

    // El llamador verifica permisos y mantiene una transacción de lectura coherente.
    internal async Task<ProduccionMensualDto> ConsultarEnLecturaAsync(
        int localidadId, ConsultarProduccionMensualDto datos, CancellationToken ct)
    {
        var pozos = await calendario.ProduccionEnLecturaAsync(localidadId, datos.Anio, datos.Mes, ct);
        var (inicio, fin) = IntervalosCalendario.Mes(datos.Anio, datos.Mes, pozos.ZonaHoraria);
        var horas = (decimal)(fin.Ticks - inicio.Ticks) / TimeSpan.TicksPerHour;
        var ids = datos.IdFuentesAcueducto;
        var fuentes = await db.FuentesAgua.AsNoTracking()
            .Where(f => ids.Contains(f.IdFuente) && f.IdLocalidad == localidadId).OrderBy(f => f.IdFuente).ToListAsync(ct);
        if (fuentes.Count != ids.Count)
            throw new ReglaNegocioException(404, "Algún acueducto no existe en esta localidad.");
        var especializaciones = await db.Acueductos.Where(a => ids.Contains(a.IdFuente)).Select(a => a.IdFuente).ToListAsync(ct);
        var fuentesConPozo = await db.Pozos.Where(p => ids.Contains(p.IdFuente)).Select(p => p.IdFuente).ToListAsync(ct);
        if (fuentes.Any(f => f.TipoFuente != "Acueducto" || f.Estado != "Activa" || f.Caudal < 0 ||
                !especializaciones.Contains(f.IdFuente) || fuentesConPozo.Contains(f.IdFuente)))
            throw new ReglaNegocioException(409, "Seleccionar acueductos activos con tipo coherente y caudal no negativo.");
        var acueductos = fuentes.Select(f => new AporteAcueductoMensualDto(f.IdFuente, f.Nombre, f.Caudal, horas, f.Caudal * horas)).ToList();
        var mediciones = db.MedicionesCloro.AsNoTracking()
            .Where(m => m.IdLocalidad == localidadId && m.FechaHoraUtc >= inicio && m.FechaHoraUtc < fin);
        var resumen = await mediciones.GroupBy(m => m.TipoCloro).Select(g => new
        {
            Tipo = g.Key, Cantidad = g.Count(), Minimo = g.Min(m => m.ValorMgL), Maximo = g.Max(m => m.ValorMgL)
        }).ToListAsync(ct);
        var ultimas = await mediciones.Where(m => !mediciones.Any(otra => otra.TipoCloro == m.TipoCloro &&
            (otra.FechaHoraUtc > m.FechaHoraUtc || (otra.FechaHoraUtc == m.FechaHoraUtc && otra.IdMedicionCloro > m.IdMedicionCloro))))
            .ToDictionaryAsync(m => m.TipoCloro, ct);
        var cloro = resumen.OrderBy(r => r.Tipo).Select(r => new CloroMensualDto(r.Tipo, r.Cantidad, r.Minimo, r.Maximo,
            MedicionCloroService.Detalle(ultimas[r.Tipo]))).ToList();
        List<string> advertencias =
        [
            "Volumen estimado: los turnos programados no acreditan ejecución real.",
            "Acueductos seleccionados: caudal promedio actual constante durante todo el mes, sin modelar interrupciones."
        ];
        if (pozos.Pozos.Count == 0) advertencias.Add("No hay producción de pozos programada para el período.");
        if (acueductos.Count == 0) advertencias.Add("No se incluyeron acueductos en esta consulta.");
        if (acueductos.Any(a => a.CaudalPromedioM3H == 0)) advertencias.Add("Hay acueductos seleccionados con caudal cero.");
        if (cloro.Count == 0) advertencias.Add("No hay mediciones de cloro en este mes.");
        if (fin > DateTime.UtcNow) advertencias.Add("El período todavía no terminó; las mediciones registradas pueden estar incompletas.");
        advertencias.Add("Consulta recalculada con los datos actuales; no es un cierre histórico guardado ni una evaluación de potabilidad.");
        var volumenAcueducto = acueductos.Sum(a => a.VolumenEstimadoM3);
        var total = pozos.VolumenEstimadoM3 + volumenAcueducto;
        return new(localidadId, datos.Anio, datos.Mes, pozos.ZonaHoraria,
            Utc(inicio), Utc(fin), horas, pozos.VolumenEstimadoM3, volumenAcueducto, total,
            total / horas, pozos.Pozos, acueductos, cloro, DateTimeOffset.UtcNow, advertencias);
    }

    private static DateTimeOffset Utc(DateTime fecha) => new(DateTime.SpecifyKind(fecha, DateTimeKind.Utc));
}
