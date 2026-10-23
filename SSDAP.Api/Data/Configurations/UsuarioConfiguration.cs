using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSDAP.Api.Models;
namespace SSDAP.Api.Data.Configurations;
public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.Property(usuario => usuario.EmailNormalizado).HasMaxLength(100).HasColumnName("email_normalizado");
        builder.HasIndex(usuario => usuario.EmailNormalizado).IsUnique();
    }
}
