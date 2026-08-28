using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSDAP.Api.Models
{
    [Table("pozo")]
    public class Pozo
    {
        [Key]
        [Column("id_pozo")]
        public int IdPozo { get; set; }

        [Column("id_fuente")]
        public int IdFuente { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("codigo")]
        public string Codigo { get; set; } = string.Empty;

        [Column("latitud", TypeName = "decimal(10,7)")]
        public decimal Latitud { get; set; }

        [Column("longitud", TypeName = "decimal(10,7)")]
        public decimal Longitud { get; set; }

        [ForeignKey("IdFuente")]
        public FuenteAgua FuenteAgua { get; set; } = null!;
    }
}