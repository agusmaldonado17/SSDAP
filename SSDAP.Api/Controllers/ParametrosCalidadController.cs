using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSDAP.Api.Services;
using SSDAP.Shared.DTOs.Calidad;

namespace SSDAP.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/parametros-calidad")]
[ProducesResponseType<ValidationProblemDetails>(400)]
[ProducesResponseType(401)]
[ProducesResponseType<ProblemDetails>(403)]
[ProducesResponseType<ProblemDetails>(404)]
[ProducesResponseType<ProblemDetails>(409)]
public sealed class ParametrosCalidadController(ParametroCalidadService service) : ControllerBase
{
    [HttpGet("catalogo")]
    [EndpointSummary("Códigos y unidades admitidos para configurar límites.")]
    public ActionResult<List<ParametroCatalogoDto>> Catalogo() =>
        Ok(CatalogoCalidad.Todos.Select(p => new ParametroCatalogoDto(p.Codigo, p.Unidad, p.BaseAnaliticaPendiente)).ToList());

    [HttpGet]
    [EndpointSummary("Lista configuraciones globales, incluidas las inactivas; páginas de 50.")]
    public async Task<ActionResult<List<ParametroCalidadDto>>> Listar(
        CancellationToken ct, [Range(1, 1000000)] int pagina = 1) =>
        Ok(await service.ListarAsync(pagina, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ParametroCalidadDto>> Obtener(int id, CancellationToken ct) =>
        Ok(await service.ObtenerAsync(id, ct));

    [HttpPost]
    [Authorize(Policy = "Administracion")]
    [EndpointSummary("Configura límites y referencia documental para un parámetro.")]
    [EndpointDescription("Unidad fijada por el catálogo. Mínimo y máximo inclusivos; al menos uno obligatorio. No se cargan límites legales automáticamente.")]
    [ProducesResponseType<ParametroCalidadDto>(201)]
    public async Task<ActionResult<ParametroCalidadDto>> Crear(GuardarParametroCalidadDto datos, CancellationToken ct)
    {
        var parametro = await service.CrearAsync(datos, ct);
        return CreatedAtAction(nameof(Obtener), new { id = parametro.IdParametroLey }, parametro);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "Administracion")]
    [EndpointSummary("Modifica o desactiva límites, verificando su versión.")]
    public async Task<ActionResult<ParametroCalidadDto>> Actualizar(
        int id, ActualizarParametroCalidadDto datos, CancellationToken ct) =>
        Ok(await service.ActualizarAsync(id, datos, ct));
}
