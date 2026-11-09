using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSDAP.Api.Services;
using SSDAP.Shared.DTOs.Reportes;

namespace SSDAP.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/localidades/{localidadId:int}/reportes")]
[ProducesResponseType<ValidationProblemDetails>(400)]
[ProducesResponseType(401)]
[ProducesResponseType<ProblemDetails>(403)]
[ProducesResponseType<ProblemDetails>(404)]
[ProducesResponseType<ProblemDetails>(409)]
public sealed class ReportesController(ProduccionMensualService service, CalidadMensualService calidad) : ControllerBase
{
    [HttpPost("produccion-mensual")]
    [EndpointSummary("Consulta producción estimada y mediciones de cloro de un mes.")]
    [EndpointDescription("Reutiliza caudales históricos de los turnos y caudales actuales de los acueductos seleccionados. idFuentesAcueducto es obligatorio: [] para no incluirlos. Cada acueducto aporta continuamente durante todo el mes. No guarda un cierre, no calcula calidad mensual. Cloro agrupado por tipo, sin promediar libre y total.")]
    public async Task<ActionResult<ProduccionMensualDto>> Consultar(
        int localidadId, ConsultarProduccionMensualDto datos, CancellationToken ct) =>
        Ok(await service.ConsultarAsync(localidadId, datos, ct));

    [HttpPost("calidad-mensual")]
    [EndpointSummary("Simula la mezcla mensual con volúmenes programados y análisis seleccionados por fuente.")]
    [EndpointDescription("Cuerpo: produccion y calidad.analisis [{idFuente,idAnalisis}]. Solo fuentes con volumen positivo. Análisis omitido produce cálculo incompleto. Supone concentración constante en el mes, sin reacciones; no calcula pH ni conductividad ni certifica potabilidad. No guarda resultados.")]
    public async Task<ActionResult<CalidadMensualDto>> ConsultarCalidad(int localidadId, ConsultarCalidadMensualDto datos, CancellationToken ct) =>
        Ok(await calidad.ConsultarAsync(localidadId, datos, ct));
}
