using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSDAP.Api.Models
{
    [Table("bateria")]
    public class Bateria
    {
        [Key]
        [Column("id_bateria")]
        public int IdBateria { get; set; }

        [Column("id_localidad")]
        public int IdLocalidad { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(255)]
        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [Column("caudal", TypeName = "decimal(10,2)")]
        public decimal Caudal { get; set; }

        [Column("potencia_kw", TypeName = "decimal(10,2)")]
        public decimal PotenciaKw { get; set; }

        [Required]
        [MaxLength(30)]
        [Column("estado")]
        public string Estado { get; set; } = string.Empty;

        [ForeignKey("IdLocalidad")]
        public Localidad Localidad { get; set; } = null!;
    }
}