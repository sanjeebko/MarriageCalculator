using MarriageCalculator.Core.Models;

namespace MarriageCalculator.API.Services;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user);
    System.Security.Claims.ClaimsPrincipal? ValidateToken(string token);
}
