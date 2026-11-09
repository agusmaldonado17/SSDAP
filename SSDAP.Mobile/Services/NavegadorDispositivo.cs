namespace SSDAP.Mobile.Services;
public sealed class NavegadorDispositivo : NavegadorExterno
{
    public async Task AbrirAsync(Uri direccion)
    {
        try
        {
            if (!await Launcher.Default.OpenAsync(direccion))
                throw new ErrorMovil(503, "No se pudo abrir el mapa. Comprobá que haya un navegador disponible.");
        }
        catch (FeatureNotSupportedException)
        {
            throw new ErrorMovil(503, "No hay una aplicación disponible para abrir el mapa.");
        }
    }
}
