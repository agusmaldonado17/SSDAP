using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSDAP.Api.Models
{
    [Table("rol")]
    public class Rol
    {
        [Key]
        [Column("id_rol")]
        public int IdRol { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(150)]
        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [MaxLength(30)]
        [Column("codigo")]
        public string? Codigo { get; set; }

        [Column("activo")]
        public bool Activo { get; set; }
    }
}