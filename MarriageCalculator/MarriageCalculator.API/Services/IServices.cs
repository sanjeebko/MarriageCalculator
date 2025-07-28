using MarriageCalculator.API.DTOs;

namespace MarriageCalculator.API.Services;

public interface IPlayerService
{
    Task<IEnumerable<PlayerDto>> GetAllPlayersAsync();
    Task<PlayerDto?> GetPlayerByIdAsync(int id);
    Task<PlayerDto> CreatePlayerAsync(CreatePlayerDto createPlayerDto);
    Task<PlayerDto?> UpdatePlayerAsync(int id, UpdatePlayerDto updatePlayerDto);
    Task<bool> DeletePlayerAsync(int id);
    Task<bool> PlayerExistsAsync(int id);
}

public interface IGameSettingsService
{
    Task<IEnumerable<GameSettingsDto>> GetAllGameSettingsAsync();
    Task<GameSettingsDto?> GetGameSettingsByIdAsync(int id);
    Task<GameSettingsDto> CreateGameSettingsAsync(CreateGameSettingsDto createGameSettingsDto);
    Task<GameSettingsDto?> UpdateGameSettingsAsync(int id, CreateGameSettingsDto updateGameSettingsDto);
    Task<bool> DeleteGameSettingsAsync(int id);
    Task<bool> GameSettingsExistsAsync(int id);
}

public interface IMarriageGameSetService
{
    Task<IEnumerable<MarriageGameSetDto>> GetAllGameSetsAsync();
    Task<MarriageGameSetDto?> GetGameSetByIdAsync(int id);
    Task<MarriageGameSetDto> CreateGameSetAsync(CreateMarriageGameSetDto createGameSetDto);
    Task<MarriageGameSetDto?> UpdateGameSetAsync(int id, CreateMarriageGameSetDto updateGameSetDto);
    Task<bool> DeleteGameSetAsync(int id);
    Task<bool> GameSetExistsAsync(int id);
    Task<MarriageGameSetDto?> GetLatestActiveGameSetAsync();
}

public interface IMarriageGameService
{
    Task<IEnumerable<MarriageGameDto>> GetAllGamesAsync();
    Task<MarriageGameDto?> GetGameByIdAsync(int id);
    Task<MarriageGameDto> CreateGameAsync(CreateMarriageGameDto createGameDto);
    Task<MarriageGameDto?> UpdateGameAsync(int id, CreateMarriageGameDto updateGameDto);
    Task<bool> DeleteGameAsync(int id);
    Task<bool> GameExistsAsync(int id);
    Task<IEnumerable<MarriageGameDto>> GetGamesByRoundIdAsync(int roundId);
}

public interface IDatabaseService
{
    Task<DatabaseInfoDto> GetDatabaseInfoAsync();
    Task<ApiResponse> SeedDefaultDataAsync();
    Task<ApiResponse> CleanupDatabaseAsync();
}