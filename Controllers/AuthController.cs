// Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MiPrimeraApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly string _claveSecreta = "EstaEsUnaClaveSuperSecretaYMuyLargaParaElEjemplo123!";

        // Modelo simple para recibir el login

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // 1. VALIDAR CREDENCIALES (Simulado)
            // En la vida real, aquí consultarías tu Base de Datos y verificarías el hash de la contraseña
            if (request.Usuario != "admin" || request.Password != "1234")            
                return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos" });
 
            // 2. CREAR EL TOKEN JWT
            var key = Encoding.ASCII.GetBytes(_claveSecreta);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    // Aquí guardamos información del usuario dentro del token (Claims)
                    new Claim(ClaimTypes.Name, request.Usuario),
                    new Claim(ClaimTypes.Role, "Administrador") // Le damos un rol
                }),
                Expires = DateTime.UtcNow.AddHours(2), // El token expira en 2 horas
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // 3. DEVOLVER EL TOKEN AL CLIENTE
            return Ok(new { Token = tokenString });
        }
    }
}


        public class LoginRequest
        {
            public string Usuario { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }
