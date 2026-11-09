using System.Data;
using Microsoft.EntityFrameworkCore;
using SSDAP.Api.Data;
using SSDAP.Api.Models;

namespace SSDAP.Api.Services;

public static class DatosEjemplo
{
    private const string NombreLocalidad = "Quehué";

    // Reparación puntual del alta errónea de la Web; no permite convertir fuentes por API.
    public static async Task CorregirPozo003Async(IServiceProvider services, IHostEnvironment environment, bool aplicar)
    {
        if (!environment.IsDevelopment()) throw new InvalidOperationException("Solo disponible en Development.");
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var fuente = await db.FuentesAgua.SingleAsync(f => f.Nombre == "Pozo P-003" && f.Localidad.Nombre == NombreLocalidad);
        var acueducto = await db.Acueductos.SingleOrDefaultAsync(a => a.IdFuente == fuente.IdFuente);
        var pozo = await db.Pozos.SingleOrDefaultAsync(p => p.IdFuente == fuente.IdFuente);
        Console.WriteLine($"Fuente {fuente.IdFuente}: {fuente.Nombre}, {fuente.TipoFuente}, {fuente.Estado}, {fuente.Caudal} m3/h.");
        if (fuente.TipoFuente == "Pozo" && pozo is not null && acueducto is null)
        {
            Console.WriteLine($"Ya corregido: {pozo.Codigo}, latitud {pozo.Latitud}, longitud {pozo.Longitud}.");
            return;
        }
        if (fuente.TipoFuente != "Acueducto" || acueducto is null || pozo is not null)
            throw new InvalidOperationException("Estado inesperado; no se modificaron datos.");
        if (!aplicar) return;
        db.Acueductos.Remove(acueducto);
        fuente.TipoFuente = "Pozo";
        db.Pozos.Add(new Pozo { IdFuente = fuente.IdFuente, Codigo = "P-003", Latitud = -37.1217m, Longitud = -64.5175m });
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        Console.WriteLine("P-003 corregido. Se conservaron nombre, caudal, estado e identificador de fuente.");
    }

    public static async Task AjustarAsync(IServiceProvider services, IHostEnvironment environment, bool aplicar)
    {
        if (!environment.IsDevelopment()) throw new InvalidOperationException("Solo disponible en Development.");
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var localidades = await db.Localidades.Where(l => l.Nombre == "[EJEMPLO TESIS] Quehué" || l.Nombre == NombreLocalidad).ToListAsync();
        if (localidades.Count != 1) throw new InvalidOperationException("No se identificó una única localidad de prueba Quehué.");
        var localidad = localidades.Single();
        Console.WriteLine($"Localidad {localidad.IdLocalidad}: {localidad.Nombre} -> Quehué");
        localidad.Nombre = NombreLocalidad;
        var fuentes = await db.FuentesAgua.Where(f => f.IdLocalidad == localidad.IdLocalidad).OrderBy(f => f.IdFuente).ToListAsync();
        var pozos = await db.Pozos.Where(p => p.FuenteAgua.IdLocalidad == localidad.IdLocalidad).OrderBy(p => p.IdPozo).ToListAsync();
        foreach (var fuente in fuentes)
        {
            var anterior = $"{fuente.Nombre}: {fuente.Caudal} m3/h";
            fuente.Nombre = fuente.Nombre.Replace(" (ejemplo tesis)", "", StringComparison.OrdinalIgnoreCase);
            if (fuente.TipoFuente == "Pozo" && (fuente.Nombre == "string" || fuente.Caudal == 99999999.99m))
            {
                var pozo = pozos.Single(p => p.IdFuente == fuente.IdFuente);
                if (pozo.Codigo == "string") pozo.Codigo = $"P-{pozo.IdPozo:000}";
                fuente.Nombre = $"Pozo {pozo.Codigo}";
                fuente.Caudal = 2m;
                fuente.Observaciones = null;
            }
            if (fuente.Observaciones?.StartsWith("EJEMPLO HISTÓRICO:") == true) fuente.Observaciones = null;
            Console.WriteLine($"Fuente {fuente.IdFuente}: {anterior} -> {fuente.Nombre}: {fuente.Caudal} m3/h");
        }
        foreach (var bateria in await db.Baterias.Where(b => b.IdLocalidad == localidad.IdLocalidad).ToListAsync())
        {
            bateria.Nombre = bateria.Nombre.Replace(" (ejemplo tesis)", "", StringComparison.OrdinalIgnoreCase);
            if (bateria.Descripcion?.StartsWith("EJEMPLO HISTÓRICO:") == true) bateria.Descripcion = null;
            Console.WriteLine($"Batería {bateria.IdBateria}: {bateria.Nombre}");
        }
        if (aplicar) { await db.SaveChangesAsync(); await tx.CommitAsync(); }
        Console.WriteLine(aplicar ? "Ajustes guardados. Estados, asociaciones e históricos conservados." : "Solo revisión. No se guardaron cambios.");
    }

