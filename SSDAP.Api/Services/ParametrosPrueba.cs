using System.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SSDAP.Api.Data;
using SSDAP.Api.Models;

namespace SSDAP.Api.Services;

/// <summary>Configuración sintética para pruebas, aplicada únicamente por comando explícito.</summary>
public static class ParametrosPrueba
{
    private const string Referencia = "PRUEBA — sin validez normativa. Rangos sintéticos para verificar la comparación de calidad.";

    public static async Task CargarAsync(IServiceProvider services, IHostEnvironment environment)
    {
        if (!environment.IsDevelopment())
            throw new InvalidOperationException("Los límites de prueba solo se permiten en Development.");

        var rangos = new Dictionary<string, (decimal Min, decimal Max)>
        {
            ["ResiduoSeco"] = (0m, 1500m), ["Conductividad"] = (0m, 2500m),
            ["Ph"] = (6.5m, 8.5m), ["Cloruro"] = (0m, 350m),
            ["Sulfato"] = (0m, 400m), ["AlcalinidadTotal"] = (0m, 300m),
            ["Carbonato"] = (0m, 50m), ["Bicarbonato"] = (0m, 500m),
            ["DurezaTotal"] = (0m, 400m), ["Calcio"] = (0m, 150m),
            ["Magnesio"] = (0m, 80m), ["Hierro"] = (0m, 0.3m),
            ["Nitrato"] = (0m, 45m), ["Nitrito"] = (0m, 0.1m),
            ["Amonio"] = (0m, 0.2m), ["Fluor"] = (0m, 1.5m),
            ["Arsenico"] = (0m, 0.01m), ["Sodio"] = (0m, 200m),
            ["Potasio"] = (0m, 20m)
        };
        if (CatalogoCalidad.Todos.Any(d => !rangos.ContainsKey(d.Codigo)) || rangos.Count != CatalogoCalidad.Todos.Count)
            throw new InvalidOperationException("Los rangos deben cubrir exactamente el catálogo de calidad.");

        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if ((await db.Database.GetPendingMigrationsAsync()).Any())
            throw new InvalidOperationException("Aplicar las migraciones pendientes antes de cargar límites.");
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var existentes = await db.ParametrosLey.ToListAsync();
        var directorio = Path.Combine(environment.ContentRootPath, "..", "docs", "respaldos");
        Directory.CreateDirectory(directorio);
        var respaldo = Path.Combine(directorio, $"limites-antes-prueba-{DateTime.UtcNow:yyyyMMdd-HHmmssfff}.json");
        await File.WriteAllTextAsync(respaldo,
            JsonSerializer.Serialize(existentes, new JsonSerializerOptions { WriteIndented = true }));

        foreach (var definicion in CatalogoCalidad.Todos)
        {
            var rango = rangos[definicion.Codigo];
            var entidad = existentes.SingleOrDefault(p => p.Codigo == definicion.Codigo);
            if (entidad is null)
            {
                entidad = new ParametroLey { Codigo = definicion.Codigo };
                db.ParametrosLey.Add(entidad);
            }
            entidad.NombreParametro = definicion.Codigo;
            entidad.Unidad = definicion.Unidad;
            entidad.ValorMinimo = rango.Min;
            entidad.ValorMaximo = rango.Max;
            entidad.Activo = true;
            entidad.Referencia = Referencia;
            entidad.Observaciones = "Datos ficticios de prueba. No certifican potabilidad ni representan requisitos legales. El mínimo cero admite ausencia del componente.";
            entidad.Version = checked(entidad.Version + 1);
        }
        await db.SaveChangesAsync();
        var guardados = await db.ParametrosLey.AsNoTracking().ToListAsync();
        foreach (var definicion in CatalogoCalidad.Todos)
        {
            var guardado = guardados.Single(p => p.Codigo == definicion.Codigo);
            var rango = rangos[definicion.Codigo];
            if (guardado.ValorMinimo != rango.Min || guardado.ValorMaximo != rango.Max ||
                guardado.Referencia != Referencia || guardado.Unidad != definicion.Unidad ||
                !guardado.Activo)
                throw new InvalidOperationException($"No se pudo verificar el parámetro {definicion.Codigo}.");
        }
        await tx.CommitAsync();
        Console.WriteLine($"Verificados {rangos.Count} rangos de prueba activos con mínimo y máximo. Respaldo: {Path.GetFullPath(respaldo)}");
    }
}
