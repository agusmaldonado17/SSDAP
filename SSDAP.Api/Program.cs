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
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AccesoLocalidadService>();
builder.Services.AddScoped<FuenteService>();
builder.Services.AddScoped<BateriaService>();
builder.Services.AddScoped<CalendarioService>();
builder.Services.AddScoped<AnalisisCalidadService>();
builder.Services.AddScoped<ParametroCalidadService>();
builder.Services.AddScoped<EvaluacionCalidadService>();
builder.Services.AddScoped<MedicionCloroService>();
builder.Services.AddScoped<AlertasService>();
builder.Services.AddScoped<SimulacionMezclaService>();
builder.Services.AddScoped<ProduccionMensualService>();
builder.Services.AddScoped<ResultadoMensualService>();
builder.Services.AddScoped<CalidadMensualService>();
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
if (args.Contains("--limpiar-observaciones-cloro"))
{
    await DatosPresentacion.LimpiarObservacionesCloroAsync(app.Services, app.Environment);
    return;
}
if (args.Contains("--alternar-octubre-presentacion"))
{
    await CalendarioPresentacion.CargarAsync(app.Services, app.Environment);
    return;
}
if (args.Contains("--cargar-presentacion"))
{
    await DatosPresentacion.CargarAsync(app.Services, app.Environment);
    return;
}
if (args.Contains("--revisar-presentacion"))
{
    await DatosPresentacion.RevisarAsync(app.Services);
    return;
}
if (args.Contains("--cargar-parametros-prueba"))
{
    await ParametrosPrueba.CargarAsync(app.Services, app.Environment);
    return;
}
if (args.Contains("--cargar-parametros-nacionales"))
{
    await ParametrosNacionales.CargarAsync(app.Services, app.Environment);
    return;
}
if (args.Contains("--revisar-p003") || args.Contains("--corregir-p003"))
{
    await DatosEjemplo.CorregirPozo003Async(app.Services, app.Environment, args.Contains("--corregir-p003"));
    return;
}
if (args.Contains("--revisar-datos-demo") || args.Contains("--ajustar-datos-demo"))
{
    await DatosEjemplo.AjustarAsync(app.Services, app.Environment, args.Contains("--ajustar-datos-demo"));
    return;
}
if (args.Contains("--cargar-parametros-tesis"))
{
    await ParametrosEjemploTesis.CargarAsync(app.Services, app.Environment);
    return;
}
if (args.Contains("--cargar-ejemplos"))
{
    await DatosEjemplo.CargarAsync(app.Services, app.Environment);
    return;
}
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
