using MarriageCalculator.Core.DTOs;
using MarriageCalculator.Core.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MarriageCalculator.API.Services.Interfaces;

namespace MarriageCalculator.API.Services.Implementations;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtService> _logger;
    private readonly HashSet<string> _blacklistedTokens;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;

    public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _blacklistedTokens = new HashSet<string>();
        
        _secretKey = _configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");
        _issuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured");
        _audience = _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience is not configured");
        _expirationMinutes = _configuration.GetValue<int>("Jwt:ExpirationMinutes", 60); // Default 1 hour
    }

    public string GenerateToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secretKey);
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.DisplayName),
            new(ClaimTypes.Email, user.Email),
            new("email_verified", user.IsEmailVerified.ToString().ToLower()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_expirationMinutes),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string? ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("Token validation failed: Token is null or empty");
            return null;
        }

        if (IsTokenBlacklisted(token))
        {
            _logger.LogWarning("Token validation failed: Token is blacklisted");
            return null;
        }

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            // Use the exact same order and configuration as Program.cs
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero, // Remove default 5-minute clock skew
                ValidIssuer = _issuer,
                ValidAudience = _audience,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };

            tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;

            // Safely get user ID - try multiple possible claim types
            var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier) 
                            ?? jwtToken.Claims.FirstOrDefault(x => x.Type == "nameid")
                            ?? jwtToken.Claims.FirstOrDefault(x => x.Type == "sub")
                            ?? jwtToken.Claims.FirstOrDefault(x => x.Type == "user_id");

            if (userIdClaim == null)
            {
                _logger.LogError("No user ID claim found in token. Available claim types: {ClaimTypes}", 
                    string.Join(", ", jwtToken.Claims.Select(c => c.Type)));
                return null;
            }

            var userId = userIdClaim.Value;
            _logger.LogDebug("Token validation successful for user: {UserId}", userId);
            return userId;
        }
        catch (SecurityTokenExpiredException ex)
        {
            _logger.LogError(ex, "JWT Token expired: {Message}", ex.Message);
            return null;
        }
        catch (SecurityTokenInvalidSignatureException ex)
        {
            _logger.LogError(ex, "JWT Token signature invalid: {Message}", ex.Message);
            return null;
        }
        catch (SecurityTokenValidationException ex)
        {
            _logger.LogError(ex, "JWT Security Token validation failed: {Message}", ex.Message);
            return null;
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "JWT Argument validation failed: {Message}", ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "JWT Token validation failed with unexpected error: {ErrorType} - {Message}", ex.GetType().Name, ex.Message);
            return null;
        }
    }

    public bool IsTokenBlacklisted(string token)
    {
        return _blacklistedTokens.Contains(token);
    }

    public void BlacklistToken(string token)
    {
        if (!string.IsNullOrWhiteSpace(token))
        {
            _blacklistedTokens.Add(token);
            _logger.LogInformation("Token blacklisted successfully");
        }
    }

    public JwtTokenDto CreateJwtTokenDto(User user)
    {
        var token = GenerateToken(user);
        var expires = DateTime.UtcNow.AddMinutes(_expirationMinutes);

        return new JwtTokenDto
        {
            Token = token,
            Expires = expires,
            TokenType = "Bearer"
        };
    }
}