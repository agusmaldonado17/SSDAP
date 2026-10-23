using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using SSDAP.Api.Data;
using SSDAP.Api.Security;
using SSDAP.Api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection")!));
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddScoped<RolService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<LocalidadService>();
builder.Services.AddScoped<AutenticacionService>();
builder.AgregarSeguridad();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "local",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 20,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));
});
builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerDocumentTransformer>());
var app = builder.Build();
if (args.Contains("--inicializar-admin"))
{
    await AdministradorInicial.CrearAsync(app.Services, app.Configuration);
    return;
}
app.UseExceptionHandler();
app.UseHttpsRedirection();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "SSDAP API v1"));
}
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();
app.Run();
