using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSDAP.Api.Services;
using SSDAP.Shared.DTOs.Localidades;
namespace SSDAP.Api.Controllers;
[ApiController, Route("api/localidades"), Authorize(Policy = "Administracion")]
public sealed class LocalidadesController(LocalidadService service) : ControllerBase
{
    [HttpGet, EndpointSummary("Lista localidades en páginas de 50 registros.")]
    public async Task<ActionResult<List<LocalidadDto>>> Listar(CancellationToken ct, [Range(1, 1000000)] int pagina = 1)
        => Ok(await service.ListarAsync(null, pagina, ct));
    [HttpGet("{id:int}")]
    public async Task<ActionResult<LocalidadDto>> Obtener([Range(1, int.MaxValue)] int id, CancellationToken ct)
        => Ok(await service.ObtenerAsync(id, ct));
    [HttpPost, EndpointSummary("Crea una localidad activa.")]
    [ProducesResponseType<LocalidadDto>(201)]
    public async Task<ActionResult<LocalidadDto>> Crear(CrearLocalidadDto datos, CancellationToken ct)
    {
        var localidad = await service.CrearAsync(datos, ct);
        return CreatedAtAction(nameof(Obtener), new { id = localidad.IdLocalidad }, localidad);
    }
}
