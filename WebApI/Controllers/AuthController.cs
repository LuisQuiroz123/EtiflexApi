//using Microsoft.AspNetCore.Mvc;
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;
//using WebApi.Data;
//using WebApi.DTOs.Authentication;
//using Microsoft.EntityFrameworkCore;

//namespace WebApi.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class AuthController : ControllerBase
//    {
//        private readonly EtiDbContext _dbContext;
//        private readonly IConfiguration _configuration;

//        public AuthController(EtiDbContext dbContext, IConfiguration configuration)
//        {
//            _dbContext = dbContext;
//            _configuration = configuration;
//        }

//        [HttpPost("login")]
//        public async Task<IActionResult> Login([FromBody] ApplicationCredentials credentials)
//        {
//            if (credentials == null)
//                return BadRequest(new ErrorMessage { Error = "InvalidRequest", ErrorDescription = "No credentials provided" });

//            // Buscar usuario en base de datos
//            var user = await _dbContext.ApplicationCredentials
//                                       .FirstOrDefaultAsync(u => u.Username == credentials.Username
//                                                              && u.Password == credentials.Password);

//            if (user == null)
//                return Unauthorized(new ErrorMessage { Error = "InvalidUser", ErrorDescription = "Usuario o contraseña incorrectos" });

//            // Clave para JWT (asegúrate de que sea al menos 128 bits / 16 bytes)
//            var keyString = _configuration["Jwt:Key"];
//            if (string.IsNullOrEmpty(keyString) || Encoding.UTF8.GetBytes(keyString).Length < 16)
//                return StatusCode(500, new ErrorMessage { Error = "ServerError", ErrorDescription = "JWT Key no configurada correctamente" });

//            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
//            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

//            var token = new JwtSecurityToken(
//                issuer: null,
//                audience: null,
//                claims: new[]
//                {
//                    new Claim(ClaimTypes.Name, user.Username),
//                    new Claim("userId", user.Id.ToString())
//                },
//                expires: DateTime.UtcNow.AddHours(2),
//                signingCredentials: creds
//            );

//            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

//            return Ok(new TokenResponse
//            {
//                AccessToken = jwt,
//                TokenType = "Bearer",
//                ExpiresIn = "7200" // segundos
//            });
//        }
//    }
//}


using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs.Authentication;
using WebApi.Services;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AccessManagerService _accessManager;

        public AuthController(AccessManagerService accessManager)
        {
            _accessManager = accessManager;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] ApplicationCredentialsDto credentials)
        {
            try
            {
                var token = await _accessManager.AuthenticateAsync(credentials.Username, credentials.Password);
                return Ok(token);
            }
            catch (Exception ex)
            {
                // Devuelve mensaje de error claro
                return BadRequest(new
                {
                    error = "LOGIN_FAILED",
                    message = ex.Message
                });
            }
        }
    }
}

