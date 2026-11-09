using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SSDAP.Api.Data;
using SSDAP.Api.Security;

namespace SSDAP.Api.Services;

public sealed class AccesoLocalidadService(AppDbContext db, IHttpContextAccessor context)
{
    public async Task VerificarAsync(int localidadId, CancellationToken cancellationToken)
    {
        var usuario = context.HttpContext?.User;
        if (usuario?.Identity?.IsAuthenticated != true ||
            !int.TryParse(usuario.FindFirstValue("sub"), out var usuarioId))
            throw new ReglaNegocioException(401, "Es necesario iniciar sesión.");

        // Comprobar la asignación antes de revelar si una localidad ajena existe.
        if (!usuario.IsInRole(JwtService.Administrador) &&
            !await db.UsuariosLocalidades.AnyAsync(
                x => x.IdUsuario == usuarioId && x.IdLocalidad == localidadId, cancellationToken))
            throw new ReglaNegocioException(403, "No tenés acceso a esta localidad.");

        if (!await db.Localidades.AnyAsync(x => x.IdLocalidad == localidadId, cancellationToken))
            throw new ReglaNegocioException(404, "Localidad no encontrada.");
    }
}
