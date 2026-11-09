using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSDAP.Api.Services;
using SSDAP.Shared.DTOs.Fuentes;

namespace SSDAP.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/localidades/{localidadId:int}/fuentes")]
[ProducesResponseType<ValidationProblemDetails>(400)]
[ProducesResponseType(401)]
[ProducesResponseType<ProblemDetails>(403)]
[ProducesResponseType<ProblemDetails>(404)]
[ProducesResponseType<ProblemDetails>(409)]
public sealed class FuentesController(FuenteService service) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Lista las fuentes de una localidad, en páginas de 50 registros.")]
    public async Task<ActionResult<List<FuenteResumenDto>>> Listar(
        [Range(1, int.MaxValue)] int localidadId, CancellationToken ct,
        [RegularExpression("^(Pozo|Acueducto)$")] string? tipo = null,
        [RegularExpression("^(Activa|Inactiva|Mantenimiento)$")] string? estado = null,
        [Range(1, 1000000)] int pagina = 1) =>
        Ok(await service.ListarAsync(localidadId, tipo, estado, pagina, ct));

    [HttpGet("{id:int}")]
    [EndpointSummary("Consulta una fuente y los datos de su pozo o acueducto.")]
    public async Task<ActionResult<FuenteDto>> Obtener(
        [Range(1, int.MaxValue)] int localidadId, [Range(1, int.MaxValue)] int id, CancellationToken ct) =>
        Ok(await service.ObtenerAsync(localidadId, id, ct));

    [HttpPost("pozos")]
    [EndpointSummary("Crea un pozo con su fuente de agua.")]
    [EndpointDescription("Caudal en m³/h (hasta 2 decimales); coordenadas en grados decimales (hasta 7). Estados: Activa, Inactiva o Mantenimiento.")]
    [ProducesResponseType<FuenteDto>(201)]
    public async Task<ActionResult<FuenteDto>> CrearPozo(
        [Range(1, int.MaxValue)] int localidadId, GuardarPozoDto datos, CancellationToken ct)
    {
        var fuente = await service.CrearPozoAsync(localidadId, datos, ct);
        return CreatedAtAction(nameof(Obtener), new { localidadId, id = fuente.Fuente.IdFuente }, fuente);
    }

    [HttpPost("acueductos")]
    [EndpointSummary("Crea un acueducto con su fuente de agua.")]
    [EndpointDescription("Caudal promedio constante en m³/h (hasta 2 decimales), calculado fuera del sistema. Estados: Activa, Inactiva o Mantenimiento.")]
    [ProducesResponseType<FuenteDto>(201)]
    public async Task<ActionResult<FuenteDto>> CrearAcueducto(
        [Range(1, int.MaxValue)] int localidadId, GuardarFuenteDto datos, CancellationToken ct)
    {
        var fuente = await service.CrearAcueductoAsync(localidadId, datos, ct);
        return CreatedAtAction(nameof(Obtener), new { localidadId, id = fuente.Fuente.IdFuente }, fuente);
    }

    [HttpPut("{id:int}/pozo")]
    [EndpointSummary("Actualiza un pozo sin cambiar su tipo ni localidad.")]
    public async Task<ActionResult<FuenteDto>> ActualizarPozo(
        [Range(1, int.MaxValue)] int localidadId, [Range(1, int.MaxValue)] int id,
        GuardarPozoDto datos, CancellationToken ct) =>
        Ok(await service.ActualizarPozoAsync(localidadId, id, datos, ct));

    [HttpPut("{id:int}/acueducto")]
    [EndpointSummary("Actualiza un acueducto sin cambiar su tipo ni localidad.")]
    public async Task<ActionResult<FuenteDto>> ActualizarAcueducto(
        [Range(1, int.MaxValue)] int localidadId, [Range(1, int.MaxValue)] int id,
        GuardarFuenteDto datos, CancellationToken ct) =>
        Ok(await service.ActualizarAcueductoAsync(localidadId, id, datos, ct));

    [HttpPatch("{id:int}/estado")]
    [EndpointSummary("Activa, desactiva o marca en mantenimiento una fuente conservando sus datos.")]
    public async Task<ActionResult<FuenteDto>> CambiarEstado(
        [Range(1, int.MaxValue)] int localidadId, [Range(1, int.MaxValue)] int id,
        CambiarEstadoFuenteDto datos, CancellationToken ct) =>
        Ok(await service.CambiarEstadoAsync(localidadId, id, datos, ct));
}
