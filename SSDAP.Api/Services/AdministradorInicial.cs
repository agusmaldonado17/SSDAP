using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SSDAP.Api.Data;
using SSDAP.Api.Models;
using SSDAP.Api.Security;
using SSDAP.Shared.DTOs.Usuarios;
namespace SSDAP.Api.Services;
public static class AdministradorInicial
{
    public static async Task CrearAsync(IServiceProvider services, IConfiguration config)
    {
        var datos = new CrearUsuarioDto
        {
            Nombre = config["AdministradorInicial:Nombre"] ?? "Administrador",
            Apellido = config["AdministradorInicial:Apellido"] ?? "SSDAP",
            Email = config["AdministradorInicial:Email"] ?? "",
            Password = config["AdministradorInicial:Password"] ?? "",
            IdRol = 1
        };
        Validator.ValidateObject(datos, new ValidationContext(datos), true);
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if ((await db.Database.GetPendingMigrationsAsync()).Any())
            throw new InvalidOperationException(
                "Falta actualizar la base de datos. Ejecutá: dotnet ef database update PrepararAutenticacion --project SSDAP.Api");
        // Ejecución explícita por consola; nunca se aplica una migración al arrancar la API.
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        if (await db.Usuarios.AnyAsync())
            throw new InvalidOperationException("La creación inicial solo está disponible si no existen usuarios.");
        var rol = await db.Roles.SingleOrDefaultAsync(r => r.Codigo == JwtService.Administrador);
        if (rol is null)
        {
            if (await db.Roles.AnyAsync(r => r.Nombre == "Administrador"))
                throw new InvalidOperationException("Ya existe un rol Administrador sin código de sistema. Revisarlo antes de inicializar.");
            rol = new Rol { Nombre = "Administrador", Codigo = JwtService.Administrador, Activo = true };
            db.Roles.Add(rol);
        }
        rol.Activo = true;
        var usuario = new Usuario { Nombre = datos.Nombre.Trim(), Apellido = datos.Apellido.Trim(),
            Email = datos.Email.Trim(), EmailNormalizado = UsuarioService.NormalizarEmail(datos.Email), Rol = rol, Activo = true };
        usuario.PasswordHash = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Usuario>>().HashPassword(usuario, datos.Password);
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        Console.WriteLine("Administrador inicial creado.");
    }
}
