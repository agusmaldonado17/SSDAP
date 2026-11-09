using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using SSDAP.Shared.DTOs.Autenticacion;

namespace SSDAP.Mobile.Services;

public interface AlmacenMovil
{
    Task<string?> LeerAsync(string clave);
    Task GuardarAsync(string clave, string valor);
    void Borrar(string clave);
    string DireccionApi { get; set; }
}

// Un único usuario por instalación. El token nunca se guarda en preferencias sin cifrar.
public sealed class SesionMovil(HttpClient http, AlmacenMovil almacen)
{
    private const string Clave = "ssdap.sesion.v1";
    private readonly SemaphoreSlim mutex = new(1, 1);
    public LoginRespuestaDto? Actual { get; private set; }
    public bool Vigente => Actual is not null && Actual.ExpiraEn > DateTimeOffset.UtcNow;
    public event Action? Invalidada;
    public string DireccionApi => almacen.DireccionApi;
    public static string ValidarDireccion(string valor)
    {
        if (!Uri.TryCreate(valor.Trim(), UriKind.Absolute, out var uri) || uri.Scheme != "https" ||
            !string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment))
            throw new ErrorMovil(400, "Ingresá una dirección HTTPS válida de la API, sin credenciales ni parámetros.");
        return uri.AbsoluteUri.TrimEnd('/') + "/";
    }
    public void Configurar(string direccion)
    {
        if (Actual is not null) throw new ErrorMovil(400, "Cerrá la sesión antes de cambiar el servidor.");
        almacen.DireccionApi = ValidarDireccion(direccion);
    }
    public async Task RestaurarAsync()
    {
        try
        {
            var json = await almacen.LeerAsync(Clave);
            Actual = json is null ? null : JsonSerializer.Deserialize<LoginRespuestaDto>(json);
        }
        catch (Exception ex) when (ex is JsonException or System.Security.Cryptography.CryptographicException) { Limpiar(); }
        if (Actual is not null && !Vigente) Limpiar();
    }
    public async Task IngresarAsync(LoginDto datos)
    {
        await mutex.WaitAsync();
        try
        {
            using var req = Crear(HttpMethod.Post,"api/autenticacion/login", false);
            req.Content = JsonContent.Create(datos);
            using var res = await http.SendAsync(req);
            await ComprobarAsync(res);
            await GuardarAsync((await res.Content.ReadFromJsonAsync<LoginRespuestaDto>())!);
        }
        finally { mutex.Release(); }
    }
    public async Task<T> ObtenerAsync<T>(string ruta)
    {
        using var req = Crear(HttpMethod.Get,ruta);
        using var res = await http.SendAsync(req);
        await ComprobarAsync(res);
        return (await res.Content.ReadFromJsonAsync<T>())!;
    }
    public async Task<T?> OpcionalAsync<T>(string ruta) where T : class
    {
        using var req = Crear(HttpMethod.Get,ruta);
        using var res = await http.SendAsync(req);
        if (res.StatusCode == HttpStatusCode.NotFound) return null;
        await ComprobarAsync(res);
        return await res.Content.ReadFromJsonAsync<T>();
    }
    public async Task EnviarAsync<T>(HttpMethod metodo,string ruta,T datos)
    {
        using var req = Crear(metodo,ruta);
        req.Content = JsonContent.Create(datos);
        using var res = await http.SendAsync(req);
        await ComprobarAsync(res);
    }
    public async Task<List<T>> TodasAsync<T>(string ruta)
    {
        var lista = new List<T>();
        for(var p=1;;p++)
        {
            var lote = await ObtenerAsync<List<T>>($"{ruta}{(ruta.Contains('?') ? '&' : '?')}pagina={p}");
            lista.AddRange(lote);
            if(lote.Count < 50) return lista;
        }
    }
    public async Task ActividadAsync()
    {
        await mutex.WaitAsync();
        try
        {
            if (!Vigente) { Limpiar(); return; }
            using var req = Crear(HttpMethod.Post,"api/autenticacion/actividad");
            using var res = await http.SendAsync(req);
            await ComprobarAsync(res);
            await GuardarAsync((await res.Content.ReadFromJsonAsync<LoginRespuestaDto>())!);
        }
        finally { mutex.Release(); }
    }
    // Volver del segundo plano valida el acceso sin extender el tiempo de inactividad.
    public async Task RevisarAsync()
    {
        if (Actual is null) return;
        if (!Vigente) { Limpiar(); return; }
        await ObtenerAsync<SSDAP.Shared.DTOs.Usuarios.UsuarioDto>("api/mi-cuenta");
    }
    public async Task CerrarAsync()
    {
        await mutex.WaitAsync();
        try
        {
            if (!Vigente) return;
            using var req = Crear(HttpMethod.Post,"api/autenticacion/logout");
            using var res = await http.SendAsync(req);
            if(res.StatusCode != HttpStatusCode.Unauthorized) await ComprobarAsync(res);
        }
        finally { Limpiar(); mutex.Release(); }
    }
    public void Limpiar() { Actual = null; almacen.Borrar(Clave); Invalidada?.Invoke(); }
    private async Task GuardarAsync(LoginRespuestaDto datos)
    {
        await almacen.GuardarAsync(Clave,JsonSerializer.Serialize(datos));
        Actual = datos;
    }
    private HttpRequestMessage Crear(HttpMethod metodo,string ruta,bool autenticado=true)
    {
        if (autenticado && !Vigente) { Limpiar(); throw new ErrorMovil(401,"Tu sesión venció. Volvé a ingresar."); }
        if (Uri.TryCreate(ruta,UriKind.Absolute,out _) || ruta.StartsWith('/')) throw new ArgumentException("Ruta relativa requerida.");
        var req = new HttpRequestMessage(metodo,new Uri(new Uri(ValidarDireccion(almacen.DireccionApi)),ruta));
        if(autenticado) req.Headers.Authorization = new AuthenticationHeaderValue("Bearer",Actual!.AccessToken);
        return req;
    }
    private async Task ComprobarAsync(HttpResponseMessage res)
    {
        if(res.IsSuccessStatusCode) return;
        var mensaje = res.StatusCode switch {
            HttpStatusCode.Unauthorized => "Credenciales incorrectas o sesión vencida.",
            HttpStatusCode.Forbidden => "No tenés acceso a esta operación o localidad.",
            HttpStatusCode.NotFound => "No se encontró el registro solicitado.",
            HttpStatusCode.TooManyRequests => "Demasiados intentos. Esperá antes de reintentar.",
            _ => "No se pudo completar la operación." };
        if(res.StatusCode == HttpStatusCode.Unauthorized) Limpiar();
        if((int)res.StatusCode is 400 or 409)
        {
            try {
                using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
                if(doc.RootElement.TryGetProperty("title",out var title)) mensaje=title.GetString() ?? mensaje;
                if(doc.RootElement.TryGetProperty("errors",out var errors)) mensaje=string.Join(" ",errors.EnumerateObject().SelectMany(p=>p.Value.EnumerateArray()).Select(v=>v.GetString()));
            } catch(JsonException) { }
        }
        throw new ErrorMovil((int)res.StatusCode,mensaje);
    }
}
public sealed class ErrorMovil(int estado,string mensaje) : Exception(mensaje) { public int Estado { get; } = estado; }
