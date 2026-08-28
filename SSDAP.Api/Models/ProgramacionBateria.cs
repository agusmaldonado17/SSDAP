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

        [ForeignKey("IdBateria")]
        public Bateria Bateria { get; set; } = null!;
    }
}