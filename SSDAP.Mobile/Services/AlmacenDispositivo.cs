using System.Text.Json;

namespace SSDAP.Mobile.Services;
public sealed class AlmacenDispositivo : AlmacenMovil
{
    public async Task<string?> LeerAsync(string clave)
    {
        if (clave == "ssdap.googlemaps")
        {
            var configurada = await LeerClaveMapaAsync();
            if (!string.IsNullOrWhiteSpace(configurada)) return configurada;
        }
        try { return await SecureStorage.Default.GetAsync(clave); }
        catch (Exception ex) when (ex is System.Security.Cryptography.CryptographicException or InvalidOperationException)
        { SecureStorage.Default.Remove(clave); return null; }
    }
    private static async Task<string?> LeerClaveMapaAsync()
    {
        try
        {
            using var archivo = await FileSystem.Current.OpenAppPackageFileAsync("maps.local.json");
            using var json = await JsonDocument.ParseAsync(archivo);
            if (json.RootElement.ValueKind == JsonValueKind.Object &&
                json.RootElement.TryGetProperty("GoogleMaps", out var mapas) &&
                mapas.ValueKind == JsonValueKind.Object &&
                mapas.TryGetProperty("ApiKey", out var clave) && clave.ValueKind == JsonValueKind.String)
                return clave.GetString()?.Trim();
            return null;
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            return null;
        }
    }
    public async Task GuardarAsync(string clave,string valor)
    {
        try { await SecureStorage.Default.SetAsync(clave,valor); }
        catch (Exception ex) when (ex is System.Security.Cryptography.CryptographicException or InvalidOperationException)
        { throw new ErrorMovil(503,"No se pudo guardar la sesión de forma segura. Reiniciá la aplicación e intentá nuevamente."); }
    }
    public void Borrar(string clave) => SecureStorage.Default.Remove(clave);
    public string DireccionApi {
        get => Preferences.Default.Get("ssdap.api","https://ssdap-api.runasp.net/");
        set => Preferences.Default.Set("ssdap.api",value);
    }
}
