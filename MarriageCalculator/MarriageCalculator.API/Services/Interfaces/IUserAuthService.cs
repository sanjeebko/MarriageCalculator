using MarriageCalculator.Core.DTOs;
using MarriageCalculator.Core.Models;

namespace MarriageCalculator.API.Services.Interfaces;

public interface IUserAuthService
{
    Task<ApiResponse<UserDto>> RegisterUserAsync(RegisterUserDto registerDto);
    Task<ApiResponse<LoginResponseDto>> LoginUserAsync(LoginUserDto loginDto);
    Task<ApiResponse> VerifyEmailAsync(VerifyEmailDto verifyDto);
    Task<ApiResponse> ResendVerificationCodeAsync(ResendVerificationDto resendDto);
    Task<ApiResponse> LogoutUserAsync(string token);
}