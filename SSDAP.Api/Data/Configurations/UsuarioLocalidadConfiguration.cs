using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSDAP.Api.Models;

namespace SSDAP.Api.Data.Configurations;

public sealed class UsuarioLocalidadConfiguration : IEntityTypeConfiguration<UsuarioLocalidad>
{
    public void Configure(EntityTypeBuilder<UsuarioLocalidad> builder)
    {
        builder.HasIndex(asignacion => new { asignacion.IdUsuario, asignacion.IdLocalidad })
            .IsUnique();
    }
}
