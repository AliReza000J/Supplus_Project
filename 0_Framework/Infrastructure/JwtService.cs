using _0_Framework.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace _0_Framework.Infrastructure
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _config;
        public JwtService(IConfiguration config) => _config = config;

        public string GenerateAccessToken(long accountId, string username, long roleId, IEnumerable<int> permissions, int expireMinutes = 15)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, accountId.ToString()),
                new Claim(ClaimTypes.Name, username ?? string.Empty),
                new Claim(ClaimTypes.Role, roleId.ToString())
            };

            if (permissions != null)
            {
                foreach (var p in permissions.Distinct())
                    claims.Add(new Claim("Permission", p.ToString()));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(expireMinutes);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        public DateTime GetExpiration(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            return jwt.ValidTo;
        }

        public ClaimsPrincipal ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;

            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

            try
            {
                var principal = handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidAudience = _config["Jwt:Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out var validatedToken);

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
