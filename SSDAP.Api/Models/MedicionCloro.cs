namespace SSDAP.Api.Models;

public sealed class MedicionCloro
{
    public int IdMedicionCloro { get; set; }
    public int IdLocalidad { get; set; }
    public DateTime FechaHoraUtc { get; set; }
    public decimal ValorMgL { get; set; }
    public string TipoCloro { get; set; } = "";
    public string? Observaciones { get; set; }
    public DateTime CreadoEnUtc { get; set; }
    public int CreadoPorId { get; set; }
}
