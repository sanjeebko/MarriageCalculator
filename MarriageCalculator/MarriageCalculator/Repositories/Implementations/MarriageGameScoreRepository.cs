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
            
            // If it's an authentication error, re-throw to let the caller handle it
            if (ex.Message.Contains("Unauthorized") || ex.Message.Contains("401"))
            {
                throw new UnauthorizedAccessException("Authentication required to access game scores", ex);
            }
            
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
            
            // If it's an authentication error, re-throw to let the caller handle it
            if (ex.Message.Contains("Unauthorized") || ex.Message.Contains("401"))
            {
                throw new UnauthorizedAccessException("Authentication required to access game scores", ex);
            }
            
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
            
            // If it's an authentication error, re-throw with clearer message
            if (ex.Message.Contains("Unauthorized") || ex.Message.Contains("401"))
            {
                throw new UnauthorizedAccessException("Authentication required to create game scores", ex);
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
            
            // If it's an authentication error, re-throw with clearer message
            if (ex.Message.Contains("Unauthorized") || ex.Message.Contains("401"))
            {
                throw new UnauthorizedAccessException("Authentication required to update game scores", ex);
            }
            
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
            
            // If it's an authentication error, re-throw with clearer message
            if (ex.Message.Contains("Unauthorized") || ex.Message.Contains("401"))
            {
                throw new UnauthorizedAccessException("Authentication required to delete game scores", ex);
            }
            
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
            
            // If it's an authentication error, re-throw to let the caller handle it
            if (ex.Message.Contains("Unauthorized") || ex.Message.Contains("401"))
            {
                throw new UnauthorizedAccessException("Authentication required to access game scores", ex);
            }
            
            return new List<MarriageGameScore>();
        }
    }
}
