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
        var (token, expira) = jwt.Emitir(usuario);
        return new LoginRespuestaDto(token, expira, UsuarioService.Convertir(usuario));
    }
}
