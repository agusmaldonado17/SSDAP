using System.ComponentModel.DataAnnotations;
using SSDAP.Shared.DTOs.Usuarios;
namespace SSDAP.Shared.DTOs.Autenticacion;
public sealed class LoginDto
{
    [Required, EmailAddress, StringLength(100)] public string Email { get; init; } = string.Empty;
    [Required, StringLength(128)] public string Password { get; init; } = string.Empty;
}
public sealed record LoginRespuestaDto(string AccessToken, DateTimeOffset ExpiraEn, UsuarioDto Usuario);
