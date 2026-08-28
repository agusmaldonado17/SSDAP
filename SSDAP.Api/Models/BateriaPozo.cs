using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSDAP.Api.Models
{
    [Table("bateria_pozo")]
    public class BateriaPozo
    {
        [Key]
        [Column("id_bateria_pozo")]
        public int IdBateriaPozo { get; set; }

        [Column("id_bateria")]
        public int IdBateria { get; set; }

        [Column("id_pozo")]
        public int IdPozo { get; set; }

        [ForeignKey("IdBateria")]
        public Bateria Bateria { get; set; } = null!;

        [ForeignKey("IdPozo")]
        public Pozo Pozo { get; set; } = null!;
    }
}