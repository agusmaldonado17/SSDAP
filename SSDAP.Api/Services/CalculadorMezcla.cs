using SSDAP.Shared.DTOs.Calidad;
using SSDAP.Shared.DTOs.Simulaciones;

namespace SSDAP.Api.Services;

/// <summary>Balance instantáneo sin reacciones: suma(Q*C)/suma(Q). Q en m³/h; C en mg/L.</summary>
public static class CalculadorMezcla
{
    public static List<ParametroMezclaDto> Calcular(
        IReadOnlyList<FuenteSimulacionDto> fuentes, IReadOnlyList<ParametroCalidadDto> limites)
        => CalcularPonderado(fuentes.Select(f => new AporteMezcla(f.IdFuente, f.CaudalM3H, f.Analisis.Mediciones)).ToList(), limites);

    internal sealed record AporteMezcla(int IdFuente, decimal Peso, MedicionesCalidadDto? Mediciones);

    // Todos los pesos deben usar la misma unidad: caudal instantáneo o volumen del período.
    internal static List<ParametroMezclaDto> CalcularPonderado(
        IReadOnlyList<AporteMezcla> fuentes, IReadOnlyList<ParametroCalidadDto> limites)
    {
        if (fuentes.Count == 0 || fuentes.Any(f => f.Peso <= 0) ||
            fuentes.Select(f => f.IdFuente).Distinct().Count() != fuentes.Count)
            throw new ReglaNegocioException(400, "Seleccionar fuentes distintas con caudal positivo.");
        var pesoTotal = fuentes.Sum(f => f.Peso);
        // La desactivación excluye el parámetro de la mezcla y de su evaluación.
        // Un parámetro sin configuración sigue visible como no evaluado.
        var inactivos = limites.Where(l => !l.Activo).Select(l => l.Codigo).ToHashSet();
        return CatalogoCalidad.Todos.Where(d => !inactivos.Contains(d.Codigo)).Select(definicion =>
        {
            var limite = limites.SingleOrDefault(l => l.Codigo == definicion.Codigo && l.Activo);
            var ausentes = fuentes.Where(f => f.Mediciones is null || definicion.Leer(f.Mediciones) is null)
                .Select(f => f.IdFuente).ToList();
            var estado = definicion.BaseAnaliticaPendiente ? "BaseAnaliticaPendiente"
                : definicion.Codigo is "Ph" or "Conductividad" ? "ModeloNoAplicable"
                : ausentes.Count > 0 ? "DatosIncompletos"
                : fuentes.Any(f => definicion.Leer(f.Mediciones!) < 0) ? "MedicionInvalida"
                : "Calculado";
            decimal? valor = estado == "Calculado"
                ? fuentes.Sum(f => f.Peso * definicion.Leer(f.Mediciones!)!.Value) / pesoTotal
                : null;
            var evaluacion = estado == "Calculado"
                ? EvaluadorCalidad.Estado(definicion, valor, limite) : "NoEvaluado";
            return new ParametroMezclaDto(definicion.Codigo, definicion.Unidad, valor, estado, evaluacion, limite, ausentes);
        }).ToList();
    }
}
