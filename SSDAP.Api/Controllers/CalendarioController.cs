using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSDAP.Api.Services;
using SSDAP.Shared.DTOs.Calendario;

namespace SSDAP.Api.Controllers;

[ApiController, Authorize]
[Route("api/localidades/{localidadId:int}/calendario")]
[ProducesResponseType<ProblemDetails>(400)]
[ProducesResponseType(401)]
[ProducesResponseType<ProblemDetails>(403)]
[ProducesResponseType<ProblemDetails>(404)]
[ProducesResponseType<ProblemDetails>(409)]
public sealed class CalendarioController(CalendarioService service) : ControllerBase
{
    [HttpGet, EndpointSummary("Lista turnos que se cruzan con el mes local, incluidos los cancelados. Páginas de 50.")]
    public async Task<ActionResult<List<ProgramacionDto>>> Listar(
        [Range(1, int.MaxValue)] int localidadId, [Range(1000, 9998)] int anio,
        [Range(1, 12)] int mes, CancellationToken ct, [Range(1, 1000000)] int pagina = 1) =>
        Ok(await service.ListarAsync(localidadId, anio, mes, pagina, ct));

    [HttpGet("{id:int}"), EndpointSummary("Consulta un turno y sus caudales guardados.")]
    public async Task<ActionResult<ProgramacionDto>> Obtener(
        [Range(1, int.MaxValue)] int localidadId, [Range(1, int.MaxValue)] int id, CancellationToken ct) =>
        Ok(await service.ObtenerAsync(localidadId, id, ct));

    [HttpPost, EndpointSummary("Programa una batería y guarda sus pozos y caudales actuales.")]
    [EndpointDescription("Inicio y fin con segundos y zona: 2026-11-01T08:00:00-03:00. Se admiten turnos contiguos, no superpuestos con el mismo pozo.")]
    [ProducesResponseType<ProgramacionDto>(201)]
    public async Task<ActionResult<ProgramacionDto>> Crear(
        [Range(1, int.MaxValue)] int localidadId, GuardarProgramacionDto datos, CancellationToken ct)
    {
        var turno = await service.GuardarAsync(localidadId, null, datos, ct);
        return CreatedAtAction(nameof(Obtener), new { localidadId, id = turno.IdProgramacion }, turno);
    }

    [HttpPost("lote"), EndpointSummary("Crea hasta 62 turnos en una operación: todos se guardan o ninguno.")]
    public async Task<ActionResult<List<ProgramacionDto>>> CrearLote(int localidadId, CrearTurnosDto datos, CancellationToken ct) =>
        Ok(await service.CrearLoteAsync(localidadId, datos, ct));

    [HttpPut("{id:int}"), EndpointSummary("Edita el turno y vuelve a capturar pozos y caudales actuales; requiere su versión.")]
    public async Task<ActionResult<ProgramacionDto>> Editar(
        [Range(1, int.MaxValue)] int localidadId, [Range(1, int.MaxValue)] int id,
        EditarProgramacionDto datos, CancellationToken ct) =>
        Ok(await service.GuardarAsync(localidadId, id, datos, ct));

    [HttpPost("{id:int}/cancelacion"), EndpointSummary("Cancela el turno sin borrar su detalle; requiere su versión.")]
    public async Task<ActionResult<ProgramacionDto>> Cancelar(
        [Range(1, int.MaxValue)] int localidadId, [Range(1, int.MaxValue)] int id,
        CancelarProgramacionDto datos, CancellationToken ct) =>
        Ok(await service.CancelarAsync(localidadId, id, datos.Version!.Value, ct));

    [HttpGet("produccion"), EndpointSummary("Estima horas por pozo y volumen programado del mes local; no representa producción medida.")]
    public async Task<ActionResult<ProduccionProgramadaDto>> Produccion(
        [Range(1, int.MaxValue)] int localidadId, [Range(1000, 9998)] int anio,
        [Range(1, 12)] int mes, CancellationToken ct) =>
        Ok(await service.ProduccionAsync(localidadId, anio, mes, ct));
}
