using MarriageCalculator.Core.Models;

namespace MarriageCalculator.Repositories;

/// <summary>
/// MarriageGame repository implementation using API service
/// </summary>
public class MarriageGameRepository : IMarriageGameRepository
{
    private readonly IApiService _apiService;

    public MarriageGameRepository(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<List<MarriageGame>> GetAllGamesAsync()
    {
        var games = await _apiService.GetAsync<List<MarriageGame>>("api/marriagegames");
        return games ?? new List<MarriageGame>();
    }

    public async Task<List<MarriageGame>> GetGamesByRoundIdAsync(int roundId)
    {
        var games = await _apiService.GetAsync<List<MarriageGame>>($"api/marriagegames/round/{roundId}");
        return games ?? new List<MarriageGame>();
    }

    public async Task<MarriageGame?> GetGameByIdAsync(int id)
    {
        return await _apiService.GetAsync<MarriageGame>($"api/marriagegames/{id}");
    }

    public async Task<MarriageGame> CreateGameAsync(MarriageGame game)
    {
        var createDto = new
        {
            Sequence = game.Sequence,
            MarriageGameRoundId = game.MarriageGameRoundId,
            WinnerId = game.WinnerId,
            DealerId = game.DealerId,
            TotalMaal = game.TotalMaal,
            ClosedRound = game.ClosedRound
        };
        
        var result = await _apiService.PostAsync<MarriageGame>("api/marriagegames", createDto);
        return result ?? throw new Exception("Failed to create marriage game");
    }

    public async Task<MarriageGame?> UpdateGameAsync(MarriageGame game)
    {
        var updateDto = new
        {
            Sequence = game.Sequence,
            MarriageGameRoundId = game.MarriageGameRoundId,
            WinnerId = game.WinnerId,
            DealerId = game.DealerId,
            TotalMaal = game.TotalMaal,
            ClosedRound = game.ClosedRound
        };
        
        return await _apiService.PutAsync<MarriageGame>($"api/marriagegames/{game.Id}", updateDto);
    }

    public async Task<bool> DeleteGameAsync(int id)
    {
        return await _apiService.DeleteAsync($"api/marriagegames/{id}");
    }
}

/// <summary>
/// MarriageGameRound repository implementation using API service
/// Note: This assumes the API has round endpoints (you may need to add these to the API)
/// </summary>
public class MarriageGameRoundRepository : IMarriageGameRoundRepository
{
    private readonly IApiService _apiService;

    public MarriageGameRoundRepository(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<List<MarriageGameRound>> GetRoundsByGameSetIdAsync(int gameSetId)
    {
        // Note: You may need to add this endpoint to your API
        var rounds = await _apiService.GetAsync<List<MarriageGameRound>>($"api/marriagegamerounds/gameset/{gameSetId}");
        return rounds ?? new List<MarriageGameRound>();
    }

    public async Task<MarriageGameRound?> GetRoundByIdAsync(int id)
    {
        return await _apiService.GetAsync<MarriageGameRound>($"api/marriagegamerounds/{id}");
    }

    public async Task<MarriageGameRound> CreateRoundAsync(MarriageGameRound round)
    {
        var createDto = new
        {
            Sequence = round.Sequence,
            MarriageGameSetId = round.MarriageGameSetId,
            Completed = round.Completed
        };
        
        var result = await _apiService.PostAsync<MarriageGameRound>("api/marriagegamerounds", createDto);
        return result ?? throw new Exception("Failed to create marriage game round");
    }

    public async Task<MarriageGameRound?> UpdateRoundAsync(MarriageGameRound round)
    {
        var updateDto = new
        {
            Sequence = round.Sequence,
            MarriageGameSetId = round.MarriageGameSetId,
            Completed = round.Completed
        };
        
        return await _apiService.PutAsync<MarriageGameRound>($"api/marriagegamerounds/{round.Id}", updateDto);
    }

    public async Task<bool> DeleteRoundAsync(int id)
    {
        return await _apiService.DeleteAsync($"api/marriagegamerounds/{id}");
    }
}

/// <summary>
/// MarriageGameScore repository implementation using API service
/// Note: This assumes the API has score endpoints (you may need to add these to the API)
/// </summary>
public class MarriageGameScoreRepository : IMarriageGameScoreRepository
{
    private readonly IApiService _apiService;

