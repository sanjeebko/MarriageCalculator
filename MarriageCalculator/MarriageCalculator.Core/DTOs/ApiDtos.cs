using System.ComponentModel.DataAnnotations;
using MarriageCalculator.Core.Models;

namespace MarriageCalculator.Core.DTOs;

public class PlayerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Deleted { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreatePlayerDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class UpdatePlayerDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class GameSettingsDto
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public bool Murder { get; set; }
    public bool Kidnap { get; set; }
    public int SeenPoint { get; set; }
    public int UnseenPoint { get; set; }
    public double PointRate { get; set; }
    public Currency Currency { get; set; }
    public bool Dublee { get; set; }
    public bool DubleePointLess { get; set; }
    public int DubleePointBonus { get; set; }
    public int FoulPoint { get; set; }
    public FoulPointBonusType FoulPointBonus { get; set; }
    public bool Audio { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateGameSettingsDto
{
    public bool Murder { get; set; }
    public bool Kidnap { get; set; }
    public int SeenPoint { get; set; }
    public int UnseenPoint { get; set; }
    public double PointRate { get; set; }
    public Currency Currency { get; set; }
    public bool Dublee { get; set; }
    public bool DubleePointLess { get; set; }
    public int DubleePointBonus { get; set; }
    public int FoulPoint { get; set; }
    public FoulPointBonusType FoulPointBonus { get; set; }
    public bool Audio { get; set; }
}

public class MarriageGameSetDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime LastPlayed { get; set; }
    public DateTime Created { get; set; }
    public bool IsActive { get; set; }
    public int GameSettingsId { get; set; }
}

public class CreateMarriageGameSetDto
{
    public string Name { get; set; } = string.Empty;
    public int GameSettingsId { get; set; }
}

public class MarriageGameDto
{
    public int Id { get; set; }
    public int Sequence { get; set; }
    public int MarriageGameRoundId { get; set; }
    public Guid WinnerId { get; set; }
    public Guid DealerId { get; set; }
    public int TotalMaal { get; set; }
    public bool ClosedRound { get; set; }
    public DateTime CreatedTime { get; set; }
}

public class CreateMarriageGameDto
{
    public int Sequence { get; set; }
    public int MarriageGameRoundId { get; set; }
    public Guid WinnerId { get; set; }
    public Guid DealerId { get; set; }
    public int TotalMaal { get; set; }
    public bool ClosedRound { get; set; }
}

public class MarriageGameRoundDto
{
    public int Id { get; set; }
    public int Sequence { get; set; }
    public int MarriageGameSetId { get; set; }
    public bool Completed { get; set; }
}

public class CreateMarriageGameRoundDto
{
    public int Sequence { get; set; }
    public int MarriageGameSetId { get; set; }
    public bool Completed { get; set; }
}

public class MarriageGameScoreDto
{
    public int Id { get; set; }
    public int MarriageGameId { get; set; }
    public Guid PlayerId { get; set; }
    public bool Seen { get; set; }
    public bool Playing { get; set; }
    public int Maal { get; set; }
    public int BonusPoint { get; set; }
    public bool Duply { get; set; }
    public bool Winner { get; set; }
    public int Score { get; set; }
    public double MoneyWon { get; set; }
    public bool Deal { get; set; }
    public int Position { get; set; }
}

public class CreateMarriageGameScoreDto
{
    public int MarriageGameId { get; set; }
    public Guid PlayerId { get; set; }
    public bool Seen { get; set; }
    public bool Playing { get; set; }
    public int Maal { get; set; }
    public int BonusPoint { get; set; }
    public bool Duply { get; set; }
    public bool Winner { get; set; }
    public int Score { get; set; }
    public double MoneyWon { get; set; }
    public bool Deal { get; set; }
    public int Position { get; set; }
}

public class MarriageGameSetPlayerDto
{
    public int MarriageGameSetId { get; set; }
    public Guid PlayerId { get; set; }
    public Player Player { get; set; }
}

public class CreateMarriageGameSetPlayerDto
{
    public int MarriageGameSetId { get; set; }
    public Guid PlayerId { get; set; }
}

public class DatabaseInfoDto
{
    public bool CanConnect { get; set; }
    public string Provider { get; set; } = string.Empty;
    public int TableCount { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}

public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

// User Authentication DTOs
public class UserDto
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; }
}

public class RegisterUserDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string DisplayName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
}

public class LoginUserDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
}

public class VerifyEmailDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [StringLength(5, MinimumLength = 5)]
    public string VerificationCode { get; set; } = string.Empty;
}

public class ResendVerificationDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expires { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpires { get; set; }
    public UserDto User { get; set; } = null!;
}

public class JwtTokenDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expires { get; set; }
    public string TokenType { get; set; } = "Bearer";
}

// Refresh Token DTOs
public class RefreshTokenRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

public class RefreshTokenResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expires { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpires { get; set; }
}

public class RevokeTokenRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

public class ValidateTokenDto
{
    [Required]
    public string Token { get; set; } = string.Empty;
}