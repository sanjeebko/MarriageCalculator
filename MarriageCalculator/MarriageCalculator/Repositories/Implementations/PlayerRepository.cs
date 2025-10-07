using MarriageCalculator.Repositories.Interfaces;
using MarriageCalculator.Services.Interfaces;

namespace MarriageCalculator.Repositories.Implementations;

public class PlayerRepository(IApiService apiService, IMapper mapper) : IPlayerRepository
{
    private readonly IApiService _apiService = apiService;
    private readonly IMapper _mapper = mapper;

    public async Task<List<Player>> GetAllPlayersAsync()
    {
        var playersDto = await _apiService.GetAsync<List<PlayerDto>>("api/players");
        return playersDto?.Select(_mapper.Map<Player>).ToList() ?? [];
    }

    public async Task<Player?> GetPlayerByIdAsync(Guid id)
    {
        var playerDto = await _apiService.GetAsync<PlayerDto>($"api/players/{id}");
        return playerDto != null ? _mapper.Map<Player>(playerDto) : null;
    }

    public async Task<Player> CreatePlayerAsync(Player player)
    {
        var createDto = _mapper.Map<CreatePlayerDto>(player);
        var resultDto = await _apiService.PostAsync<PlayerDto>("api/players", createDto);
        var created = resultDto != null ? _mapper.Map<Player>(resultDto) : throw new Exception("Failed to create player");
        player.Id = created.Id;
        return created;
    }

    public async Task<Player?> UpdatePlayerAsync(Player player)
    {
        var updateDto = _mapper.Map<UpdatePlayerDto>(player);
        var resultDto = await _apiService.PutAsync<PlayerDto>($"api/players/{player.Id}", updateDto);
        return resultDto != null ? _mapper.Map<Player>(resultDto) : null;
    }

    public async Task<Player?> UpdatePlayerByGuidAsync(Player player)
    {
        if (player.Id == Guid.Empty) return null;
        var updateDto = _mapper.Map<UpdatePlayerDto>(player);
        var resultDto = await _apiService.PutAsync<PlayerDto>($"api/players/{player.Id}", updateDto);
        return resultDto != null ? _mapper.Map<Player>(resultDto) : null;
    }

    public async Task<bool> DeletePlayerAsync(Guid id)
    {
        return await _apiService.DeleteAsync($"api/players/{id}");
    }

    public async Task<Player?> EnsureCurrentUserPlayerAsync()
    {
        var resultDto = await _apiService.PostAsync<PlayerDto>("api/players/ensure-me", new { });
        return resultDto != null ? _mapper.Map<Player>(resultDto) : null;
    }
}