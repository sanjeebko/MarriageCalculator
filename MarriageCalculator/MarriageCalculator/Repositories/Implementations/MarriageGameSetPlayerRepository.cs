using MarriageCalculator.Repositories.Interfaces;
using MarriageCalculator.Services.Interfaces;

namespace MarriageCalculator.Repositories.Implementations;

/// <summary>
/// MarriageGameSetPlayer repository implementation using API service
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
        var players = await _apiService.GetAsync<List<MarriageGameSetPlayer>>($"api/marriagegamesetplayers/gameset/{gameSetId}/players");
        return players ?? new List<MarriageGameSetPlayer>();
    }

    public async Task<MarriageGameSetPlayer?> GetGameSetPlayerByIdAsync(int gameSetId, Guid playerId)
    {
        return await _apiService.GetAsync<MarriageGameSetPlayer>($"api/marriagegamesetplayers/{gameSetId}/{playerId}");
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

    public async Task<bool> DeleteGameSetPlayerAsync(int GameSetId,Guid playerId)
    {
        return await _apiService.DeleteAsync($"api/marriagegamesetplayers/{GameSetId}/{playerId}");
    }
}
