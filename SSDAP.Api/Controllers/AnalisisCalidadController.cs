using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSDAP.Api.Services;
using SSDAP.Shared.DTOs.Calidad;

namespace SSDAP.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/localidades/{localidadId:int}/fuentes/{fuenteId:int}/analisis")]
[ProducesResponseType<ValidationProblemDetails>(400)]
[ProducesResponseType(401)]
[ProducesResponseType<ProblemDetails>(403)]
[ProducesResponseType<ProblemDetails>(404)]
public sealed class AnalisisCalidadController(AnalisisCalidadService service, EvaluacionCalidadService evaluacion) : ControllerBase
{
    [HttpGet("{id:int}/evaluacion")]
    [EndpointSummary("Compara un análisis con los límites configurados actualmente.")]
    [EndpointDescription("Devuelve rangos inclusivos, resultados no evaluados y los límites/referencias utilizados. No certifica potabilidad ni reconstruye la normativa vigente en la fecha del análisis.")]
    public async Task<ActionResult<EvaluacionCalidadDto>> Evaluar(
        int localidadId, int fuenteId, int id, CancellationToken ct) =>
        Ok(await evaluacion.EvaluarAsync(localidadId, fuenteId, id, ct));

    [HttpGet]
    [EndpointSummary("Consulta el histórico de análisis de una fuente, en páginas de 50.")]
    [EndpointDescription("Orden descendente por fecha e ID. Desde y hasta son inclusivos (AAAA-MM-DD).")]
    public async Task<ActionResult<List<AnalisisCalidadDto>>> Listar(
        int localidadId, int fuenteId, CancellationToken ct, DateOnly? desde = null,
        DateOnly? hasta = null, [Range(1, 1000000)] int pagina = 1) =>
        Ok(await service.ListarAsync(localidadId, fuenteId, desde, hasta, pagina, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AnalisisCalidadDto>> Obtener(
        int localidadId, int fuenteId, int id, CancellationToken ct) =>
        Ok(await service.ObtenerAsync(localidadId, fuenteId, id, ct));

    [HttpGet("ultimo")]
    [EndpointSummary("Devuelve el análisis con fecha más reciente; desempata por ID.")]
    [EndpointDescription("No completa campos vacíos con mediciones de otras fechas.")]
    public async Task<ActionResult<AnalisisCalidadDto>> Ultimo(
        int localidadId, int fuenteId, CancellationToken ct) =>
        Ok(await service.ObtenerAsync(localidadId, fuenteId, null, ct));

    [HttpPost]
    [EndpointSummary("Registra un análisis parcial o completo de un pozo o acueducto.")]
    [EndpointDescription("Concentraciones en mg/L, conductividad en µS/cm y pH sin unidad. Omitir o usar null para parámetros no medidos; cero es un resultado real. Hasta 6 decimales (pH: 2). RAS y cloro de mezcla no se cargan aquí. No evalúa potabilidad.")]
    [ProducesResponseType<AnalisisCalidadDto>(201)]
    public async Task<ActionResult<AnalisisCalidadDto>> Crear(
        int localidadId, int fuenteId, CrearAnalisisDto datos, CancellationToken ct)
    {
        var analisis = await service.CrearAsync(localidadId, fuenteId, datos, ct);
        return CreatedAtAction(nameof(Obtener), new { localidadId, fuenteId, id = analisis.IdAnalisis }, analisis);
    }
    [HttpPut("{id:int}")]
    [EndpointSummary("Corrige fecha y mediciones, verificando la versión. Conserva las revisiones mensuales guardadas.")]
    public async Task<ActionResult<AnalisisCalidadDto>> Editar(int localidadId, int fuenteId, int id, EditarAnalisisDto datos, CancellationToken ct) =>
        Ok(await service.EditarAsync(localidadId, fuenteId, id, datos, ct));
}
