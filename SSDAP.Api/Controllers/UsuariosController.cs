using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSDAP.Api.Services;
using SSDAP.Shared.DTOs.Usuarios;
namespace SSDAP.Api.Controllers;
[ApiController, Route("api/usuarios"), Authorize(Policy = "Administracion")]
public sealed class UsuariosController(UsuarioService service) : ControllerBase
{
    [HttpGet, EndpointSummary("Lista usuarios en páginas de 50 registros.")]
    public async Task<ActionResult<List<UsuarioDto>>> Listar(CancellationToken ct, [Range(1, 1000000)] int pagina = 1)
        => Ok(await service.ListarAsync(pagina, ct));
    [HttpGet("{id:int}"), EndpointSummary("Consulta un usuario sin exponer su contraseña.")]
    public async Task<ActionResult<UsuarioDto>> Obtener([Range(1, int.MaxValue)] int id, CancellationToken ct)
        => Ok(await service.ObtenerAsync(id, ct));
    [HttpPost, EndpointSummary("Crea un usuario activo con contraseña.")]
    [ProducesResponseType<UsuarioDto>(201)]
    [ProducesResponseType<ProblemDetails>(409)]
    public async Task<ActionResult<UsuarioDto>> Crear(CrearUsuarioDto datos, CancellationToken ct)
    {
        var usuario = await service.CrearAsync(datos, ct);
        return CreatedAtAction(nameof(Obtener), new { id = usuario.IdUsuario }, usuario);
    }
    [HttpPut("{id:int}/localidades/{localidadId:int}"), EndpointSummary("Asigna una localidad al usuario.")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Asignar([Range(1, int.MaxValue)] int id, [Range(1, int.MaxValue)] int localidadId, CancellationToken ct)
    {
        await service.AsignarLocalidadAsync(id, localidadId, true, ct);
        return NoContent();
    }
    [HttpDelete("{id:int}/localidades/{localidadId:int}"), EndpointSummary("Retira la asignación de una localidad.")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Desasignar([Range(1, int.MaxValue)] int id, [Range(1, int.MaxValue)] int localidadId, CancellationToken ct)
    {
        await service.AsignarLocalidadAsync(id, localidadId, false, ct);
        return NoContent();
    }

    [HttpPut("{id:int}"), EndpointSummary("Edita datos, rol y estado del usuario; conserva un administrador activo.")]
    public async Task<ActionResult<UsuarioDto>> Actualizar([Range(1, int.MaxValue)] int id, ActualizarUsuarioDto datos, CancellationToken ct)
        => Ok(await service.ActualizarAsync(id, datos, ct));

    [HttpPut("{id:int}/password"), EndpointSummary("Restablece la contraseña e invalida las sesiones anteriores (administrador).")]
    public async Task<IActionResult> RestablecerPassword([Range(1, int.MaxValue)] int id, RestablecerPasswordDto datos, CancellationToken ct)
    {
        await service.CambiarPasswordAsync(id, datos.PasswordNueva, null, ct);
        return NoContent();
    }

    [HttpGet("{id:int}/localidades"), EndpointSummary("Consulta las localidades asignadas al usuario.")]
    public async Task<ActionResult<List<SSDAP.Shared.DTOs.Localidades.LocalidadDto>>> Localidades(
        [Range(1, int.MaxValue)] int id, CancellationToken ct, [Range(1, 1000000)] int pagina = 1)
        => Ok(await service.LocalidadesAsync(id, pagina, ct));
}
