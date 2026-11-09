using System.ComponentModel.DataAnnotations;

namespace SSDAP.Shared.DTOs.Calidad;

/// <summary>Concentraciones en mg/L; conductividad en µS/cm; pH sin unidad.</summary>
public sealed record MedicionesCalidadDto : IValidatableObject
{
    public decimal? ResiduoSeco { get; init; }
    public decimal? Conductividad { get; init; }
    public decimal? Ph { get; init; }
    public decimal? Cloruro { get; init; }
    public decimal? Sulfato { get; init; }
    public decimal? AlcalinidadTotal { get; init; }
    public decimal? Carbonato { get; init; }
    public decimal? Bicarbonato { get; init; }
    public decimal? DurezaTotal { get; init; }
    public decimal? Calcio { get; init; }
    public decimal? Magnesio { get; init; }
    public decimal? Hierro { get; init; }
    public decimal? Nitrato { get; init; }
    public decimal? Nitrito { get; init; }
    public decimal? Amonio { get; init; }
    public decimal? Fluor { get; init; }
    public decimal? Arsenico { get; init; }
    public decimal? Sodio { get; init; }
    public decimal? Potasio { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        (string Nombre, decimal? Valor)[] mediciones =
        [
        (nameof(ResiduoSeco), ResiduoSeco),
        (nameof(Conductividad), Conductividad),
        (nameof(Ph), Ph),
        (nameof(Cloruro), Cloruro),
        (nameof(Sulfato), Sulfato),
        (nameof(AlcalinidadTotal), AlcalinidadTotal),
        (nameof(Carbonato), Carbonato),
        (nameof(Bicarbonato), Bicarbonato),
        (nameof(DurezaTotal), DurezaTotal),
        (nameof(Calcio), Calcio),
        (nameof(Magnesio), Magnesio),
        (nameof(Hierro), Hierro),
        (nameof(Nitrato), Nitrato),
        (nameof(Nitrito), Nitrito),
        (nameof(Amonio), Amonio),
        (nameof(Fluor), Fluor),
        (nameof(Arsenico), Arsenico),
        (nameof(Sodio), Sodio),
        (nameof(Potasio), Potasio),
        ];
        if (mediciones.All(m => m.Valor is null))
            yield return new ValidationResult("Ingresá al menos un resultado numérico.");
        foreach (var (nombre, valor) in mediciones)
        {
            if (valor is not decimal numero) continue;
            var esPh = nombre == nameof(Ph);
            var maximo = esPh ? 14m : 99999999.999999m;
            var decimales = esPh ? 2 : 6;
            if (numero < 0 || numero > maximo || decimal.Round(numero, decimales) != numero)
                yield return new ValidationResult(
                    $"{nombre}: valor entre 0 y {maximo}, hasta {decimales} decimales.", [nombre]);
        }
    }
}

public record CrearAnalisisDto : IValidatableObject
{
    [Required]
    public DateOnly? Fecha { get; init; }
    [Required]
    public MedicionesCalidadDto Mediciones { get; init; } = null!;
    [MaxLength(255)]
    public string? Observaciones { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Fecha is { Year: < 1000 })
            yield return new ValidationResult("La fecha debe ser igual o posterior a 1000-01-01.", [nameof(Fecha)]);
    }
}

public sealed record AnalisisCalidadDto(
    int IdAnalisis, int IdFuente, DateOnly Fecha, MedicionesCalidadDto Mediciones,
    string? Observaciones)
{
    public int Version { get; init; } = 1;
}

public sealed record EditarAnalisisDto : CrearAnalisisDto
{
    [Required, Range(1, int.MaxValue)] public int? Version { get; init; }
}

