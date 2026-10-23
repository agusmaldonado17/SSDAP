namespace SSDAP.Api.Services;
public sealed class ReglaNegocioException(int estado, string mensaje) : Exception(mensaje)
{
    public int Estado { get; } = estado;
}
