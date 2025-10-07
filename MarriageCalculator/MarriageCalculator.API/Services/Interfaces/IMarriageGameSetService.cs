using MarriageCalculator.Core.DTOs;

namespace MarriageCalculator.API.Services.Interfaces;

public interface IMarriageGameSetService
{
    Task<IEnumerable<MarriageGameSetDto>> GetAllGameSetsAsync(int gameSettingsId);
    Task<MarriageGameSetDto?> GetGameSetByIdAsync(int id);
    Task<MarriageGameSetDto> CreateGameSetAsync(CreateMarriageGameSetDto createGameSetDto);
    Task<MarriageGameSetDto?> UpdateGameSetAsync(int id, CreateMarriageGameSetDto updateGameSetDto);
    Task<bool> DeleteGameSetAsync(int id);
    Task<bool> GameSetExistsAsync(int id);
    Task<MarriageGameSetDto?> GetLatestActiveGameSetAsync();
    Task<MarriageGameSetDto?> GetLatestActiveGameSetForUserAsync(Guid userId);
    Task<MarriageGameSetDto?> GetActiveGameSetByGameSettingsIdAsync(int gameSettingsId);
}