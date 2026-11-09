using System.ComponentModel.DataAnnotations;
namespace SSDAP.Shared.DTOs.Usuarios;

public sealed record UsuarioDto(int IdUsuario, string Nombre, string Apellido, string Email, int IdRol, string Rol, bool Activo);
public sealed class CrearUsuarioDto
{
    [Required, StringLength(100)] public string Nombre { get; init; } = string.Empty;
    [Required, StringLength(100)] public string Apellido { get; init; } = string.Empty;
    [Required, EmailAddress, StringLength(100)] public string Email { get; init; } = string.Empty;
    [Required, StringLength(128)] public string Password { get; init; } = string.Empty;
    [Range(1, int.MaxValue)] public int IdRol { get; init; }
}

public sealed class ActualizarUsuarioDto
{
    [Required, StringLength(100)] public string Nombre { get; init; } = string.Empty;
    [Required, StringLength(100)] public string Apellido { get; init; } = string.Empty;
    [Required, EmailAddress, StringLength(100)] public string Email { get; init; } = string.Empty;
    [Range(1, int.MaxValue)] public int IdRol { get; init; }
    [Required] public bool? Activo { get; init; }
}

public sealed class CambiarPasswordDto
{
    [Required, StringLength(128)] public string PasswordActual { get; init; } = string.Empty;
    [Required, StringLength(128)] public string PasswordNueva { get; init; } = string.Empty;
}

public sealed class RestablecerPasswordDto
{
    [Required, StringLength(128)] public string PasswordNueva { get; init; } = string.Empty;
}
