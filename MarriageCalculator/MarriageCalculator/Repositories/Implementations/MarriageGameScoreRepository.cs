using MarriageCalculator.Repositories.Interfaces;
using MarriageCalculator.Services.Interfaces;

namespace MarriageCalculator.Repositories.Implementations;

/// <summary>
/// MarriageGameScore repository implementation using API service
/// </summary>
public class MarriageGameScoreRepository : IMarriageGameScoreRepository
{
    private readonly IApiService _apiService;
    private readonly IMapper _mapper;

    public MarriageGameScoreRepository(IApiService apiService, IMapper mapper)
    {
        _apiService = apiService;
        _mapper = mapper;
    }

    public async Task<List<MarriageGameScore>> GetScoresByGameIdAsync(int gameId)
    {
        try
        {
            var scoresDto = await _apiService.GetAsync<List<MarriageGameScoreDto>>($"api/marriagegamescores/game/{gameId}");
            return scoresDto?.Select(_mapper.Map<MarriageGameScore>).ToList() ?? new List<MarriageGameScore>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MarriageGameScoreRepository.GetScoresByGameIdAsync failed for game ID {gameId}: {ex.Message}");
            return new List<MarriageGameScore>();
        }
    }

    public async Task<MarriageGameScore?> GetScoreByIdAsync(int id)
    {
        try
        {
            var scoreDto = await _apiService.GetAsync<MarriageGameScoreDto>($"api/marriagegamescores/{id}");
            return scoreDto != null ? _mapper.Map<MarriageGameScore>(scoreDto) : null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MarriageGameScoreRepository.GetScoreByIdAsync failed for ID {id}: {ex.Message}");
            return null;
        }
    }

    public async Task<MarriageGameScore> CreateScoreAsync(MarriageGameScore score)
    {
        try
        {
            var createDto = _mapper.Map<CreateMarriageGameScoreDto>(score);
            var resultDto = await _apiService.PostAsync<MarriageGameScoreDto>("api/marriagegamescores", createDto);
            return resultDto != null ? _mapper.Map<MarriageGameScore>(resultDto) : throw new Exception("Failed to create marriage game score");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MarriageGameScoreRepository.CreateScoreAsync failed: {ex.Message}");
            
            // Handle conflict (duplicate score) errors - this is a business logic error, not authentication
            if (ex.Message.Contains("Conflict") || ex.Message.Contains("409") || ex.Message.Contains("already exists"))
            {
                throw new InvalidOperationException($"A score already exists for this player in this game. Player: {score.PlayerId}, Game: {score.MarriageGameId}", ex);
            }
            
            throw;
        }
    }

    public async Task<MarriageGameScore?> UpdateScoreAsync(MarriageGameScore score)
    {
        try
        {
            var updateDto = _mapper.Map<CreateMarriageGameScoreDto>(score);
            var resultDto = await _apiService.PutAsync<MarriageGameScoreDto>($"api/marriagegamescores/{score.Id}", updateDto);
            return resultDto != null ? _mapper.Map<MarriageGameScore>(resultDto) : null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MarriageGameScoreRepository.UpdateScoreAsync failed: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> DeleteScoreAsync(int id)
    {
        try
        {
            return await _apiService.DeleteAsync($"api/marriagegamescores/{id}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MarriageGameScoreRepository.DeleteScoreAsync failed for ID {id}: {ex.Message}");
            return false;
        }
    }

    public async Task<List<MarriageGameScore>> GetScoresByPlayerIdAsync(Guid playerId)
    {
        try
        {
            var scoresDto = await _apiService.GetAsync<List<MarriageGameScoreDto>>($"api/marriagegamescores/player/{playerId}");
            return scoresDto?.Select(_mapper.Map<MarriageGameScore>).ToList() ?? new List<MarriageGameScore>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MarriageGameScoreRepository.GetScoresByPlayerIdAsync failed for player ID {playerId}: {ex.Message}");
            return new List<MarriageGameScore>();
        }
    }
}
