using Microsoft.EntityFrameworkCore;
using SSDAP.Api.Data;
using SSDAP.Api.Models;
using SSDAP.Shared.DTOs.Calidad;

namespace SSDAP.Api.Services;

public sealed class ParametroCalidadService(AppDbContext db)
{
    public async Task<List<ParametroCalidadDto>> ListarAsync(int pagina, CancellationToken ct) =>
        (await db.ParametrosLey.AsNoTracking().OrderBy(p => p.IdParametroLey)
            .Skip((pagina - 1) * 50).Take(50).ToListAsync(ct)).Select(Detalle).ToList();

    public async Task<ParametroCalidadDto> ObtenerAsync(int id, CancellationToken ct) =>
        Detalle(await db.ParametrosLey.AsNoTracking().SingleOrDefaultAsync(p => p.IdParametroLey == id, ct)
            ?? throw new ReglaNegocioException(404, "Parámetro no encontrado."));

    public async Task<ParametroCalidadDto> CrearAsync(GuardarParametroCalidadDto datos, CancellationToken ct)
    {
        var definicion = Validar(datos);
        var entidad = new ParametroLey
        {
            Codigo = definicion.Codigo, NombreParametro = definicion.Codigo, Unidad = definicion.Unidad,
            ValorMinimo = datos.ValorMinimo, ValorMaximo = datos.ValorMaximo,
            Referencia = datos.Referencia.Trim(), Activo = datos.Activo!.Value, Version = 1,
            Observaciones = datos.Observaciones?.Trim()
        };
        db.ParametrosLey.Add(entidad);
        await db.SaveChangesAsync(ct);
        return Detalle(entidad);
    }

    public async Task<ParametroCalidadDto> ActualizarAsync(int id, ActualizarParametroCalidadDto datos, CancellationToken ct)
    {
        var definicion = Validar(datos);
        var anterior = await ObtenerAsync(id, ct);
        if (anterior.Codigo is not null && anterior.Codigo != definicion.Codigo)
            throw new ReglaNegocioException(409, "El código de un parámetro vinculado no se puede cambiar.");
        if (anterior.Version != datos.Version || anterior.Version == int.MaxValue)
            throw new ReglaNegocioException(409, "La versión cambió. Volvé a consultar el parámetro.");
        var modificados = await db.ParametrosLey.Where(p => p.IdParametroLey == id && p.Version == datos.Version)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.Codigo, definicion.Codigo)
                .SetProperty(p => p.NombreParametro, definicion.Codigo)
                .SetProperty(p => p.Unidad, definicion.Unidad)
                .SetProperty(p => p.ValorMinimo, datos.ValorMinimo)
                .SetProperty(p => p.ValorMaximo, datos.ValorMaximo)
                .SetProperty(p => p.Referencia, datos.Referencia.Trim())
                .SetProperty(p => p.Activo, datos.Activo!.Value)
                .SetProperty(p => p.Observaciones, datos.Observaciones == null ? null : datos.Observaciones.Trim())
                .SetProperty(p => p.Version, p => p.Version + 1), ct);
        if (modificados == 0)
            throw new ReglaNegocioException(409, "La versión cambió. Volvé a consultar el parámetro.");
        return anterior with
        {
            Codigo = definicion.Codigo, Nombre = definicion.Codigo, Unidad = definicion.Unidad,
            ValorMinimo = datos.ValorMinimo, ValorMaximo = datos.ValorMaximo,
            Referencia = datos.Referencia.Trim(), Activo = datos.Activo!.Value,
            Observaciones = datos.Observaciones?.Trim(), Version = anterior.Version + 1
        };
    }

    private static DefinicionParametro Validar(GuardarParametroCalidadDto datos)
    {
        var definicion = CatalogoCalidad.Buscar(datos.Codigo);
        if (datos.Activo == true && datos.ValorMinimo is null && datos.ValorMaximo is null)
            throw new ReglaNegocioException(400, "Ingresá al menos un límite.");
        if (datos.ValorMinimo > datos.ValorMaximo)
            throw new ReglaNegocioException(400, "El mínimo no puede superar al máximo.");
        var maximo = definicion.Codigo == "Ph" ? 14m : 99999999.999999m;
        foreach (var valor in new[] { datos.ValorMinimo, datos.ValorMaximo })
            if (valor is decimal numero && (numero < 0 || numero > maximo || decimal.Round(numero, 6) != numero))
                throw new ReglaNegocioException(400, "Límite fuera de rango o con más de seis decimales.");
        return definicion;
    }

    public static ParametroCalidadDto Detalle(ParametroLey p) =>
        new(p.IdParametroLey, p.Codigo, p.NombreParametro, p.Unidad, p.ValorMinimo,
            p.ValorMaximo, p.Referencia, p.Activo, p.Version, p.Observaciones);
}
