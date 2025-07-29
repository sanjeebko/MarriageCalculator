using MarriageCalculator.Core.Models;
using AutoMapper;

namespace MarriageCalculator.Repositories;

/// <summary>
/// Player repository implementation using API service
/// </summary>
public class PlayerRepository : IPlayerRepository
{
    private readonly IApiService _apiService;
    private readonly IMapper _mapper;

    public PlayerRepository(IApiService apiService, IMapper mapper)
    {
        _apiService = apiService;
        _mapper = mapper;
    }

    public async Task<List<Player>> GetAllPlayersAsync()
    {
        var playersDto = await _apiService.GetAsync<List<PlayerDto>>("api/players");
        return playersDto?.Select(_mapper.Map<Player>).ToList() ?? new List<Player>();
    }

    public async Task<Player?> GetPlayerByIdAsync(int id)
    {
        var playerDto = await _apiService.GetAsync<PlayerDto>($"api/players/{id}");
        return playerDto != null ? _mapper.Map<Player>(playerDto) : null;
    }

    public async Task<Player> CreatePlayerAsync(Player player)
    {
        var createDto = _mapper.Map<CreatePlayerDto>(player);
        var resultDto = await _apiService.PostAsync<PlayerDto>("api/players", createDto);
        return resultDto != null ? _mapper.Map<Player>(resultDto) : throw new Exception("Failed to create player");
    }

    public async Task<Player?> UpdatePlayerAsync(Player player)
    {
        var updateDto = _mapper.Map<UpdatePlayerDto>(player);
        var resultDto = await _apiService.PutAsync<PlayerDto>($"api/players/{player.Id}", updateDto);
        return resultDto != null ? _mapper.Map<Player>(resultDto) : null;
    }

    public async Task<bool> DeletePlayerAsync(int id)
    {
        return await _apiService.DeleteAsync($"api/players/{id}");
    }
}

/// <summary>
/// GameSettings repository implementation using API service with AutoMapper
/// </summary>
public class GameSettingsRepository : IGameSettingsRepository
{
    private readonly IApiService _apiService;
    private readonly IMapper _mapper;

    public GameSettingsRepository(IApiService apiService, IMapper mapper)
    {
        _apiService = apiService;
        _mapper = mapper;
    }

    public async Task<List<GameSettings>> GetAllGameSettingsAsync()
    {
        var settingsDto = await _apiService.GetAsync<List<GameSettingsDto>>("api/gamesettings");
        return settingsDto?.Select(_mapper.Map<GameSettings>).ToList() ?? new List<GameSettings>();
    }

    public async Task<GameSettings?> GetGameSettingsByIdAsync(int id)
    {
        var settingsDto = await _apiService.GetAsync<GameSettingsDto>($"api/gamesettings/{id}");
        return settingsDto != null ? _mapper.Map<GameSettings>(settingsDto) : null;
    }

    public async Task<GameSettings?> GetLatestGameSettingsAsync()
    {
        var allSettings = await GetAllGameSettingsAsync();
        return allSettings.OrderByDescending(s => s.Id).FirstOrDefault();
    }

    public async Task<GameSettings> CreateGameSettingsAsync(GameSettings gameSettings)
    {
        var createDto = _mapper.Map<CreateGameSettingsDto>(gameSettings);
        var resultDto = await _apiService.PostAsync<GameSettingsDto>("api/gamesettings", createDto);
        return resultDto != null ? _mapper.Map<GameSettings>(resultDto) : throw new Exception("Failed to create game settings");
    }

    public async Task<GameSettings?> UpdateGameSettingsAsync(GameSettings gameSettings)
    {
        var updateDto = _mapper.Map<CreateGameSettingsDto>(gameSettings);
        var resultDto = await _apiService.PutAsync<GameSettingsDto>($"api/gamesettings/{gameSettings.Id}", updateDto);
        return resultDto != null ? _mapper.Map<GameSettings>(resultDto) : null;
    }

    public async Task<bool> DeleteGameSettingsAsync(int id)
    {
        return await _apiService.DeleteAsync($"api/gamesettings/{id}");
    }
}

/// <summary>
/// MarriageGameSet repository implementation using API service with AutoMapper
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
        catch(Exception ex)
        {
            //Log exception;
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