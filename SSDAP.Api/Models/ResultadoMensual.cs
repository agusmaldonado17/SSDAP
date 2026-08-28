using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSDAP.Api.Models
{
    [Table("resultado_mensual")]
    public class ResultadoMensual
    {
        [Key]
        [Column("id_resultado")]
        public int IdResultado { get; set; }

        [Column("id_localidad")]
        public int IdLocalidad { get; set; }

        [Column("mes")]
        public int Mes { get; set; }

        [Column("anio")]
        public int Anio { get; set; }

        [Column("caudal_total", TypeName = "decimal(10,2)")]
        public decimal CaudalTotal { get; set; }

        [Column("dosis_cloro", TypeName = "decimal(10,2)")]
        public decimal DosisCloro { get; set; }

        [Required]
        [MaxLength(30)]
        [Column("estado_calidad")]
        public string EstadoCalidad { get; set; } = string.Empty;

        [Column("fecha_generacion")]
        public DateOnly FechaGeneracion { get; set; }

        [MaxLength(255)]
        [Column("observaciones")]
        public string? Observaciones { get; set; }

        [ForeignKey("IdLocalidad")]
        public Localidad Localidad { get; set; } = null!;
    }
}