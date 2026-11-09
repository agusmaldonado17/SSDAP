using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSDAP.Api.Models
{
    [Table("programacion_bateria")]
    public class ProgramacionBateria
    {
        [Key]
        [Column("id_programacion")]
        public int IdProgramacion { get; set; }

        [Column("id_bateria")]
        public int IdBateria { get; set; }

        [Column("fecha")]
        public DateOnly Fecha { get; set; }

        [Column("hora_inicio")]
        public TimeOnly HoraInicio { get; set; }

        [Column("hora_fin")]
        public TimeOnly HoraFin { get; set; }

        [MaxLength(255)]
        [Column("observaciones")]
        public string? Observaciones { get; set; }

        [Column("inicio_utc")]
        public DateTime? InicioUtc { get; set; }

        [Column("fin_utc")]
        public DateTime? FinUtc { get; set; }

        [Required, MaxLength(30), Column("estado")]
        public string Estado { get; set; } = "PendienteRevision";

        [MaxLength(100), Column("nombre_bateria")]
        public string? NombreBateria { get; set; }

        [ConcurrencyCheck, Column("version")]
        public int Version { get; set; }

        [Column("creado_en_utc")]
        public DateTime? CreadoEnUtc { get; set; }

        [Column("modificado_en_utc")]
        public DateTime? ModificadoEnUtc { get; set; }

        [Column("creado_por_id")]
        public int? CreadoPorId { get; set; }

        [Column("modificado_por_id")]
        public int? ModificadoPorId { get; set; }

        public List<ProgramacionPozo> Pozos { get; set; } = [];

        [ForeignKey("IdBateria")]
        public Bateria Bateria { get; set; } = null!;
    }
}
