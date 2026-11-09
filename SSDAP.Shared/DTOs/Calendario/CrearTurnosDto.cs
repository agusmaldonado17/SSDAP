using System.ComponentModel.DataAnnotations;
namespace SSDAP.Shared.DTOs.Calendario;

public sealed class CrearTurnosDto : IValidatableObject
{
    [Required, MinLength(1), MaxLength(62)]
    public List<GuardarProgramacionDto> Turnos { get; init; } = [];
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (Turnos?.Any(t => t is null) == true) yield return new("Los turnos no pueden ser nulos.", [nameof(Turnos)]);
    }
}
