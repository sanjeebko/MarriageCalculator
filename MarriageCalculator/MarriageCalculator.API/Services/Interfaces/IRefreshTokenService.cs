using MarriageCalculator.Core.DTOs;
using MarriageCalculator.Core.Models;

namespace MarriageCalculator.API.Services.Interfaces;

public interface IRefreshTokenService
{
    Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId);
    Task<ApiResponse<RefreshTokenResponseDto>> RefreshTokenAsync(string refreshToken);
    Task<ApiResponse> RevokeTokenAsync(string refreshToken);
    Task<ApiResponse> RevokeAllUserTokensAsync(Guid userId);
    Task<bool> IsValidRefreshTokenAsync(string refreshToken);
    Task CleanupExpiredTokensAsync();
}