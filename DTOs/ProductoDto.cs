// DTOs/ProductoDto.cs
namespace MiPrimeraApi.DTOs
{
    public class ProductoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public bool EnStock { get; set; }
        
        // Campos adicionales que podrías querer exponer solo en lecturas
        public decimal PrecioConIVA => Math.Round(Precio * 1.16m, 2); // Ejemplo: cálculo derivado
    }
}