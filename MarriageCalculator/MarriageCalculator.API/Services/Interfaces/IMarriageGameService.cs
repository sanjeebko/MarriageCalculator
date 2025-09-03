using MarriageCalculator.Core.DTOs;

namespace MarriageCalculator.API.Services.Interfaces;

public interface IMarriageGameService
{
    Task<IEnumerable<MarriageGameDto>> GetAllGamesAsync();
    Task<MarriageGameDto?> GetGameByIdAsync(int id);
    Task<MarriageGameDto> CreateGameAsync(CreateMarriageGameDto createGameDto);
    Task<MarriageGameDto?> UpdateGameAsync(int id, CreateMarriageGameDto updateGameDto);
    Task<bool> DeleteGameAsync(int id);
    Task<bool> GameExistsAsync(int id);
    Task<IEnumerable<MarriageGameDto>> GetGamesByRoundIdAsync(int roundId);
}