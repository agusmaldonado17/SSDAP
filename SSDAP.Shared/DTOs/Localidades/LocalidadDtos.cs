using System.ComponentModel.DataAnnotations;
namespace SSDAP.Shared.DTOs.Localidades;
public sealed record LocalidadDto(int IdLocalidad, string Nombre, string Provincia, string Estado);
public sealed class CrearLocalidadDto
{
    [Required, StringLength(100)] public string Nombre { get; init; } = string.Empty;
    [Required, StringLength(100)] public string Provincia { get; init; } = string.Empty;
}

public sealed class ActualizarLocalidadDto
{
    [Required, StringLength(100)] public string Nombre { get; init; } = string.Empty;
    [Required, StringLength(100)] public string Provincia { get; init; } = string.Empty;
    [Required, RegularExpression("^(Activa|Inactiva)$")] public string Estado { get; init; } = string.Empty;
}
