using System.Globalization;
namespace SSDAP.Mobile.Services;

public static class Presentacion
{
    public static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-AR");
    private static readonly TimeZoneInfo Zona = TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Buenos_Aires");
    public static DateTime Ahora => TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, Zona).DateTime;
    public static string Numero(decimal? valor, string ausente = "Sin dato") => valor?.ToString("0.######", Cultura) ?? ausente;
    public static string Estado(string codigo) => codigo switch
    {
        "EvaluacionParcial" => "Evaluación parcial",
        "SinProduccion" => "Sin producción",
        "BaseAnaliticaPendiente" => "Base analítica pendiente",
        "ModeloNoAplicable" => "Modelo no aplicable",
        "DatosIncompletos" => "Datos incompletos",
        "MedicionInvalida" => "Medición inválida",
        "NoEvaluado" => "No evaluado",
        "NoMedido" => "No medido",
        "SinLimiteConfigurado" => "Sin límite configurado",
        "LimiteInvalido" => "Límite inválido",
        "FueraDeRango" => "Fuera de rango",
        "DentroDeRango" => "Dentro de rango",
        _ => codigo
    };
    public static string Parametro(string codigo) => codigo switch
    {
        "ResiduoSeco" => "Residuo seco",
        "Ph" => "pH",
        "AlcalinidadTotal" => "Alcalinidad total",
        "DurezaTotal" => "Dureza total",
        "Fluor" => "Flúor",
        "Arsenico" => "Arsénico",
        _ => codigo
    };
    public static string Alerta(string codigo) => codigo switch
    {
        "CaudalNoPositivo" => "El caudal es cero o negativo.",
        "AnalisisConFechaFutura" => "Hay análisis registrados con fecha futura.",
        "SinAnalisis" => "No hay un análisis disponible para evaluar esta fuente.",
        "AnalisisAntiguo" => "El análisis supera la antigüedad indicada.",
        "CalidadFueraDeRango" => "Hay parámetros fuera de los límites configurados.",
        "CalidadNoEvaluadaCompletamente" => "La calidad no pudo evaluarse completamente.",
        "BateriaSinPozos" => "La batería no tiene pozos asociados.",
        "PozosNoDisponibles" => "Hay pozos asociados que no están disponibles para programar.",
        _ => codigo
    };
    public static string Fecha(DateTimeOffset valor) => TimeZoneInfo.ConvertTime(valor, Zona).ToString("dd/MM/yyyy HH:mm", Cultura);
    public static string Instante(DateTime local)
    {
        local = DateTime.SpecifyKind(local, DateTimeKind.Unspecified);
        return new DateTimeOffset(local, Zona.GetUtcOffset(local)).ToString("yyyy-MM-dd'T'HH:mm:sszzz", CultureInfo.InvariantCulture);
    }
    public static DateTime Mes(string valor)
    {
        if (!DateTime.TryParseExact(valor, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var mes) || mes.Year < 1000 || mes.Year > 9998)
            throw new ErrorMovil(400, "Seleccioná un mes válido.");
        return mes;
    }
}