    public MarriageGameScoreRepository(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<List<MarriageGameScore>> GetScoresByGameIdAsync(int gameId)
    {
        var scores = await _apiService.GetAsync<List<MarriageGameScore>>($"api/marriagegamescores/game/{gameId}");
        return scores ?? new List<MarriageGameScore>();
    }

    public async Task<MarriageGameScore?> GetScoreByIdAsync(int id)
    {
        return await _apiService.GetAsync<MarriageGameScore>($"api/marriagegamescores/{id}");
    }

    public async Task<MarriageGameScore> CreateScoreAsync(MarriageGameScore score)
    {
        var createDto = new
        {
            MarriageGameId = score.MarriageGameId,
            PlayerId = score.PlayerId,
            Seen = score.Seen,
            Playing = score.Playing,
            Maal = score.Maal,
            BonusPoint = score.BonusPoint,
            Duply = score.Duply,
            Winner = score.Winner,
            Score = score.Score,
            MoneyWon = score.MoneyWon,
            Deal = score.Deal,
            Position = score.Position
        };
        
        var result = await _apiService.PostAsync<MarriageGameScore>("api/marriagegamescores", createDto);
        return result ?? throw new Exception("Failed to create marriage game score");
    }

    public async Task<MarriageGameScore?> UpdateScoreAsync(MarriageGameScore score)
    {
        var updateDto = new
        {
            MarriageGameId = score.MarriageGameId,
            PlayerId = score.PlayerId,
            Seen = score.Seen,
            Playing = score.Playing,
            Maal = score.Maal,
            BonusPoint = score.BonusPoint,
            Duply = score.Duply,
            Winner = score.Winner,
            Score = score.Score,
            MoneyWon = score.MoneyWon,
            Deal = score.Deal,
            Position = score.Position
        };
        
        return await _apiService.PutAsync<MarriageGameScore>($"api/marriagegamescores/{score.Id}", updateDto);
    }

    public async Task<bool> DeleteScoreAsync(int id)
    {
        return await _apiService.DeleteAsync($"api/marriagegamescores/{id}");
    }
}

/// <summary>
/// MarriageGameSetPlayer repository implementation using API service
/// Note: This assumes the API has gamesetplayer endpoints (you may need to add these to the API)
/// </summary>
public class MarriageGameSetPlayerRepository : IMarriageGameSetPlayerRepository
{
    private readonly IApiService _apiService;

    public MarriageGameSetPlayerRepository(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<List<MarriageGameSetPlayer>> GetPlayersByGameSetIdAsync(int gameSetId)
    {
        var players = await _apiService.GetAsync<List<MarriageGameSetPlayer>>($"api/marriagegamesetplayers/gameset/{gameSetId}");
        return players ?? new List<MarriageGameSetPlayer>();
    }

    public async Task<MarriageGameSetPlayer?> GetGameSetPlayerByIdAsync(int id)
    {
        return await _apiService.GetAsync<MarriageGameSetPlayer>($"api/marriagegamesetplayers/{id}");
    }

    public async Task<MarriageGameSetPlayer> CreateGameSetPlayerAsync(MarriageGameSetPlayer gameSetPlayer)
    {
        var createDto = new
        {
            MarriageGameSetId = gameSetPlayer.MarriageGameSetId,
            PlayerId = gameSetPlayer.PlayerId
        };
        
        var result = await _apiService.PostAsync<MarriageGameSetPlayer>("api/marriagegamesetplayers", createDto);
        return result ?? throw new Exception("Failed to create game set player");
    }

    public async Task<bool> DeleteGameSetPlayerAsync(int id)
    {
        return await _apiService.DeleteAsync($"api/marriagegamesetplayers/{id}");
    }
}

/// <summary>
/// Database repository implementation using API service
/// </summary>
public class DatabaseRepository : IDatabaseRepository
{
    private readonly IApiService _apiService;

    public DatabaseRepository(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<bool> TestConnectionAsync()
    {
        return await _apiService.TestConnectionAsync();
    }

    public async Task SeedDefaultDataAsync()
    {
        await _apiService.PostAsync<object>("api/database/seed", new { });
    }

    public async Task CleanupDatabaseAsync()
    {
        await _apiService.DeleteAsync("api/database/cleanup");
    }
}