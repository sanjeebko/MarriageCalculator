namespace MarriageCalculator.Core.DTOs;

public class PlayerDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhotoUri { get; set; }
    public string? CreatedByUserId { get; set; }
    public bool Deleted { get; set; }
}

public class CreatePlayerDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhotoUri { get; set; }
}

public class UpdatePlayerDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhotoUri { get; set; }
}

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FcmToken { get; set; }
    public string? PhotoUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RegisterFcmTokenDto
{
    public string Token { get; set; } = string.Empty;
}

public class CreateUserDto
{
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
}

public class UpdateUserDto
{
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
}

public class GameSettingsDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public bool Murder { get; set; }
    public bool Kidnap { get; set; }
    public int SeenPoint { get; set; }
    public int UnseenPoint { get; set; }
    public double PointRate { get; set; }
    public string Currency { get; set; } = string.Empty;
    public bool Dublee { get; set; }
    public bool DubleePointLess { get; set; }
    public int DubleePointBonus { get; set; }
    public int FoulPoint { get; set; }
    public string FoulPointBonus { get; set; } = string.Empty;
    public bool Audio { get; set; }
}

public class CreateGameSettingsDto
{
    public string UserId { get; set; } = string.Empty;
    public bool Murder { get; set; }
    public bool Kidnap { get; set; }
    public int SeenPoint { get; set; }
    public int UnseenPoint { get; set; }
    public double PointRate { get; set; }
    public string Currency { get; set; } = string.Empty;
    public bool Dublee { get; set; }
    public bool DubleePointLess { get; set; }
    public int DubleePointBonus { get; set; }
    public int FoulPoint { get; set; }
    public string FoulPointBonus { get; set; } = string.Empty;
    public bool Audio { get; set; }
}

public class MarriageGameSetDto
{
    public string Id { get; set; } = string.Empty;
    public string HostUserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime LastPlayed { get; set; }
    public DateTime Created { get; set; }
    public bool IsActive { get; set; }
    public string GameSettingsId { get; set; } = string.Empty;
    public List<string> PlayerIds { get; set; } = [];
    public GameSettingsDto? GameSettings { get; set; }
    public Dictionary<string, MarriageGameSetPlayerDto>? GameSetPlayers { get; set; }
    public List<MarriageGameRoundDto>? Rounds { get; set; }
}

public class CreateMarriageGameSetDto
{
    public string HostUserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string GameSettingsId { get; set; } = string.Empty;
    public List<string> PlayerIds { get; set; } = [];
}

public class MarriageGameDto
{
    public string Id { get; set; } = string.Empty;
    public int Sequence { get; set; }
    public string MarriageGameRoundId { get; set; } = string.Empty;
    public string WinnerId { get; set; } = string.Empty;
    public string DealerId { get; set; } = string.Empty;
    public int TotalMaal { get; set; }
    public bool ClosedRound { get; set; }
    public DateTime CreatedTime { get; set; }
    public Dictionary<string, MarriageGameScoreDto>? MarriageGameScores { get; set; }
}

public class CreateMarriageGameDto
{
    public int Sequence { get; set; }
    public string MarriageGameRoundId { get; set; } = string.Empty;
    public string WinnerId { get; set; } = string.Empty;
    public string DealerId { get; set; } = string.Empty;
    public int TotalMaal { get; set; }
    public bool ClosedRound { get; set; }
}

public class MarriageGameRoundDto
{
    public string Id { get; set; } = string.Empty;
    public int Sequence { get; set; }
    public string MarriageGameSetId { get; set; } = string.Empty;
    public bool Completed { get; set; }
    public List<string> PlayerIds { get; set; } = [];
    public List<MarriageGameDto>? MarriageGames { get; set; }
    public Dictionary<string, double>? TotalScore { get; set; }
}

public class CreateMarriageGameRoundDto
{
    public int Sequence { get; set; }
    public string MarriageGameSetId { get; set; } = string.Empty;
    public bool Completed { get; set; }
}

