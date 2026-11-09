using System.Data;
using System.Security.Claims;
using System.Text.Json;
using SSDAP.Api.Models;
using SSDAP.Api.Security;
using SSDAP.Shared.DTOs.Reportes;
using SSDAP.Shared.DTOs.Simulaciones;
using Microsoft.EntityFrameworkCore;
using SSDAP.Api.Data;

namespace SSDAP.Api.Services;

public static class DatosPresentacion
{
    private const string Marca = "Presentación agosto-octubre 2026 (datos ficticios)";

    public static async Task CargarAsync(IServiceProvider services, IHostEnvironment environment)
    {
        if (!environment.IsDevelopment()) throw new InvalidOperationException("Solo disponible en Development.");
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if ((await db.Database.GetPendingMigrationsAsync()).Any()) throw new InvalidOperationException("Hay migraciones pendientes.");
        var localidad = await db.Localidades.SingleAsync(l => l.Nombre == "Prueba Quehue 2016");
        var autor = await db.Usuarios.Where(u => u.Activo && u.Rol.Nombre == JwtService.Administrador).OrderBy(u => u.IdUsuario).Select(u => u.IdUsuario).FirstAsync();
        var contexto = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        var anterior = contexto.HttpContext;
        contexto.HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("sub", autor.ToString()), new Claim(ClaimTypes.Role, JwtService.Administrador)], "CargaLocal")) };
        try
        {
            await using (var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable))
            {
                await db.Localidades.FromSqlInterpolated($"SELECT * FROM localidad WHERE id_localidad = {localidad.IdLocalidad} FOR UPDATE").AsNoTracking().ToListAsync();
                var anteriores = await db.AnalisisCalidad.AsNoTracking().ToListAsync();
                var carpeta = Path.Combine(environment.ContentRootPath, "..", "docs", "respaldos");
                Directory.CreateDirectory(carpeta);
                var archivo = Path.Combine(carpeta, $"analisis-antes-presentacion-{DateTime.UtcNow:yyyyMMdd-HHmmssfff}.json");
                await File.WriteAllTextAsync(archivo, JsonSerializer.Serialize(anteriores, new JsonSerializerOptions { WriteIndented = true }));
                Console.WriteLine($"Respaldo de análisis: {Path.GetFullPath(archivo)}");
                var pozos = await db.Pozos.Include(p => p.FuenteAgua).Where(p => p.FuenteAgua.IdLocalidad == localidad.IdLocalidad).ToListAsync();
                foreach (var (codigo, caudal, latitud, longitud) in new[]
                {
                    ("PI-03", 2.5m, -37.1186m, -64.5201m),
                    ("PI-04", 3m, -37.1240m, -64.5130m)
                })
                {
                    if (pozos.Any(p => p.Codigo == codigo)) continue;
                    var p = new Pozo { Codigo = codigo, Latitud = latitud, Longitud = longitud,
                        FuenteAgua = new FuenteAgua { IdLocalidad = localidad.IdLocalidad, Nombre = $"Pozo {codigo}",
                            TipoFuente = "Pozo", Estado = "Activa", Caudal = caudal, Observaciones = Marca } };
                    db.Pozos.Add(p); pozos.Add(p);
                }
                await db.SaveChangesAsync();
                var grupos = new[] { new[] { "PI-01", "PI-03" }, new[] { "PI-02", "PI-04" } };
                for (var i = 0; i < grupos.Length; i++)
                {
                    var nombre = i == 0 ? "Batería Norte" : "Batería Sur";
                    var integrantes = grupos[i].Select(c => pozos.Single(p => p.Codigo == c)).ToList();
                    if (integrantes.Any(p => p.FuenteAgua.Estado != "Activa" || p.FuenteAgua.Caudal <= 0))
                        throw new InvalidOperationException("Los pozos del ejemplo deben estar activos y tener caudal positivo.");
                    var bateria = await db.Baterias.SingleOrDefaultAsync(b => b.IdLocalidad == localidad.IdLocalidad && b.Nombre == nombre);
                    if (bateria is null)
                    {
                        bateria = new Bateria { IdLocalidad = localidad.IdLocalidad, Nombre = nombre, Estado = "Activa", Caudal = integrantes.Sum(p => p.FuenteAgua.Caudal), Descripcion = Marca };
                        db.Baterias.Add(bateria); await db.SaveChangesAsync();
                    }
                    else if (bateria.Descripcion != Marca) throw new InvalidOperationException("Existe una batería ajena al ejemplo con el mismo nombre.");
                    foreach (var pozo in integrantes)
                        if (!await db.BateriasPozos.AnyAsync(x => x.IdBateria == bateria.IdBateria && x.IdPozo == pozo.IdPozo))
                            db.BateriasPozos.Add(new BateriaPozo { IdBateria = bateria.IdBateria, IdPozo = pozo.IdPozo });
                    await db.SaveChangesAsync();
                    foreach (var mes in new[] { 8, 9, 10 })
                    foreach (var dia in new[] { 1, 5, 10, 15, 20, 25 })
                    {
                        var hora = i == 0 ? 6 : 14;
                        var inicio = new DateTime(2026,mes,dia,hora+3,0,0,DateTimeKind.Utc);
                        var fin = inicio.AddHours(mes == 8 ? 6 : mes == 9 ? 8 : 7);
                        if (await db.ProgramacionesBateria.AnyAsync(p => p.IdBateria == bateria.IdBateria && p.InicioUtc == inicio && p.Observaciones == Marca)) continue;
                        var ids = integrantes.Select(p => p.IdPozo).ToArray();
                        if (await db.ProgramacionesPozos.AnyAsync(p => ids.Contains(p.IdPozo) && p.Programacion.Estado == "Programada" && p.Programacion.InicioUtc < fin && p.Programacion.FinUtc > inicio))
                            throw new InvalidOperationException("Los turnos de ejemplo se superponen con la programación existente.");
                        db.ProgramacionesBateria.Add(new ProgramacionBateria { IdBateria = bateria.IdBateria, NombreBateria = nombre,
                            Fecha = new(2026,mes,dia), HoraInicio = new(hora,0), HoraFin = new(hora+(mes==8?6:mes==9?8:7),0),
                            InicioUtc=inicio, FinUtc=fin, Estado="Programada", Version=1, CreadoEnUtc=DateTime.UtcNow,
                            ModificadoEnUtc=DateTime.UtcNow, CreadoPorId=autor, ModificadoPorId=autor, Observaciones=Marca,
                            Pozos=integrantes.Select(p => new ProgramacionPozo { IdPozo=p.IdPozo,Codigo=p.Codigo,CaudalM3H=p.FuenteAgua.Caudal }).ToList() });
                        await db.SaveChangesAsync();
                    }
                }
                var fuentes = await db.FuentesAgua.Where(f => f.IdLocalidad == localidad.IdLocalidad).OrderBy(f => f.IdFuente).ToListAsync();
                foreach (var fuente in fuentes)
                foreach (var mes in new[] { 8,9,10 })
                {
                    var fecha = new DateOnly(2026,mes,1);
                    if (await db.AnalisisCalidad.AnyAsync(a => a.IdFuente == fuente.IdFuente && a.Fecha == fecha)) continue;
                    var muestra = new AnalisisCalidad { IdFuente=fuente.IdFuente,Fecha=fecha,Observaciones=Marca };
                    Completar(muestra, fuente.TipoFuente == "Acueducto", mes, fuente.IdFuente);
                    db.AnalisisCalidad.Add(muestra);
                }
                await db.SaveChangesAsync();
                // El usuario pidió completar TODOS los análisis; solo se rellenan campos ausentes.
                foreach (var a in await db.AnalisisCalidad.ToListAsync())
                {
                    var faltantes = CatalogoCalidad.Todos.Any(d => d.Leer(AnalisisCalidadService.Detalle(a).Mediciones) is null);
                    if (!faltantes) continue;
                    var agua = await db.FuentesAgua.Where(f => f.IdFuente == a.IdFuente).Select(f => f.TipoFuente == "Acueducto").SingleAsync();
                    Completar(a, agua, a.Fecha.Month, a.IdFuente);
                    a.Version = checked(a.Version + 1); a.ModificadoEnUtc=DateTime.UtcNow; a.ModificadoPorId=autor;
                    a.Observaciones = string.IsNullOrWhiteSpace(a.Observaciones) ? "Campos faltantes completados con datos ficticios para presentación." :
                        a.Observaciones[..Math.Min(a.Observaciones.Length,175)] + " | Faltantes completados con datos ficticios.";
                }
                foreach (var mes in new[] { 8,9,10 })
                foreach (var dia in new[] { 10,25 })
                {
                    var fecha = new DateTime(2026,mes,dia,13,0,0,DateTimeKind.Utc);
                    if (!await db.MedicionesCloro.AnyAsync(m => m.IdLocalidad == localidad.IdLocalidad && m.FechaHoraUtc == fecha && m.TipoCloro == "Libre"))
                        db.MedicionesCloro.Add(new MedicionCloro { IdLocalidad=localidad.IdLocalidad, FechaHoraUtc=fecha,
                            TipoCloro="Libre",ValorMgL=mes==9?0.25m:dia==10?0.4m:0.35m,Observaciones=null,CreadoPorId=autor,CreadoEnUtc=DateTime.UtcNow });
                }
                await db.SaveChangesAsync();
                var todos = await db.AnalisisCalidad.ToListAsync();
                if (todos.Any(a => CatalogoCalidad.Todos.Any(d => d.Leer(AnalisisCalidadService.Detalle(a).Mediciones) is null)))
                    throw new InvalidOperationException("Quedaron parámetros sin completar.");
                await tx.CommitAsync();
                Console.WriteLine($"Verificados {todos.Count} análisis completos (19/19).");
            }
            var acueductos = await db.FuentesAgua.Where(f => f.IdLocalidad == localidad.IdLocalidad && f.TipoFuente == "Acueducto" && f.Estado == "Activa").Select(f => f.IdFuente).ToListAsync();
            foreach (var mes in new[] { 8,9,10 })
            {
                if (await db.ResultadosMensuales.AnyAsync(r => r.IdLocalidad == localidad.IdLocalidad && r.Anio==2026 && r.Mes==mes && r.Observaciones==Marca)) continue;
                var seleccion = await db.AnalisisCalidad.Where(a => a.FuenteAgua.IdLocalidad == localidad.IdLocalidad && a.Fecha == new DateOnly(2026,mes,1))
                    .Select(a => new FuenteSimulacionSeleccionDto { IdFuente=a.IdFuente,IdAnalisis=a.IdAnalisis }).ToListAsync();
                var resultado = await scope.ServiceProvider.GetRequiredService<ResultadoMensualService>().GuardarAsync(localidad.IdLocalidad,
                    new GuardarResultadoMensualDto { Produccion = new() { Anio=2026,Mes=mes,IdFuentesAcueducto=acueductos },
                        Calidad=new() { Analisis=seleccion },Observaciones=Marca }, CancellationToken.None);
                Console.WriteLine($"Revisión {mes}/2026: {resultado.Reporte!.VolumenTotalEstimadoM3} m³; {resultado.Calidad!.Estado}.");
            }
            var resumenes = await db.ResultadosMensuales.AsNoTracking().Where(r => r.IdLocalidad == localidad.IdLocalidad && r.Observaciones == Marca).ToListAsync();
            var turnos = await db.ProgramacionesBateria.Include(p => p.Pozos).Where(p => p.Bateria.IdLocalidad == localidad.IdLocalidad && p.Observaciones == Marca).ToListAsync();
            var muestras = await db.AnalisisCalidad.Where(a => a.FuenteAgua.IdLocalidad == localidad.IdLocalidad && a.Observaciones == Marca).CountAsync();
            var cloro = await db.MedicionesCloro.CountAsync(m => m.IdLocalidad == localidad.IdLocalidad && m.TipoCloro == "Libre" && m.FechaHoraUtc.Year == 2026 && m.FechaHoraUtc.Month >= 8 && m.FechaHoraUtc.Month <= 10 && (m.FechaHoraUtc.Day == 10 || m.FechaHoraUtc.Day == 25) && m.FechaHoraUtc.Hour == 13);
            if (resumenes.Count != 3 || turnos.Count != 36 || turnos.Any(t => t.Pozos.Count != 2) || muestras != 15 || cloro != 6)
                throw new InvalidOperationException("Las cantidades de presentación no coinciden con lo esperado.");
            foreach (var r in resumenes.OrderBy(r => r.Mes))
            {
                using var json = JsonDocument.Parse(r.ReporteJson!);
                var esperado = r.Mes == 8 ? 3318m : r.Mes == 9 ? 3336m : 3375m;
                var volumen = json.RootElement.GetProperty("produccion").GetProperty("volumenTotalEstimadoM3").GetDecimal();
                var parametros = json.RootElement.GetProperty("calidad").GetProperty("parametros").EnumerateArray().ToList();
                if (volumen != esperado || parametros.Any(p => p.GetProperty("estadoCalculo").GetString() == "DatosIncompletos"))
                    throw new InvalidOperationException("Reporte inconsistente con la programación o con los análisis completos.");
                Console.WriteLine($"VERIFICADO {r.Mes}/2026: {volumen} m³; {r.EstadoCalidad}; sin mediciones faltantes.");
            }
            Console.WriteLine("Presentación lista: 2 pozos nuevos, 2 baterías de 2 pozos, 15 análisis mensuales, 36 turnos, 6 mediciones de cloro y 3 revisiones. Carga repetible sin duplicados.");
        }
        finally { contexto.HttpContext=anterior; }
    }

    private static void Completar(AnalisisCalidad a, bool acueducto, int mes, int fuente)
    {
        var variacion = fuente % 3 * 5m + (mes % 3) * 3m;
        a.ResiduoSeco ??= acueducto ? 420m+variacion : 850m+variacion;
        a.Conductividad ??= acueducto ? 700m+variacion : 1400m+variacion;
        a.Ph ??= acueducto ? 7.5m : 7.8m;
        a.Cloruro ??= acueducto ? 75m : 160m+variacion;
        a.Sulfato ??= mes==9 ? (acueducto ? 430m : 460m) : acueducto ? 90m : 220m+variacion;
        a.AlcalinidadTotal ??= acueducto ? 120m : 210m;
        a.Carbonato ??= 0m;
        a.Bicarbonato ??= acueducto ? 146.4m : 256.2m;
        a.Calcio ??= acueducto ? 35m : 65m;
        a.Magnesio ??= acueducto ? 12m : 24m;
        a.DurezaTotal ??= acueducto ? 136.8m : 261.15m;
        a.Hierro ??= acueducto ? 0.05m : 0.12m;
        a.Nitrato ??= acueducto ? 15m : mes==10 && fuente%2==0 ? 55m : 32m+variacion/5m;
        a.Nitrito ??= 0.02m; a.Amonio ??= 0.05m;
        a.Fluor ??= acueducto ? 0.5m : 1.1m;
        a.Arsenico ??= acueducto ? 0.003m : 0.008m;
        a.Sodio ??= acueducto ? 55m : 120m;
        a.Potasio ??= acueducto ? 4m : 8m;
    }

    public static async Task LimpiarObservacionesCloroAsync(IServiceProvider services, IHostEnvironment environment)
    {
        if (!environment.IsDevelopment()) throw new InvalidOperationException("Solo disponible en Development.");
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var localidad = await db.Localidades.SingleAsync(l => l.Nombre == "Prueba Quehue 2016");
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var mediciones = await db.MedicionesCloro.Where(m => m.IdLocalidad == localidad.IdLocalidad &&
            (m.Observaciones == Marca || m.Observaciones == "Medición de prueba de cierre" || m.Observaciones == "Medición de prueba intermedia")).ToListAsync();
        var carpeta = Path.Combine(environment.ContentRootPath, "..", "docs", "respaldos");
        Directory.CreateDirectory(carpeta);
        await File.WriteAllTextAsync(Path.Combine(carpeta, $"cloro-observaciones-{DateTime.UtcNow:yyyyMMdd-HHmmssfff}.json"), JsonSerializer.Serialize(mediciones));
        foreach (var medicion in mediciones) medicion.Observaciones = null;
        await db.SaveChangesAsync();
        var ids = mediciones.Select(m => m.IdMedicionCloro).ToArray();
        if (await db.MedicionesCloro.AsNoTracking().AnyAsync(m => ids.Contains(m.IdMedicionCloro) && m.Observaciones != null))
            throw new InvalidOperationException("No se pudieron limpiar las observaciones.");
        await tx.CommitAsync();
        Console.WriteLine($"VERIFICADO: observaciones eliminadas de {mediciones.Count} mediciones de cloro. Fechas y valores conservados.");
    }

    public static async Task RevisarAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        foreach (var l in await db.Localidades.AsNoTracking().ToListAsync())
        {
            Console.WriteLine($"LOCALIDAD {l.IdLocalidad}: {l.Nombre}");
            foreach (var f in await db.FuentesAgua.Where(f => f.IdLocalidad == l.IdLocalidad).ToListAsync())
                Console.WriteLine($"FUENTE {f.IdFuente} {f.Nombre} {f.TipoFuente} {f.Estado} Q={f.Caudal}; análisis={await db.AnalisisCalidad.CountAsync(a => a.IdFuente == f.IdFuente)}");
            foreach (var b in await db.Baterias.Where(b => b.IdLocalidad == l.IdLocalidad).ToListAsync())
                Console.WriteLine($"BATERIA {b.IdBateria} {b.Nombre}: pozos={string.Join(',', await db.BateriasPozos.Where(x => x.IdBateria == b.IdBateria).Select(x => x.IdPozo).ToListAsync())}");
            Console.WriteLine($"Turnos agosto-octubre 2026: {await db.ProgramacionesBateria.CountAsync(p => p.Bateria.IdLocalidad == l.IdLocalidad && p.Fecha >= new DateOnly(2026,8,1) && p.Fecha < new DateOnly(2026,11,1))}");
        }
    }
}

