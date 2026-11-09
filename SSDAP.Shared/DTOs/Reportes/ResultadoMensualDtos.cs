using System.ComponentModel.DataAnnotations;

namespace SSDAP.Shared.DTOs.Reportes;

public sealed class GuardarResultadoMensualDto
{
    [Required]
    public ConsultarProduccionMensualDto Produccion { get; init; } = null!;
    [Required]
    public SeleccionCalidadMensualDto Calidad { get; init; } = new();
    [MaxLength(255)]
    public string? Observaciones { get; init; }
}

public sealed record ResultadoMensualResumenDto(int IdResultado, int IdLocalidad,
    int Anio, int Mes, int? Revision, string EstadoRegistro, DateOnly FechaGeneracion,
    DateTimeOffset? CreadoEnUtc, int? CreadoPorId, string? Observaciones);

public sealed record ResultadoMensualAnteriorDto(decimal? CaudalTotal,
    decimal? DosisCloro, string EstadoCalidad);

public sealed record ResultadoMensualDetalleDto(ResultadoMensualResumenDto Resultado,
    int? VersionFormato, ProduccionMensualDto? Reporte, ResultadoMensualAnteriorDto? DatosAnteriores)
{
    public CalidadMensualDto? Calidad { get; init; }
}
