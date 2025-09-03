using MarriageCalculator.Core.Models;
using MarriageCalculator.Core.DTOs;
using AutoMapper;
using MarriageCalculator.Repositories.Interfaces;
using MarriageCalculator.Services.Interfaces;

namespace MarriageCalculator.Repositories.Implementations;

public class GameSettingsRepository(IApiService apiService, IMapper mapper) : IGameSettingsRepository
{
    private readonly IApiService _apiService = apiService;
    private readonly IMapper _mapper = mapper;

    public async Task<List<GameSettings>> GetAllGameSettingsAsync()
    {
        try
        {
            var settingsDto = await _apiService.GetAsync<List<GameSettingsDto>>("api/gamesettings");
            return settingsDto?.Select(_mapper.Map<GameSettings>).ToList() ?? new List<GameSettings>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GameSettingsRepository.GetAllGameSettingsAsync failed: {ex.Message}");
            
            // If it's an authentication error, re-throw to let the caller handle it
            if (ex.Message.Contains("Unauthorized") || ex.Message.Contains("401"))
            {
                throw new UnauthorizedAccessException("Authentication required to access game settings", ex);
            }
            
            // For other errors, return empty list as fallback
            return new List<GameSettings>();
        }
    }

    public async Task<GameSettings?> GetGameSettingsByIdAsync(int id)
    {
        try
        {
            var settingsDto = await _apiService.GetAsync<GameSettingsDto>($"api/gamesettings/{id}");
            return settingsDto != null ? _mapper.Map<GameSettings>(settingsDto) : null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GameSettingsRepository.GetGameSettingsByIdAsync failed for ID {id}: {ex.Message}");
            
            // If it's an authentication error, re-throw to let the caller handle it
            if (ex.Message.Contains("Unauthorized") || ex.Message.Contains("401"))
            {
                throw new UnauthorizedAccessException("Authentication required to access game settings", ex);
            }
            
            return null;
        }
    }

    public async Task<GameSettings?> GetLatestGameSettingsAsync()
    {
        var allSettings = await GetAllGameSettingsAsync();
        return allSettings.OrderByDescending(s => s.Id).FirstOrDefault();
    }

    public async Task<GameSettings> CreateGameSettingsAsync(GameSettings gameSettings)
    {
        try
        {
            var createDto = _mapper.Map<CreateGameSettingsDto>(gameSettings);
            var resultDto = await _apiService.PostAsync<GameSettingsDto>("api/gamesettings", createDto);
            return resultDto != null ? _mapper.Map<GameSettings>(resultDto) : throw new Exception("Failed to create game settings");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GameSettingsRepository.CreateGameSettingsAsync failed: {ex.Message}");
            
            // If it's an authentication error, re-throw with clearer message
            if (ex.Message.Contains("Unauthorized") || ex.Message.Contains("401"))
            {
                throw new UnauthorizedAccessException("Authentication required to create game settings", ex);
            }
            
            throw;
        }
    }

    public async Task<GameSettings?> UpdateGameSettingsAsync(GameSettings gameSettings)
    {
        try
        {
            var updateDto = _mapper.Map<CreateGameSettingsDto>(gameSettings);
            var resultDto = await _apiService.PutAsync<GameSettingsDto>($"api/gamesettings/{gameSettings.Id}", updateDto);
            return resultDto != null ? _mapper.Map<GameSettings>(resultDto) : null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GameSettingsRepository.UpdateGameSettingsAsync failed: {ex.Message}");
            
            // If it's an authentication error, re-throw with clearer message
            if (ex.Message.Contains("Unauthorized") || ex.Message.Contains("401"))
            {
                throw new UnauthorizedAccessException("Authentication required to update game settings", ex);
            }
            
            return null;
        }
    }

    public async Task<bool> DeleteGameSettingsAsync(int id)
    {
        try
        {
            return await _apiService.DeleteAsync($"api/gamesettings/{id}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GameSettingsRepository.DeleteGameSettingsAsync failed for ID {id}: {ex.Message}");
            
            // If it's an authentication error, re-throw with clearer message
            if (ex.Message.Contains("Unauthorized") || ex.Message.Contains("401"))
            {
                throw new UnauthorizedAccessException("Authentication required to delete game settings", ex);
            }
            
            return false;
        }
    }
}