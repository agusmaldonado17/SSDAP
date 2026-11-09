using System.Globalization;

namespace SSDAP.Api.Services;

public static class FechasApi
{
    public static DateTime LeerInstante(string texto)
    {
        if (!DateTimeOffset.TryParseExact(texto,
                ["yyyy-MM-dd'T'HH:mm:sszzz", "yyyy-MM-dd'T'HH:mm:ss'Z'"],
                CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var instante) ||
            instante.UtcDateTime.Year < 1000 || instante.UtcDateTime.Year > 9998)
            throw new ReglaNegocioException(400,
                "Usar fecha y hora con segundos y zona, por ejemplo 2026-11-01T08:00:00-03:00.");
        return instante.UtcDateTime;
    }
}
