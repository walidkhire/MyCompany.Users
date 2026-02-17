using System;
using System.Security.Claims; // Indispensable pour FindFirstValue
using System.IdentityModel.Tokens.Jwt; // Pour JwtRegisteredClaimNames
using Microsoft.AspNetCore.Identity;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace MyCompany.Shared.Helpers
{
    public static class JwtHelper
    {
        /// <summary>
        /// Génère un JWT pour un utilisateur
        /// </summary>
        public static string GenerateToken(
            string email,
            string userId,
            string secretKey,
            string issuer,
            string audience,
            int expireHours = 1)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim("id", userId)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expireHours),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Valide un JWT et retourne les claims si valide
        /// </summary>
        public static ClaimsPrincipal? ValidateToken(
            string token,
            string secretKey,
            string issuer,
            string audience)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secretKey);

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                }, out SecurityToken validatedToken);

                return principal;
            }
            catch
            {
                return null; // Token invalide
            }
        }

        /// <summary>
        /// Récupère l’email depuis les claims
        /// </summary>
        public static string? GetEmail(ClaimsPrincipal user) =>
            user.FindFirstValue(JwtRegisteredClaimNames.Sub);

        /// <summary>
        /// Récupère l’ID utilisateur depuis les claims
        /// </summary>
        public static string? GetUserId(ClaimsPrincipal user) =>
            user.FindFirstValue("id");
    }
}