    public static async Task CargarAsync(IServiceProvider services, IHostEnvironment environment)
    {
        if (!environment.IsDevelopment())
            throw new InvalidOperationException("Los datos de ejemplo solo se cargan en Development.");

        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if ((await db.Database.GetPendingMigrationsAsync()).Any())
            throw new InvalidOperationException("Aplicar las migraciones pendientes antes de cargar ejemplos.");

        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var localidad = await db.Localidades.SingleOrDefaultAsync(l => l.Nombre == NombreLocalidad || l.Nombre == "[EJEMPLO TESIS] Quehué");
        if (localidad is not null)
        {
            Console.WriteLine($"El ejemplo ya existe. Localidad {localidad.IdLocalidad}; no se modificaron sus datos.");
            return;
        }

        localidad = new Localidad { Nombre = NombreLocalidad, Provincia = "La Pampa", Estado = "Activa" };
        // Tesis JLP, 5.5.1, PDF 110 (impresa 102): 2 m³/h por pozo, una batería por pozo.
        // Coordenadas: calidad_ssdap.sql, registros de pozos 22 y 23. Solo datos técnicos.
        var pozo1 = CrearPozo(localidad, "09B16/01Ep", -37.1215m, -64.5166m);
        var pozo2 = CrearPozo(localidad, "09B16/02Ep", -37.1213m, -64.5184m);
        var bateria1 = CrearBateria(localidad, "Batería 1");
        var bateria2 = CrearBateria(localidad, "Batería 2");
        db.BateriasPozos.AddRange(
            new BateriaPozo { Bateria = bateria1, Pozo = pozo1 },
            new BateriaPozo { Bateria = bateria2, Pozo = pozo2 });
        db.Acueductos.Add(new Acueducto
        {
            FuenteAgua = new FuenteAgua
            {
                Localidad = localidad, Nombre = "Acueducto Río Colorado",
                TipoFuente = "Acueducto", Estado = "Activa", Caudal = 4m,
                Observaciones = null
            }
        });
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        Console.WriteLine($"Ejemplo creado. Localidad: {localidad.IdLocalidad}.");
        Console.WriteLine($"Pozos: {pozo1.IdPozo}, {pozo2.IdPozo}. Baterías: {bateria1.IdBateria}, {bateria2.IdBateria}.");
        Console.WriteLine("Consultar en Swagger usando el ID de localidad. No se crearon usuarios ni contraseñas.");
    }

    private static Pozo CrearPozo(Localidad localidad, string codigo, decimal latitud, decimal longitud) => new()
    {
        Codigo = codigo, Latitud = latitud, Longitud = longitud,
        FuenteAgua = new FuenteAgua
        {
            Localidad = localidad, Nombre = $"Pozo {codigo}",
            TipoFuente = "Pozo", Estado = "Activa", Caudal = 2m,
            Observaciones = null
        }
    };

    private static Bateria CrearBateria(Localidad localidad, string nombre) => new()
    {
        Localidad = localidad, Nombre = nombre, Estado = "Activa", Caudal = 2m,
        Descripcion = null
    };
}
