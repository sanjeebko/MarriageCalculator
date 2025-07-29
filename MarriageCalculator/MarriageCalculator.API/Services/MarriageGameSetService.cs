using MarriageCalculator.Core.DTOs;
using MarriageCalculator.API.Repositories;
using MarriageCalculator.Core.Models;

namespace MarriageCalculator.API.Services;

public class MarriageGameSetService(IMarriageGameSetRepository gameSetRepository) : IMarriageGameSetService
{
    private readonly IMarriageGameSetRepository _gameSetRepository = gameSetRepository;

    public async Task<IEnumerable<MarriageGameSetDto>> GetAllGameSetsAsync()
    {
        var gameSets = await _gameSetRepository.GetAllAsync();
        return gameSets.Select(MapToDto);
    }

    public async Task<MarriageGameSetDto?> GetGameSetByIdAsync(int id)
    {
        var gameSet = await _gameSetRepository.GetByIdAsync(id);
        return gameSet != null ? MapToDto(gameSet) : null;
    }

    public async Task<MarriageGameSetDto> CreateGameSetAsync(CreateMarriageGameSetDto createDto)
    {
        var gameSet = new MarriageGameSet
        {
            Name = createDto.Name,
            GameSettingsId = createDto.GameSettingsId,
            Created = DateTime.UtcNow,
            LastPlayed = DateTime.UtcNow,
            IsActive = true
        };

        var createdGameSet = await _gameSetRepository.CreateAsync(gameSet);
        return MapToDto(createdGameSet);
    }

    public async Task<MarriageGameSetDto?> UpdateGameSetAsync(int id, CreateMarriageGameSetDto updateDto)
    {
        var gameSetToUpdate = new MarriageGameSet
        {
            Name = updateDto.Name,
            GameSettingsId = updateDto.GameSettingsId,
            LastPlayed = DateTime.UtcNow
        };

        var updatedGameSet = await _gameSetRepository.UpdateAsync(id, gameSetToUpdate);
        return updatedGameSet != null ? MapToDto(updatedGameSet) : null;
    }

    public async Task<bool> DeleteGameSetAsync(int id)
    {
        return await _gameSetRepository.DeleteAsync(id);
    }

    public async Task<bool> GameSetExistsAsync(int id)
    {
        return await _gameSetRepository.ExistsAsync(id);
    }

    public async Task<MarriageGameSetDto?> GetLatestActiveGameSetAsync()
    {
        var gameSet = await _gameSetRepository.GetLatestActiveAsync();
        return gameSet != null ? MapToDto(gameSet) : null;
    }

    private static MarriageGameSetDto MapToDto(MarriageGameSet gameSet)
    {
        return new MarriageGameSetDto
        {
            Id = gameSet.Id,
            Name = gameSet.Name,
            LastPlayed = gameSet.LastPlayed,
            Created = gameSet.Created,
            IsActive = gameSet.IsActive,
            GameSettingsId = gameSet.GameSettingsId
        };
    }
}