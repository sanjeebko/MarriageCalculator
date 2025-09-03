using MarriageCalculator.Core.DTOs;
using MarriageCalculator.Core.Models;

namespace MarriageCalculator.API.Services.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
    string? ValidateToken(string token);
    bool IsTokenBlacklisted(string token);
    void BlacklistToken(string token);
    JwtTokenDto CreateJwtTokenDto(User user);
}