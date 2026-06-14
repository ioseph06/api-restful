namespace MiPrimeraApi.DTOs
{
    public class ProductoQueryParams
    {
        private const int MaxPageSize = 50;
        private int _pageSize = 10;

        public int PageNumber { get; set; } = 1;
        
        public int PageSize 
        { 
            get => _pageSize; 
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value; 
        }

        public string? Nombre { get; set; }
        public bool? EnStock { get; set; }
        
        // Para ordenamiento: ej. "precio", "nombre"
        public string? OrderBy { get; set; } 
        public bool IsDescending { get; set; } = false;
    }
}