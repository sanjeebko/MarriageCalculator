namespace MarriageCalculator.Services;

public interface IDbService
{
    Task<int> AddPlayerAsync(Player model);
    Task<int> DeletePlayerAsync(Player model);
    Task<List<Player>> GetPlayersAsync();
    Task<List<MarriageGame>> GetMarriageGamesAsync();

    Task<int> AddMarriageGameAsync(MarriageGame model);

    Task<int> DeleteMarriageGameAsync(MarriageGame model);

    Task<int> UpdateMarriageGameAsync(MarriageGame model);
 
}