using MarriageCalculator.Core.DTOs;
using MarriageCalculator.API.Repositories;
using MarriageCalculator.Core.Models;

namespace MarriageCalculator.API.Services;

public class MarriageGameSetService : IMarriageGameSetService
{
    private readonly IMarriageGameSetRepository _gameSetRepository;
    private readonly IPlayerRepository _playerRepository;

    public MarriageGameSetService(IMarriageGameSetRepository gameSetRepository, IPlayerRepository playerRepository)
    {
        _gameSetRepository = gameSetRepository;
        _playerRepository = playerRepository;
    }

    public async Task<IEnumerable<MarriageGameSetDto>> GetAllGameSetsAsync(string hostUserId, string email)
    {
        var playerIds = new List<string>();
        if (!string.IsNullOrEmpty(email))
        {
            var players = await _playerRepository.GetPlayersByEmailAsync(email);
            playerIds.AddRange(players.Select(p => p.Id));
        }

        var gameSets = await _gameSetRepository.GetAllForUserAsync(hostUserId, playerIds);
        return gameSets.Select(MapToDto);
    }

    public async Task<MarriageGameSetDto?> GetGameSetByIdAsync(string id, string hostUserId, string email)
    {
        var gameSet = await _gameSetRepository.GetByIdRawAsync(id);
        if (gameSet == null) return null;

        // Verify authorization: is owner/host or participant player
        if (gameSet.HostUserId == hostUserId)
        {
            return MapToDto(gameSet);
        }

        if (!string.IsNullOrEmpty(email))
        {
            var players = await _playerRepository.GetPlayersByEmailAsync(email);
            var playerIds = players.Select(p => p.Id).ToList();
            if (gameSet.PlayerIds.Any(pId => playerIds.Contains(pId)))
            {
                return MapToDto(gameSet);
            }
        }

        return null; // Not authorized or not found
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
            IsActive = true,
            PlayerIds = createDto.PlayerIds
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
            PlayerIds = updateDto.PlayerIds,
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

    public async Task<MarriageGameSetDto?> TransferHostAsync(string id, string currentHostUserId, string newHostUserId)
    {
        var gameSet = await _gameSetRepository.GetByIdRawAsync(id);
        if (gameSet == null) return null;

        if (gameSet.HostUserId != currentHostUserId)
        {
            throw new UnauthorizedAccessException("Only the current host can transfer game set ownership.");
        }

        gameSet.HostUserId = newHostUserId;
        gameSet.LastPlayed = DateTime.UtcNow;

        var updated = await _gameSetRepository.UpdateAsync(id, gameSet, currentHostUserId);
        return updated != null ? MapToDto(updated) : null;
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
            GameSettingsId = gameSet.GameSettingsId,
            PlayerIds = gameSet.PlayerIds
        };
    }
}