using MarriageCalculator.Core.DTOs;

namespace MarriageCalculator.API.Services;

public interface IPlayerService
{
    Task<IEnumerable<PlayerDto>> GetAllPlayersAsync();
    Task<PlayerDto?> GetPlayerByIdAsync(string id);
    Task<PlayerDto> CreatePlayerAsync(CreatePlayerDto createPlayerDto);
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
    Task<FriendshipDto> SendFriendRequestAsync(string requesterUserId, SendFriendRequestDto requestDto);
    Task<FriendshipDto?> RespondFriendRequestAsync(string id, string receiverUserId, RespondFriendRequestDto respondDto);
    Task<bool> RemoveFriendAsync(string id, string userId);
}