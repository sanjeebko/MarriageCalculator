using System.Security.Cryptography;
using MarriageCalculator.API.Repositories;
using MarriageCalculator.Core.DTOs;
using MarriageCalculator.Core.Models;
using Microsoft.AspNetCore.Identity;

namespace MarriageCalculator.API.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailVerificationCodeRepository _verificationCodeRepository;
    private readonly IEmailService _emailService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthService> _logger;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public AuthService(
        IUserRepository userRepository,
        IEmailVerificationCodeRepository verificationCodeRepository,
        IEmailService emailService,
        IJwtTokenService jwtTokenService,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _verificationCodeRepository = verificationCodeRepository;
        _emailService = emailService;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<SendVerificationCodeResultDto> SendVerificationCodeAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            return new SendVerificationCodeResultDto
            {
                Success = false,
                Message = "Please provide a valid email address."
            };
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();

        // Generate 6-digit OTP code
        var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

        var verificationCode = new EmailVerificationCode
        {
            Email = normalizedEmail,
            Code = code,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false
        };

        await _verificationCodeRepository.CreateCodeAsync(verificationCode);

        // Send email with OTP code
        var subject = "Marriage Calculator - Email Verification Code";
        var body = $@"
            <div style='font-family: Arial, sans-serif; padding: 20px; color: #333;'>
                <h2>Marriage Calculator Verification Code</h2>
                <p>Your 6-digit email verification code is:</p>
                <h1 style='color: #D32F2F; letter-spacing: 4px; font-size: 32px;'>{code}</h1>
                <p>This code will expire in 10 minutes.</p>
                <p>If you did not request this verification code, please ignore this email.</p>
            </div>";

        await _emailService.SendAsync(normalizedEmail, subject, body);

        _logger.LogInformation("Sent verification code to {Email}", normalizedEmail);

        return new SendVerificationCodeResultDto
        {
            Success = true,
            Message = $"Verification code has been sent to {normalizedEmail}."
        };
    }

    public async Task<AuthTokenResultDto> RegisterAsync(RegisterUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.VerificationCode) ||
            string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
        {
            throw new ArgumentException("Email, verification code, username, and password are required.");
        }

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var normalizedUsername = dto.Username.Trim().ToLowerInvariant();

        // Validate verification code
        var validCode = await _verificationCodeRepository.GetLatestValidCodeAsync(normalizedEmail);
        if (validCode == null || validCode.Code != dto.VerificationCode.Trim())
        {
            throw new InvalidOperationException("Invalid or expired verification code.");
        }

        // Check if username is already taken
        var existingUsername = await _userRepository.GetByUsernameAsync(normalizedUsername);
        if (existingUsername != null)
        {
            throw new InvalidOperationException($"Username '{dto.Username}' is already taken.");
        }

        // Check if user with email already exists
        var existingEmail = await _userRepository.GetByEmailAsync(normalizedEmail);
        if (existingEmail != null && !string.IsNullOrWhiteSpace(existingEmail.PasswordHash))
        {
            throw new InvalidOperationException($"An account with email '{dto.Email}' already exists. Please login.");
        }

        // Mark code as used
        await _verificationCodeRepository.MarkCodeAsUsedAsync(validCode.Id);

        // Create or update user
        var user = existingEmail ?? new User
        {
            UserId = $"user_{Guid.NewGuid():N}",
            Email = normalizedEmail,
            CreatedAt = DateTime.UtcNow
        };

        user.Username = normalizedUsername;
        user.DisplayName = string.IsNullOrWhiteSpace(dto.DisplayName) ? dto.Username : dto.DisplayName;
        user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

        if (string.IsNullOrEmpty(user.Id) || existingEmail == null)
        {
            await _userRepository.CreateAsync(user);
        }
        else
        {
            await _userRepository.UpdateAsync(user.Id, user);
        }

        var (token, expiresAt) = _jwtTokenService.GenerateToken(user);

        return new AuthTokenResultDto
        {
            Token = token,
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            DisplayName = user.DisplayName,
            ExpiresAt = expiresAt
        };
    }

    public async Task<AuthTokenResultDto> LoginAsync(LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.UsernameOrEmail) || string.IsNullOrWhiteSpace(dto.Password))
        {
            throw new ArgumentException("Username or email and password are required.");
        }

        var input = dto.UsernameOrEmail.Trim().ToLowerInvariant();

        User? user = null;
        if (input.Contains('@'))
        {
            user = await _userRepository.GetByEmailAsync(input);
        }

        user ??= await _userRepository.GetByUsernameAsync(input);

        if (user == null || string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var (token, expiresAt) = _jwtTokenService.GenerateToken(user);

        return new AuthTokenResultDto
        {
            Token = token,
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            DisplayName = user.DisplayName,
            ExpiresAt = expiresAt
        };
    }
}
