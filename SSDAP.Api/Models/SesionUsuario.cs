namespace SSDAP.Api.Models;

public sealed class SesionUsuario
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public int IdUsuario { get; set; }
    public DateTime CreadaEnUtc { get; set; }
    public DateTime UltimaActividadUtc { get; set; }
    public DateTime ExpiraEnUtc { get; set; }
}
