using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSDAP.Api.Models
{
    [Table("fuente_agua")]
    public class FuenteAgua
    {
        [Key]
        [Column("id_fuente")]
        public int IdFuente { get; set; }

        [Column("id_localidad")]
        public int IdLocalidad { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        [Column("tipo_fuente")]
        public string TipoFuente { get; set; } = string.Empty;

        [Column("caudal", TypeName = "decimal(10,2)")]
        public decimal Caudal { get; set; }

        [Required]
        [MaxLength(30)]
        [Column("estado")]
        public string Estado { get; set; } = string.Empty;

        [MaxLength(255)]
        [Column("observaciones")]
        public string? Observaciones { get; set; }

        [ForeignKey("IdLocalidad")]
        public Localidad Localidad { get; set; } = null!;
    }
}