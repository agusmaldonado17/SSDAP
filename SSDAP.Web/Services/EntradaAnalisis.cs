using System.ComponentModel.DataAnnotations;
using SSDAP.Shared.DTOs.Calidad;
namespace SSDAP.Web.Services;

public sealed class EntradaAnalisis : IValidatableObject
{
    [Required(ErrorMessage = "Ingresá la fecha del análisis.")]
    public DateOnly? Fecha { get; set; }
    [StringLength(255)] public string? Observaciones { get; set; }
    public decimal? ResiduoSeco { get; set; }
    public decimal? Conductividad { get; set; }
    public decimal? Ph { get; set; }
    public decimal? Cloruro { get; set; }
    public decimal? Sulfato { get; set; }
    public decimal? AlcalinidadTotal { get; set; }
    public decimal? Carbonato { get; set; }
    public decimal? Bicarbonato { get; set; }
    public decimal? DurezaTotal { get; set; }
    public decimal? Calcio { get; set; }
    public decimal? Magnesio { get; set; }
    public decimal? Hierro { get; set; }
    public decimal? Nitrato { get; set; }
    public decimal? Nitrito { get; set; }
    public decimal? Amonio { get; set; }
    public decimal? Fluor { get; set; }
    public decimal? Arsenico { get; set; }
    public decimal? Sodio { get; set; }
    public decimal? Potasio { get; set; }
    public MedicionesCalidadDto Mediciones() => new()
    {
        ResiduoSeco = ResiduoSeco,
        Conductividad = Conductividad,
        Ph = Ph,
        Cloruro = Cloruro,
        Sulfato = Sulfato,
        AlcalinidadTotal = AlcalinidadTotal,
        Carbonato = Carbonato,
        Bicarbonato = Bicarbonato,
        DurezaTotal = DurezaTotal,
        Calcio = Calcio,
        Magnesio = Magnesio,
        Hierro = Hierro,
        Nitrato = Nitrato,
        Nitrito = Nitrito,
        Amonio = Amonio,
        Fluor = Fluor,
        Arsenico = Arsenico,
        Sodio = Sodio,
        Potasio = Potasio,
    };
    public CrearAnalisisDto Contrato() => new() { Fecha = Fecha, Observaciones = Observaciones, Mediciones = Mediciones() };
    public static EntradaAnalisis Desde(AnalisisCalidadDto datos) => new()
    {
        Fecha = datos.Fecha, Observaciones = datos.Observaciones,
        ResiduoSeco = datos.Mediciones.ResiduoSeco,
        Conductividad = datos.Mediciones.Conductividad,
        Ph = datos.Mediciones.Ph,
        Cloruro = datos.Mediciones.Cloruro,
        Sulfato = datos.Mediciones.Sulfato,
        AlcalinidadTotal = datos.Mediciones.AlcalinidadTotal,
        Carbonato = datos.Mediciones.Carbonato,
        Bicarbonato = datos.Mediciones.Bicarbonato,
        DurezaTotal = datos.Mediciones.DurezaTotal,
        Calcio = datos.Mediciones.Calcio,
        Magnesio = datos.Mediciones.Magnesio,
        Hierro = datos.Mediciones.Hierro,
        Nitrato = datos.Mediciones.Nitrato,
        Nitrito = datos.Mediciones.Nitrito,
        Amonio = datos.Mediciones.Amonio,
        Fluor = datos.Mediciones.Fluor,
        Arsenico = datos.Mediciones.Arsenico,
        Sodio = datos.Mediciones.Sodio,
        Potasio = datos.Mediciones.Potasio,
    };
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (Fecha is { Year: < 1000 }) yield return new("La fecha debe ser igual o posterior a 1000-01-01.");
        foreach (var error in Mediciones().Validate(context)) yield return error;
    }
    public static (string Nombre, string Unidad, decimal? Valor)[] Filas(MedicionesCalidadDto datos) =>
    [
        ("Residuo seco", "mg/L", datos.ResiduoSeco),
        ("Conductividad", "µS/cm", datos.Conductividad),
        ("pH", "sin unidad", datos.Ph),
        ("Cloruro", "mg/L", datos.Cloruro),
        ("Sulfato", "mg/L", datos.Sulfato),
        ("Alcalinidad total", "mg/L", datos.AlcalinidadTotal),
        ("Carbonato", "mg/L", datos.Carbonato),
        ("Bicarbonato", "mg/L", datos.Bicarbonato),
        ("Dureza total", "mg/L", datos.DurezaTotal),
        ("Calcio", "mg/L", datos.Calcio),
        ("Magnesio", "mg/L", datos.Magnesio),
        ("Hierro", "mg/L", datos.Hierro),
        ("Nitrato", "mg/L", datos.Nitrato),
        ("Nitrito", "mg/L", datos.Nitrito),
        ("Amonio", "mg/L", datos.Amonio),
        ("Flúor", "mg/L", datos.Fluor),
        ("Arsénico", "mg/L", datos.Arsenico),
        ("Sodio", "mg/L", datos.Sodio),
        ("Potasio", "mg/L", datos.Potasio),
    ];
}
