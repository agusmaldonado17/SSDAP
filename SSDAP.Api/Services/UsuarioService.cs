using Microsoft.AspNetCore.Identity;
using System.Data;
using SSDAP.Api.Security;
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

    public async Task<UsuarioDto> ActualizarAsync(int id, ActualizarUsuarioDto datos, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        // Serializar los cambios de administradores para conservar al menos uno activo.
        await db.Roles.FromSqlInterpolated($"SELECT * FROM rol WHERE codigo = {JwtService.Administrador} FOR UPDATE")
            .AsNoTracking().ToListAsync(ct);
        var usuario = await BloquearAsync(id, ct);
        var rol = await db.Roles.SingleOrDefaultAsync(r => r.IdRol == datos.IdRol && r.Activo, ct)
            ?? throw new ReglaNegocioException(400, "El rol no existe o está inactivo.");
        var eraAdministrador = await db.Roles.AnyAsync(r => r.IdRol == usuario.IdRol && r.Codigo == JwtService.Administrador, ct);
        if (usuario.Activo && eraAdministrador && (datos.Activo != true || rol.Codigo != JwtService.Administrador) &&
            !await db.Usuarios.AnyAsync(u => u.IdUsuario != id && u.Activo && u.Rol.Activo && u.Rol.Codigo == JwtService.Administrador, ct))
            throw new ReglaNegocioException(409, "Debe permanecer al menos un administrador activo.");
        if (usuario.Activo != datos.Activo || usuario.IdRol != datos.IdRol || usuario.EmailNormalizado != NormalizarEmail(datos.Email))
            usuario.SelloSeguridad = Guid.NewGuid().ToString();
        usuario.Nombre = datos.Nombre.Trim();
        usuario.Apellido = datos.Apellido.Trim();
        usuario.Email = datos.Email.Trim();
        usuario.EmailNormalizado = NormalizarEmail(datos.Email);
        usuario.Rol = rol;
        usuario.Activo = datos.Activo!.Value;
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return Convertir(usuario);
    }

    public async Task CambiarPasswordAsync(int id, string nueva, string? actual, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        var usuario = await BloquearAsync(id, ct);
        if (actual is not null)
        {
            var resultado = PasswordVerificationResult.Failed;
            try { resultado = hasher.VerifyHashedPassword(usuario, usuario.PasswordHash, actual); }
            catch (FormatException) { }
            if (resultado == PasswordVerificationResult.Failed)
                throw new ReglaNegocioException(400, "La contraseña actual no es correcta.");
        }
        usuario.PasswordHash = hasher.HashPassword(usuario, nueva);
        usuario.SelloSeguridad = Guid.NewGuid().ToString();
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    public async Task<List<SSDAP.Shared.DTOs.Localidades.LocalidadDto>> LocalidadesAsync(int id, int pagina, CancellationToken ct)
    {
        if (!await db.Usuarios.AnyAsync(u => u.IdUsuario == id, ct))
            throw new ReglaNegocioException(404, "Usuario no encontrado.");
        return await db.UsuariosLocalidades.AsNoTracking().Where(x => x.IdUsuario == id)
            .OrderBy(x => x.IdLocalidad).Skip((pagina - 1) * 50).Take(50)
            .Select(x => new SSDAP.Shared.DTOs.Localidades.LocalidadDto(x.Localidad.IdLocalidad,
                x.Localidad.Nombre, x.Localidad.Provincia, x.Localidad.Estado)).ToListAsync(ct);
    }

    private async Task<Usuario> BloquearAsync(int id, CancellationToken ct) =>
        (await db.Usuarios.FromSqlInterpolated($"SELECT * FROM usuario WHERE id_usuario = {id} FOR UPDATE")
            .ToListAsync(ct)).SingleOrDefault() ?? throw new ReglaNegocioException(404, "Usuario no encontrado.");
}
