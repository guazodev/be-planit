using Microsoft.Extensions.Configuration; 
using Microsoft.IdentityModel.Tokens;
using PlanIT.BusinessLogic.Interfaces; 
using PlanIT.Domain;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PlanIT.Infraestructure.Authentication
{
    public class JwtProvider : IJwtProvider
    {
        private readonly IConfiguration _configuration;

        // Inyectamos IConfiguration para poder leer el appsettings.json
        public JwtProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(User user)
        {
            // 1. Leer appsettings
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)
            );
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 2. Crear los datos del usuario con el token
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // ID del usuario
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // ID único del token
            };

            // 3. Crear el Token
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2), // Duración del token (2 horas)
                signingCredentials: credentials);

            // 4. Escribir el token como un string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}