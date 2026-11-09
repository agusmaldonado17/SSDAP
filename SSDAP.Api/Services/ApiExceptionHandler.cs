using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
namespace SSDAP.Api.Services;
public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var error = exception switch
        {
            ReglaNegocioException regla => (regla.Estado, regla.Message),
            DbUpdateException { InnerException: MySqlException { Number: 1062 } } => (409, "Ya existe un registro con esos datos."),
            MySqlException { Number: 1062 } => (409, "Ya existe un registro con esos datos."),
            _ => (500, "No se pudo completar la operación.")
        };
        context.Response.StatusCode = error.Item1;
        if (error.Item1 == 500)
            logger.LogError("Error no controlado {TipoError}. Solicitud {TraceId}",
                exception.GetType().Name, context.TraceIdentifier);
        await context.Response.WriteAsJsonAsync(new ProblemDetails { Status = error.Item1, Title = error.Item2 }, cancellationToken);
        return true;
    }
}
