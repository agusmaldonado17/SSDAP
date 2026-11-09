using SSDAP.Shared.DTOs.Calidad;

namespace SSDAP.Api.Services;

public sealed record DefinicionParametro(string Codigo, string Unidad, bool BaseAnaliticaPendiente,
    Func<MedicionesCalidadDto, decimal?> Leer);

public static class CatalogoCalidad
{
    public static IReadOnlyList<DefinicionParametro> Todos { get; } = Array.AsReadOnly<DefinicionParametro>(
    [
        new("ResiduoSeco", "mg/L", false, m => m.ResiduoSeco),
        new("Conductividad", "µS/cm", false, m => m.Conductividad),
        new("Ph", "sin unidad", false, m => m.Ph),
        new("Cloruro", "mg/L", false, m => m.Cloruro),
        new("Sulfato", "mg/L", false, m => m.Sulfato),
        new("AlcalinidadTotal", "mg/L", true, m => m.AlcalinidadTotal),
        new("Carbonato", "mg/L", false, m => m.Carbonato),
        new("Bicarbonato", "mg/L", false, m => m.Bicarbonato),
        new("DurezaTotal", "mg/L", true, m => m.DurezaTotal),
        new("Calcio", "mg/L", false, m => m.Calcio),
        new("Magnesio", "mg/L", false, m => m.Magnesio),
        new("Hierro", "mg/L", false, m => m.Hierro),
        new("Nitrato", "mg/L", false, m => m.Nitrato),
        new("Nitrito", "mg/L", false, m => m.Nitrito),
        new("Amonio", "mg/L", false, m => m.Amonio),
        new("Fluor", "mg/L", false, m => m.Fluor),
        new("Arsenico", "mg/L", false, m => m.Arsenico),
        new("Sodio", "mg/L", false, m => m.Sodio),
        new("Potasio", "mg/L", false, m => m.Potasio),
    ]);

    public static DefinicionParametro Buscar(string codigo) =>
        Todos.FirstOrDefault(p => p.Codigo == codigo)
        ?? throw new ReglaNegocioException(400, "Código de parámetro desconocido. Consultá el catálogo.");
}

