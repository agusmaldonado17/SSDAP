using System.ComponentModel.DataAnnotations;

namespace SSDAP.Shared.DTOs.Calidad;

public class GuardarParametroCalidadDto
{
    [Required, MaxLength(40)]
    public string Codigo { get; init; } = "";
    public decimal? ValorMinimo { get; init; }
    public decimal? ValorMaximo { get; init; }
    [Required, MaxLength(255)]
    public string Referencia { get; init; } = "";
    [Required]
    public bool? Activo { get; init; }
    [MaxLength(255)]
    public string? Observaciones { get; init; }
}

public sealed class ActualizarParametroCalidadDto : GuardarParametroCalidadDto
{
    [Required, Range(0, int.MaxValue)]
    public int? Version { get; init; }
}

public sealed record ParametroCalidadDto(int IdParametroLey, string? Codigo, string Nombre,
    string Unidad, decimal? ValorMinimo, decimal? ValorMaximo, string? Referencia,
    bool Activo, int Version, string? Observaciones);

public sealed record ParametroCatalogoDto(string Codigo, string Unidad, bool BaseAnaliticaPendiente);

public sealed record ComparacionParametroDto(string Codigo, string Unidad, decimal? Valor,
    string Estado, ParametroCalidadDto? Limite);

public sealed record EvaluacionCalidadDto(int IdAnalisis, int IdFuente, DateOnly Fecha,
    DateTimeOffset EvaluadoEnUtc, int FueraDeRango, int DentroDeRango, int NoEvaluados,
    IReadOnlyList<ComparacionParametroDto> Parametros);
