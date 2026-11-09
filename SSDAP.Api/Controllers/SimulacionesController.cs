using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSDAP.Api.Services;
using SSDAP.Shared.DTOs.Simulaciones;

namespace SSDAP.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/localidades/{localidadId:int}/simulaciones")]
[ProducesResponseType<ValidationProblemDetails>(400)]
[ProducesResponseType(401)]
[ProducesResponseType<ProblemDetails>(403)]
[ProducesResponseType<ProblemDetails>(404)]
[ProducesResponseType<ProblemDetails>(409)]
public sealed class SimulacionesController(SimulacionMezclaService service) : ControllerBase
{
    [HttpPost("mezcla")]
    [EndpointSummary("Simula una mezcla usando caudales y análisis ya registrados.")]
    [EndpointDescription("Seleccionar de 1 a 100 fuentes activas distintas y un análisis de cada una. No guarda resultados. Mezcla instantánea sin reacciones: suma(Q*C)/suma(Q), sin redondeo intermedio. pH, conductividad y parámetros con base analítica pendiente no se calculan. Un dato ausente deja ese parámetro sin resultado.")]
    public async Task<ActionResult<SimulacionMezclaDto>> Simular(
        int localidadId, SimularMezclaDto datos, CancellationToken ct) =>
        Ok(await service.SimularAsync(localidadId, datos, ct));
}
