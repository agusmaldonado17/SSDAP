using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSDAP.Api.Models
{
    [Table("analisis_calidad")]
    public class AnalisisCalidad
    {
        [Key]
        [Column("id_analisis")]
        public int IdAnalisis { get; set; }

        [Column("id_fuente")]
        public int IdFuente { get; set; }

        [Column("fecha")]
        public DateOnly Fecha { get; set; }

        [Column("residuo_seco", TypeName = "decimal(10,2)")]
        public decimal ResiduoSeco { get; set; }

        [Column("conductividad", TypeName = "decimal(10,2)")]
        public decimal Conductividad { get; set; }

        [Column("ph", TypeName = "decimal(4,2)")]
        public decimal Ph { get; set; }

        [Column("cloruro", TypeName = "decimal(10,2)")]
        public decimal Cloruro { get; set; }

        [Column("sulfato", TypeName = "decimal(10,2)")]
        public decimal Sulfato { get; set; }

        [Column("alcalinidad_total", TypeName = "decimal(10,2)")]
        public decimal AlcalinidadTotal { get; set; }

        [Column("carbonato", TypeName = "decimal(10,2)")]
        public decimal Carbonato { get; set; }

        [Column("bicarbonato", TypeName = "decimal(10,2)")]
        public decimal Bicarbonato { get; set; }

        [Column("dureza_total", TypeName = "decimal(10,2)")]
        public decimal DurezaTotal { get; set; }

        [Column("calcio", TypeName = "decimal(10,2)")]
        public decimal Calcio { get; set; }

        [Column("magnesio", TypeName = "decimal(10,2)")]
        public decimal Magnesio { get; set; }

        [Column("hierro", TypeName = "decimal(10,2)")]
        public decimal Hierro { get; set; }

        [Column("nitrato", TypeName = "decimal(10,2)")]
        public decimal Nitrato { get; set; }

        [Column("nitrito", TypeName = "decimal(10,2)")]
        public decimal Nitrito { get; set; }

        [Column("amonio", TypeName = "decimal(10,2)")]
        public decimal Amonio { get; set; }

        [Column("fluor", TypeName = "decimal(10,2)")]
        public decimal Fluor { get; set; }

        [Column("arsenico", TypeName = "decimal(10,2)")]
        public decimal Arsenico { get; set; }

        [Column("sodio", TypeName = "decimal(10,2)")]
        public decimal Sodio { get; set; }

        [Column("potasio", TypeName = "decimal(10,2)")]
        public decimal Potasio { get; set; }

        [Column("ras", TypeName = "decimal(10,2)")]
        public decimal Ras { get; set; }

        [MaxLength(255)]
        [Column("observaciones")]
        public string? Observaciones { get; set; }

        [ForeignKey("IdFuente")]
        public FuenteAgua FuenteAgua { get; set; } = null!;
    }
}