using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSDAP.Api.Services;
using SSDAP.Shared.DTOs.Localidades;
using SSDAP.Shared.DTOs.Usuarios;
namespace SSDAP.Api.Controllers;
[ApiController, Route("api/mi-cuenta"), Authorize]
public sealed class MiCuentaController(UsuarioService usuarios, LocalidadService localidades) : ControllerBase
{
    private int UsuarioId => int.Parse(User.FindFirstValue("sub")!);
    [HttpGet, EndpointSummary("Consulta el usuario autenticado.")]
    public async Task<ActionResult<UsuarioDto>> Obtener(CancellationToken ct) => Ok(await usuarios.ObtenerAsync(UsuarioId, ct));
    [HttpGet("localidades"), EndpointSummary("Lista únicamente las localidades asignadas al usuario autenticado.")]
    public async Task<ActionResult<List<LocalidadDto>>> ListarLocalidades(CancellationToken ct, [Range(1, 1000000)] int pagina = 1)
        => Ok(await localidades.ListarAsync(UsuarioId, pagina, ct));
}
