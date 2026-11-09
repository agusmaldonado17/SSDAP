using System.ComponentModel.DataAnnotations;

namespace SSDAP.Shared.DTOs.Baterias;

public sealed class GuardarBateriaDto
{
    [Required, StringLength(100)]
    public string Nombre { get; init; } = string.Empty;

    [StringLength(255)]
    public string? Descripcion { get; init; }

    [Required, RegularExpression("^(Activa|Inactiva|Mantenimiento)$")]
    public string Estado { get; init; } = "Activa";
}

public sealed record BateriaResumenDto(int IdBateria, int IdLocalidad, string Nombre,
    string? Descripcion, string Estado, int CantidadPozos, decimal CaudalTotalM3H,
    decimal CaudalDisponibleM3H);

public sealed record PozoBateriaDto(int IdPozo, int IdFuente, string Codigo,
    string Nombre, string Estado, decimal CaudalM3H);

public sealed record BateriaDto(BateriaResumenDto Bateria, IReadOnlyList<PozoBateriaDto> Pozos);
