using MarriageCalculator.Core.Models;

namespace MarriageCalculator.Repositories;

/// <summary>
/// Repository interface for Player operations
/// </summary>
public interface IPlayerRepository
{
    Task<List<Player>> GetAllPlayersAsync();
    Task<Player?> GetPlayerByIdAsync(int id);
    Task<Player> CreatePlayerAsync(Player player);
    Task<Player?> UpdatePlayerAsync(Player player);
    Task<bool> DeletePlayerAsync(int id);
}

/// <summary>
/// Repository interface for GameSettings operations
/// </summary>
public interface IGameSettingsRepository
{
    Task<List<GameSettings>> GetAllGameSettingsAsync();
    Task<GameSettings?> GetGameSettingsByIdAsync(int id);
    Task<GameSettings?> GetLatestGameSettingsAsync();
    Task<GameSettings> CreateGameSettingsAsync(GameSettings gameSettings);
    Task<GameSettings?> UpdateGameSettingsAsync(GameSettings gameSettings);
    Task<bool> DeleteGameSettingsAsync(int id);
}

/// <summary>
/// Repository interface for MarriageGameSet operations
/// </summary>
public interface IMarriageGameSetRepository
{
    Task<List<MarriageGameSet>> GetAllGameSetsAsync();
    Task<MarriageGameSet?> GetGameSetByIdAsync(int id);
    Task<MarriageGameSet?> GetLatestGameSetAsync();
    Task<MarriageGameSet> CreateGameSetAsync(MarriageGameSet gameSet);
    Task<MarriageGameSet?> UpdateGameSetAsync(MarriageGameSet gameSet);
    Task<bool> DeleteGameSetAsync(int id);
}

/// <summary>
/// Repository interface for MarriageGame operations
/// </summary>
public interface IMarriageGameRepository
{
    Task<List<MarriageGame>> GetAllGamesAsync();
    Task<List<MarriageGame>> GetGamesByRoundIdAsync(int roundId);
    Task<MarriageGame?> GetGameByIdAsync(int id);
    Task<MarriageGame> CreateGameAsync(MarriageGame game);
    Task<MarriageGame?> UpdateGameAsync(MarriageGame game);
    Task<bool> DeleteGameAsync(int id);
}

/// <summary>
/// Repository interface for MarriageGameRound operations
/// </summary>
public interface IMarriageGameRoundRepository
{
    Task<List<MarriageGameRound>> GetRoundsByGameSetIdAsync(int gameSetId);
    Task<MarriageGameRound?> GetRoundByIdAsync(int id);
    Task<MarriageGameRound> CreateRoundAsync(MarriageGameRound round);
    Task<MarriageGameRound?> UpdateRoundAsync(MarriageGameRound round);
    Task<bool> DeleteRoundAsync(int id);
}

/// <summary>
/// Repository interface for MarriageGameScore operations
/// </summary>
public interface IMarriageGameScoreRepository
{
    Task<List<MarriageGameScore>> GetScoresByGameIdAsync(int gameId);
    Task<MarriageGameScore?> GetScoreByIdAsync(int id);
    Task<MarriageGameScore> CreateScoreAsync(MarriageGameScore score);
    Task<MarriageGameScore?> UpdateScoreAsync(MarriageGameScore score);
    Task<bool> DeleteScoreAsync(int id);
}

/// <summary>
/// Repository interface for MarriageGameSetPlayer operations
/// </summary>
public interface IMarriageGameSetPlayerRepository
{
    Task<List<MarriageGameSetPlayer>> GetPlayersByGameSetIdAsync(int gameSetId);
    Task<MarriageGameSetPlayer?> GetGameSetPlayerByIdAsync(int id);
    Task<MarriageGameSetPlayer> CreateGameSetPlayerAsync(MarriageGameSetPlayer gameSetPlayer);
    Task<bool> DeleteGameSetPlayerAsync(int id);
}

/// <summary>
/// Repository interface for Database operations
/// </summary>
public interface IDatabaseRepository
{
    Task<bool> TestConnectionAsync();
    Task SeedDefaultDataAsync();
    Task CleanupDatabaseAsync();
}