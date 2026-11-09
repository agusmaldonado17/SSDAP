namespace SSDAP.Web.Services;

/// <summary>Separa las notas conocidas del modelo de las incidencias del resultado.
/// Los mensajes desconocidos quedan visibles para no ocultar nuevas incidencias.</summary>
public static class NotasResultado
{
    private static readonly HashSet<string> Aclaraciones = new(StringComparer.Ordinal)
    {
        "Volumen estimado: los turnos programados no acreditan ejecución real.",
        "Acueductos seleccionados: caudal promedio actual constante durante todo el mes, sin modelar interrupciones.",
        "Consulta recalculada con los datos actuales; no es un cierre histórico guardado ni una evaluación de potabilidad.",
        "Resumen estimado guardado; no acredita ejecución real ni certifica potabilidad.",
        "Mezcla mensual estimada por suma(volumen × concentración)/volumen total, sin reacciones ni tratamiento.",
        "Cada análisis elegido se supone representativo de su fuente durante todo el mes; no se interpolan muestras.",
        "Es una media ponderada por volumen, no describe variaciones horarias ni acredita calidad real o potabilidad.",
        "pH y conductividad no se promedian; alcalinidad y dureza siguen pendientes de base analítica. Cloro medido se conserva separado.",
        "Mezcla instantánea sin reacciones, precipitación ni tratamiento; caudales constantes y simultáneos.",
        "El resultado no certifica potabilidad. El cloro se consulta como medición final independiente."
    };

    public static bool EsAclaracion(string texto) => Aclaraciones.Contains(texto);
    public static string Texto(string texto) => texto;
}
