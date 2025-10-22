using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.DTOs.Authentication;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;

namespace WebApi.Services
{
    public class AccessManagerService
    {
        private readonly EtiDbContext _context;         
        private readonly IConfiguration _config;        
        public AccessManagerService(EtiDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<TokenResponse> AuthenticateAsync(string username, string password)
        {
            // Buscar credencial en la base de datos
            var user = await _context.ApplicationCredentials
                .FirstOrDefaultAsync(u => u.Username == username && u.Password == password);

            if (user == null)
                throw new Exception("Usuario o contraseña incorrectos");

            // Obtener clave JWT desde configuración
            var key = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(key) || Encoding.UTF8.GetBytes(key).Length < 16)
                throw new Exception("JWT Key no configurada correctamente (mínimo 128 bits)");

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.UTF8.GetBytes(key);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new TokenResponse
            {
                AccessToken = tokenHandler.WriteToken(token),
                TokenType = "Bearer",
                ExpiresIn = "3600"
            };
        }
    }
}
