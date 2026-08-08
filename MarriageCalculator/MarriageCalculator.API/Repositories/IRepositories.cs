using MarriageCalculator.Core.Models;

namespace MarriageCalculator.API.Repositories;

public interface IPlayerRepository
{
    Task<IEnumerable<Player>> GetAllAsync();
    Task<Player?> GetByIdAsync(string id);
    Task<IEnumerable<Player>> GetPlayersByEmailAsync(string email);
    Task<IEnumerable<Player>> GetByCreatedByAsync(string createdByUserId);
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
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<IEnumerable<User>> SearchUsersAsync(string query);
    Task<User> CreateAsync(User user);
    Task<User?> UpdateAsync(string id, User user);
    Task<User?> UpdateFcmTokenAsync(string userId, string fcmToken);
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
    Task<IEnumerable<MarriageGameSet>> GetAllForUserAsync(string userId, List<string> playerIds);
    Task<MarriageGameSet?> GetByIdAsync(string id, string hostUserId);
    Task<MarriageGameSet?> GetByIdRawAsync(string id);
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

public interface IFriendshipRepository
{
    Task<IEnumerable<Friendship>> GetAllForUserAsync(string userId);
    Task<Friendship?> GetByIdAsync(string id);
    Task<Friendship?> GetByUsersAsync(string requesterId, string receiverId);
    Task<Friendship> CreateAsync(Friendship friendship);
    Task<Friendship?> UpdateAsync(string id, Friendship friendship);
    Task<bool> DeleteAsync(string id);
}

public interface IFriendInviteCodeRepository
{
    Task<FriendInviteCode?> GetActiveByOwnerAsync(string ownerUserId);
    Task<FriendInviteCode?> GetByCodeAsync(string code);
    Task<FriendInviteCode> CreateAsync(FriendInviteCode inviteCode);
}

public interface IPendingEmailInviteRepository
{
    Task<IEnumerable<PendingEmailInvite>> GetPendingByEmailAsync(string email);
    Task<PendingEmailInvite?> GetPendingByInviterAndEmailAsync(string inviterUserId, string email);
    Task<PendingEmailInvite> CreateAsync(PendingEmailInvite invite);
    Task<bool> MarkClaimedAsync(string id);
}