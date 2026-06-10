using MarriageCalculator.Core.DTOs;
using MarriageCalculator.API.Repositories;
using MarriageCalculator.Core.Models;

namespace MarriageCalculator.API.Services;

public class GameSettingsService : IGameSettingsService
{
    private readonly IGameSettingsRepository _gameSettingsRepository;

    public GameSettingsService(IGameSettingsRepository gameSettingsRepository)
    {
        _gameSettingsRepository = gameSettingsRepository;
    }

    public async Task<IEnumerable<GameSettingsDto>> GetAllGameSettingsAsync(string userId)
    {
        var settings = await _gameSettingsRepository.GetAllByUserIdAsync(userId);
        return settings.Select(MapToDto);
    }

    public async Task<GameSettingsDto?> GetGameSettingsByIdAsync(string id, string userId)
    {
        var settings = await _gameSettingsRepository.GetByIdAsync(id, userId);
        return settings != null ? MapToDto(settings) : null;
    }

    public async Task<GameSettingsDto> CreateGameSettingsAsync(CreateGameSettingsDto createDto)
    {
        var settings = MapFromCreateDto(createDto);
        var createdSettings = await _gameSettingsRepository.CreateAsync(settings);
        return MapToDto(createdSettings);
    }

    public async Task<GameSettingsDto?> UpdateGameSettingsAsync(string id, CreateGameSettingsDto updateDto, string userId)
    {
        var settingsToUpdate = MapFromCreateDto(updateDto);
        var updatedSettings = await _gameSettingsRepository.UpdateAsync(id, settingsToUpdate, userId);
        return updatedSettings != null ? MapToDto(updatedSettings) : null;
    }

    public async Task<bool> DeleteGameSettingsAsync(string id, string userId)
    {
        return await _gameSettingsRepository.DeleteAsync(id, userId);
    }

    public async Task<bool> GameSettingsExistsAsync(string id, string userId)
    {
        return await _gameSettingsRepository.ExistsAsync(id, userId);
    }

    private static GameSettingsDto MapToDto(GameSettings settings)
    {
        return new GameSettingsDto
        {
            Id = settings.Id,
            UserId = settings.UserId,
            Murder = settings.Murder,
            Kidnap = settings.Kidnap,
            SeenPoint = settings.SeenPoint,
            UnseenPoint = settings.UnseenPoint,
            PointRate = settings.PointRate,
            Currency = settings.Currency.ToString(),
            Dublee = settings.Dublee,
            DubleePointLess = settings.DubleePointLess,
            DubleePointBonus = settings.DubleePointBonus,
            FoulPoint = settings.FoulPoint,
            FoulPointBonus = settings.FoulPointBonus.ToString(),
            Audio = settings.Audio
        };
    }

    private static GameSettings MapFromCreateDto(CreateGameSettingsDto dto)
    {
        return new GameSettings
        {
            UserId = dto.UserId,
            Murder = dto.Murder,
            Kidnap = dto.Kidnap,
            SeenPoint = dto.SeenPoint,
            UnseenPoint = dto.UnseenPoint,
            PointRate = dto.PointRate,
            Currency = Enum.Parse<Currency>(dto.Currency),
            Dublee = dto.Dublee,
            DubleePointLess = dto.DubleePointLess,
            DubleePointBonus = dto.DubleePointBonus,
            FoulPoint = dto.FoulPoint,
            FoulPointBonus = Enum.Parse<FoulPointBonusType>(dto.FoulPointBonus),
            Audio = dto.Audio
        };
    }
}