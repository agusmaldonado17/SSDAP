using Microsoft.AspNetCore.Components;
using SSDAP.Shared.DTOs.Localidades;
namespace SSDAP.Mobile.Services;
public abstract class PanelMovil : ComponentBase
{
    [Inject] protected SesionMovil Sesion { get; set; } = default!;
    [Parameter, EditorRequired] public LocalidadDto Localidad { get; set; } = default!;
    protected string Ruta => $"api/localidades/{Localidad.IdLocalidad}";
    protected bool Ocupado { get; private set; }
    protected string? Error, Mensaje;
    protected async Task EjecutarAsync(Func<Task> accion)
    {
        if(Ocupado) return;
        Ocupado=true; Error=null; Mensaje=null;
        try { await accion(); }
        catch(ErrorMovil ex) { Error=ex.Message; }
        catch(Exception ex) when(ex is HttpRequestException or TaskCanceledException) { Error="No se pudo confirmar la operación. Comprobá la conexión y actualizá antes de repetir un guardado."; }
        finally { Ocupado=false; }
    }
}
