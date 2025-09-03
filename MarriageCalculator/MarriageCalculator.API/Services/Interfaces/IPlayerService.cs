using MarriageCalculator.Core.DTOs;

namespace MarriageCalculator.API.Services.Interfaces;

public interface IPlayerService
{
    Task<IEnumerable<PlayerDto>> GetAllPlayersAsync();
    Task<IEnumerable<PlayerDto>> GetPlayersByCreatorAsync(Guid userId);
    Task<PlayerDto?> GetPlayerByIdAsync(Guid id);
    Task<PlayerDto> CreatePlayerForUserAsync(CreatePlayerDto createPlayerDto, Guid userId);
    Task<PlayerDto> EnsureUserPlayerAsync(Guid userId, string displayName, string email);
    Task<PlayerDto?> UpdatePlayerAsync(Guid id, UpdatePlayerDto updatePlayerDto);
    Task<bool> DeletePlayerAsync(Guid id);
    Task<bool> PlayerExistsAsync(Guid id);
}