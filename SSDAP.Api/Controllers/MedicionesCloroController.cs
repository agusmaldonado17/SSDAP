using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSDAP.Api.Services;
using SSDAP.Shared.DTOs.Cloro;

namespace SSDAP.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/localidades/{localidadId:int}/mediciones-cloro")]
[ProducesResponseType<ValidationProblemDetails>(400)]
[ProducesResponseType(401)]
[ProducesResponseType<ProblemDetails>(403)]
[ProducesResponseType<ProblemDetails>(404)]
public sealed class MedicionesCloroController(MedicionCloroService service) : ControllerBase
{
    [HttpPost]
    [EndpointSummary("Registra cloro medido en la mezcla final, en mg/L.")]
    [EndpointDescription("Fecha y hora con segundos y zona: 2026-01-01T08:00:00-03:00. Tipos: Libre o Total. Cero es válido; valor obligatorio, hasta seis decimales. Es una medición, no una dosis aplicada ni consumo de producto.")]
    [ProducesResponseType<MedicionCloroDto>(201)]
    public async Task<ActionResult<MedicionCloroDto>> Crear(
        int localidadId, CrearMedicionCloroDto datos, CancellationToken ct)
    {
        var medicion = await service.CrearAsync(localidadId, datos, ct);
        return CreatedAtAction(nameof(Obtener), new { localidadId, id = medicion.IdMedicionCloro }, medicion);
    }

    [HttpGet]
    [EndpointSummary("Histórico de cloro en páginas de 50, ordenado por fecha e ID descendentes.")]
    [EndpointDescription("Desde inclusive y hasta exclusivo, ambos con zona explícita. Tipo opcional. No promedia mediciones.")]
    public async Task<ActionResult<List<MedicionCloroDto>>> Listar(
        int localidadId, CancellationToken ct, string? desde = null, string? hasta = null,
        [RegularExpression("^(Libre|Total|SinEspecificar)$")] string? tipoCloro = null,
        [Range(1, 1000000)] int pagina = 1) =>
        Ok(await service.ListarAsync(localidadId, desde, hasta, tipoCloro, pagina, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MedicionCloroDto>> Obtener(int localidadId, int id, CancellationToken ct) =>
        Ok(await service.ObtenerAsync(localidadId, id, null, ct));

    [HttpGet("ultima")]
    [EndpointSummary("Última medición por fecha, opcionalmente filtrada por tipo de cloro.")]
    public async Task<ActionResult<MedicionCloroDto>> Ultima(
        int localidadId, CancellationToken ct,
        [RegularExpression("^(Libre|Total|SinEspecificar)$")] string? tipoCloro = null) =>
        Ok(await service.ObtenerAsync(localidadId, null, tipoCloro, ct));
}
