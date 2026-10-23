using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using SSDAP.Api.Data;

// Ejecutar desde la raíz del repositorio. Usa una base desechable con nombre generado.
// Requiere permisos CREATE/DROP DATABASE en el servidor de desarrollo configurado.
var root = Directory.GetCurrentDirectory();
var apiPath = Path.Combine(root, "SSDAP.Api");
var configuration = new ConfigurationBuilder().SetBasePath(apiPath)
    .AddJsonFile("appsettings.json").AddJsonFile("appsettings.Development.json", true)
    .AddEnvironmentVariables().Build();
var connection = new MySqlConnectionStringBuilder(configuration.GetConnectionString("DefaultConnection"));
var database = "ssdap_checks_" + Guid.NewGuid().ToString("N");
var serverConnection = new MySqlConnectionStringBuilder(connection.ConnectionString) { Database = "" };
connection.Database = database;
var created = false;
Process? api = null;
var password = Guid.NewGuid().ToString("N") + "aA1!";
var listener = new TcpListener(IPAddress.Loopback, 0);
listener.Start();
var port = ((IPEndPoint)listener.LocalEndpoint).Port;
listener.Stop();
var jwtKey = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(48));
var dll = Path.Combine(apiPath, "bin", "Release", "net10.0", "SSDAP.Api.dll");
Process Start(params string[] arguments)
{
    var info = new ProcessStartInfo("dotnet") { WorkingDirectory = apiPath, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
    info.ArgumentList.Add(dll);
    foreach (var argument in arguments) info.ArgumentList.Add(argument);
    info.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
    info.Environment["ASPNETCORE_URLS"] = $"http://127.0.0.1:{port}";
    info.Environment["ConnectionStrings__DefaultConnection"] = connection.ConnectionString;
    info.Environment["Jwt__Key"] = jwtKey;
    info.Environment["AdministradorInicial__Email"] = "admin@example.invalid";
    info.Environment["AdministradorInicial__Password"] = password;
    return Process.Start(info)!;
}
void Check(bool condition, string name)
{
    if (!condition) throw new InvalidOperationException("FALLO: " + name);
    Console.WriteLine("OK: " + name);
}
try
{
    await using (var server = new MySqlConnection(serverConnection.ConnectionString))
    {
        await server.OpenAsync();
        await new MySqlCommand($"CREATE DATABASE `{database}` CHARACTER SET utf8mb4", server).ExecuteNonQueryAsync();
        created = true;
    }
    await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseMySQL(connection.ConnectionString).Options);
    await db.Database.MigrateAsync();
    using (var initializer = Start("--inicializar-admin"))
    {
        var output = initializer.StandardOutput.ReadToEndAsync();
        var errors = initializer.StandardError.ReadToEndAsync();
        await initializer.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(30));
        await Task.WhenAll(output, errors);
        Check(initializer.ExitCode == 0, "creación explícita del primer administrador");
    }
    using (var initializer = Start("--inicializar-admin"))
    {
        var output = initializer.StandardOutput.ReadToEndAsync();
        var errors = initializer.StandardError.ReadToEndAsync();
        await initializer.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(30));
        await Task.WhenAll(output, errors);
        Check(initializer.ExitCode != 0, "no reinicializar si ya existen usuarios");
    }
    api = Start();
    var apiOutput = api.StandardOutput.ReadToEndAsync();
    var apiErrors = api.StandardError.ReadToEndAsync();
    using var http = new HttpClient { BaseAddress = new Uri($"http://127.0.0.1:{port}"), Timeout = TimeSpan.FromSeconds(10) };
    var ready = false;
    for (var attempt = 0; attempt < 40; attempt++)
    {
        try { ready = (await http.GetAsync("/swagger/index.html")).IsSuccessStatusCode; } catch (HttpRequestException) { }
        if (ready) break;
        await Task.Delay(250);
    }
    Check(ready, "Swagger disponible");
    var openapi = await http.GetStringAsync("/openapi/v1.json");
    Check(openapi.Contains("Bearer") && openapi.Contains("/api/autenticacion/login"), "OpenAPI incluye autenticación");
    Check((await http.GetAsync("/api/roles")).StatusCode == HttpStatusCode.Unauthorized, "operación protegida sin token: 401");
    Check((await http.PostAsJsonAsync("/api/autenticacion/login", new { email = "admin@example.invalid", password = "incorrecta" })).StatusCode == HttpStatusCode.Unauthorized, "contraseña incorrecta: 401");
    async Task<string> Login(string email)
    {
        using var response = await http.PostAsJsonAsync("/api/autenticacion/login", new { email, password });
        Check(response.IsSuccessStatusCode, "login " + email);
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("accessToken").GetString()!;
    }
    var adminToken = await Login("admin@example.invalid");
    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
    var roleResponse = await http.PostAsJsonAsync("/api/roles", new { nombre = "Operador", descripcion = "Prueba", activo = true });
    Check(roleResponse.StatusCode == HttpStatusCode.Created, "alta de rol: 201");
    var roleId = (await roleResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("idRol").GetInt32();
    Check((await http.PostAsJsonAsync("/api/roles", new { nombre = "operador", activo = true })).StatusCode == HttpStatusCode.Conflict, "rol repetido: 409");
    var userData = new { nombre = "Operador", apellido = "Prueba", email = "operador@example.invalid", password, idRol = roleId };
    var userResponse = await http.PostAsJsonAsync("/api/usuarios", userData);
    Check(userResponse.StatusCode == HttpStatusCode.Created, "alta de usuario: 201");
    var userText = await userResponse.Content.ReadAsStringAsync();
    Check(!userText.Contains("password", StringComparison.OrdinalIgnoreCase), "respuesta sin contraseña ni hash");
    var userId = JsonDocument.Parse(userText).RootElement.GetProperty("idUsuario").GetInt32();
    Check((await http.PostAsJsonAsync("/api/usuarios", userData with { email = "OPERADOR@example.invalid" })).StatusCode == HttpStatusCode.Conflict, "email equivalente repetido: 409");
    var localityResponse = await http.PostAsJsonAsync("/api/localidades", new { nombre = "Localidad de prueba", provincia = "La Pampa" });
    Check(localityResponse.StatusCode == HttpStatusCode.Created, "alta de localidad: 201");
    var localityId = (await localityResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("idLocalidad").GetInt32();
    var operatorToken = await Login("operador@example.invalid");
    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", operatorToken);
    Check((await http.GetAsync("/api/usuarios")).StatusCode == HttpStatusCode.Forbidden, "operador sin acceso administrativo: 403");
    Check((await http.GetFromJsonAsync<JsonElement>("/api/mi-cuenta/localidades")).GetArrayLength() == 0, "usuario sin asignaciones no ve localidades");
    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
    var assignment = $"/api/usuarios/{userId}/localidades/{localityId}";
    Check((await http.PutAsync(assignment, null)).StatusCode == HttpStatusCode.NoContent, "asignación: 204");
    Check((await http.PutAsync(assignment, null)).StatusCode == HttpStatusCode.NoContent, "asignación repetida idempotente");
    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", operatorToken);
    Check((await http.GetFromJsonAsync<JsonElement>("/api/mi-cuenta/localidades")).GetArrayLength() == 1, "usuario consulta su localidad asignada");
    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
    Check((await http.DeleteAsync(assignment)).StatusCode == HttpStatusCode.NoContent, "retirar asignación: 204");
    await http.PutAsJsonAsync($"/api/roles/{roleId}", new { nombre = "Operador", activo = false });
    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", operatorToken);
    Check((await http.GetAsync("/api/mi-cuenta")).StatusCode == HttpStatusCode.Unauthorized, "rol desactivado invalida acceso con token previo");
    Check((await http.PostAsJsonAsync("/api/autenticacion/login", new { email = "operador@example.invalid", password })).StatusCode == HttpStatusCode.Unauthorized, "rol desactivado impide login");
    var stored = await db.Usuarios.AsNoTracking().SingleAsync(u => u.IdUsuario == userId);
    Check(stored.PasswordHash != password && !stored.PasswordHash.Contains(password), "contraseña persistida como hash");
    Console.WriteLine("Verificaciones de integración completadas.");
}
finally
{
    if (api is not null)
    {
        if (!api.HasExited) api.Kill(entireProcessTree: true);
        await api.WaitForExitAsync();
        api.Dispose();
    }
    MySqlConnection.ClearAllPools();
    if (created && System.Text.RegularExpressions.Regex.IsMatch(database, "^ssdap_checks_[a-f0-9]{32}$"))
    {
        await using var server = new MySqlConnection(serverConnection.ConnectionString);
        await server.OpenAsync();
        await new MySqlCommand($"DROP DATABASE `{database}`", server).ExecuteNonQueryAsync();
        Console.WriteLine("Base temporal eliminada.");
    }
}
