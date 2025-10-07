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
        return players ?? [];
    }

    public async Task<MarriageGameSetPlayer?> GetGameSetPlayerByIdAsync(int gameSetId, Guid playerId)
    {
        return await _apiService.GetAsync<MarriageGameSetPlayer>($"api/marriagegamesetplayers/{gameSetId}/{playerId}");
    }

    public async Task<MarriageGameSetPlayer> CreateGameSetPlayerAsync(MarriageGameSetPlayer gameSetPlayer)
    {
        var createDto = new
        {
            gameSetPlayer.MarriageGameSetId,
            gameSetPlayer.PlayerId
        };
        
        try
        {
            var result = await _apiService.PostAsync<MarriageGameSetPlayer>("api/marriagegamesetplayers", createDto);
            
            if (result != null)
            {
                return result; // 201 Created - success case
            }
            
            throw new Exception("Failed to create game set player - no data returned");
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("400"))
        {
            throw new ArgumentException("Invalid request data provided for creating game set player", ex);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("409"))
        {
            return gameSetPlayer;
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("500"))
        {
            throw new Exception("Server error occurred while creating game set player", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to create game set player", ex);
        }
    }

    public async Task<bool> DeleteGameSetPlayerAsync(int GameSetId,Guid playerId)
    {
        return await _apiService.DeleteAsync($"api/marriagegamesetplayers/{GameSetId}/{playerId}");
    }
}
