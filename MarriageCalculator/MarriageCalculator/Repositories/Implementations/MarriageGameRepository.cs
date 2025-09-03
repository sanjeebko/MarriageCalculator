using MarriageCalculator.Repositories.Interfaces;
using MarriageCalculator.Services.Interfaces;

namespace MarriageCalculator.Repositories.Implementations;

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
            game.Sequence,
            game.MarriageGameRoundId,
            game.WinnerId,
            game.DealerId,
            game.TotalMaal,
            game.ClosedRound
        };
        
        var result = await _apiService.PostAsync<MarriageGame>("api/marriagegames", createDto);
        return result ?? throw new Exception("Failed to create marriage game");
    }

    public async Task<MarriageGame?> UpdateGameAsync(MarriageGame game)
    {
        var updateDto = new
        {
             game.Sequence,
            game.MarriageGameRoundId,
            game.WinnerId,
            game.DealerId,
            game.TotalMaal,
            game.ClosedRound
        };
        
        return await _apiService.PutAsync<MarriageGame>($"api/marriagegames/{game.Id}", updateDto);
    }

    public async Task<bool> DeleteGameAsync(int id)
    {
        return await _apiService.DeleteAsync($"api/marriagegames/{id}");
    }
}
