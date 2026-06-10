using MarriageCalculator.Core.DTOs;
using MarriageCalculator.API.Repositories;
using MarriageCalculator.Core.Models;

namespace MarriageCalculator.API.Services;

public class MarriageGameSetService(IMarriageGameSetRepository gameSetRepository) : IMarriageGameSetService
{
    private readonly IMarriageGameSetRepository _gameSetRepository = gameSetRepository;

    public async Task<IEnumerable<MarriageGameSetDto>> GetAllGameSetsAsync(string hostUserId)
    {
        var gameSets = await _gameSetRepository.GetAllByHostUserIdAsync(hostUserId);
        return gameSets.Select(MapToDto);
    }

    public async Task<MarriageGameSetDto?> GetGameSetByIdAsync(string id, string hostUserId)
    {
        var gameSet = await _gameSetRepository.GetByIdAsync(id, hostUserId);
        return gameSet != null ? MapToDto(gameSet) : null;
    }

    public async Task<MarriageGameSetDto> CreateGameSetAsync(CreateMarriageGameSetDto createDto)
    {
        var gameSet = new MarriageGameSet
        {
            HostUserId = createDto.HostUserId,
            Name = createDto.Name,
            GameSettingsId = createDto.GameSettingsId,
            Created = DateTime.UtcNow,
            LastPlayed = DateTime.UtcNow,
            IsActive = true
        };

        var createdGameSet = await _gameSetRepository.CreateAsync(gameSet);
        return MapToDto(createdGameSet);
    }

    public async Task<MarriageGameSetDto?> UpdateGameSetAsync(string id, CreateMarriageGameSetDto updateDto, string hostUserId)
    {
        var gameSetToUpdate = new MarriageGameSet
        {
            HostUserId = updateDto.HostUserId,
            Name = updateDto.Name,
            GameSettingsId = updateDto.GameSettingsId,
            LastPlayed = DateTime.UtcNow
        };

        var updatedGameSet = await _gameSetRepository.UpdateAsync(id, gameSetToUpdate, hostUserId);
        return updatedGameSet != null ? MapToDto(updatedGameSet) : null;
    }

    public async Task<bool> DeleteGameSetAsync(string id, string hostUserId)
    {
        return await _gameSetRepository.DeleteAsync(id, hostUserId);
    }

    public async Task<bool> GameSetExistsAsync(string id, string hostUserId)
    {
        return await _gameSetRepository.ExistsAsync(id, hostUserId);
    }

    public async Task<MarriageGameSetDto?> GetLatestActiveGameSetAsync(string hostUserId)
    {
        var gameSet = await _gameSetRepository.GetLatestActiveAsync(hostUserId);
        return gameSet != null ? MapToDto(gameSet) : null;
    }

    private static MarriageGameSetDto MapToDto(MarriageGameSet gameSet)
    {
        return new MarriageGameSetDto
        {
            Id = gameSet.Id,
            HostUserId = gameSet.HostUserId,
            Name = gameSet.Name,
            LastPlayed = gameSet.LastPlayed,
            Created = gameSet.Created,
            IsActive = gameSet.IsActive,
            GameSettingsId = gameSet.GameSettingsId
        };
    }
}