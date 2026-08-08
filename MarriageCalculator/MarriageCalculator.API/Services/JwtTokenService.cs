using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MarriageCalculator.Core.Models;
using Microsoft.IdentityModel.Tokens;

namespace MarriageCalculator.API.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private string GetSecretKey() =>
        _configuration["JwtSettings:SecretKey"]
        ?? "MarriageCalculatorSecretKeyForJwtAuthenticationTokenSigning2026";

    private string GetIssuer() =>
        _configuration["JwtSettings:Issuer"] ?? "MarriageCalculatorAPI";

    private string GetAudience() =>
        _configuration["JwtSettings:Audience"] ?? "MarriageCalculatorClient";

    public (string Token, DateTime ExpiresAt) GenerateToken(User user)
    {
        var secretKey = GetSecretKey();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTime.UtcNow.AddDays(7);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId),
            new Claim(ClaimTypes.Name, string.IsNullOrWhiteSpace(user.Username) ? user.UserId : user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("displayName", user.DisplayName ?? string.Empty)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = GetIssuer(),
            Audience = GetAudience(),
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return (tokenHandler.WriteToken(token), expiresAt);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var secretKey = GetSecretKey();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = GetIssuer(),
                ValidateAudience = true,
                ValidAudience = GetAudience(),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            if (validatedToken is JwtSecurityToken jwtToken &&
                jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return principal;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}
