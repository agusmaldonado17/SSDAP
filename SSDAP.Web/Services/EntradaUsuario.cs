using System.ComponentModel.DataAnnotations;
using SSDAP.Shared.DTOs.Usuarios;

namespace SSDAP.Web.Services;

public sealed class EntradaUsuario : IValidatableObject
{
    public int? Id { get; set; }
    [Required(ErrorMessage = "Ingresá el nombre."), StringLength(100)]
    public string Nombre { get; set; } = "";
    [Required(ErrorMessage = "Ingresá el apellido."), StringLength(100)]
    public string Apellido { get; set; } = "";
    [Required(ErrorMessage = "Ingresá el correo."), EmailAddress(ErrorMessage = "Ingresá un correo válido."), StringLength(100)]
    public string Email { get; set; } = "";
    [Range(1, int.MaxValue, ErrorMessage = "Seleccioná un rol.")]
    public int IdRol { get; set; }
    [StringLength(128)] public string Password { get; set; } = "";
    public bool Activo { get; set; } = true;
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (Id is null && string.IsNullOrWhiteSpace(Password))
            yield return new("Ingresá la contraseña inicial.", [nameof(Password)]);
    }
    public CrearUsuarioDto Alta() => new() { Nombre = Nombre.Trim(), Apellido = Apellido.Trim(),
        Email = Email.Trim(), IdRol = IdRol, Password = Password };
    public ActualizarUsuarioDto Edicion() => new() { Nombre = Nombre.Trim(), Apellido = Apellido.Trim(),
        Email = Email.Trim(), IdRol = IdRol, Activo = Activo };
    public static EntradaUsuario Desde(UsuarioDto u) => new() { Id = u.IdUsuario, Nombre = u.Nombre,
        Apellido = u.Apellido, Email = u.Email, IdRol = u.IdRol, Activo = u.Activo };
}
