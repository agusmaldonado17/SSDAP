using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SSDAP.Api.Data;
using SSDAP.Api.Models;
using SSDAP.Shared.DTOs.Usuarios;
namespace SSDAP.Api.Services;
public sealed class UsuarioService(AppDbContext db, IPasswordHasher<Usuario> hasher)
{
    public static string NormalizarEmail(string email) => email.Trim().ToUpperInvariant();
    public static UsuarioDto Convertir(Usuario u) => new(u.IdUsuario, u.Nombre, u.Apellido, u.Email, u.IdRol, u.Rol.Nombre, u.Activo);
    public Task<List<UsuarioDto>> ListarAsync(int pagina, CancellationToken ct) => db.Usuarios.AsNoTracking()
        .OrderBy(u => u.IdUsuario).Skip((pagina - 1) * 50).Take(50)
        .Select(u => new UsuarioDto(u.IdUsuario, u.Nombre, u.Apellido, u.Email, u.IdRol, u.Rol.Nombre, u.Activo)).ToListAsync(ct);
    public async Task<UsuarioDto> ObtenerAsync(int id, CancellationToken ct)
    {
        var usuario = await db.Usuarios.AsNoTracking().Include(u => u.Rol).SingleOrDefaultAsync(u => u.IdUsuario == id, ct)
            ?? throw new ReglaNegocioException(404, "Usuario no encontrado.");
        return Convertir(usuario);
    }
    public async Task<UsuarioDto> CrearAsync(CrearUsuarioDto datos, CancellationToken ct)
    {
        var rol = await db.Roles.SingleOrDefaultAsync(r => r.IdRol == datos.IdRol && r.Activo, ct)
            ?? throw new ReglaNegocioException(400, "El rol no existe o está inactivo.");
        var usuario = new Usuario { Nombre = datos.Nombre.Trim(), Apellido = datos.Apellido.Trim(),
            Email = datos.Email.Trim(), EmailNormalizado = NormalizarEmail(datos.Email), Rol = rol, Activo = true };
        usuario.PasswordHash = hasher.HashPassword(usuario, datos.Password);
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync(ct);
        return Convertir(usuario);
    }
    public async Task AsignarLocalidadAsync(int id, int localidadId, bool asignar, CancellationToken ct)
    {
        if (!await db.Usuarios.AnyAsync(u => u.IdUsuario == id, ct))
            throw new ReglaNegocioException(404, "Usuario no encontrado.");
        if (!await db.Localidades.AnyAsync(l => l.IdLocalidad == localidadId, ct))
            throw new ReglaNegocioException(404, "Localidad no encontrada.");
        var relacion = await db.UsuariosLocalidades.SingleOrDefaultAsync(x => x.IdUsuario == id && x.IdLocalidad == localidadId, ct);
        if (asignar && relacion is null)
            db.UsuariosLocalidades.Add(new UsuarioLocalidad { IdUsuario = id, IdLocalidad = localidadId });
        else if (!asignar && relacion is not null)
            db.UsuariosLocalidades.Remove(relacion);
        await db.SaveChangesAsync(ct);
    }
}
