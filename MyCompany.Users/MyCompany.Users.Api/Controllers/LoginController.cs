using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MyCompany.Users.Application.Services;
using MyCompany.Users.Application.DTOs;
using MyCompany.Users.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using MyCompany.Users.Application.Interfaces;
using MyCompany.Shared.DTOs;

namespace MyCompany.Users.API.Controllers
{
    [ApiController]
    [Route("api/auths")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService userService, IConfiguration config, ILogger<AuthController> logger)
        {
            _userService = userService;
            _config = config;
            _logger = logger;
        }

        // ======================
        // Login - accès public
        // ======================
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            _logger.LogInformation("Tentative de login pour {Email}", dto.Email);

            var user = await _userService.GetUserByEmailAsync(dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            {
                _logger.LogWarning("Login échoué pour {Email}", dto.Email);
                return Unauthorized(new { message = "Email ou mot de passe incorrect" });
            }

            var token = GenerateJwtToken(user);

            _logger.LogInformation("Login réussi pour {Email}", dto.Email);

            return Ok(new LoginResponseDto
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow
           .AddHours(_config.GetValue<int>("Jwt:ExpireHours"))
           .ToLocalTime() // 🔹 affichage pour l’utilisateur
            });
        }

        // ======================
        // Génération du JWT
        // ======================
        private string GenerateJwtToken(User user)
        {
            var jwtKey = _config["Jwt:Key"]!;
            var jwtIssuer = _config["Jwt:Issuer"]!;
            var jwtAudience = _config["Jwt:Audience"]!;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Name, user.Email) // optionnel mais utile
    };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(
                    _config.GetValue<int>("Jwt:ExpireHours")
                ),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        // ======================
        // Endpoint protégé
        // ======================
        [AllowAnonymous]
        [HttpGet("profile")]
        public IActionResult Profile()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Ok(new { id, email });
        }
    }
}
