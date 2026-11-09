using System.ComponentModel.DataAnnotations;

namespace SSDAP.Shared.DTOs.Cloro;

public sealed class CrearMedicionCloroDto : IValidatableObject
{
    [Required]
    public string FechaHora { get; init; } = "";
    [Required, Range(typeof(decimal), "0", "99999999.999999",
        ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)]
    public decimal? ValorMgL { get; init; }
    [Required, RegularExpression("^(Libre|Total)$")]
    public string TipoCloro { get; init; } = "";
    [MaxLength(255)]
    public string? Observaciones { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ValorMgL is decimal valor && decimal.Round(valor, 6) != valor)
            yield return new ValidationResult("El valor admite hasta seis decimales.", [nameof(ValorMgL)]);
    }
}

public sealed record MedicionCloroDto(int IdMedicionCloro, int IdLocalidad,
    DateTimeOffset FechaHoraUtc, decimal ValorMgL, string TipoCloro,
    string? Observaciones, DateTimeOffset CreadoEnUtc, int CreadoPorId);
