using SSDAP.Web.Components;
using SSDAP.Web.Services;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

var builder = WebApplication.CreateBuilder(args);
// La licencia de evaluación permite probar la integración; configurar la licencia aplicable antes de producción.
var licenciaPdf = builder.Configuration["Pdf:Licencia"] ?? "Evaluation";
if (!Enum.TryParse<QuestPDF.Infrastructure.LicenseType>(licenciaPdf, true, out var licencia))
    throw new InvalidOperationException("Pdf:Licencia no es válida.");
QuestPDF.Settings.License = licencia;

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<SesionWeb>();
builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"]
        ?? throw new InvalidOperationException("Configurar Api:BaseUrl."));
    client.Timeout = TimeSpan.FromSeconds(20);
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
