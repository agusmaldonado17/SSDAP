using System.ComponentModel.DataAnnotations;
using SSDAP.Shared.DTOs.Calidad;

namespace SSDAP.Shared.DTOs.Simulaciones;

public sealed class SimularMezclaDto : IValidatableObject
{
    [Required, MinLength(1), MaxLength(100)]
    public List<FuenteSimulacionSeleccionDto> Fuentes { get; init; } = [];
    [Range(0, 36500)]
    public int? MaxDiasAntiguedad { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Fuentes is null) yield break;
        if (Fuentes.Any(f => f is null))
            yield return new ValidationResult("Las fuentes no pueden contener elementos nulos.", [nameof(Fuentes)]);
        else if (Fuentes.Select(f => f.IdFuente).Distinct().Count() != Fuentes.Count)
            yield return new ValidationResult("No se puede incluir una fuente más de una vez.", [nameof(Fuentes)]);
    }
}

public sealed class FuenteSimulacionSeleccionDto
{
    [Range(1, int.MaxValue)]
    public int IdFuente { get; init; }
    [Range(1, int.MaxValue)]
    public int IdAnalisis { get; init; }
}

public sealed record FuenteSimulacionDto(int IdFuente, string Nombre, string TipoFuente,
    decimal CaudalM3H, AnalisisCalidadDto Analisis);

public sealed record ParametroMezclaDto(string Codigo, string Unidad, decimal? Valor,
    string EstadoCalculo, string EstadoEvaluacion, ParametroCalidadDto? Limite,
    IReadOnlyList<int> FuentesSinMedicion);

public sealed record SimulacionMezclaDto(int IdLocalidad, DateOnly FechaReferencia,
    DateTimeOffset CalculadoEnUtc, string Modelo, decimal CaudalTotalM3H,
    IReadOnlyList<FuenteSimulacionDto> Fuentes, IReadOnlyList<ParametroMezclaDto> Parametros,
    IReadOnlyList<string> Advertencias);
