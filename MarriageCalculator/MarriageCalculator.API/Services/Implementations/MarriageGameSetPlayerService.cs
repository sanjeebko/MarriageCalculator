using MarriageCalculator.Core.DTOs;
using MarriageCalculator.API.Repositories.Interfaces;
using MarriageCalculator.Core.Models;
using MarriageCalculator.API.Services.Interfaces;

namespace MarriageCalculator.API.Services.Implementations;

public class MarriageGameSetPlayerService : IMarriageGameSetPlayerService
{
    private readonly IMarriageGameSetPlayerRepository _repository;

    public MarriageGameSetPlayerService(IMarriageGameSetPlayerRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<MarriageGameSetPlayerDto>> GetAllGameSetPlayersAsync()
    {
        var gameSetPlayers = await _repository.GetAllAsync();
        return gameSetPlayers.Select(MapToDto);
    }

    public async Task<MarriageGameSetPlayerDto?> GetGameSetPlayerByIdAsync(int gameSetId, Guid playerId)
    {
        var gameSetPlayer = await _repository.GetByIdAsync(gameSetId, playerId);
        return gameSetPlayer != null ? MapToDto(gameSetPlayer) : null;
    }

    public async Task<IEnumerable<MarriageGameSetPlayerDto>> GetPlayersByGameSetIdAsync(int gameSetId)
    {
        var gameSetPlayers = await _repository.GetByGameSetIdAsync(gameSetId);
        return gameSetPlayers.Select(MapToDto);
    }

    public async Task<IEnumerable<MarriageGameSetPlayerDto>> GetGameSetsByPlayerIdAsync(Guid playerId)
    {
        var gameSetPlayers = await _repository.GetByPlayerIdAsync(playerId);
        return gameSetPlayers.Select(MapToDto);
    }

    public async Task<MarriageGameSetPlayerDto> CreateGameSetPlayerAsync(CreateMarriageGameSetPlayerDto createDto)
    {
        var gameSetPlayer = new MarriageGameSetPlayer
        {
            MarriageGameSetId = createDto.MarriageGameSetId,
            PlayerId = createDto.PlayerId
        };

        var createdGameSetPlayer = await _repository.CreateAsync(gameSetPlayer);
        return MapToDto(createdGameSetPlayer);
    }

    public async Task<bool> DeleteGameSetPlayerAsync(int gameSetId, Guid playerId)
    {
        return await _repository.DeleteAsync(gameSetId, playerId);
    }

    public async Task<bool> DeletePlayersByGameSetIdAsync(int gameSetId)
    {
        return await _repository.DeleteByGameSetIdAsync(gameSetId);
    }

    public async Task<bool> GameSetPlayerExistsAsync(int gameSetId, Guid playerId)
    {
        return await _repository.ExistsAsync(gameSetId, playerId);
    }

    private static MarriageGameSetPlayerDto MapToDto(MarriageGameSetPlayer gameSetPlayer)
    {
        return new MarriageGameSetPlayerDto
        {
            MarriageGameSetId = gameSetPlayer.MarriageGameSetId,
            PlayerId = gameSetPlayer.PlayerId,
            Player = gameSetPlayer.Player            

            
        };
    }
}