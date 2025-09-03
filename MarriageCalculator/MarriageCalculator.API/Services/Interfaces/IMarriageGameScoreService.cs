using MarriageCalculator.Core.DTOs;

namespace MarriageCalculator.API.Services.Interfaces;

public interface IMarriageGameScoreService
{
    Task<IEnumerable<MarriageGameScoreDto>> GetAllScoresAsync();
    Task<MarriageGameScoreDto?> GetScoreByIdAsync(int id);
    Task<MarriageGameScoreDto> CreateScoreAsync(CreateMarriageGameScoreDto createScoreDto);
    Task<MarriageGameScoreDto?> UpdateScoreAsync(int id, CreateMarriageGameScoreDto updateScoreDto);
    Task<bool> DeleteScoreAsync(int id);
    Task<bool> ScoreExistsAsync(int id);
    Task<IEnumerable<MarriageGameScoreDto>> GetScoresByGameIdAsync(int gameId);
    Task<IEnumerable<MarriageGameScoreDto>> GetScoresByPlayerIdAsync(Guid playerId);
}