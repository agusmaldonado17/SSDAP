using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSDAP.Api.Models
{
    [Table("acueducto")]
    public class Acueducto
    {
        [Key]
        [Column("id_acueducto")]
        public int IdAcueducto { get; set; }

        [Column("id_fuente")]
        public int IdFuente { get; set; }

        [ForeignKey("IdFuente")]
        public FuenteAgua FuenteAgua { get; set; } = null!;
    }
}