using System.ComponentModel.DataAnnotations;

namespace SSDAP.Shared.DTOs.Fuentes;

public class GuardarFuenteDto
{
    [Required, StringLength(100)]
    public string Nombre { get; init; } = string.Empty;

    /// <summary>Caudal en m³/h. Para acueductos es el promedio constante informado.</summary>
    [Required, Range(typeof(decimal), "0", "99999999.99", ParseLimitsInInvariantCulture = true)]
    public decimal? CaudalM3H { get; init; }

    [Required, RegularExpression("^(Activa|Inactiva|Mantenimiento)$")]
    public string Estado { get; init; } = "Activa";

    [StringLength(255)]
    public string? Observaciones { get; init; }
}

public sealed class GuardarPozoDto : GuardarFuenteDto
{
    [Required, StringLength(50)]
    public string Codigo { get; init; } = string.Empty;

    [Required, Range(typeof(decimal), "-90", "90", ParseLimitsInInvariantCulture = true)]
    public decimal? Latitud { get; init; }

    [Required, Range(typeof(decimal), "-180", "180", ParseLimitsInInvariantCulture = true)]
    public decimal? Longitud { get; init; }
}

public sealed class CambiarEstadoFuenteDto
{
    [Required, RegularExpression("^(Activa|Inactiva|Mantenimiento)$")]
    public string Estado { get; init; } = string.Empty;
}
