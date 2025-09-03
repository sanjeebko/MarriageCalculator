using MarriageCalculator.Core.DTOs;

namespace MarriageCalculator.API.Services.Interfaces;

public interface IMarriageGameRoundService
{
    Task<IEnumerable<MarriageGameRoundDto>> GetAllRoundsAsync();
    Task<MarriageGameRoundDto?> GetRoundByIdAsync(int id);
    Task<MarriageGameRoundDto> CreateRoundAsync(CreateMarriageGameRoundDto createRoundDto);
    Task<MarriageGameRoundDto?> UpdateRoundAsync(int id, CreateMarriageGameRoundDto updateRoundDto);
    Task<bool> DeleteRoundAsync(int id);
    Task<bool> RoundExistsAsync(int id);
    Task<IEnumerable<MarriageGameRoundDto>> GetRoundsByGameSetIdAsync(int gameSetId);
}