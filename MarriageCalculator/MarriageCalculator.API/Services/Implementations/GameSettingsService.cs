using MarriageCalculator.Core.DTOs;
using MarriageCalculator.API.Repositories.Interfaces;
using MarriageCalculator.Core.Models;
using MarriageCalculator.API.Services.Interfaces;

namespace MarriageCalculator.API.Services.Implementations;

public class GameSettingsService : IGameSettingsService
{
    private readonly IGameSettingsRepository _gameSettingsRepository;

    public GameSettingsService(IGameSettingsRepository gameSettingsRepository)
    {
        _gameSettingsRepository = gameSettingsRepository;
    }

    public async Task<IEnumerable<GameSettingsDto>> GetAllGameSettingsAsync(Guid userId)
    {
        var settings = await _gameSettingsRepository.GetByUserIdAsync(userId);
        return settings.Select(MapToDto);
    }

    public async Task<GameSettingsDto?> GetGameSettingsByIdAsync(int id)
    {
        var settings = await _gameSettingsRepository.GetByIdAsync(id);
        return settings != null ? MapToDto(settings) : null;
    }

    public Task<GameSettingsDto> CreateGameSettingsAsync(CreateGameSettingsDto createDto)
    {
        throw new InvalidOperationException("CreateGameSettingsAsync must be called with a userId parameter. Use CreateGameSettingsAsync(createDto, userId) instead.");
    }

    public async Task<GameSettingsDto> CreateGameSettingsAsync(CreateGameSettingsDto createDto, Guid userId)
    {
        var settings = MapFromCreateDto(createDto, userId);
        var createdSettings = await _gameSettingsRepository.CreateAsync(settings);
        return MapToDto(createdSettings);
    }

    public async Task<GameSettingsDto?> UpdateGameSettingsAsync(int id, CreateGameSettingsDto updateDto)
    {
        // For updates, we get the existing settings to preserve the UserId
        var existingSettings = await _gameSettingsRepository.GetByIdAsync(id);
        if (existingSettings == null)
            return null;

        var settingsToUpdate = MapFromCreateDto(updateDto, existingSettings.UserId);
        var updatedSettings = await _gameSettingsRepository.UpdateAsync(id, settingsToUpdate);
        return updatedSettings != null ? MapToDto(updatedSettings) : null;
    }

    public async Task<bool> DeleteGameSettingsAsync(int id)
    {
        return await _gameSettingsRepository.DeleteAsync(id);
    }

    public async Task<bool> GameSettingsExistsAsync(int id)
    {
        return await _gameSettingsRepository.ExistsAsync(id);
    }

    private static GameSettingsDto MapToDto(GameSettings settings)
    {
        return new GameSettingsDto
        {
            Id = settings.Id,
            UserId = settings.UserId, // Include UserId in DTO
            Murder = settings.Murder,
            Kidnap = settings.Kidnap,
            SeenPoint = settings.SeenPoint,
            UnseenPoint = settings.UnseenPoint,
            PointRate = settings.PointRate,
            Currency = settings.Currency,
            Dublee = settings.Dublee,
            DubleePointLess = settings.DubleePointLess,
            DubleePointBonus = settings.DubleePointBonus,
            FoulPoint = settings.FoulPoint,
            FoulPointBonus = settings.FoulPointBonus,
            Audio = settings.Audio,
            CreatedAt = settings.CreatedAt
        };
    }

    private static GameSettings MapFromCreateDto(CreateGameSettingsDto dto, Guid userId)
    {
        return new GameSettings
        {
            UserId = userId, // Set the required UserId
            Murder = dto.Murder,
            Kidnap = dto.Kidnap,
            SeenPoint = dto.SeenPoint,
            UnseenPoint = dto.UnseenPoint,
            PointRate = dto.PointRate,
            Currency = dto.Currency,
            Dublee = dto.Dublee,
            DubleePointLess = dto.DubleePointLess,
            DubleePointBonus = dto.DubleePointBonus,
            FoulPoint = dto.FoulPoint,
            FoulPointBonus = dto.FoulPointBonus,
            Audio = dto.Audio,
            CreatedAt = DateTime.UtcNow
        };
    }
}