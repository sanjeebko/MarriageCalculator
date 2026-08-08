using MarriageCalculator.Core.DTOs;

namespace MarriageCalculator.API.Services;

public interface IAuthService
{
    Task<SendVerificationCodeResultDto> SendVerificationCodeAsync(string email);
    Task<AuthTokenResultDto> RegisterAsync(RegisterUserDto dto);
    Task<AuthTokenResultDto> LoginAsync(LoginDto dto);
}
