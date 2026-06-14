using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MiPrimeraApi.Models;
using MiPrimeraApi.DTOs.Auth;

namespace MiPrimeraApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            
            // Identity se encarga de hashear la contraseña de forma segura
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return BadRequest(ModelState);
            }

            // Asignar rol por defecto "User" a todos los nuevos registros
            await _userManager.AddToRoleAsync(user, "User");

            return Ok(new { Message = "Usuario registrado exitosamente. Ahora puedes iniciar sesión." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Unauthorized(new { Message = "Credenciales inválidas" }); // Mensaje genérico por seguridad

            // Verifica la contraseña hasheada y el bloqueo de cuenta
            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                    return Unauthorized(new { Message = "Cuenta bloqueada por demasiados intentos fallidos." });
                
                return Unauthorized(new { Message = "Credenciales inválidas" });
            }

            return Ok(await GenerateJwtToken(user));
        }

        private async Task<AuthResponseDto> GenerateJwtToken(ApplicationUser user)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? "ClaveSuperSecretaDeProduccion1234567890!";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Obtener roles del usuario
            var roles = await _userManager.GetRolesAsync(user);

            // Crear Claims (la identidad dentro del token)
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Agregar cada rol como un Claim separado
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var token = new JwtSecurityToken(
                expires: DateTime.UtcNow.AddHours(2),
                claims: claims,
                signingCredentials: creds
            );

            return new AuthResponseDto
            {
                Id = user.Id,
                Email = user.Email!,
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo,
                Roles = roles
            };
        }
    }
}


//// Controllers/AuthController.cs
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;
//
//namespace MiPrimeraApi.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class AuthController : ControllerBase
//    {
//        private readonly string _claveSecreta = "EstaEsUnaClaveSuperSecretaYMuyLargaParaElEjemplo123!";
//
//        // Modelo simple para recibir el login
//
//        [HttpPost("login")]
//        public IActionResult Login([FromBody] LoginRequest request)
//        {
//            // 1. VALIDAR CREDENCIALES (Simulado)
//            // En la vida real, aquí consultarías tu Base de Datos y verificarías el hash de la contraseña
//            if (request.Usuario != "admin" || request.Password != "1234")            
//                return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos" });
// 
//            // 2. CREAR EL TOKEN JWT
//            var key = Encoding.ASCII.GetBytes(_claveSecreta);
//            var tokenDescriptor = new SecurityTokenDescriptor
//            {
//                Subject = new ClaimsIdentity(new[]
//                {
//                    // Aquí guardamos información del usuario dentro del token (Claims)
//                    new Claim(ClaimTypes.Name, request.Usuario),
//                    new Claim(ClaimTypes.Role, "Administrador") // Le damos un rol
//                }),
//                Expires = DateTime.UtcNow.AddHours(2), // El token expira en 2 horas
//                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
//            };
//
//            var tokenHandler = new JwtSecurityTokenHandler();
//            var token = tokenHandler.CreateToken(tokenDescriptor);
//            var tokenString = tokenHandler.WriteToken(token);
//
//            // 3. DEVOLVER EL TOKEN AL CLIENTE
//            return Ok(new { Token = tokenString });
//        }
//    }
//}
//
//
//        public class LoginRequest
//        {
//            public string Usuario { get; set; } = string.Empty;
//            public string Password { get; set; } = string.Empty;
//        }
