using System.ComponentModel.DataAnnotations;
using SSDAP.Shared.DTOs.Calendario;
using SSDAP.Shared.DTOs.Cloro;

namespace SSDAP.Shared.DTOs.Reportes;

public sealed class ConsultarProduccionMensualDto : IValidatableObject
{
    [Range(1000, 9998)]
    public int Anio { get; init; }
    [Range(1, 12)]
    public int Mes { get; init; }
    [Required, MaxLength(100)]
    public List<int> IdFuentesAcueducto { get; init; } = null!;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (IdFuentesAcueducto is null) yield break;
        if (IdFuentesAcueducto.Any(id => id <= 0) ||
            IdFuentesAcueducto.Distinct().Count() != IdFuentesAcueducto.Count)
            yield return new ValidationResult("Los IDs de acueductos deben ser positivos y distintos.", [nameof(IdFuentesAcueducto)]);
    }
}

public sealed record AporteAcueductoMensualDto(int IdFuente, string Nombre,
    decimal CaudalPromedioM3H, decimal HorasPeriodo, decimal VolumenEstimadoM3);

public sealed record CloroMensualDto(string TipoCloro, int CantidadMediciones,
    decimal MinimoMgL, decimal MaximoMgL, MedicionCloroDto UltimaMedicion);

public sealed record ProduccionMensualDto(int IdLocalidad, int Anio, int Mes, string ZonaHoraria,
    DateTimeOffset InicioUtc, DateTimeOffset FinUtc, decimal HorasPeriodo,
    decimal VolumenPozosM3, decimal VolumenAcueductosM3, decimal VolumenTotalEstimadoM3,
    decimal CaudalPromedioPeriodoM3H, IReadOnlyList<ProduccionPozoDto> Pozos,
    IReadOnlyList<AporteAcueductoMensualDto> Acueductos, IReadOnlyList<CloroMensualDto> Cloro,
    DateTimeOffset CalculadoEnUtc, IReadOnlyList<string> Advertencias);
