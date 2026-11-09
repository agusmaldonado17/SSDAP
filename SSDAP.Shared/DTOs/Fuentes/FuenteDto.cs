namespace SSDAP.Shared.DTOs.Fuentes;

public sealed record FuenteResumenDto(
    int IdFuente, int IdLocalidad, string Nombre, string TipoFuente,
    decimal CaudalM3H, string Estado, string? Observaciones);

public sealed record PozoDetalleDto(int IdPozo, string Codigo, decimal Latitud, decimal Longitud);

public sealed record FuenteDto(FuenteResumenDto Fuente, PozoDetalleDto? Pozo, int? IdAcueducto);
