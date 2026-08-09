using MarriageCalculator.Core.DTOs;

namespace MarriageCalculator.API.Services;

public interface IPlayerService
{
    Task<IEnumerable<PlayerDto>> GetAllPlayersAsync();
    Task<PlayerDto?> GetPlayerByIdAsync(string id);
    Task<IEnumerable<PlayerDto>> GetPlayersByCreatedByAsync(string createdByUserId);
    Task<PlayerDto> CreatePlayerAsync(CreatePlayerDto createPlayerDto, string createdByUserId);
    Task<PlayerDto?> UpdatePlayerAsync(string id, UpdatePlayerDto updatePlayerDto);
    Task<bool> DeletePlayerAsync(string id);
    Task<bool> PlayerExistsAsync(string id);
}

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(string id);
    Task<UserDto?> GetUserByUserIdAsync(string userId);
    Task<IEnumerable<UserDto>> SearchUsersAsync(string query);
    Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
    Task<UserDto?> UpdateUserAsync(string id, UpdateUserDto updateUserDto);
    Task<bool> UpdateFcmTokenAsync(string userId, string fcmToken);
    Task<bool> DeleteUserAsync(string id);
    Task<bool> UserExistsAsync(string id);
    Task<UserDto> GetOrCreateUserFromClaimsAsync(System.Security.Claims.ClaimsPrincipal principal);
}

public interface IGameSettingsService
{
    Task<IEnumerable<GameSettingsDto>> GetAllGameSettingsAsync(string userId);
    Task<GameSettingsDto?> GetGameSettingsByIdAsync(string id, string userId);
    Task<GameSettingsDto> CreateGameSettingsAsync(CreateGameSettingsDto createGameSettingsDto);
    Task<GameSettingsDto?> UpdateGameSettingsAsync(string id, CreateGameSettingsDto updateGameSettingsDto, string userId);
    Task<bool> DeleteGameSettingsAsync(string id, string userId);
    Task<bool> GameSettingsExistsAsync(string id, string userId);
}

public interface IMarriageGameSetService
{
    Task<IEnumerable<MarriageGameSetDto>> GetAllGameSetsAsync(string hostUserId, string email);
    Task<MarriageGameSetDto?> GetGameSetByIdAsync(string id, string hostUserId, string email);
    Task<MarriageGameSetDto> CreateGameSetAsync(CreateMarriageGameSetDto createGameSetDto);
    Task<MarriageGameSetDto?> UpdateGameSetAsync(string id, CreateMarriageGameSetDto updateGameSetDto, string hostUserId);
    Task<bool> DeleteGameSetAsync(string id, string hostUserId);
    Task<bool> GameSetExistsAsync(string id, string hostUserId);
    Task<MarriageGameSetDto?> GetLatestActiveGameSetAsync(string hostUserId);
    Task<MarriageGameSetDto?> TransferHostAsync(string id, string currentHostUserId, string newHostUserId);
    Task<bool> NudgePlayerAsync(string gameSetId, string hostUserId, string playerId);
    Task<MarriageGameRoundDto> SubmitRoundAsync(string gameSetId, string hostUserId, SubmitRoundDto dto);
    Task<MarriageGameRoundDto?> CloseRoundAsync(string gameSetId, string roundId, string hostUserId);
    Task<MarriageGameRoundDto?> TogglePaymentClearedAsync(string gameSetId, string roundId, string hostUserId, bool paymentCleared);
    Task<MarriageGameRoundDto?> DeleteLastGameAsync(string gameSetId, string hostUserId);
    Task<bool> DeleteRoundAsync(string gameSetId, string roundId, string hostUserId);
    Task<MarriageGameRoundDto?> UpdateGameAsync(string gameSetId, string gameId, string hostUserId, SubmitRoundDto dto);
}

public interface IMarriageGameService
{
    Task<IEnumerable<MarriageGameDto>> GetAllGamesAsync();
    Task<MarriageGameDto?> GetGameByIdAsync(string id);
    Task<MarriageGameDto> CreateGameAsync(CreateMarriageGameDto createGameDto);
    Task<MarriageGameDto?> UpdateGameAsync(string id, CreateMarriageGameDto updateGameDto);
    Task<bool> DeleteGameAsync(string id);
    Task<bool> GameExistsAsync(string id);
    Task<IEnumerable<MarriageGameDto>> GetGamesByRoundIdAsync(string roundId);
}

public interface IDatabaseService
{
    Task<DatabaseInfoDto> GetDatabaseInfoAsync();
    Task<ApiResponse> SeedDefaultDataAsync();
    Task<ApiResponse> CleanupDatabaseAsync();
}

public interface IFriendshipService
{
    Task<IEnumerable<FriendshipDto>> GetPendingRequestsAsync(string userId);
    Task<IEnumerable<FriendshipDto>> GetSentRequestsAsync(string userId);
    Task<IEnumerable<UserDto>> GetFriendsAsync(string userId);
    /// <summary>
    /// Complete-email friend request (requirement §4.4). Exact email match only.
    /// Registered receiver → pending request; unknown email → stored invite + invitation
    /// email. Returns the identical generic message in both cases (anti-enumeration).
    /// </summary>
    Task<FriendRequestResultDto> SendFriendRequestAsync(string requesterUserId, SendFriendRequestDto requestDto);
    Task<FriendshipDto?> RespondFriendRequestAsync(string id, string receiverUserId, RespondFriendRequestDto respondDto);
    Task<bool> RemoveFriendAsync(string id, string userId);
}

/// <summary>Invite-code friend discovery + email-invite claiming (requirement §4.4).</summary>
public interface IFriendInviteService
{
    /// <summary>Returns the caller's active invite code, creating one (valid 7 days) if none exists.</summary>
    Task<InviteCodeDto> GetOrCreateInviteCodeAsync(string userId);
    /// <summary>Redeems a code: instant auto-accepted friendship with the code owner. Rate-limited.</summary>
    Task<RedeemInviteCodeResultDto> RedeemInviteCodeAsync(string userId, RedeemInviteCodeDto redeemDto);
    /// <summary>Converts pending email invites addressed to the caller's email into pending friend requests.</summary>
    Task<ClaimInvitesResultDto> ClaimPendingInvitesAsync(string userId);
}
