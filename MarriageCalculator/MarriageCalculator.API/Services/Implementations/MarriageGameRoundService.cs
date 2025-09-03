using MarriageCalculator.Core.DTOs;
using MarriageCalculator.API.Repositories.Interfaces;
using MarriageCalculator.Core.Models;
using MarriageCalculator.API.Services.Interfaces;

namespace MarriageCalculator.API.Services.Implementations;

public class MarriageGameRoundService : IMarriageGameRoundService
{
    private readonly IMarriageGameRoundRepository _roundRepository;

    public MarriageGameRoundService(IMarriageGameRoundRepository roundRepository)
    {
        _roundRepository = roundRepository;
    }

    public async Task<IEnumerable<MarriageGameRoundDto>> GetAllRoundsAsync()
    {
        var rounds = await _roundRepository.GetAllAsync();
        return rounds.Select(MapToDto);
    }

    public async Task<MarriageGameRoundDto?> GetRoundByIdAsync(int id)
    {
        var round = await _roundRepository.GetByIdAsync(id);
        return round != null ? MapToDto(round) : null;
    }

    public async Task<MarriageGameRoundDto> CreateRoundAsync(CreateMarriageGameRoundDto createDto)
    {
        var round = new MarriageGameRound
        {
            Sequence = createDto.Sequence,
            MarriageGameSetId = createDto.MarriageGameSetId,
            Completed = createDto.Completed
        };

        var createdRound = await _roundRepository.CreateAsync(round);
        return MapToDto(createdRound);
    }

    public async Task<MarriageGameRoundDto?> UpdateRoundAsync(int id, CreateMarriageGameRoundDto updateDto)
    {
        var roundToUpdate = new MarriageGameRound
        {
            Sequence = updateDto.Sequence,
            MarriageGameSetId = updateDto.MarriageGameSetId,
            Completed = updateDto.Completed
        };

        var updatedRound = await _roundRepository.UpdateAsync(id, roundToUpdate);
        return updatedRound != null ? MapToDto(updatedRound) : null;
    }

    public async Task<bool> DeleteRoundAsync(int id)
    {
        return await _roundRepository.DeleteAsync(id);
    }

    public async Task<bool> RoundExistsAsync(int id)
    {
        return await _roundRepository.ExistsAsync(id);
    }

    public async Task<IEnumerable<MarriageGameRoundDto>> GetRoundsByGameSetIdAsync(int gameSetId)
    {
        var rounds = await _roundRepository.GetByGameSetIdAsync(gameSetId);
        return rounds.Select(MapToDto);
    }

    private static MarriageGameRoundDto MapToDto(MarriageGameRound round)
    {
        return new MarriageGameRoundDto
        {
            Id = round.Id,
            Sequence = round.Sequence,
            MarriageGameSetId = round.MarriageGameSetId,
            Completed = round.Completed
        };
    }
}