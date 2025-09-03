using MarriageCalculator.Core.DTOs;
using MarriageCalculator.API.Repositories.Interfaces;
using MarriageCalculator.Core.Models;
using MarriageCalculator.API.Services.Interfaces;

namespace MarriageCalculator.API.Services.Implementations;

public class MarriageGameScoreService : IMarriageGameScoreService
{
    private readonly IMarriageGameScoreRepository _scoreRepository;

    public MarriageGameScoreService(IMarriageGameScoreRepository scoreRepository)
    {
        _scoreRepository = scoreRepository;
    }

    public async Task<IEnumerable<MarriageGameScoreDto>> GetAllScoresAsync()
    {
        var scores = await _scoreRepository.GetAllAsync();
        return scores.Select(MapToDto);
    }

    public async Task<MarriageGameScoreDto?> GetScoreByIdAsync(int id)
    {
        var score = await _scoreRepository.GetByIdAsync(id);
        return score != null ? MapToDto(score) : null;
    }

    public async Task<MarriageGameScoreDto> CreateScoreAsync(CreateMarriageGameScoreDto createDto)
    {
        var score = new MarriageGameScore
        {
            MarriageGameId = createDto.MarriageGameId,
            PlayerId = createDto.PlayerId,
            Seen = createDto.Seen,
            Playing = createDto.Playing,
            Maal = createDto.Maal,
            BonusPoint = createDto.BonusPoint,
            Duply = createDto.Duply,
            Winner = createDto.Winner,
            Score = createDto.Score,
            MoneyWon = createDto.MoneyWon,
            Deal = createDto.Deal,
            Position = createDto.Position
        };

        var createdScore = await _scoreRepository.CreateAsync(score);
        return MapToDto(createdScore);
    }

    public async Task<MarriageGameScoreDto?> UpdateScoreAsync(int id, CreateMarriageGameScoreDto updateDto)
    {
        var scoreToUpdate = new MarriageGameScore
        {
            MarriageGameId = updateDto.MarriageGameId,
            PlayerId = updateDto.PlayerId,
            Seen = updateDto.Seen,
            Playing = updateDto.Playing,
            Maal = updateDto.Maal,
            BonusPoint = updateDto.BonusPoint,
            Duply = updateDto.Duply,
            Winner = updateDto.Winner,
            Score = updateDto.Score,
            MoneyWon = updateDto.MoneyWon,
            Deal = updateDto.Deal,
            Position = updateDto.Position
        };

        var updatedScore = await _scoreRepository.UpdateAsync(id, scoreToUpdate);
        return updatedScore != null ? MapToDto(updatedScore) : null;
    }

    public async Task<bool> DeleteScoreAsync(int id)
    {
        return await _scoreRepository.DeleteAsync(id);
    }

    public async Task<bool> ScoreExistsAsync(int id)
    {
        return await _scoreRepository.ExistsAsync(id);
    }

    public async Task<IEnumerable<MarriageGameScoreDto>> GetScoresByGameIdAsync(int gameId)
    {
        var scores = await _scoreRepository.GetByGameIdAsync(gameId);
        return scores.Select(MapToDto);
    }

    public async Task<IEnumerable<MarriageGameScoreDto>> GetScoresByPlayerIdAsync(Guid playerId)
    {
        var scores = await _scoreRepository.GetByPlayerIdAsync(playerId);
        return scores.Select(MapToDto);
    }

    private static MarriageGameScoreDto MapToDto(MarriageGameScore score)
    {
        return new MarriageGameScoreDto
        {
            Id = score.Id,
            MarriageGameId = score.MarriageGameId,
            PlayerId = score.PlayerId,
            Seen = score.Seen,
            Playing = score.Playing,
            Maal = score.Maal,
            BonusPoint = score.BonusPoint,
            Duply = score.Duply,
            Winner = score.Winner,
            Score = score.Score,
            MoneyWon = score.MoneyWon,
            Deal = score.Deal,
            Position = score.Position
        };
    }
}