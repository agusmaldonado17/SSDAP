using System.ComponentModel.DataAnnotations;
using SSDAP.Shared.DTOs.Calidad;
using SSDAP.Shared.DTOs.Simulaciones;

namespace SSDAP.Shared.DTOs.Reportes;

public sealed class SeleccionCalidadMensualDto : IValidatableObject
{
    [Required, MaxLength(1000)]
    public List<FuenteSimulacionSeleccionDto> Analisis { get; init; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (Analisis is null) yield break;
        if (Analisis.Any(a => a is null) || Analisis.Select(a => a.IdFuente).Distinct().Count() != Analisis.Count)
            yield return new("Seleccionar un único análisis por fuente, sin elementos nulos.", [nameof(Analisis)]);
    }
}

public sealed class ConsultarCalidadMensualDto
{
    [Required] public ConsultarProduccionMensualDto Produccion { get; init; } = null!;
    [Required] public SeleccionCalidadMensualDto Calidad { get; init; } = new();
}

public sealed record AporteCalidadMensualDto(int IdFuente, string Nombre, decimal VolumenM3, AnalisisCalidadDto? Analisis);

public sealed record CalidadMensualDto(int IdLocalidad, int Anio, int Mes, string Modelo, string Estado,
    decimal VolumenTotalM3, IReadOnlyList<AporteCalidadMensualDto> Aportes,
    IReadOnlyList<ParametroMezclaDto> Parametros, IReadOnlyList<string> Advertencias);
