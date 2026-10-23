using System.Data;
using MySql.Data.MySqlClient;
using SSDAP.Api.Models;
using Microsoft.EntityFrameworkCore;
using SSDAP.Api.Data;
using SSDAP.Shared.DTOs.Roles;

namespace SSDAP.Api.Services;

public sealed class RolService(AppDbContext dbContext)
{
    public async Task<List<RolDto>> ListarAsync(bool? activo, CancellationToken cancellationToken)
    {
        var consulta = dbContext.Roles.AsNoTracking();

        if (activo.HasValue)
        {
            consulta = consulta.Where(rol => rol.Activo == activo.Value);
        }

        return await consulta
            .OrderBy(rol => rol.Nombre)
            .ThenBy(rol => rol.IdRol)
            .Select(rol => new RolDto(rol.IdRol, rol.Nombre, rol.Descripcion, rol.Activo))
            .ToListAsync(cancellationToken);
    }

    public Task<RolDto?> ObtenerAsync(int id, CancellationToken cancellationToken)
    {
        return dbContext.Roles.AsNoTracking()
            .Where(rol => rol.IdRol == id)
            .Select(rol => new RolDto(rol.IdRol, rol.Nombre, rol.Descripcion, rol.Activo))
            .SingleOrDefaultAsync(cancellationToken);
    }
    public async Task<RolDto> CrearAsync(GuardarRolDto datos, CancellationToken cancellationToken)
    {
        var rol = new Rol();
        AsignarDatos(rol, datos);
        dbContext.Roles.Add(rol);
        await GuardarAsync(cancellationToken);
        return new RolDto(rol.IdRol, rol.Nombre, rol.Descripcion, rol.Activo);
    }

    public async Task<RolDto?> ActualizarAsync(int id, GuardarRolDto datos, CancellationToken cancellationToken)
    {
        var rol = await dbContext.Roles.SingleOrDefaultAsync(rol => rol.IdRol == id, cancellationToken);
        if (rol is null)
        {
            return null;
        }

        if (rol.Codigo == SSDAP.Api.Security.JwtService.Administrador && datos.Activo == false)
            throw new ReglaNegocioException(409, "El rol administrador debe permanecer activo.");

        AsignarDatos(rol, datos);
        await GuardarAsync(cancellationToken);
        return new RolDto(rol.IdRol, rol.Nombre, rol.Descripcion, rol.Activo);
    }

    private static void AsignarDatos(Rol rol, GuardarRolDto datos)
    {
        rol.Nombre = datos.Nombre.Trim();
        rol.Descripcion = string.IsNullOrWhiteSpace(datos.Descripcion) ? null : datos.Descripcion.Trim();
        rol.Activo = datos.Activo!.Value;
    }

    private async Task GuardarAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is MySqlException { Number: 1062 })
        {
            throw new DuplicateNameException("Ya existe un rol con ese nombre.", exception);
        }
    }
}
