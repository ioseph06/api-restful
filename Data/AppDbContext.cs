// Data/AppDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MiPrimeraApi.Models;

namespace MiPrimeraApi.Data
{
    // Hereda de IdentityDbContext<ApplicationUser> para que el modelo incluya las
    // entidades de Identity (AspNetUsers, AspNetRoles, AspNetUserRoles, etc.).
    // Sin esto, RoleManager/UserManager fallan: "type is not included in the model".
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSet representa una tabla en la base de datos
        public DbSet<Producto> Productos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // IMPRESCINDIBLE: deja que IdentityDbContext configure sus tablas.
            // Si se omite, las tablas AspNet* no se generan en las migraciones.
            base.OnModelCreating(modelBuilder);

            // Aquí va tu configuración propia (relaciones, nombres de tabla, etc.)
        }
    }
}
