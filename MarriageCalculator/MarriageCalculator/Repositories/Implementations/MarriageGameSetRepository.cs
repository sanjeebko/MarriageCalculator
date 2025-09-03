using MarriageCalculator.Repositories.Interfaces;
using MarriageCalculator.Services.Interfaces;

namespace MarriageCalculator.Repositories.Implementations;

/// <summary>
/// MarriageGameSet repository implementation using API service
/// </summary>
public class MarriageGameSetRepository : IMarriageGameSetRepository
{
    private readonly IApiService _apiService;
    private readonly IMapper _mapper;

    public MarriageGameSetRepository(IApiService apiService, IMapper mapper)
    {
        _apiService = apiService;
        _mapper = mapper;
    }

    public async Task<List<MarriageGameSet>> GetAllGameSetsAsync()
    {
        var gameSetsDto = await _apiService.GetAsync<List<MarriageGameSetDto>>("api/marriagegamesets");
        return gameSetsDto?.Select(_mapper.Map<MarriageGameSet>).ToList() ?? new List<MarriageGameSet>();
    }

    public async Task<MarriageGameSet?> GetGameSetByIdAsync(int id)
    {
        var gameSetDto = await _apiService.GetAsync<MarriageGameSetDto>($"api/marriagegamesets/{id}");
        return gameSetDto != null ? _mapper.Map<MarriageGameSet>(gameSetDto) : null;
    }

    public async Task<MarriageGameSet?> GetLatestGameSetAsync()
    {
        try
        {
            var gameSetDto = await _apiService.GetAsync<MarriageGameSetDto>("api/marriagegamesets/latest");
            return gameSetDto != null ? _mapper.Map<MarriageGameSet>(gameSetDto) : null;
        }
        catch(Exception)
        {
            return null;
        }
    }

    public async Task<MarriageGameSet> CreateGameSetAsync(MarriageGameSet gameSet)
    {
        var createDto = _mapper.Map<CreateMarriageGameSetDto>(gameSet);
        var resultDto = await _apiService.PostAsync<MarriageGameSetDto>("api/marriagegamesets", createDto);
        return resultDto != null ? _mapper.Map<MarriageGameSet>(resultDto) : throw new Exception("Failed to create game set");
    }

    public async Task<MarriageGameSet?> UpdateGameSetAsync(MarriageGameSet gameSet)
    {
        var updateDto = _mapper.Map<CreateMarriageGameSetDto>(gameSet);
        var resultDto = await _apiService.PutAsync<MarriageGameSetDto>($"api/marriagegamesets/{gameSet.Id}", updateDto);
        return resultDto != null ? _mapper.Map<MarriageGameSet>(resultDto) : null;
    }

    public async Task<bool> DeleteGameSetAsync(int id)
    {
        return await _apiService.DeleteAsync($"api/marriagegamesets/{id}");
    }
}