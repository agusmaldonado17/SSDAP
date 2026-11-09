using Microsoft.AspNetCore.Components;
using SSDAP.Shared.DTOs.Localidades;

namespace SSDAP.Web.Services;

public abstract class PanelLocalidadBase : PanelApiBase
{
    [Parameter, EditorRequired] public LocalidadDto Localidad { get; set; } = default!;
    protected string RutaLocalidad => $"api/localidades/{Localidad.IdLocalidad}";
}
