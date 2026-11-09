using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using SSDAP.Shared.DTOs.Autenticacion;

namespace SSDAP.Web.Services;

// Una instancia por circuito; nunca compartir credenciales entre usuarios.
public sealed class SesionWeb(IHttpClientFactory clients, ProtectedSessionStorage storage)
{
    private const string Clave = "ssdap.sesion";
    private readonly SemaphoreSlim mutex = new(1, 1);
    public LoginRespuestaDto? Actual { get; private set; }
    public bool Vigente => Actual is not null && Actual.ExpiraEn > DateTimeOffset.UtcNow;

    public async Task RestaurarAsync()
    {
        try { Actual = (await storage.GetAsync<LoginRespuestaDto>(Clave)).Value; }
        catch (CryptographicException) { await LimpiarAsync(); }
        if (!Vigente) await LimpiarAsync();
    }

    public async Task IngresarAsync(LoginDto datos)
    {
        using var respuesta = await clients.CreateClient("Api").PostAsJsonAsync("api/autenticacion/login", datos);
        await ComprobarAsync(respuesta);
        await GuardarAsync((await respuesta.Content.ReadFromJsonAsync<LoginRespuestaDto>())!);
    }

    public async Task<T> ObtenerAsync<T>(string ruta)
    {
        using var solicitud = CrearSolicitud(HttpMethod.Get, ruta);
        using var respuesta = await clients.CreateClient("Api").SendAsync(solicitud);
        await ComprobarAsync(respuesta);
        return (await respuesta.Content.ReadFromJsonAsync<T>())!;
    }

    // Para recursos cuya ausencia es un estado esperado del formulario, no un error.
    public async Task<T?> ObtenerOpcionalAsync<T>(string ruta) where T : class
    {
        using var solicitud = CrearSolicitud(HttpMethod.Get, ruta);
        using var respuesta = await clients.CreateClient("Api").SendAsync(solicitud);
        if (respuesta.StatusCode == HttpStatusCode.NotFound) return null;
        await ComprobarAsync(respuesta);
        return await respuesta.Content.ReadFromJsonAsync<T>();
    }

    public async Task EnviarAsync<T>(HttpMethod metodo, string ruta, T datos)
    {
        using var solicitud = CrearSolicitud(metodo, ruta);
        solicitud.Content = JsonContent.Create(datos);
        using var respuesta = await clients.CreateClient("Api").SendAsync(solicitud);
        await ComprobarAsync(respuesta);
    }

    public Task<TRespuesta> ConsultarAsync<TSolicitud, TRespuesta>(string ruta, TSolicitud datos) =>
        EnviarAsync<TSolicitud, TRespuesta>(HttpMethod.Post, ruta, datos);

    public async Task<TRespuesta> EnviarAsync<TSolicitud, TRespuesta>(HttpMethod metodo, string ruta, TSolicitud datos)
    {
        using var solicitud = CrearSolicitud(metodo, ruta);
        solicitud.Content = JsonContent.Create(datos);
        using var respuesta = await clients.CreateClient("Api").SendAsync(solicitud);
        await ComprobarAsync(respuesta);
        return (await respuesta.Content.ReadFromJsonAsync<TRespuesta>())!;
    }

    public async Task ActividadAsync()
    {
        await mutex.WaitAsync();
        try
        {
            if (!Vigente) { await LimpiarAsync(); return; }
            using var solicitud = CrearSolicitud(HttpMethod.Post, "api/autenticacion/actividad");
            using var respuesta = await clients.CreateClient("Api").SendAsync(solicitud);
            await ComprobarAsync(respuesta);
            await GuardarAsync((await respuesta.Content.ReadFromJsonAsync<LoginRespuestaDto>())!);
        }
        finally { mutex.Release(); }
    }

    public async Task CerrarAsync()
    {
        await mutex.WaitAsync();
        try
        {
            if (!Vigente) return;
            using var solicitud = CrearSolicitud(HttpMethod.Post, "api/autenticacion/logout");
            using var respuesta = await clients.CreateClient("Api").SendAsync(solicitud);
            if (respuesta.StatusCode != HttpStatusCode.Unauthorized) await ComprobarAsync(respuesta);
        }
        finally { await LimpiarAsync(); mutex.Release(); }
    }

    public async Task LimpiarAsync()
    {
        Actual = null;
        await storage.DeleteAsync(Clave);
    }

    private HttpRequestMessage CrearSolicitud(HttpMethod metodo, string ruta)
    {
        if (!Vigente) throw new ErrorApi(401, "Tu sesión venció. Volvé a ingresar.");
        var solicitud = new HttpRequestMessage(metodo, ruta);
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Actual!.AccessToken);
        return solicitud;
    }

    private async Task GuardarAsync(LoginRespuestaDto datos)
    {
        Actual = datos;
        await storage.SetAsync(Clave, datos);
    }

    private async Task ComprobarAsync(HttpResponseMessage respuesta)
    {
        if (respuesta.IsSuccessStatusCode) return;
        var mensaje = respuesta.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Credenciales incorrectas o sesión vencida. Volvé a ingresar.",
            HttpStatusCode.Forbidden => "No tenés permiso para realizar esta operación.",
            HttpStatusCode.TooManyRequests => "Demasiados intentos. Esperá un minuto antes de volver a intentar.",
            _ => "No se pudo completar la operación. Volvé a intentar."
        };
        if (respuesta.StatusCode == HttpStatusCode.Unauthorized) await LimpiarAsync();
        if ((int)respuesta.StatusCode is 400 or 409)
        {
            try
            {
                using var json = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
                if (json.RootElement.TryGetProperty("title", out var titulo)) mensaje = titulo.GetString() ?? mensaje;
                if (json.RootElement.TryGetProperty("detail", out var detalle)) mensaje = detalle.GetString() ?? mensaje;
                if (json.RootElement.TryGetProperty("errors", out var errores))
                    mensaje = string.Join(" ", errores.EnumerateObject().SelectMany(e => e.Value.EnumerateArray()).Select(e => e.GetString()));
            }
            catch (JsonException) { }
        }
        throw new ErrorApi((int)respuesta.StatusCode, mensaje);
    }
}

public sealed class ErrorApi(int estado, string mensaje) : Exception(mensaje)
{
    public int Estado { get; } = estado;
}
