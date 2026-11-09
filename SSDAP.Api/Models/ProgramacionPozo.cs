namespace SSDAP.Api.Models;

public sealed class ProgramacionPozo
{
    public int IdProgramacion { get; set; }
    public int IdPozo { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public decimal CaudalM3H { get; set; }
    public ProgramacionBateria Programacion { get; set; } = null!;
    public Pozo Pozo { get; set; } = null!;
}