public class MarriageGameScoreDto
{
    public string Id { get; set; } = string.Empty;
    public string MarriageGameId { get; set; } = string.Empty;
    public string PlayerId { get; set; } = string.Empty;
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
    public string MarriageGameId { get; set; } = string.Empty;
    public string PlayerId { get; set; } = string.Empty;
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
    public string Id { get; set; } = string.Empty;
    public string MarriageGameSetId { get; set; } = string.Empty;
    public string PlayerId { get; set; } = string.Empty;
    public int Position { get; set; }
    public bool IsActive { get; set; } = true;
    public PlayerDto? Player { get; set; }
}

public class CreateMarriageGameSetPlayerDto
{
    public string MarriageGameSetId { get; set; } = string.Empty;
    public string PlayerId { get; set; } = string.Empty;
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

// Scoring DTOs
public class CalculateScoreRequestDto
{
    public GameSettingsDto Settings { get; set; } = new();
    public string WinnerId { get; set; } = string.Empty;
    public List<PlayerScoreInputDto> PlayerScores { get; set; } = [];
}

public class PlayerScoreInputDto
{
    public string PlayerId { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public bool Seen { get; set; }
    public bool Playing { get; set; } = true;
    public int Maal { get; set; }
    public bool Duply { get; set; }
}

public class CalculateScoreResponseDto
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<PlayerScoreResultDto> Results { get; set; } = [];
    public int TotalMaal { get; set; }
}

public class PlayerScoreResultDto
{
    public string PlayerId { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public bool Seen { get; set; }
    public bool Winner { get; set; }
    public bool Duply { get; set; }
    public int Maal { get; set; }
    public int Score { get; set; }
    public double MoneyWon { get; set; }
}

public class FriendshipDto
{
    public string Id { get; set; } = string.Empty;
    public string RequesterUserId { get; set; } = string.Empty;
    public string RequesterName { get; set; } = string.Empty;
    public string RequesterEmail { get; set; } = string.Empty;
    public string ReceiverUserId { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    public string ReceiverEmail { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class SendFriendRequestDto
{
    public string ReceiverEmailOrUsername { get; set; } = string.Empty;
}

public class RespondFriendRequestDto
{
    public bool Accept { get; set; }
}

/// <summary>My shareable friend invite code (requirement §4.4).</summary>
public class InviteCodeDto
{
    public string Code { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class RedeemInviteCodeDto
{
    public string Code { get; set; } = string.Empty;
}

/// <summary>Result of redeeming an invite code: instant, auto-accepted friendship.</summary>
public class RedeemInviteCodeResultDto
{
    /// <summary>e.g. "Code correct! You are now friends with Sanjeeb (s***@g***.com)."</summary>
    public string Message { get; set; } = string.Empty;
    public FriendshipDto? Friendship { get; set; }
}

/// <summary>
/// Result of a complete-email friend request. Anti-enumeration: Message is identical
/// whether the email belongs to a registered user or an invitation email was sent.
/// </summary>
public class FriendRequestResultDto
{
    /// <summary>"RequestSent" | "AutoAccepted" — never reveals unregistered emails.</summary>
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    /// <summary>Set only when a Friendship document exists for the caller (registered receiver).</summary>
    public FriendshipDto? Friendship { get; set; }
}

/// <summary>Result of claiming pending email invites after login.</summary>
public class ClaimInvitesResultDto
{
    /// <summary>Number of invites converted into pending friend requests.</summary>
    public int Claimed { get; set; }
}

public class TransferHostDto
{
    public string NewHostUserId { get; set; } = string.Empty;
}

public class SubmitRoundDto
{
    public string WinnerId { get; set; } = string.Empty;
    public string DealerId { get; set; } = string.Empty;
    public List<RoundPlayerInputDto> Players { get; set; } = [];
}

public class RoundPlayerInputDto
{
    public string PlayerId { get; set; } = string.Empty;
    public bool Seen { get; set; }
    public bool Duply { get; set; }
    public int Maal { get; set; }
}

public class SendVerificationCodeRequestDto
{
    public string Email { get; set; } = string.Empty;
}

public class SendVerificationCodeResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class RegisterUserDto
{
    public string Email { get; set; } = string.Empty;
    public string VerificationCode { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

public class LoginDto
{
    public string UsernameOrEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthTokenResultDto
{
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}