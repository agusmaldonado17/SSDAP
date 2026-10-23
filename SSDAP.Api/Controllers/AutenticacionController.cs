using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SSDAP.Api.Services;
using SSDAP.Shared.DTOs.Autenticacion;
namespace SSDAP.Api.Controllers;
[ApiController, Route("api/autenticacion")]
public sealed class AutenticacionController(AutenticacionService service) : ControllerBase
{
    [AllowAnonymous, HttpPost("login", Name = "Login"), EnableRateLimiting("login")]
    [EndpointSummary("Inicia sesión y obtiene un token de acceso por 30 minutos.")]
    [ProducesResponseType<LoginRespuestaDto>(200)]
    [ProducesResponseType<ProblemDetails>(401)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    [ProducesResponseType(429)]
    public async Task<ActionResult<LoginRespuestaDto>> Login(LoginDto datos, CancellationToken ct)
    {
        var respuesta = await service.LoginAsync(datos, ct);
        return respuesta is null ? Problem(statusCode: 401, title: "Credenciales inválidas o acceso no disponible.") : Ok(respuesta);
    }
}
