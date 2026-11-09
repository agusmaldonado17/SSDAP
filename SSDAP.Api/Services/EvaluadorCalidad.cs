using SSDAP.Shared.DTOs.Calidad;

namespace SSDAP.Api.Services;

/// <summary>Comparación inclusiva con límites configurados; no certifica potabilidad.</summary>
public static class EvaluadorCalidad
{
    public static List<ComparacionParametroDto> Comparar(
        MedicionesCalidadDto mediciones, IReadOnlyList<ParametroCalidadDto> limites)
    {
        var inactivos = limites.Where(p => !p.Activo).Select(p => p.Codigo).ToHashSet();
        return CatalogoCalidad.Todos.Where(p => !inactivos.Contains(p.Codigo)).Select(definicion =>
        {
            var valor = definicion.Leer(mediciones);
            var limite = limites.SingleOrDefault(p => p.Codigo == definicion.Codigo && p.Activo);
            var estado = Estado(definicion, valor, limite);
            return new ComparacionParametroDto(definicion.Codigo, definicion.Unidad, valor, estado, limite);
        }).ToList();
    }

    internal static string Estado(DefinicionParametro definicion, decimal? valor, ParametroCalidadDto? limite)
    {
        if (valor is null) return "NoMedido";
        if (limite is null) return "SinLimiteConfigurado";
        // Validar también datos que pudieron cargarse directamente en la base.
        if (limite.Unidad != definicion.Unidad || string.IsNullOrWhiteSpace(limite.Referencia) ||
            (limite.ValorMinimo is null && limite.ValorMaximo is null) ||
            limite.ValorMinimo > limite.ValorMaximo || limite.ValorMinimo < 0 || limite.ValorMaximo < 0 ||
            (definicion.Codigo == "Ph" && (limite.ValorMinimo > 14 || limite.ValorMaximo > 14)))
            return "LimiteInvalido";
        if (valor < limite.ValorMinimo || valor > limite.ValorMaximo) return "FueraDeRango";
        return "DentroDeRango";
    }
}
