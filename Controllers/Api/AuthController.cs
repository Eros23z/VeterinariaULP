using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VeterinariaULP.Models;

namespace VeterinariaULP.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _config;

        public AuthController(DataContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public class LoginDto
        {
            public string Email { get; set; } = string.Empty;
            public string Clave { get; set; } = string.Empty;
        }

        // POST: api/Auth/Login
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            if (string.IsNullOrWhiteSpace(login.Email) || string.IsNullOrWhiteSpace(login.Clave))
            {
                return BadRequest(new { mensaje = "Debe enviar email y clave." });
            }

            var usuario = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == login.Email.Trim().ToLower() && u.Clave == login.Clave && u.Estado);

            if (usuario == null)
            {
                return Unauthorized(new { mensaje = "Credenciales incorrectas o usuario inactivo." });
            }

            // Generar Claims del Token
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, usuario.Email),
                new Claim("FullName", $"{usuario.Nombre} {usuario.Apellido}"),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config["TokenAuthentication:SecretKey"]!));
            var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["TokenAuthentication:Issuer"],
                audience: _config["TokenAuthentication:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(4),
                signingCredentials: credenciales
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiracion = token.ValidTo,
                usuario = new
                {
                    usuario.IdUsuario,
                    usuario.Nombre,
                    usuario.Apellido,
                    usuario.Email,
                    usuario.Rol,
                    usuario.Matricula
                }
            });
        }
    }
}