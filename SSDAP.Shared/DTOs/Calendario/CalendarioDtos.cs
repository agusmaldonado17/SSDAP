using System.ComponentModel.DataAnnotations;

namespace SSDAP.Shared.DTOs.Calendario;

public class GuardarProgramacionDto
{
    [Required, Range(1, int.MaxValue)]
    public int? IdBateria { get; init; }

    /// <summary>ISO 8601 con segundos y desplazamiento: 2026-11-01T08:00:00-03:00.</summary>
    [Required]
    public string Inicio { get; init; } = string.Empty;

    [Required]
    public string Fin { get; init; } = string.Empty;

    [StringLength(255)]
    public string? Observaciones { get; init; }
}

public sealed class EditarProgramacionDto : GuardarProgramacionDto
{
    [Required, Range(0, int.MaxValue)]
    public int? Version { get; init; }
}

public sealed class CancelarProgramacionDto
{
    [Required, Range(0, int.MaxValue)]
    public int? Version { get; init; }
}

public sealed record PozoProgramadoDto(int IdPozo, string Codigo, decimal CaudalM3H);
public sealed record ProgramacionDto(int IdProgramacion, int IdBateria, string? NombreBateria,
    DateTimeOffset? Inicio, DateTimeOffset? Fin, string Estado, int Version, string? Observaciones,
    IReadOnlyList<PozoProgramadoDto> Pozos);
public sealed record ProduccionPozoDto(int IdPozo, decimal HorasProgramadas, decimal VolumenEstimadoM3);
public sealed record ProduccionProgramadaDto(int Anio, int Mes, string ZonaHoraria,
    decimal VolumenEstimadoM3, IReadOnlyList<ProduccionPozoDto> Pozos);
