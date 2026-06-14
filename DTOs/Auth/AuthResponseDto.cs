namespace MiPrimeraApi.DTOs.Auth
{
    public class AuthResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public IEnumerable<string> Roles { get; set; } = new List<string>();
    }
}