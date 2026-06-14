namespace MiPrimeraApi.Middlewares
{
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; } // Opcional: para dar más contexto en desarrollo
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}