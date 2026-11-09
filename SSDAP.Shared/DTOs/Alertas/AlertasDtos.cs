using SSDAP.Shared.DTOs.Calidad;

namespace SSDAP.Shared.DTOs.Alertas;

public sealed record FuenteAlertasDto(int IdFuente, string Nombre, int? IdAnalisis,
    DateOnly? FechaAnalisis, int? AntiguedadDias, IReadOnlyList<string> Alertas,
    IReadOnlyList<ComparacionParametroDto> Calidad);

public sealed record BateriaAlertasDto(int IdBateria, string Nombre,
    IReadOnlyList<string> Alertas, IReadOnlyList<int> PozosConProblemas);

public sealed record PaginaAlertasDto<T>(DateOnly FechaReferencia, int? MaxDiasAntiguedad,
    int Pagina, int TotalElementos, bool HayMas, IReadOnlyList<T> Elementos);
