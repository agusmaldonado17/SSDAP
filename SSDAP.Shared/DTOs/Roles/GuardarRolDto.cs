using System.ComponentModel.DataAnnotations;

namespace SSDAP.Shared.DTOs.Roles;

public sealed class GuardarRolDto
{
    [Required]
    [StringLength(50)]
    public string Nombre { get; init; } = string.Empty;

    [StringLength(150)]
    public string? Descripcion { get; init; }

    [Required]
    public bool? Activo { get; init; }
}
