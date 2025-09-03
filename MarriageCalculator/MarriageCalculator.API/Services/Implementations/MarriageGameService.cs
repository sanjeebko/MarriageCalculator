using MarriageCalculator.Core.DTOs;
using MarriageCalculator.API.Repositories.Interfaces;
using MarriageCalculator.Core.Models;
using MarriageCalculator.API.Services.Interfaces;

namespace MarriageCalculator.API.Services.Implementations;

public class MarriageGameService : IMarriageGameService
{
    private readonly IMarriageGameRepository _gameRepository;

    public MarriageGameService(IMarriageGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public async Task<IEnumerable<MarriageGameDto>> GetAllGamesAsync()
    {
        var games = await _gameRepository.GetAllAsync();
        return games.Select(MapToDto);
    }

    public async Task<MarriageGameDto?> GetGameByIdAsync(int id)
    {
        var game = await _gameRepository.GetByIdAsync(id);
        return game != null ? MapToDto(game) : null;
    }

    public async Task<MarriageGameDto> CreateGameAsync(CreateMarriageGameDto createDto)
    {
        var game = new MarriageGame
        {
            Sequence = createDto.Sequence,
            MarriageGameRoundId = createDto.MarriageGameRoundId,
            WinnerId = createDto.WinnerId,
            DealerId = createDto.DealerId,
            TotalMaal = createDto.TotalMaal,
            ClosedRound = createDto.ClosedRound,
            CreatedTime = DateTime.UtcNow
        };

        var createdGame = await _gameRepository.CreateAsync(game);
        return MapToDto(createdGame);
    }

    public async Task<MarriageGameDto?> UpdateGameAsync(int id, CreateMarriageGameDto updateDto)
    {
        var gameToUpdate = new MarriageGame
        {
            Sequence = updateDto.Sequence,
            MarriageGameRoundId = updateDto.MarriageGameRoundId,
            WinnerId = updateDto.WinnerId,
            DealerId = updateDto.DealerId,
            TotalMaal = updateDto.TotalMaal,
            ClosedRound = updateDto.ClosedRound
        };

        var updatedGame = await _gameRepository.UpdateAsync(id, gameToUpdate);
        return updatedGame != null ? MapToDto(updatedGame) : null;
    }

    public async Task<bool> DeleteGameAsync(int id)
    {
        return await _gameRepository.DeleteGameAsync(id);
    }

    public async Task<bool> GameExistsAsync(int id)
    {
        return await _gameRepository.ExistsAsync(id);
    }

    public async Task<IEnumerable<MarriageGameDto>> GetGamesByRoundIdAsync(int roundId)
    {
        var games = await _gameRepository.GetByRoundIdAsync(roundId);
        return games.Select(MapToDto);
    }

    private static MarriageGameDto MapToDto(MarriageGame game)
    {
        return new MarriageGameDto
        {
            Id = game.Id,
            Sequence = game.Sequence,
            MarriageGameRoundId = game.MarriageGameRoundId,
            WinnerId = game.WinnerId,
            DealerId = game.DealerId,
            TotalMaal = game.TotalMaal,
            ClosedRound = game.ClosedRound,
            CreatedTime = game.CreatedTime
        };
    }
}