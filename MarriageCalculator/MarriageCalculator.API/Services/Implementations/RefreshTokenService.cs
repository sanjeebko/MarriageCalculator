using MarriageCalculator.API.Repositories.Interfaces;
using MarriageCalculator.Core.DTOs;
using MarriageCalculator.Core.Models;
using System.Security.Cryptography;
using MarriageCalculator.API.Services.Interfaces;

namespace MarriageCalculator.API.Services.Implementations;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RefreshTokenService> _logger;
    private readonly int _refreshTokenExpirationDays;

    public RefreshTokenService(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        IJwtService jwtService,
        IConfiguration configuration,
        ILogger<RefreshTokenService> logger)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _jwtService = jwtService;
        _configuration = configuration;
        _logger = logger;
        _refreshTokenExpirationDays = _configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays", 7); // Default 7 days
    }

    public async Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId)
    {
        // Generate cryptographically secure random token
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        var token = Convert.ToBase64String(randomBytes);

        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        return await _refreshTokenRepository.CreateAsync(refreshToken);
    }

    public async Task<ApiResponse<RefreshTokenResponseDto>> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            // Find the refresh token
            var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
            if (storedToken == null)
            {
                return new ApiResponse<RefreshTokenResponseDto>
                {
                    Success = false,
                    Message = "Invalid refresh token."
                };
            }

            // Validate the refresh token
            if (!storedToken.IsValid)
            {
                var reason = storedToken.IsExpired ? "expired" : "revoked";
                return new ApiResponse<RefreshTokenResponseDto>
                {
                    Success = false,
                    Message = $"Refresh token is {reason}."
                };
            }

            // Get the user
            var user = await _userRepository.GetByIdAsync(storedToken.UserId);
            if (user == null || !user.IsActive)
            {
                return new ApiResponse<RefreshTokenResponseDto>
                {
                    Success = false,
                    Message = "User not found or inactive."
                };
            }

            // Revoke the old refresh token and create a new one (rotation)
            storedToken.RevokedAt = DateTime.UtcNow;
            storedToken.IsActive = false;
            storedToken.ReplacedByToken = null;
            await _refreshTokenRepository.UpdateAsync(storedToken);

            var newToken = await GenerateRefreshTokenAsync(user.Id);
            storedToken.ReplacedByToken = newToken.Token;
            await _refreshTokenRepository.UpdateAsync(storedToken);

            // Generate new access token
            var jwtDto = _jwtService.CreateJwtTokenDto(user);

            return new ApiResponse<RefreshTokenResponseDto>
            {
                Success = true,
                Data = new RefreshTokenResponseDto
                {
                    Token = jwtDto.Token,
                    Expires = jwtDto.Expires,
                    RefreshToken = newToken.Token,
                    RefreshTokenExpires = newToken.ExpiresAt
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return new ApiResponse<RefreshTokenResponseDto>
            {
                Success = false,
                Message = "An error occurred while refreshing the token."
            };
        }
    }

    public async Task<ApiResponse> RevokeTokenAsync(string refreshToken)
    {
        try
        {
            var success = await _refreshTokenRepository.RevokeAsync(refreshToken, "User initiated revoke");
            return new ApiResponse
            {
                Success = success,
                Message = success ? "Token revoked successfully." : "Invalid token or already revoked."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking token");
            return new ApiResponse
            {
                Success = false,
                Message = "An error occurred while revoking the token."
            };
        }
    }

    public async Task<ApiResponse> RevokeAllUserTokensAsync(Guid userId)
    {
        try
        {
            var success = await _refreshTokenRepository.RevokeAllByUserIdAsync(userId, "User logout");
            return new ApiResponse
            {
                Success = success,
                Message = success ? "All tokens revoked successfully." : "No active tokens found for user."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking all tokens for user {UserId}", userId);
            return new ApiResponse
            {
                Success = false,
                Message = "An error occurred while revoking user tokens."
            };
        }
    }

    public async Task<bool> IsValidRefreshTokenAsync(string refreshToken)
    {
        var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
        return token != null && token.IsValid;
    }

    public async Task CleanupExpiredTokensAsync()
    {
        await _refreshTokenRepository.DeleteExpiredAsync();
    }
}