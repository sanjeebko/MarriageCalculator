using MarriageCalculator.Core.Models;

namespace MarriageCalculator.API.Repositories;

public interface IPlayerRepository
{
    Task<IEnumerable<Player>> GetAllAsync();
    Task<Player?> GetByIdAsync(int id);
    Task<Player> CreateAsync(Player player);
    Task<Player?> UpdateAsync(int id, Player player);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

public interface IGameSettingsRepository
{
    Task<IEnumerable<GameSettings>> GetAllAsync();
    Task<GameSettings?> GetByIdAsync(int id);
    Task<GameSettings> CreateAsync(GameSettings settings);
    Task<GameSettings?> UpdateAsync(int id, GameSettings settings);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

public interface IMarriageGameSetRepository
{
    Task<IEnumerable<MarriageGameSet>> GetAllAsync();
    Task<MarriageGameSet?> GetByIdAsync(int id);
    Task<MarriageGameSet> CreateAsync(MarriageGameSet gameSet);
    Task<MarriageGameSet?> UpdateAsync(int id, MarriageGameSet gameSet);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<MarriageGameSet?> GetLatestActiveAsync();
}

public interface IMarriageGameRepository
{
    Task<IEnumerable<MarriageGame>> GetAllAsync();
    Task<MarriageGame?> GetByIdAsync(int id);
    Task<MarriageGame> CreateAsync(MarriageGame game);
    Task<MarriageGame?> UpdateAsync(int id, MarriageGame game);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<IEnumerable<MarriageGame>> GetByRoundIdAsync(int roundId);
}

public interface IDatabaseRepository
{
    Task<bool> CanConnectAsync();
    Task<int> GetTableCountAsync();
    Task<string> GetProviderNameAsync();
}