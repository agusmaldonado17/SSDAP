using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
namespace SSDAP.Api.Security;
public sealed class BearerDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT",
            Description = "Iniciar sesión y pegar aquí el accessToken."
        };
        foreach (var path in document.Paths.Values)
        foreach (var operation in path.Operations?.Values.AsEnumerable() ?? Enumerable.Empty<OpenApiOperation>())
        {
            operation.Security ??= [];
            // Login es la única operación pública de esta API.
            if (operation.OperationId == "Login") continue;
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        }
        return Task.CompletedTask;
    }
}

