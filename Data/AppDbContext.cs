// Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using MiPrimeraApi.Models;

namespace MiPrimeraApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSet representa una tabla en la base de datos
        public DbSet<Producto> Productos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Opcional: Configurar nombres exactos de tablas o relaciones
            // Por defecto, EF Core creará una tabla llamada "Productos"
        }
    }
}