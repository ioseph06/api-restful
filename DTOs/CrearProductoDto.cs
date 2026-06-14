using System.ComponentModel.DataAnnotations;

namespace MiPrimeraApi.DTOs
{
    public class CrearProductoDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(0.01, 100000, ErrorMessage = "El precio debe estar entre 0.01 y 100,000")]
        public decimal Precio { get; set; }

        public bool EnStock { get; set; } = true;
    }
}