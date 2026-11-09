using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSDAP.Api.Services;
using SSDAP.Shared.DTOs.Alertas;

namespace SSDAP.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/localidades/{localidadId:int}/alertas")]
[ProducesResponseType<ValidationProblemDetails>(400)]
[ProducesResponseType(401)]
[ProducesResponseType<ProblemDetails>(403)]
[ProducesResponseType<ProblemDetails>(404)]
public sealed class AlertasController(AlertasService service) : ControllerBase
{
    [HttpGet("fuentes")]
    [EndpointSummary("Revisa fuentes activas: caudal, análisis y límites configurados.")]
    [EndpointDescription("Páginas de 50 fuentes, incluidas las que no tengan alertas. Último análisis no futuro por fecha e ID. Antigüedad en días civiles de la localidad: se alerta si supera maxDiasAntiguedad; al omitirlo no se establece vencimiento. La comparación usa límites actuales y no certifica potabilidad.")]
    public async Task<ActionResult<PaginaAlertasDto<FuenteAlertasDto>>> Fuentes(
        int localidadId, CancellationToken ct, [Range(0, 36500)] int? maxDiasAntiguedad = null,
        [Range(1, 1000000)] int pagina = 1) =>
        Ok(await service.FuentesAsync(localidadId, maxDiasAntiguedad, pagina, ct));

    [HttpGet("baterias")]
    [EndpointSummary("Revisa baterías activas: ausencia de pozos o pozos no disponibles.")]
    [EndpointDescription("Páginas de 50 baterías, incluidas las que no tengan alertas. Identifica pozos inactivos, con caudal no positivo o de otra localidad.")]
    public async Task<ActionResult<PaginaAlertasDto<BateriaAlertasDto>>> Baterias(
        int localidadId, CancellationToken ct, [Range(1, 1000000)] int pagina = 1) =>
        Ok(await service.BateriasAsync(localidadId, pagina, ct));
}
