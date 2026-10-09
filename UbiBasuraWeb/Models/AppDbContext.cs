using Microsoft.EntityFrameworkCore;

namespace UbiBasuraWeb.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Contenedor> Contenedores { get; set; }
        public DbSet<Reporte> Reportes { get; set; }
    }
}