using MarriageCalculator.API.DTOs;
using MarriageCalculator.API.Repositories;
using MarriageCalculator.Core.Models;

namespace MarriageCalculator.API.Services;

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

    public async Task<PlayerDto?> GetPlayerByIdAsync(int id)
    {
        var player = await _playerRepository.GetByIdAsync(id);
        return player != null ? MapToDto(player) : null;
    }

    public async Task<PlayerDto> CreatePlayerAsync(CreatePlayerDto createPlayerDto)
    {
        var player = new Player
        {
            Name = createPlayerDto.Name,
            Email = createPlayerDto.Email,
            Deleted = false
        };

        var createdPlayer = await _playerRepository.CreateAsync(player);
        return MapToDto(createdPlayer);
    }

    public async Task<PlayerDto?> UpdatePlayerAsync(int id, UpdatePlayerDto updatePlayerDto)
    {
        var playerToUpdate = new Player
        {
            Name = updatePlayerDto.Name,
            Email = updatePlayerDto.Email
        };

        var updatedPlayer = await _playerRepository.UpdateAsync(id, playerToUpdate);
        return updatedPlayer != null ? MapToDto(updatedPlayer) : null;
    }

    public async Task<bool> DeletePlayerAsync(int id)
    {
        return await _playerRepository.DeleteAsync(id);
    }

    public async Task<bool> PlayerExistsAsync(int id)
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
            Deleted = player.Deleted
        };
    }
}