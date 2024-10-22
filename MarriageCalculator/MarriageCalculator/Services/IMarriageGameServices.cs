using MarriageCalculator.Core.Models;

namespace MarriageCalculator.Services;

public interface IMarriageGameServices
{
    Task<List<MarriageGame>> GetMarriageGames();

    Task<int> AddMarriageGame(MarriageGame model);

    Task<int> DeleteMarriageGame(MarriageGame model);

    Task<int> UpdateMarriageGame(MarriageGame model);

    Task<List<Currency>> GetCurrency();
}