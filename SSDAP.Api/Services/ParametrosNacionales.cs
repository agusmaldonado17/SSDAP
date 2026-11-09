using System.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SSDAP.Api.Data;
using SSDAP.Api.Models;

namespace SSDAP.Api.Services;

/// <summary>Carga explícita revisada del CAA; nunca se ejecuta al iniciar el servidor.</summary>
public static class ParametrosNacionales
{
    private const string Referencia = "CAA, art. 982; Res. Conj. 33/2023. ANMAT, cap. XII (11/2025), pp. 1-3. https://www.argentina.gob.ar/sites/default/files/capitulo_xii_aguas_actualiz_2025-11.pdf";

    public static async Task CargarAsync(IServiceProvider services, IHostEnvironment environment)
    {
        if (!environment.IsDevelopment())
            throw new InvalidOperationException("Esta carga asistida solo se permite en Development.");
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if ((await db.Database.GetPendingMigrationsAsync()).Any())
            throw new InvalidOperationException("Aplicar las migraciones pendientes antes de cargar límites.");
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var existentes = await db.ParametrosLey.ToListAsync();
        var respaldo = Path.Combine(environment.ContentRootPath, "..", "docs", "respaldos");
        Directory.CreateDirectory(respaldo);
        await File.WriteAllTextAsync(Path.Combine(respaldo, $"limites-{DateTime.UtcNow:yyyyMMdd-HHmmssfff}.json"),
            JsonSerializer.Serialize(existentes, new JsonSerializerOptions { WriteIndented = true }));

        var valores = new Dictionary<string, (decimal? Min, decimal? Max)>
        {
            ["Ph"] = (6.5m, 8.5m), ["Amonio"] = (null, 0.20m),
            ["Arsenico"] = (null, 0.01m), ["Cloruro"] = (null, 350m),
            ["Hierro"] = (null, 0.30m), ["Nitrato"] = (null, 45m),
            ["Nitrito"] = (null, 0.10m), ["Sulfato"] = (null, 400m)
        };
        foreach (var definicion in CatalogoCalidad.Todos)
        {
            var codigo = definicion.Codigo;
            var activo = valores.TryGetValue(codigo, out var valor);
            var notas = codigo switch
            {
                "Arsenico" => "Límite general nacional. La excepción regional hasta 0,05 mg/L requiere admisión de la autoridad sanitaria; no se aplica automáticamente.",
                "Amonio" => "La norma identifica NH4+. Concentración expresada como ion amonio, no como nitrógeno.",
                "Hierro" => "Hierro total, expresado como Fe.",
                "Nitrato" => "Expresado como NO3-, no como nitrógeno.",
                "Nitrito" => "Expresado como NO2-, no como nitrógeno.",
                "DurezaTotal" => "CAA: máximo 400 mg/L como CaCO3. Inactivo: falta confirmar que las mediciones de SSDAP estén expresadas sobre esa base analítica.",
                "ResiduoSeco" => "CAA: sólidos disueltos totales, máximo 1500 mg/L. Inactivo: confirmar que residuo seco corresponde a SDT y el método de laboratorio antes de equipararlos.",
                "Fluor" => "Límites según temperatura zonal (art. 982). No existe un único rango nacional aplicable a todas las localidades. Inactivo hasta definir temperatura y alcance local.",
                _ when !activo => "El art. 982 del CAA no establece un límite numérico específico para este parámetro. Sin comparación automática; no equivale a ausencia de riesgo.",
                _ => "Agua potable de suministro público y uso domiciliario. Comparación con el límite general del art. 982."
            };
            var entidad = existentes.SingleOrDefault(p => p.Codigo == codigo);
            if (entidad is null)
            {
                entidad = new ParametroLey { Codigo = codigo };
                db.ParametrosLey.Add(entidad);
            }
            entidad.NombreParametro = codigo;
            entidad.Unidad = definicion.Unidad;
            entidad.ValorMinimo = valor.Min;
            // Valores condicionados se documentan, sin habilitar una comparación incorrecta.
            entidad.ValorMaximo = codigo == "DurezaTotal" ? 400m : codigo == "ResiduoSeco" ? 1500m : valor.Max;
            entidad.Activo = activo;
            entidad.Referencia = Referencia;
            entidad.Observaciones = notas;
            entidad.Version = checked(entidad.Version + 1);
        }
        await db.SaveChangesAsync();
        var guardados = await db.ParametrosLey.AsNoTracking().Where(p => p.Referencia == Referencia).ToListAsync();
        if (guardados.Count != 19 || guardados.Count(p => p.Activo) != 8)
            throw new InvalidOperationException("La verificación de la carga no coincide con los 19 parámetros revisados.");
        await tx.CommitAsync();
        Console.WriteLine("Verificados 19 parámetros: 8 límites activos y 11 referencias sin comparación automática. Respaldo local en docs/respaldos.");
    }
}
