using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSDAP.Api.Services;
using SSDAP.Shared.DTOs.Baterias;

namespace SSDAP.Api.Controllers;

[ApiController, Authorize]
[Route("api/localidades/{localidadId:int}/baterias")]
[ProducesResponseType<ValidationProblemDetails>(400)]
[ProducesResponseType(401)]
[ProducesResponseType<ProblemDetails>(403)]
[ProducesResponseType<ProblemDetails>(404)]
[ProducesResponseType<ProblemDetails>(409)]
public sealed class BateriasController(BateriaService service) : ControllerBase
{
    [HttpGet, EndpointSummary("Lista baterías con caudales calculados a partir de sus pozos.")]
    public async Task<ActionResult<List<BateriaResumenDto>>> Listar(
        [Range(1, int.MaxValue)] int localidadId, CancellationToken ct,
        [RegularExpression("^(Activa|Inactiva|Mantenimiento)$")] string? estado = null,
        [Range(1, 1000000)] int pagina = 1) =>
        Ok(await service.ListarAsync(localidadId, estado, pagina, ct));

    [HttpGet("{id:int}"), EndpointSummary("Consulta una batería y sus pozos asociados.")]
    public async Task<ActionResult<BateriaDto>> Obtener(
        [Range(1, int.MaxValue)] int localidadId, [Range(1, int.MaxValue)] int id, CancellationToken ct) =>
        Ok(await service.ObtenerAsync(localidadId, id, ct));

    [HttpPost, EndpointSummary("Crea una batería sin pozos; luego pueden asociarse con PUT.")]
    [ProducesResponseType<BateriaDto>(201)]
    public async Task<ActionResult<BateriaDto>> Crear(
        [Range(1, int.MaxValue)] int localidadId, GuardarBateriaDto datos, CancellationToken ct)
    {
        var bateria = await service.CrearAsync(localidadId, datos, ct);
        return CreatedAtAction(nameof(Obtener), new { localidadId, id = bateria.Bateria.IdBateria }, bateria);
    }

    [HttpPut("{id:int}"), EndpointSummary("Edita la batería o cambia su estado conservando las asociaciones.")]
    public async Task<ActionResult<BateriaDto>> Actualizar(
        [Range(1, int.MaxValue)] int localidadId, [Range(1, int.MaxValue)] int id,
        GuardarBateriaDto datos, CancellationToken ct) =>
        Ok(await service.ActualizarAsync(localidadId, id, datos, ct));

    [HttpPut("{id:int}/pozos/{pozoId:int}"), EndpointSummary("Asocia un pozo de la misma localidad. Repetir no duplica la asociación.")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Asociar(
        [Range(1, int.MaxValue)] int localidadId, [Range(1, int.MaxValue)] int id,
        [Range(1, int.MaxValue)] int pozoId, CancellationToken ct)
    {
        await service.AsociarPozoAsync(localidadId, id, pozoId, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}/pozos/{pozoId:int}"), EndpointSummary("Retira un pozo de la batería sin eliminar el pozo.")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Desasociar(
        [Range(1, int.MaxValue)] int localidadId, [Range(1, int.MaxValue)] int id,
        [Range(1, int.MaxValue)] int pozoId, CancellationToken ct)
    {
        await service.DesasociarPozoAsync(localidadId, id, pozoId, ct);
        return NoContent();
    }
}
