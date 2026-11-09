using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSDAP.Api.Models
{
    [Table("parametro_ley")]
    public class ParametroLey
    {
        [Key]
        [Column("id_parametro_ley")]
        public int IdParametroLey { get; set; }

        [MaxLength(40)]
        [Column("codigo")]
        public string? Codigo { get; set; }

        [MaxLength(255)]
        [Column("referencia")]
        public string? Referencia { get; set; }

        [Column("activo")]
        public bool Activo { get; set; }

        [Column("version")]
        public int Version { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("nombre_parametro")]
        public string NombreParametro { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        [Column("unidad")]
        public string Unidad { get; set; } = string.Empty;

        [Column("valor_minimo", TypeName = "decimal(14,6)")]
        public decimal? ValorMinimo { get; set; }

        [Column("valor_maximo", TypeName = "decimal(14,6)")]
        public decimal? ValorMaximo { get; set; }

        [MaxLength(255)]
        [Column("observaciones")]
        public string? Observaciones { get; set; }
    }
}
