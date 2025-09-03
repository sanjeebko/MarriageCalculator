using MarriageCalculator.Core.DTOs;
using MarriageCalculator.API.Repositories.Interfaces;
using MarriageCalculator.Core.Models;
using MarriageCalculator.API.Services.Interfaces;

namespace MarriageCalculator.API.Services.Implementations;

public class PlayerService : IPlayerService
{
    private readonly IPlayerRepository _playerRepository;

    public PlayerService(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    public async Task<IEnumerable<PlayerDto>> GetAllPlayersAsync()
    {
        var players = await _playerRepository.GetAllAsync();
        return players.Select(MapToDto);
    }

    public async Task<IEnumerable<PlayerDto>> GetPlayersByCreatorAsync(Guid userId)
    {
        var players = await _playerRepository.GetByCreatorAsync(userId);
        return players.Select(MapToDto);
    }

    public async Task<PlayerDto?> GetPlayerByIdAsync(Guid id)
    {
        var player = await _playerRepository.GetByIdAsync(id);
        return player != null ? MapToDto(player) : null;
    } 

    public async Task<PlayerDto> CreatePlayerForUserAsync(CreatePlayerDto createPlayerDto, Guid userId)
    {
        var player = new Player
        {
            Name = createPlayerDto.Name,
            Email = createPlayerDto.Email,
            Deleted = false,
            Selected = false,
            CreatedByUserId = userId,
        };

        var createdPlayer = await _playerRepository.CreateForUserAsync(player, userId);
        return MapToDto(createdPlayer);
    }

    public async Task<PlayerDto> EnsureUserPlayerAsync(Guid userId, string displayName, string email)
    {
        Player? existing = null;
        if (!string.IsNullOrWhiteSpace(email))
        {
            existing = await _playerRepository.GetByEmailAsync(email);
            if (existing != null)
            {
                existing = await _playerRepository.SetCreatorByUserIdAsync(existing.Id, userId);
            }
        }
        if (existing == null)
        {
            var mine = await _playerRepository.GetByCreatorAsync(userId);
            existing = mine.FirstOrDefault(p => p.Name.Equals(displayName, StringComparison.OrdinalIgnoreCase));
        }
        if (existing != null)
        {
            return MapToDto(existing);
        }

        var player = new Player
        {
            Name = string.IsNullOrWhiteSpace(displayName) ? (email ?? "Player") : displayName,
            Email = email ?? string.Empty,
            Deleted = false
        };
        var created = await _playerRepository.CreateForUserAsync(player, userId);
        return MapToDto(created);
    }

    public async Task<PlayerDto?> UpdatePlayerAsync(Guid id, UpdatePlayerDto updatePlayerDto)
    {
        var playerToUpdate = new Player
        {
            Name = updatePlayerDto.Name,
            Email = updatePlayerDto.Email
        };

        var updatedPlayer = await _playerRepository.UpdateAsync(id, playerToUpdate);
        return updatedPlayer != null ? MapToDto(updatedPlayer) : null;
    }

    public async Task<bool> DeletePlayerAsync(Guid id)
    {
        return await _playerRepository.DeleteAsync(id);
    }

    public async Task<bool> PlayerExistsAsync(Guid id)
    {
        return await _playerRepository.ExistsAsync(id);
    }

    private static PlayerDto MapToDto(Player player)
    {
        return new PlayerDto
        {
            Id = player.Id,
            Name = player.Name,
            Email = player.Email,
            Deleted = player.Deleted,
            CreatedByUserId = player.CreatedByUserId,
            CreatedAt = player.CreatedAt
        };
    }
}