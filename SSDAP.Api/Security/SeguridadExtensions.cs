using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SSDAP.Api.Data;
using SSDAP.Api.Models;
namespace SSDAP.Api.Security;
public static class SeguridadExtensions
{
    public static void AgregarSeguridad(this WebApplicationBuilder builder)
    {
        var jwt = new JwtService(builder.Configuration, builder.Environment);
        builder.Services.AddSingleton(jwt);
        builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true, ValidIssuer = jwt.Emisor,
                ValidateAudience = true, ValidAudience = jwt.Audiencia,
                ValidateLifetime = true, ClockSkew = TimeSpan.FromSeconds(30),
                ValidateIssuerSigningKey = true, IssuerSigningKey = jwt.Clave,
                ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                NameClaimType = "sub", RoleClaimType = "permiso"
            };
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    if (!int.TryParse(context.Principal?.FindFirstValue("sub"), out var id))
                    {
                        context.Fail("Identidad inválida.");
                        return;
                    }
                    var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
                    var usuario = await db.Usuarios.AsNoTracking().Include(u => u.Rol)
                        .SingleOrDefaultAsync(u => u.IdUsuario == id, context.HttpContext.RequestAborted);
                    if (usuario is null || !usuario.Activo || !usuario.Rol.Activo)
                    {
                        context.Fail("Acceso no disponible.");
                        return;
                    }
                    // El permiso se obtiene de la base en cada petición, no del token emitido.
                    var identity = (ClaimsIdentity)context.Principal!.Identity!;
                    foreach (var claim in identity.FindAll("permiso").ToList()) identity.RemoveClaim(claim);
                    if (usuario.Rol.Codigo == JwtService.Administrador)
                        identity.AddClaim(new Claim("permiso", JwtService.Administrador));
                }
            };
        });
        builder.Services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            options.AddPolicy("Administracion", policy => policy.RequireRole(JwtService.Administrador));
        });
    }
}
