namespace MarriageCalculator.Services;

public interface IDbService
{
    Task CleanMarriageGameSet();
    Task<int> AddPlayerAsync(Player model);
    Task<int> DeletePlayerAsync(Player model);
    Task<List<Player>> GetPlayersAsync();
    Task<List<MarriageGame>> GetMarriageGamesAsync();

    Task<int> AddMarriageGameAsync(MarriageGame model);

    Task<int> DeleteMarriageGameAsync(MarriageGame model);

    Task<int> UpdateMarriageGameAsync(MarriageGame model);
    Task<MarriageGameSet?> GetLatestMarriageGameSetAsync();
    Task CloseLastMarriageGameSet();
    Task CloseMarriageGameSet(MarriageGameSet model);
    
    Task<MarriageGameSet?> AddNewMarriageGameSetAsync(string? name);
    Task<int> UpdateMarriageGameSetAsync(MarriageGameSet model);
    Task<int> AddMarriageGameRoundAsync(MarriageGameRound model);
    Task<MarriageGameSetPlayer> AddMarriageGameSetPlayerAsync(MarriageGameSetPlayer model);
    Task<GameSettings> AddGameSettingsAsync(GameSettings model);
    Task<GameSettings?> GetGameSettingsAsync(int id);
    Task UpdateMarriageGameScoreAsync(MarriageGameScore marriageGameScore);
    Task<MarriageGameScore> AddMarriageGameScoreAsync(MarriageGameScore model);
}