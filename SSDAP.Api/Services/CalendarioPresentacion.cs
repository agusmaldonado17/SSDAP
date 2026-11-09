using System.Data;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SSDAP.Api.Data;
using SSDAP.Api.Models;
using SSDAP.Api.Security;
using SSDAP.Shared.DTOs.Reportes;
using SSDAP.Shared.DTOs.Simulaciones;

namespace SSDAP.Api.Services;

public static class CalendarioPresentacion
{
    private const string Marca = "Octubre 2026: Norte/Sur alternadas de 07 a 19 (presentación)";
    public static async Task CargarAsync(IServiceProvider services, IHostEnvironment environment)
    {
        if (!environment.IsDevelopment()) throw new InvalidOperationException("Solo disponible en Development.");
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var localidad = await db.Localidades.SingleAsync(l => l.Nombre == "Prueba Quehue 2016");
        var autor = await db.Usuarios.Where(u => u.Activo && u.Rol.Nombre == JwtService.Administrador).OrderBy(u => u.IdUsuario).Select(u => u.IdUsuario).FirstAsync();
        var baterias = await db.Baterias.Where(b => b.IdLocalidad == localidad.IdLocalidad && (b.Nombre == "Batería Norte" || b.Nombre == "Batería Sur")).ToListAsync();
        if (baterias.Count != 2 || baterias.Any(b => b.Estado != "Activa")) throw new InvalidOperationException("Se requieren Norte y Sur activas.");
        var ids = baterias.Select(b => b.IdBateria).ToArray();
        var inicioMes = new DateTime(2026,10,1,3,0,0,DateTimeKind.Utc);
        var finMes = inicioMes.AddMonths(1);
        await using (var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable))
        {
            await db.Localidades.FromSqlInterpolated($"SELECT * FROM localidad WHERE id_localidad = {localidad.IdLocalidad} FOR UPDATE").AsNoTracking().ToListAsync();
            var previos = await db.ProgramacionesBateria.Include(p => p.Pozos).Where(p => ids.Contains(p.IdBateria) && p.InicioUtc < finMes && p.FinUtc > inicioMes).ToListAsync();
            if (previos.Any(p => p.Estado == "Programada" && (p.InicioUtc < inicioMes || p.FinUtc > finMes)))
                throw new InvalidOperationException("Hay turnos que cruzan el límite del mes; revisar antes de reemplazar.");
            var carpeta = Path.Combine(environment.ContentRootPath,"..","docs","respaldos");
            Directory.CreateDirectory(carpeta);
            await File.WriteAllTextAsync(Path.Combine(carpeta,$"calendario-octubre-{DateTime.UtcNow:yyyyMMdd-HHmmssfff}.json"),
                JsonSerializer.Serialize(previos.Select(p => new { p.IdProgramacion,p.IdBateria,p.InicioUtc,p.FinUtc,p.Estado,p.Version,p.Observaciones,
                    Pozos=p.Pozos.Select(x => new {x.IdPozo,x.Codigo,x.CaudalM3H}) }),new JsonSerializerOptions {WriteIndented=true}));
            foreach (var p in previos.Where(p => p.Estado == "Programada" && p.Observaciones != Marca))
            { p.Estado="Cancelada"; p.Version++; p.ModificadoEnUtc=DateTime.UtcNow; p.ModificadoPorId=autor; }
            await db.SaveChangesAsync();
            for (var dia=1;dia<=31;dia++)
            {
                var bateria=baterias.Single(b => b.Nombre == (dia%2==1 ? "Batería Norte" : "Batería Sur"));
                var inicio=new DateTime(2026,10,dia,10,0,0,DateTimeKind.Utc); // 07:00 Argentina
                var fin=inicio.AddHours(12);
                if (previos.Any(p => p.Estado == "Programada" && p.Observaciones == Marca && p.IdBateria==bateria.IdBateria && p.InicioUtc==inicio && p.FinUtc==fin)) continue;
                var asociados=await db.BateriasPozos.Include(x => x.Pozo).ThenInclude(p => p.FuenteAgua).Where(x => x.IdBateria==bateria.IdBateria).ToListAsync();
                var pozos=asociados.Select(x => x.Pozo).ToList();
                if (pozos.Count!=2 || pozos.Any(p => p.FuenteAgua.Estado!="Activa" || p.FuenteAgua.Caudal<=0)) throw new InvalidOperationException("Composición inesperada de batería.");
                var idsPozos=pozos.Select(p => p.IdPozo).ToArray();
                if (await db.ProgramacionesPozos.AnyAsync(p => idsPozos.Contains(p.IdPozo) && p.Programacion.Estado=="Programada" && p.Programacion.InicioUtc<fin && p.Programacion.FinUtc>inicio))
                    throw new InvalidOperationException("Superposición con otro turno.");
                db.ProgramacionesBateria.Add(new ProgramacionBateria { IdBateria=bateria.IdBateria,NombreBateria=bateria.Nombre,
                    Fecha=new(2026,10,dia),HoraInicio=new(7,0),HoraFin=new(19,0),InicioUtc=inicio,FinUtc=fin,Estado="Programada",Version=1,
                    CreadoEnUtc=DateTime.UtcNow,ModificadoEnUtc=DateTime.UtcNow,CreadoPorId=autor,ModificadoPorId=autor,Observaciones=Marca,
                    Pozos=pozos.Select(p => new ProgramacionPozo {IdPozo=p.IdPozo,Codigo=p.Codigo,CaudalM3H=p.FuenteAgua.Caudal}).ToList() });
                await db.SaveChangesAsync();
            }
            var activos=await db.ProgramacionesBateria.Where(p => ids.Contains(p.IdBateria) && p.Estado=="Programada" && p.InicioUtc<finMes && p.FinUtc>inicioMes).OrderBy(p => p.InicioUtc).ToListAsync();
            if (activos.Count!=31 || activos.Select(p => p.Fecha).Distinct().Count()!=31 || activos.Any(p => p.HoraInicio!=new TimeOnly(7,0) || p.HoraFin!=new TimeOnly(19,0)))
                throw new InvalidOperationException("No coincide la programación solicitada.");
            await tx.CommitAsync();
        }
        var accessor=scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        var anterior=accessor.HttpContext;
        accessor.HttpContext=new DefaultHttpContext {User=new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub",autor.ToString()),new Claim(ClaimTypes.Role,JwtService.Administrador)],"CargaLocal"))};
        try
        {
            var produccion=new ConsultarProduccionMensualDto {Anio=2026,Mes=10,IdFuentesAcueducto=await db.FuentesAgua.Where(f => f.IdLocalidad==localidad.IdLocalidad && f.TipoFuente=="Acueducto" && f.Estado=="Activa").Select(f => f.IdFuente).ToListAsync()};
            var reporte=await scope.ServiceProvider.GetRequiredService<ProduccionMensualService>().ConsultarAsync(localidad.IdLocalidad,produccion,CancellationToken.None);
            if (reporte.VolumenPozosM3!=1764m || reporte.VolumenTotalEstimadoM3!=4740m) throw new InvalidOperationException("Volumen inesperado.");
            if (!await db.ResultadosMensuales.AnyAsync(r => r.IdLocalidad==localidad.IdLocalidad && r.Anio==2026 && r.Mes==10 && r.Observaciones==Marca))
            {
                var analisis=await db.AnalisisCalidad.Where(a => a.FuenteAgua.IdLocalidad==localidad.IdLocalidad && a.Fecha==new DateOnly(2026,10,1)).Select(a => new FuenteSimulacionSeleccionDto {IdFuente=a.IdFuente,IdAnalisis=a.IdAnalisis}).ToListAsync();
                var revision=await scope.ServiceProvider.GetRequiredService<ResultadoMensualService>().GuardarAsync(localidad.IdLocalidad,new GuardarResultadoMensualDto {Produccion=produccion,Calidad=new(){Analisis=analisis},Observaciones=Marca},CancellationToken.None);
                Console.WriteLine($"Revisión guardada: {revision.Resultado.Revision} (registro {revision.Resultado.IdResultado}).");
            }
            Console.WriteLine("VERIFICADO: 31 turnos de 07:00 a 19:00. Norte 16 días/192 horas; Sur 15 días/180 horas. Pozos 1764 m³; acueducto 2976 m³; total 4740 m³.");
        }
        finally {accessor.HttpContext=anterior;}
    }
}
