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
        try
        {
            var rounds = await _apiService.GetAsync<List<MarriageGameRound>>($"api/marriagegamerounds/gameset/{gameSetId}");
            return rounds ?? new List<MarriageGameRound>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MarriageGameRoundRepository.GetRoundsByGameSetIdAsync failed for gameSetId {gameSetId}: {ex.Message}");
            return new List<MarriageGameRound>();
        }
    }

    public async Task<MarriageGameRound?> GetRoundByIdAsync(int id)
    {
        try
        {
            return await _apiService.GetAsync<MarriageGameRound>($"api/marriagegamerounds/{id}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MarriageGameRoundRepository.GetRoundByIdAsync failed for ID {id}: {ex.Message}");
            return null;
        }
    }

    public async Task<MarriageGameRound> CreateRoundAsync(MarriageGameRound round)
    {
        try
        {
            var createDto = new
            {
                round.Sequence,
                round.MarriageGameSetId,
                round.Completed
            };
            
            var result = await _apiService.PostAsync<MarriageGameRound>("api/marriagegamerounds", createDto);
            return result ?? throw new Exception("Failed to create marriage game round");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MarriageGameRoundRepository.CreateRoundAsync failed: {ex.Message}");
            throw new Exception($"Failed to create marriage game round: {ex.Message}", ex);
        }
    }

    public async Task<MarriageGameRound?> UpdateRoundAsync(MarriageGameRound round)
    {
        try
        {
            var updateDto = new
            {
                round.Sequence,
                round.MarriageGameSetId,
                round.Completed
            };
            
            return await _apiService.PutAsync<MarriageGameRound>($"api/marriagegamerounds/{round.Id}", updateDto);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MarriageGameRoundRepository.UpdateRoundAsync failed for ID {round.Id}: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> DeleteRoundAsync(int id)
    {
        try
        {
            return await _apiService.DeleteAsync($"api/marriagegamerounds/{id}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MarriageGameRoundRepository.DeleteRoundAsync failed for ID {id}: {ex.Message}");
            return false;
        }
    }
}
