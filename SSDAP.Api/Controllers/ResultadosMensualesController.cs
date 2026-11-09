using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSDAP.Api.Services;
using SSDAP.Shared.DTOs.Reportes;

namespace SSDAP.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/localidades/{localidadId:int}/resultados-mensuales")]
[ProducesResponseType<ValidationProblemDetails>(400)]
[ProducesResponseType(401)]
[ProducesResponseType<ProblemDetails>(403)]
[ProducesResponseType<ProblemDetails>(404)]
[ProducesResponseType<ProblemDetails>(409)]
public sealed class ResultadosMensualesController(ResultadoMensualService service) : ControllerBase
{
    [HttpPost]
    [EndpointSummary("Guarda una revisión mensual de producción y mezcla de calidad simulada.")]
    [EndpointDescription("Cuerpo: produccion, calidad.analisis [{idFuente,idAnalisis}] y observaciones. Conserva entradas y límites usados. Si faltan análisis, guarda calidad parcial, sin inventar mediciones. Cada POST crea una revisión; no reemplaza anteriores ni cierra el período. La mezcla mensual se pondera por volumen, no es un análisis real.")]
    [ProducesResponseType<ResultadoMensualDetalleDto>(201)]
    public async Task<ActionResult<ResultadoMensualDetalleDto>> Guardar(
        int localidadId, GuardarResultadoMensualDto datos, CancellationToken ct)
    {
        var resultado = await service.GuardarAsync(localidadId, datos, ct);
        return CreatedAtAction(nameof(Obtener), new { localidadId, id = resultado.Resultado.IdResultado }, resultado);
    }

    [HttpGet]
    [EndpointSummary("Histórico de revisiones mensuales, en páginas de 50.")]
    public async Task<ActionResult<List<ResultadoMensualResumenDto>>> Listar(int localidadId, CancellationToken ct,
        [Range(1000, 9998)] int? anio = null, [Range(1, 12)] int? mes = null,
        [Range(1, 1000000)] int pagina = 1) =>
        Ok(await service.ListarAsync(localidadId, anio, mes, pagina, ct));

    [HttpGet("{id:int}")]
    [EndpointSummary("Consulta una copia guardada sin recalcular con los datos actuales.")]
    public async Task<ActionResult<ResultadoMensualDetalleDto>> Obtener(int localidadId, int id, CancellationToken ct) =>
        Ok(await service.ObtenerAsync(localidadId, id, ct));
}
