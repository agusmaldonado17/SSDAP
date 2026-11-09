using Microsoft.AspNetCore.Components;

namespace SSDAP.Web.Services;

// Manejo común de operaciones autenticadas y lectura paginada.
public abstract class PanelApiBase : ComponentBase
{
    [Inject] protected SesionWeb Sesion { get; set; } = default!;
    [Parameter] public EventCallback SesionVencida { get; set; }
    protected bool Ocupado { get; private set; }
    protected string? Error { get; set; }
    protected string? Mensaje { get; set; }
    protected async Task EjecutarAsync(Func<Task> accion)
    {
        if (Ocupado) return;
        Ocupado = true; Error = null; Mensaje = null;
        try { await accion(); }
        catch (ErrorApi ex)
        {
            Error = ex.Message;
            if (ex.Estado == 401) await SesionVencida.InvokeAsync();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        { Error = "No se pudo confirmar la operación. Actualizá los datos antes de repetir un guardado."; }
        finally { Ocupado = false; }
    }
    protected async Task<List<T>> LeerPaginasAsync<T>(string ruta)
    {
        var resultado = new List<T>();
        for (var pagina = 1; ; pagina++)
        {
            var lote = await Sesion.ObtenerAsync<List<T>>($"{ruta}{(ruta.Contains('?') ? '&' : '?')}pagina={pagina}");
            resultado.AddRange(lote);
            if (lote.Count < 50) return resultado;
        }
    }
}
