using Microsoft.AspNetCore.Identity;

namespace MiPrimeraApi.Models
{
    // Heredamos de IdentityUser para obtener todas las propiedades base (Id, Email, PasswordHash, etc.)
    public class ApplicationUser : IdentityUser
    {
        // Aquí puedes agregar campos personalizados en el futuro
        // public string? NombreCompleto { get; set; }
    }
}