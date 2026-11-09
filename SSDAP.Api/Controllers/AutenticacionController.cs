using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using SSDAP.Api.Services;
using SSDAP.Shared.DTOs.Autenticacion;
namespace SSDAP.Api.Controllers;
[ApiController, Route("api/autenticacion")]
public sealed class AutenticacionController(AutenticacionService service) : ControllerBase
{
    [AllowAnonymous, HttpPost("login", Name = "Login"), EnableRateLimiting("login")]
    [EndpointSummary("Inicia una sesión con vencimiento tras una hora sin actividad informada.")]
    [ProducesResponseType<LoginRespuestaDto>(200)]
    [ProducesResponseType<ProblemDetails>(401)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    [ProducesResponseType(429)]
    public async Task<ActionResult<LoginRespuestaDto>> Login(LoginDto datos, CancellationToken ct)
    {
        var respuesta = await service.LoginAsync(datos, ct);
        return respuesta is null ? Problem(statusCode: 401, title: "Credenciales inválidas o acceso no disponible.") : Ok(respuesta);
    }

    [Authorize, HttpPost("actividad")]
    [EndpointSummary("Registra actividad del usuario y renueva el acceso por una hora.")]
    [EndpointDescription("Enviar solo ante interacción del usuario, también al editar formularios. Reemplazar el token anterior por accessToken de la respuesta. Consultas automáticas o una pestaña abierta no deben invocar esta operación. Una sesión vencida requiere login.")]
    [ProducesResponseType<LoginRespuestaDto>(200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<LoginRespuestaDto>> Actividad(CancellationToken ct) =>
        Ok(await service.RegistrarActividadAsync(int.Parse(User.FindFirstValue("sub")!), User.FindFirstValue("sid")!, User.FindFirstValue("sello")!, ct));

    [Authorize, HttpPost("logout")]
    [EndpointSummary("Cierra únicamente la sesión actual en el servidor.")]
    [EndpointDescription("Sin cuerpo. Identifica la sesión mediante el token. Tras responder 204, sus tokens dejan de permitir acceso o renovación; otras sesiones permanecen abiertas. El cliente debe eliminar el token local. Una sesión ya vencida o cerrada responde 401.")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        await service.CerrarSesionAsync(int.Parse(User.FindFirstValue("sub")!), User.FindFirstValue("sid")!, ct);
        return NoContent();
    }
}
