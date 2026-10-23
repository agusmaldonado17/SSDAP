using Microsoft.AspNetCore.Authorization;
using System.Data;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using SSDAP.Api.Services;
using SSDAP.Shared.DTOs.Roles;

namespace SSDAP.Api.Controllers;

[Authorize(Policy = "Administracion")]
[ApiController]
[Route("api/roles")]
public sealed class RolesController(RolService rolService) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Lista los roles registrados.")]
    [EndpointDescription("El filtro activo es opcional. Sin filtro devuelve todos los roles; si no hay coincidencias devuelve una lista vacía.")]
    [ProducesResponseType<List<RolDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<RolDto>>> Listar(
        [FromQuery] bool? activo, CancellationToken cancellationToken)
    {
        return Ok(await rolService.ListarAsync(activo, cancellationToken));
    }

    [HttpGet("{id:int}")]
    [EndpointSummary("Consulta un rol por su ID.")]
    [ProducesResponseType<RolDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RolDto>> Obtener(
        [Range(1, int.MaxValue)] int id, CancellationToken cancellationToken)
    {
        var rol = await rolService.ObtenerAsync(id, cancellationToken);
        return rol is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Rol no encontrado.")
            : Ok(rol);
    }
    [HttpPost]
    [EndpointSummary("Crea un rol.")]
    [ProducesResponseType<RolDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RolDto>> Crear(GuardarRolDto datos, CancellationToken cancellationToken)
    {
        try
        {
            var rol = await rolService.CrearAsync(datos, cancellationToken);
            return CreatedAtAction(nameof(Obtener), new { id = rol.IdRol }, rol);
        }
        catch (DuplicateNameException)
        {
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Ya existe un rol con ese nombre.");
        }
    }

    [HttpPut("{id:int}")]
    [EndpointSummary("Edita un rol, incluyendo su estado activo.")]
    [EndpointDescription("Permite desactivar el rol sin eliminarlo ni borrar sus asociaciones con usuarios.")]
    [ProducesResponseType<RolDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RolDto>> Actualizar(
        [Range(1, int.MaxValue)] int id, GuardarRolDto datos, CancellationToken cancellationToken)
    {
        try
        {
            var rol = await rolService.ActualizarAsync(id, datos, cancellationToken);
            return rol is null
                ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Rol no encontrado.")
                : Ok(rol);
        }
        catch (DuplicateNameException)
        {
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Ya existe un rol con ese nombre.");
        }
    }
}
