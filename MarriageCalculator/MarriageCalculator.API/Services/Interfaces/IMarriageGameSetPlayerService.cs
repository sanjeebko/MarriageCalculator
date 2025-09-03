using MarriageCalculator.Core.DTOs;

namespace MarriageCalculator.API.Services.Interfaces;

public interface IMarriageGameSetPlayerService
{
    Task<IEnumerable<MarriageGameSetPlayerDto>> GetAllGameSetPlayersAsync();
    Task<MarriageGameSetPlayerDto?> GetGameSetPlayerByIdAsync(int gameSetId, Guid playerId);
    Task<IEnumerable<MarriageGameSetPlayerDto>> GetPlayersByGameSetIdAsync(int gameSetId);
    Task<IEnumerable<MarriageGameSetPlayerDto>> GetGameSetsByPlayerIdAsync(Guid playerId);
    Task<MarriageGameSetPlayerDto> CreateGameSetPlayerAsync(CreateMarriageGameSetPlayerDto createDto);
    Task<bool> DeleteGameSetPlayerAsync(int gameSetId, Guid playerId);
    Task<bool> DeletePlayersByGameSetIdAsync(int gameSetId);
    Task<bool> GameSetPlayerExistsAsync(int gameSetId, Guid playerId);
}