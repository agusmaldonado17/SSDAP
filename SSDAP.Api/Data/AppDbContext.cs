using Microsoft.EntityFrameworkCore;
using SSDAP.Api.Models;

namespace SSDAP.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        //Registro de clases de models para la base de datos
        public DbSet<Localidad> Localidades { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<ParametroLey> ParametrosLey { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<UsuarioLocalidad> UsuariosLocalidades { get; set; }
        public DbSet<FuenteAgua> FuentesAgua { get; set; }
        public DbSet<Pozo> Pozos { get; set; }
        public DbSet<Acueducto> Acueductos { get; set; }
        public DbSet<Bateria> Baterias { get; set; }
        public DbSet<BateriaPozo> BateriasPozos { get; set; }
        public DbSet<ProgramacionBateria> ProgramacionesBateria { get; set; }
        public DbSet<AnalisisCalidad> AnalisisCalidad { get; set; }
        public DbSet<ResultadoMensual> ResultadosMensuales { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

    }
}
