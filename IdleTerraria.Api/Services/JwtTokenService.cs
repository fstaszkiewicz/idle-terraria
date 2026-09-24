using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IdleTerraria.Api.DTOs.Responses;
using IdleTerraria.Api.Entities;
using IdleTerraria.Api.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace IdleTerraria.Api.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtOptions _jwtOptions;

        public JwtTokenService(IOptions<JwtOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions.Value;

            if (string.IsNullOrWhiteSpace(_jwtOptions.Key))
            {
                throw new InvalidOperationException(
                    "Brak konfiguracji Jwt:Key. Ustaw sekret JWT w User Secrets.");
            }

            if (_jwtOptions.Key.Length < 32)
            {
                throw new InvalidOperationException(
                    "Jwt:Key musi mieć co najmniej 32 znaki.");
            }
        }

        public AuthResponse CreateToken(Account account, Player player)
        {
            var expiresAtUtc = DateTime.UtcNow.AddMinutes(
                _jwtOptions.ExpirationMinutes);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
                new("account_id", account.Id.ToString()),
                new("player_id", player.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, account.Email),
                new(ClaimTypes.Name, player.Username)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtOptions.Key));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var jwtToken = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: expiresAtUtc,
                signingCredentials: credentials);

            return new AuthResponse
            {
                Token = new JwtSecurityTokenHandler()
                    .WriteToken(jwtToken),

                ExpiresAtUtc = expiresAtUtc,
                AccountId = account.Id,
                PlayerId = player.Id,
                Username = player.Username
            };
        }
    }
}