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

    public async Task<IEnumerable<GameSettingsDto>> GetAllGameSettingsAsync()
    {
        var settings = await _gameSettingsRepository.GetAllAsync();
        return settings.Select(MapToDto);
    }

    public async Task<GameSettingsDto?> GetGameSettingsByIdAsync(int id)
    {
        var settings = await _gameSettingsRepository.GetByIdAsync(id);
        return settings != null ? MapToDto(settings) : null;
    }

    public async Task<GameSettingsDto> CreateGameSettingsAsync(CreateGameSettingsDto createDto)
    {
        var settings = MapFromCreateDto(createDto);
        var createdSettings = await _gameSettingsRepository.CreateAsync(settings);
        return MapToDto(createdSettings);
    }

    public async Task<GameSettingsDto?> UpdateGameSettingsAsync(int id, CreateGameSettingsDto updateDto)
    {
        var settingsToUpdate = MapFromCreateDto(updateDto);
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