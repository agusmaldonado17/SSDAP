using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SSDAP.Api.Data;
using SSDAP.Api.Models;
using SSDAP.Api.Security;
using SSDAP.Shared.DTOs.Autenticacion;
namespace SSDAP.Api.Services;
public sealed class AutenticacionService(AppDbContext db, IPasswordHasher<Usuario> hasher, JwtService jwt)
{
    private static readonly Usuario UsuarioFicticio = new();
    private static readonly string HashFicticio = new PasswordHasher<Usuario>().HashPassword(UsuarioFicticio, Guid.NewGuid().ToString());
    public async Task<LoginRespuestaDto?> LoginAsync(LoginDto datos, CancellationToken ct)
    {
        var email = UsuarioService.NormalizarEmail(datos.Email);
        var usuario = await db.Usuarios.Include(u => u.Rol).SingleOrDefaultAsync(u => u.EmailNormalizado == email, ct);
        var resultado = PasswordVerificationResult.Failed;
        try
        {
            resultado = hasher.VerifyHashedPassword(usuario ?? UsuarioFicticio, usuario?.PasswordHash ?? HashFicticio, datos.Password);
        }
        catch (FormatException) { /* Una contraseña histórica sin hash no habilita el acceso. */ }
        if (usuario is null || !usuario.Activo || !usuario.Rol.Activo || resultado == PasswordVerificationResult.Failed)
            return null;
        if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
        {
            usuario.PasswordHash = hasher.HashPassword(usuario, datos.Password);
            await db.SaveChangesAsync(ct);
        }
        var ahora = DateTime.UtcNow;
        var sesion = new SesionUsuario { IdUsuario = usuario.IdUsuario, CreadaEnUtc = ahora,
            UltimaActividadUtc = ahora, ExpiraEnUtc = ahora.AddHours(1) };
        db.SesionesUsuario.Add(sesion);
        await db.SaveChangesAsync(ct);
        var (token, expira) = jwt.Emitir(usuario, sesion);
        return new LoginRespuestaDto(token, expira, UsuarioService.Convertir(usuario));
    }

    public async Task<LoginRespuestaDto> RegistrarActividadAsync(int usuarioId, string sesionId, string sello, CancellationToken ct)
    {
        var ahora = DateTime.UtcNow;
        var vence = ahora.AddHours(1);
        // El predicado impide revivir una sesión vencida entre la autenticación y esta operación.
        var actualizadas = await db.SesionesUsuario.Where(s => s.Id == sesionId && s.IdUsuario == usuarioId && s.ExpiraEnUtc > ahora)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.UltimaActividadUtc, x => x.UltimaActividadUtc < ahora ? ahora : x.UltimaActividadUtc)
                .SetProperty(x => x.ExpiraEnUtc, x => x.ExpiraEnUtc < vence ? vence : x.ExpiraEnUtc), ct);
        if (actualizadas == 0) throw new ReglaNegocioException(401, "La sesión venció. Iniciá sesión nuevamente.");
        var usuario = await db.Usuarios.AsNoTracking().Include(u => u.Rol).SingleAsync(u => u.IdUsuario == usuarioId, ct);
        if (!usuario.Activo || !usuario.Rol.Activo || usuario.SelloSeguridad != sello)
            throw new ReglaNegocioException(401, "La sesión ya no está habilitada.");
        var sesion = await db.SesionesUsuario.AsNoTracking().SingleOrDefaultAsync(s => s.Id == sesionId && s.IdUsuario == usuarioId, ct)
            ?? throw new ReglaNegocioException(401, "La sesión ya no está habilitada.");
        var (token, expira) = jwt.Emitir(usuario, sesion);
        return new(token, expira, UsuarioService.Convertir(usuario));
    }

    public async Task CerrarSesionAsync(int usuarioId, string sesionId, CancellationToken ct)
    {
        // Eliminar la sesión impide que una renovación concurrente vuelva a habilitarla.
        await db.SesionesUsuario.Where(s => s.Id == sesionId && s.IdUsuario == usuarioId).ExecuteDeleteAsync(ct);
    }
}
