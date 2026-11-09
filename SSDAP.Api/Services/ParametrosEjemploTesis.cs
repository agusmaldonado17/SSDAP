using System.Data;
using Microsoft.EntityFrameworkCore;
using SSDAP.Api.Data;
using SSDAP.Api.Models;

namespace SSDAP.Api.Services;

public static class ParametrosEjemploTesis
{
    public static async Task CargarAsync(IServiceProvider services, IHostEnvironment environment)
    {
        if (!environment.IsDevelopment())
            throw new InvalidOperationException("Los ejemplos solo se cargan en Development.");
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if ((await db.Database.GetPendingMigrationsAsync()).Any())
            throw new InvalidOperationException("Aplicar las migraciones antes de cargar los ejemplos.");
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        // Tesis JLP, tabla 3.5.4.2, PDF 68 / página impresa 60. Valores históricos de ejemplo.
        (string Codigo, decimal? Minimo, decimal? Maximo)[] ejemplos =
        [
            ("Ph", 6.5m, 8.5m), ("Amonio", null, 0.20m), ("Arsenico", null, 0.05m),
            ("Cloruro", null, 350m), ("Hierro", null, 0.30m), ("Nitrato", null, 45m),
            ("Nitrito", null, 0.10m), ("Sulfato", null, 400m)
        ];
        var existentes = await db.ParametrosLey.Select(p => p.Codigo).ToListAsync();
        var cantidad = 0;
        foreach (var (codigo, minimo, maximo) in ejemplos)
        {
            if (existentes.Contains(codigo)) continue;
            var definicion = CatalogoCalidad.Buscar(codigo);
            db.ParametrosLey.Add(new ParametroLey
            {
                Codigo = codigo, NombreParametro = codigo, Unidad = definicion.Unidad,
                ValorMinimo = minimo, ValorMaximo = maximo, Activo = true, Version = 1,
                Referencia = "EJEMPLO TESIS JLP: tabla 3.5.4.2, PDF 68 (pág. 60). Valores históricos, sin validación de vigencia.",
                Observaciones = "Datos para pruebas. NH4+, As, Cl-, Fe total, NO3-, NO2-, SO4. No certifica potabilidad."
            });
            cantidad++;
        }
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        Console.WriteLine($"Parámetros de ejemplo creados: {cantidad}. Configuraciones existentes conservadas.");
    }
}
