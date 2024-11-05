using MarriageCalculator.Core.Models;

namespace MarriageCalculator.Services;

public interface IMarriageGameServices
{
    Task<int> AddPlayerAsync(Player model);
    Task<int> DeletePlayer(Player model);
    Task<List<Player>> GetPlayers();
    Task<List<MarriageGame>> GetMarriageGames();

    Task<int> AddMarriageGame(MarriageGame model);

    Task<int> DeleteMarriageGame(MarriageGame model);

    Task<int> UpdateMarriageGame(MarriageGame model);

    Task<List<Currency>> GetCurrency();
}