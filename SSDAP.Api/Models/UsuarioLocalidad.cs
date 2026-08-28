using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSDAP.Api.Models
{
    [Table("usuario_localidad")]
    public class UsuarioLocalidad
    {
        [Key]
        [Column("id_usuario_localidad")]
        public int IdUsuarioLocalidad { get; set; }

        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("id_localidad")]
        public int IdLocalidad { get; set; }

        [ForeignKey("IdUsuario")]
        public Usuario Usuario { get; set; } = null!;

        [ForeignKey("IdLocalidad")]
        public Localidad Localidad { get; set; } = null!;
    }
}