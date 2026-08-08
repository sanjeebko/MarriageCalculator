using MarriageCalculator.Core.DTOs;
using MarriageCalculator.API.Repositories;
using MarriageCalculator.Core.Models;

namespace MarriageCalculator.API.Services;

public class PlayerService : IPlayerService
{
    private readonly IPlayerRepository _playerRepository;
    private readonly IUserRepository _userRepository;

    public PlayerService(IPlayerRepository playerRepository, IUserRepository userRepository)
    {
        _playerRepository = playerRepository;
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<PlayerDto>> GetAllPlayersAsync()
    {
        var players = await _playerRepository.GetAllAsync();
        var dtoList = new List<PlayerDto>();
        foreach (var player in players)
        {
            dtoList.Add(await MapToDtoAsync(player));
        }
        return dtoList;
    }

    public async Task<PlayerDto?> GetPlayerByIdAsync(string id)
    {
        var player = await _playerRepository.GetByIdAsync(id);
        if (player == null)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user != null)
            {
                return new PlayerDto
                {
                    Id = user.Id,
                    Name = user.DisplayName,
                    Email = user.Email,
                    PhotoUri = user.PhotoUrl,
                    Deleted = false
                };
            }
        }
        return player != null ? await MapToDtoAsync(player) : null;
    }

    public async Task<IEnumerable<PlayerDto>> GetPlayersByCreatedByAsync(string createdByUserId)
    {
        var players = await _playerRepository.GetByCreatedByAsync(createdByUserId);
        var dtoList = new List<PlayerDto>();
        foreach (var player in players)
        {
            dtoList.Add(await MapToDtoAsync(player));
        }
        return dtoList;
    }

    public async Task<PlayerDto> CreatePlayerAsync(CreatePlayerDto createPlayerDto, string createdByUserId)
    {
        var player = new Player
        {
            Name = createPlayerDto.Name,
            Email = createPlayerDto.Email,
            PhotoUri = createPlayerDto.PhotoUri,
            CreatedByUserId = createdByUserId,
            Deleted = false
        };

        var createdPlayer = await _playerRepository.CreateAsync(player);
        return await MapToDtoAsync(createdPlayer);
    }

    public async Task<PlayerDto?> UpdatePlayerAsync(string id, UpdatePlayerDto updatePlayerDto)
    {
        var playerToUpdate = new Player
        {
            Name = updatePlayerDto.Name,
            Email = updatePlayerDto.Email,
            PhotoUri = updatePlayerDto.PhotoUri
        };

        var updatedPlayer = await _playerRepository.UpdateAsync(id, playerToUpdate);
        return updatedPlayer != null ? await MapToDtoAsync(updatedPlayer) : null;
    }

    public async Task<bool> DeletePlayerAsync(string id)
    {
        return await _playerRepository.DeleteAsync(id);
    }

    public async Task<bool> PlayerExistsAsync(string id)
    {
        return await _playerRepository.ExistsAsync(id);
    }

    private async Task<PlayerDto> MapToDtoAsync(Player player)
    {
        var dto = new PlayerDto
        {
            Id = player.Id,
            Name = player.Name,
            Email = player.Email,
            PhotoUri = player.PhotoUri,
            CreatedByUserId = player.CreatedByUserId,
            Deleted = player.Deleted
        };

        if (!string.IsNullOrEmpty(player.Email))
        {
            var user = await _userRepository.GetByEmailAsync(player.Email);
            if (user != null && !string.IsNullOrEmpty(user.PhotoUrl))
            {
                dto.PhotoUri = user.PhotoUrl;
            }
        }

        return dto;
    }
}