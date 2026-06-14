using System.ComponentModel.DataAnnotations;

namespace MiPrimeraApi.DTOs
{
    public class ActualizarProductoDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 100000)]
        public decimal Precio { get; set; }

        public bool EnStock { get; set; }
    }
}