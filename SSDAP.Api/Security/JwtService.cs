using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SSDAP.Api.Models;
namespace SSDAP.Api.Security;

public sealed class JwtService
{
    public const string Administrador = "ADMINISTRADOR";
    public string Emisor { get; }
    public string Audiencia { get; }
    public SymmetricSecurityKey Clave { get; }
    public JwtService(IConfiguration configuration, IHostEnvironment environment)
    {
        Emisor = configuration["Jwt:Issuer"] ?? "SSDAP.Api";
        Audiencia = configuration["Jwt:Audience"] ?? "SSDAP.Clientes";
        var clave = configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(clave))
        {
            if (!environment.IsDevelopment())
                throw new InvalidOperationException("Configurar Jwt:Key fuera del código antes de iniciar en producción.");
            // En desarrollo los tokens caducan también al reiniciar el proceso.
            clave = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
        }
        if (Encoding.UTF8.GetByteCount(clave) < 32)
            throw new InvalidOperationException("Jwt:Key debe tener al menos 32 bytes.");
        Clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clave));
    }
    public (string Token, DateTimeOffset Expira) Emitir(Usuario usuario)
    {
        var ahora = DateTimeOffset.UtcNow;
        var expira = ahora.AddMinutes(30);
        var token = new JwtSecurityToken(Emisor, Audiencia,
            [new Claim(JwtRegisteredClaimNames.Sub, usuario.IdUsuario.ToString()),
             new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
             new Claim(JwtRegisteredClaimNames.Iat, ahora.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)],
            ahora.UtcDateTime, expira.UtcDateTime, new SigningCredentials(Clave, SecurityAlgorithms.HmacSha256));
        return (new JwtSecurityTokenHandler().WriteToken(token), expira);
    }
}
