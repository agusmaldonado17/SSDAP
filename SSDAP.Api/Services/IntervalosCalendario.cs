namespace SSDAP.Api.Services;

public static class IntervalosCalendario
{
    public static (DateTime Inicio, DateTime Fin) Leer(string inicio, string fin)
    {
        var desde = FechasApi.LeerInstante(inicio);
        var hasta = FechasApi.LeerInstante(fin);
        if (hasta <= desde)
            throw new ReglaNegocioException(400, "El fin debe ser posterior al inicio.");
        return (desde, hasta);
    }

    public static (DateTime Inicio, DateTime Fin) Mes(int anio, int mes, string zona)
    {
        var timezone = TimeZoneInfo.FindSystemTimeZoneById(zona);
        var inicio = new DateTime(anio, mes, 1, 0, 0, 0, DateTimeKind.Unspecified);
        var fin = inicio.AddMonths(1);
        if (timezone.IsInvalidTime(inicio) || timezone.IsInvalidTime(fin) ||
            timezone.IsAmbiguousTime(inicio) || timezone.IsAmbiguousTime(fin))
            throw new ReglaNegocioException(400, "El límite mensual es ambiguo o inexistente en esta zona horaria.");
        return (TimeZoneInfo.ConvertTimeToUtc(inicio, timezone), TimeZoneInfo.ConvertTimeToUtc(fin, timezone));
    }

    public static decimal Horas(DateTime inicio, DateTime fin, DateTime desde, DateTime hasta)
    {
        var a = inicio > desde ? inicio : desde;
        var b = fin < hasta ? fin : hasta;
        return b <= a ? 0m : (decimal)(b.Ticks - a.Ticks) / TimeSpan.TicksPerHour;
    }
}
