using MarriageCalculator.Repositories.Interfaces;
using MarriageCalculator.Services.Interfaces;

namespace MarriageCalculator.Repositories.Implementations;

/// <summary>
/// MarriageGameRound repository implementation using API service
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
