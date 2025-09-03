using MarriageCalculator.Core.DTOs;

namespace MarriageCalculator.API.Services.Interfaces;

public interface IGameSettingsService
{
    Task<IEnumerable<GameSettingsDto>> GetAllGameSettingsAsync(Guid userId);
    Task<GameSettingsDto?> GetGameSettingsByIdAsync(int id);
    Task<GameSettingsDto> CreateGameSettingsAsync(CreateGameSettingsDto createGameSettingsDto);
    Task<GameSettingsDto> CreateGameSettingsAsync(CreateGameSettingsDto createGameSettingsDto, Guid userId);
    Task<GameSettingsDto?> UpdateGameSettingsAsync(int id, CreateGameSettingsDto updateGameSettingsDto);
    Task<bool> DeleteGameSettingsAsync(int id);
    Task<bool> GameSettingsExistsAsync(int id);
}