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
    }
}