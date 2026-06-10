using MarriageCalculator.Core.Models;

namespace MarriageCalculator.API.Repositories;

public interface IPlayerRepository
{
    Task<IEnumerable<Player>> GetAllAsync();
    Task<Player?> GetByIdAsync(string id);
    Task<Player> CreateAsync(Player player);
    Task<Player?> UpdateAsync(string id, Player player);
    Task<bool> DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
}

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByUserIdAsync(string userId);
    Task<User> CreateAsync(User user);
    Task<User?> UpdateAsync(string id, User user);
    Task<bool> DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
}

public interface IGameSettingsRepository
{
    Task<IEnumerable<GameSettings>> GetAllByUserIdAsync(string userId);
    Task<GameSettings?> GetByIdAsync(string id, string userId);
    Task<GameSettings> CreateAsync(GameSettings settings);
    Task<GameSettings?> UpdateAsync(string id, GameSettings settings, string userId);
    Task<bool> DeleteAsync(string id, string userId);
    Task<bool> ExistsAsync(string id, string userId);
}

public interface IMarriageGameSetRepository
{
    Task<IEnumerable<MarriageGameSet>> GetAllByHostUserIdAsync(string hostUserId);
    Task<MarriageGameSet?> GetByIdAsync(string id, string hostUserId);
    Task<MarriageGameSet> CreateAsync(MarriageGameSet gameSet);
    Task<MarriageGameSet?> UpdateAsync(string id, MarriageGameSet gameSet, string hostUserId);
    Task<bool> DeleteAsync(string id, string hostUserId);
    Task<bool> ExistsAsync(string id, string hostUserId);
    Task<MarriageGameSet?> GetLatestActiveAsync(string hostUserId);
}

public interface IMarriageGameRepository
{
    Task<IEnumerable<MarriageGame>> GetAllAsync();
    Task<MarriageGame?> GetByIdAsync(string id);
    Task<MarriageGame> CreateAsync(MarriageGame game);
    Task<MarriageGame?> UpdateAsync(string id, MarriageGame game);
    Task<bool> DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
    Task<IEnumerable<MarriageGame>> GetByRoundIdAsync(string roundId);
}

public interface IDatabaseRepository
{
    Task<bool> CanConnectAsync();
    Task<int> GetTableCountAsync();
    Task<string> GetProviderNameAsync();
}