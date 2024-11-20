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
    Task<MarriageGameSet?> AddNewMarriageGameSetAsync(string? name);
    Task<int> UpdateMarriageGameSetAsync(MarriageGameSet model);
    Task<int> AddMarriageGameRoundAsync(MarriageGameRound model);
    Task<MarriageGameSetPlayer> AddMarriageGameSetPlayerAsync(MarriageGameSetPlayer model);
    Task<GameSettings> AddGameSettingsAsync(GameSettings model);
    Task<GameSettings?> GetGameSettingsAsync(int id);
    Task<Dictionary<int,GameSettings>> GetAllGameSettingsAsync();
    Task<MarriageGameScore> AddMarriageGameScoreAsync(MarriageGameScore model);
    Task UpdateMarriageGameRoundAsync(MarriageGameRound currentMarriageGameRound);    
    Task<List<MarriageGameRound>> GetMarriageGameRoundsByMarriageGameSetIdAsync(int marriageGameSetId);
    Task<List<MarriageGame>> GetMarriageGamesByRoundIdAsync(int id);
    Task <List<MarriageGameScore>> GetMarriageGameScoresByMarriageGameIdAsync(int id);
    Task<List<MarriageGameSetPlayer>> GetMarriageGameSetPlayersByMarriageGameSetIdAsync(int id);
    Task<GameSettings?> GetLastGameSettingsAsync();
    Task<GameSettings?> GetGameSettingsByGameSetIdAsync(int gameSetId);
}