using MarriageCalculator.API.Repositories.Interfaces;
using MarriageCalculator.Core.DTOs;
using MarriageCalculator.Core.Models;
using System.Security.Cryptography;
using MarriageCalculator.API.Services.Interfaces;

namespace MarriageCalculator.API.Services.Implementations;

public class UserAuthService : IUserAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserEmailVerificationRepository _verificationRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IUserPlayerService _userPlayerService;
    private readonly IGameSettingsService _gameSettingsService;
    private readonly ILogger<UserAuthService> _logger;

    public UserAuthService(
        IUserRepository userRepository,
        IUserEmailVerificationRepository verificationRepository,
        IPasswordService passwordService,
        IJwtService jwtService,
        IEmailService emailService,
        IRefreshTokenService refreshTokenService,
        IUserPlayerService userPlayerService,
        IGameSettingsService gameSettingsService,
        ILogger<UserAuthService> logger)
    {
        _userRepository = userRepository;
        _verificationRepository = verificationRepository;
        _passwordService = passwordService;
        _jwtService = jwtService;
        _emailService = emailService;
        _refreshTokenService = refreshTokenService;
        _userPlayerService = userPlayerService;
        _gameSettingsService = gameSettingsService;
        _logger = logger;
    }

    public async Task<ApiResponse<UserDto>> RegisterUserAsync(RegisterUserDto registerDto)
    {
        try
        {
            _logger.LogInformation("Starting user registration for email: {Email}", registerDto.Email);

            // Validate password strength
            if (!_passwordService.ValidatePasswordStrength(registerDto.Password))
            {
                _logger.LogWarning("Password validation failed for email: {Email}", registerDto.Email);
                return new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "Password must be at least 8 characters long with at least one capital letter and one number or symbol."
                };
            }

            // Check if user already exists
            _logger.LogDebug("Checking if user exists for email: {Email}", registerDto.Email);
            if (await _userRepository.ExistsByEmailAsync(registerDto.Email))
            {
                _logger.LogWarning("Registration attempt for existing email: {Email}", registerDto.Email);
                return new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "A user with this email address already exists."
                };
            }

            // Hash password
            _logger.LogDebug("Hashing password for email: {Email}", registerDto.Email);
            var passwordHash = _passwordService.HashPassword(registerDto.Password, out string salt);

            // Create user
            _logger.LogDebug("Creating user entity for email: {Email}", registerDto.Email);
            var user = new User
            {
                DisplayName = registerDto.DisplayName.Trim(),
                Email = registerDto.Email.Trim().ToLower(),
                PasswordHash = passwordHash,
                Salt = salt,
                IsEmailVerified = false,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _logger.LogDebug("Saving user to database for email: {Email}", registerDto.Email);
            var createdUser = await _userRepository.CreateAsync(user);
            _logger.LogInformation("User created successfully with ID: {UserId} for email: {Email}", createdUser.Id, registerDto.Email);

            // Generate verification code
            _logger.LogDebug("Generating verification code for user: {UserId}", createdUser.Id);
            var verificationCode = GenerateVerificationCode();
            var verification = new UserEmailVerification
            {
                UserId = createdUser.Id,
                VerificationCode = verificationCode,
                ExpiresAt = DateTime.UtcNow.AddHours(2), // 2 hours expiration
                CreatedAt = DateTime.UtcNow
            };

            _logger.LogDebug("Saving verification code for user: {UserId}", createdUser.Id);
            await _verificationRepository.CreateAsync(verification);

            // Send verification email
            _logger.LogDebug("Sending verification email to: {Email}", createdUser.Email);
            var emailSent = await _emailService.SendVerificationEmailAsync(
                createdUser.Email,
                createdUser.DisplayName,
                verificationCode);

            if (!emailSent)
            {
                _logger.LogWarning("Failed to send verification email to {Email}", createdUser.Email);
            }

            // Map to DTO
            var userDto = new UserDto
            {
                Id = createdUser.Id,
                DisplayName = createdUser.DisplayName,
                Email = createdUser.Email,
                IsEmailVerified = createdUser.IsEmailVerified,
                CreatedAt = createdUser.CreatedAt,
                LastLoginAt = createdUser.LastLoginAt,
                IsActive = createdUser.IsActive
            };

            _logger.LogInformation("User registration completed successfully for email: {Email}", registerDto.Email);
            return new ApiResponse<UserDto>
            {
                Success = true,
                Message = "User registered successfully. Please check your email for verification code.",
                Data = userDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Detailed error during user registration for email: {Email}. Exception Type: {ExceptionType}, Message: {ExceptionMessage}, StackTrace: {StackTrace}", 
                registerDto.Email, ex.GetType().Name, ex.Message, ex.StackTrace);
            
            // Return more specific error information in development
            var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
            
            return new ApiResponse<UserDto>
            {
                Success = false,
                Message = isDevelopment 
                    ? $"Registration error: {ex.Message} (Type: {ex.GetType().Name})"
                    : "An error occurred during registration. Please try again."
            };
        }
    }

    public async Task<ApiResponse<LoginResponseDto>> LoginUserAsync(LoginUserDto loginDto)
    {
        try
        {
            // Find user by email
            var user = await _userRepository.GetByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return new ApiResponse<LoginResponseDto>
                {
                    Success = false,
                    Message = "Invalid email or password."
                };
            }

            // Verify password
            if (!_passwordService.VerifyPassword(loginDto.Password, user.PasswordHash, user.Salt))
            {
                return new ApiResponse<LoginResponseDto>
                {
                    Success = false,
                    Message = "Invalid email or password."
                };
            }

            // Generate JWT token (even for unverified users)
            var token = _jwtService.GenerateToken(user);
            var jwtDto = _jwtService.CreateJwtTokenDto(user);

            // Generate refresh token
            var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user.Id);

            // Map to DTO
            var userDto = new UserDto
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Email = user.Email,
                IsEmailVerified = user.IsEmailVerified,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                IsActive = user.IsActive
            };

            var loginResponse = new LoginResponseDto
            {
                Token = token,
                Expires = jwtDto.Expires,
                RefreshToken = refreshToken.Token,
                RefreshTokenExpires = refreshToken.ExpiresAt,
                User = userDto
            };

            // If email is not verified, return success but with appropriate message
            if (!user.IsEmailVerified)
            {
                return new ApiResponse<LoginResponseDto>
                {
                    Success = true,
                    Message = "Login successful, but email verification is required.",
                    Data = loginResponse
                };
            }

            // Update last login for verified users
            await _userRepository.UpdateLastLoginAsync(user.Id);
            userDto.LastLoginAt = DateTime.UtcNow;

            return new ApiResponse<LoginResponseDto>
            {
                Success = true,
                Message = "Login successful.",
                Data = loginResponse
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user login for email: {Email}", loginDto.Email);
            return new ApiResponse<LoginResponseDto>
            {
                Success = false,
                Message = "An error occurred during login. Please try again."
            };
        }
    }

    public async Task<ApiResponse> VerifyEmailAsync(VerifyEmailDto verifyDto)
    {
        try
        {
            // Find user by email
            var user = await _userRepository.GetByEmailAsync(verifyDto.Email);
            if (user == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            // Check if already verified
            if (user.IsEmailVerified)
            {
                return new ApiResponse
                {
                    Success = true,
                    Message = "Email is already verified."
                };
            }

            // Find valid verification code
            var verification = await _verificationRepository.GetValidVerificationAsync(user.Id, verifyDto.VerificationCode);
            if (verification == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Invalid or expired verification code."
                };
            }

            // Mark verification as used
            await _verificationRepository.MarkAsUsedAsync(verification.Id);

            // Update user email verification status
            user.IsEmailVerified = true;
            await _userRepository.UpdateAsync(user.Id, user);

            // ? CREATE DEFAULT GAME SETTINGS AFTER EMAIL VERIFICATION
            try
            {
                var defaultGameSettings = GameSettings.Default(user.Id);
                await _gameSettingsService.CreateGameSettingsAsync(new CreateGameSettingsDto
                {
                    Murder = defaultGameSettings.Murder,
                    Kidnap = defaultGameSettings.Kidnap,
                    SeenPoint = defaultGameSettings.SeenPoint,
                    UnseenPoint = defaultGameSettings.UnseenPoint,
                    PointRate = defaultGameSettings.PointRate,
                    Currency = defaultGameSettings.Currency,
                    Dublee = defaultGameSettings.Dublee,
                    DubleePointLess = defaultGameSettings.DubleePointLess,
                    DubleePointBonus = defaultGameSettings.DubleePointBonus,
                    FoulPoint = defaultGameSettings.FoulPoint,
                    FoulPointBonus = defaultGameSettings.FoulPointBonus,
                    Audio = defaultGameSettings.Audio
                }, user.Id); // Pass the user ID
                
                _logger.LogInformation("Created default GameSettings for verified user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create default GameSettings for verified user {UserId}", user.Id);
                // Continue with verification success even if GameSettings creation fails
            }

            // ? CREATE INITIAL PLAYERS (1 + 4) AFTER EMAIL VERIFICATION
            try
            {
                await _userPlayerService.CreateDefaultPlayerForUserAsync(user);
                _logger.LogInformation("Created initial players for verified user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create initial players for verified user {UserId}", user.Id);
                // Continue with verification success even if player creation fails
            }

            return new ApiResponse
            {
                Success = true,
                Message = "Email verified successfully. Your account is now ready to use!"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during email verification for email: {Email}", verifyDto.Email);
            return new ApiResponse
            {
                Success = false,
                Message = "An error occurred during email verification. Please try again."
            };
        }
    }

    public async Task<ApiResponse> ResendVerificationCodeAsync(ResendVerificationDto resendDto)
    {
        try
        {
            // Find user by email
            var user = await _userRepository.GetByEmailAsync(resendDto.Email);
            if (user == null)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            // Check if already verified
            if (user.IsEmailVerified)
            {
                return new ApiResponse
                {
                    Success = true,
                    Message = "Email is already verified."
                };
            }

            // Delete existing verification codes for this user
            await _verificationRepository.DeleteByUserIdAsync(user.Id);

            // Generate new verification code
            var verificationCode = GenerateVerificationCode();
            var verification = new UserEmailVerification
            {
                UserId = user.Id,
                VerificationCode = verificationCode,
                ExpiresAt = DateTime.UtcNow.AddHours(2), // 2 hours expiration
                CreatedAt = DateTime.UtcNow
            };

            await _verificationRepository.CreateAsync(verification);

            // Send verification email
            var emailSent = await _emailService.SendVerificationEmailAsync(
                user.Email,
                user.DisplayName,
                verificationCode);

            if (!emailSent)
            {
                _logger.LogWarning("Failed to send verification email to {Email}", user.Email);
                return new ApiResponse
                {
                    Success = false,
                    Message = "Failed to send verification email. Please try again."
                };
            }

            return new ApiResponse
            {
                Success = true,
                Message = "Verification code sent successfully. Please check your email."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during resend verification for email: {Email}", resendDto.Email);
            return new ApiResponse
            {
                Success = false,
                Message = "An error occurred while resending verification code. Please try again."
            };
        }
    }

    public async Task<ApiResponse> LogoutUserAsync(string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Invalid token."
                };
            }

            // Blacklist the token
            _jwtService.BlacklistToken(token);

            return new ApiResponse
            {
                Success = true,
                Message = "Logged out successfully."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user logout");
            return new ApiResponse
            {
                Success = false,
                Message = "An error occurred during logout."
            };
        }
    }

    private static string GenerateVerificationCode()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[3]; // 3 bytes for 5 digits
        rng.GetBytes(bytes);
        
        // Convert to integer and ensure it's 5 digits
        var code = (BitConverter.ToUInt32(bytes.Concat(new byte[] { 0 }).ToArray(), 0) % 90000) + 10000;
        return code.ToString("D5");
    }
}