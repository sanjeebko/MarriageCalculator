using MarriageCalculator.Core.Models;
using MarriageCalculator.Repositories;

namespace MarriageCalculator.Services;

/// <summary>
/// Implementation of IDbService using Repository pattern and API backend
/// This replaces the SQLite-based SqLiteDbService
/// </summary>
public class ApiDbService : IDbService
{
    private readonly IPlayerRepository _playerRepository;
    private readonly IGameSettingsRepository _gameSettingsRepository;
    private readonly IMarriageGameSetRepository _gameSetRepository;
    private readonly IMarriageGameRepository _gameRepository;
    private readonly IMarriageGameRoundRepository _roundRepository;
    private readonly IMarriageGameScoreRepository _scoreRepository;
    private readonly IMarriageGameSetPlayerRepository _gameSetPlayerRepository;
    private readonly IDatabaseRepository _databaseRepository;

    public ApiDbService(
        IPlayerRepository playerRepository,
        IGameSettingsRepository gameSettingsRepository,
        IMarriageGameSetRepository gameSetRepository,
        IMarriageGameRepository gameRepository,
        IMarriageGameRoundRepository roundRepository,
        IMarriageGameScoreRepository scoreRepository,
        IMarriageGameSetPlayerRepository gameSetPlayerRepository,
        IDatabaseRepository databaseRepository)
    {
        _playerRepository = playerRepository;
        _gameSettingsRepository = gameSettingsRepository;
        _gameSetRepository = gameSetRepository;
        _gameRepository = gameRepository;
        _roundRepository = roundRepository;
        _scoreRepository = scoreRepository;
        _gameSetPlayerRepository = gameSetPlayerRepository;
        _databaseRepository = databaseRepository;
    }

    #region Player Operations

    public async Task<int> AddPlayerAsync(Player model)
    {
        var createdPlayer = await _playerRepository.CreatePlayerAsync(model);
        return createdPlayer.Id;
    }

    public async Task<int> DeletePlayerAsync(Player model)
    {
        var success = await _playerRepository.DeletePlayerAsync(model.Id);
        return success ? 1 : 0;
    }

    public async Task<List<Player>> GetPlayersAsync()
    {
        return await _playerRepository.GetAllPlayersAsync();
    }

    #endregion

    #region Game Operations

    public async Task<List<MarriageGame>> GetMarriageGamesAsync()
    {
        return await _gameRepository.GetAllGamesAsync();
    }

    public async Task<int> AddMarriageGameAsync(MarriageGame model)
    {
        var createdGame = await _gameRepository.CreateGameAsync(model);
        return createdGame.Id;
    }

    public async Task<int> DeleteMarriageGameAsync(MarriageGame model)
    {
        var success = await _gameRepository.DeleteGameAsync(model.Id);
        return success ? 1 : 0;
    }

    public async Task<int> UpdateMarriageGameAsync(MarriageGame model)
    {
        var updatedGame = await _gameRepository.UpdateGameAsync(model);
        return updatedGame != null ? 1 : 0;
    }

    public async Task<List<MarriageGame>> GetMarriageGamesByRoundIdAsync(int id)
    {
        return await _gameRepository.GetGamesByRoundIdAsync(id);
    }

    #endregion

    #region GameSet Operations

    public async Task<MarriageGameSet?> GetLatestMarriageGameSetAsync()
    {
        return await _gameSetRepository.GetLatestGameSetAsync();
    }

    public async Task<MarriageGameSet?> AddNewMarriageGameSetAsync(string? name)
    {
        // Create a default game settings first if none exist
        var gameSettings = await GetLastGameSettingsAsync();
        if (gameSettings == null)
        {
            gameSettings = GameSettings.Default();
            await AddGameSettingsAsync(gameSettings);
        }

        var gameSet = new MarriageGameSet
        {
            Name = name ?? $"Game Set {DateTime.Now:yyyy-MM-dd HH:mm}",
            Created = DateTime.Now,
            LastPlayed = DateTime.Now,
            IsActive = true,
            GameSettingsId = gameSettings.Id
        };

        return await _gameSetRepository.CreateGameSetAsync(gameSet);
    }

    public async Task<int> UpdateMarriageGameSetAsync(MarriageGameSet model)
    {
        var updatedGameSet = await _gameSetRepository.UpdateGameSetAsync(model);
        return updatedGameSet != null ? 1 : 0;
    }

    #endregion

    #region Round Operations

    public async Task<int> AddMarriageGameRoundAsync(MarriageGameRound model)
    {
        var createdRound = await _roundRepository.CreateRoundAsync(model);
        return createdRound.Id;
    }

    public async Task UpdateMarriageGameRoundAsync(MarriageGameRound currentMarriageGameRound)
    {
        await _roundRepository.UpdateRoundAsync(currentMarriageGameRound);
    }

    public async Task<List<MarriageGameRound>> GetMarriageGameRoundsByMarriageGameSetIdAsync(int marriageGameSetId)
    {
        return await _roundRepository.GetRoundsByGameSetIdAsync(marriageGameSetId);
    }

    #endregion

    #region GameSetPlayer Operations

    public async Task<MarriageGameSetPlayer> AddMarriageGameSetPlayerAsync(MarriageGameSetPlayer model)
    {
        return await _gameSetPlayerRepository.CreateGameSetPlayerAsync(model);
    }

    public async Task<List<MarriageGameSetPlayer>> GetMarriageGameSetPlayersByMarriageGameSetIdAsync(int id)
    {
        return await _gameSetPlayerRepository.GetPlayersByGameSetIdAsync(id);
    }

    #endregion

    #region GameSettings Operations

    public async Task<GameSettings> AddGameSettingsAsync(GameSettings model)
    {
        return await _gameSettingsRepository.CreateGameSettingsAsync(model);
    }

    public async Task<GameSettings?> GetGameSettingsAsync(int id)
    {
        return await _gameSettingsRepository.GetGameSettingsByIdAsync(id);
    }

    public async Task<Dictionary<int, GameSettings>> GetAllGameSettingsAsync()
    {
        var allSettings = await _gameSettingsRepository.GetAllGameSettingsAsync();
        return allSettings.ToDictionary(s => s.Id, s => s);
    }

    public async Task<GameSettings?> GetLastGameSettingsAsync()
    {
        return await _gameSettingsRepository.GetLatestGameSettingsAsync();
    }

    public async Task<GameSettings?> GetGameSettingsByGameSetIdAsync(int gameSetId)
    {
        var gameSet = await _gameSetRepository.GetGameSetByIdAsync(gameSetId);
        if (gameSet == null) return null;
        
        return await _gameSettingsRepository.GetGameSettingsByIdAsync(gameSet.GameSettingsId);
    }

    #endregion

    #region Score Operations

    public async Task<MarriageGameScore> AddMarriageGameScoreAsync(MarriageGameScore model)
    {
        return await _scoreRepository.CreateScoreAsync(model);
    }

    public async Task<List<MarriageGameScore>> GetMarriageGameScoresByMarriageGameIdAsync(int id)
    {
        return await _scoreRepository.GetScoresByGameIdAsync(id);
    }

    #endregion

    #region Database Operations

    public async Task CleanMarriageGameSet()
    {
        await _databaseRepository.CleanupDatabaseAsync();
    }

    public async Task<bool> TestConnectionAsync()
    {
        return await _databaseRepository.TestConnectionAsync();
    }

    #endregion
}