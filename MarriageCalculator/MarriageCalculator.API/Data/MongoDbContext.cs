using MarriageCalculator.Core.Models;
using MongoDB.Driver;

namespace MarriageCalculator.API.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IMongoClient mongoClient, MongoDbSettings settings)
    {
        _database = mongoClient.GetDatabase(settings.DatabaseName);
    }

    public IMongoCollection<Player> Players =>
        _database.GetCollection<Player>("players");

    public IMongoCollection<User> Users =>
        _database.GetCollection<User>("users");

    public IMongoCollection<Friendship> Friendships =>
        _database.GetCollection<Friendship>("friendships");

    public IMongoCollection<GameSettings> GameSettings =>
        _database.GetCollection<GameSettings>("gameSettings");

    public IMongoCollection<MarriageGameSet> MarriageGameSets =>
        _database.GetCollection<MarriageGameSet>("marriageGameSets");

    public IMongoCollection<MarriageGameSetPlayer> MarriageGameSetPlayers =>
        _database.GetCollection<MarriageGameSetPlayer>("marriageGameSetPlayers");

    public IMongoCollection<MarriageGameRound> MarriageGameRounds =>
        _database.GetCollection<MarriageGameRound>("marriageGameRounds");

    public IMongoCollection<MarriageGame> MarriageGames =>
        _database.GetCollection<MarriageGame>("marriageGames");

    public IMongoCollection<MarriageGameScore> MarriageGameScores =>
        _database.GetCollection<MarriageGameScore>("marriageGameScores");

    public IMongoCollection<FriendInviteCode> FriendInviteCodes =>
        _database.GetCollection<FriendInviteCode>("friendInviteCodes");

    public IMongoCollection<PendingEmailInvite> PendingEmailInvites =>
        _database.GetCollection<PendingEmailInvite>("pendingEmailInvites");

    /// <summary>
    /// Idempotent index setup (called at startup). Unique invite codes, TTL cleanup of
    /// expired codes/invites, and lookup indexes for the friend-discovery flows.
    /// </summary>
    public async Task EnsureIndexesAsync()
    {
        var ttl = new CreateIndexOptions { ExpireAfter = TimeSpan.Zero };

        await FriendInviteCodes.Indexes.CreateManyAsync(new[]
        {
            new CreateIndexModel<FriendInviteCode>(
                Builders<FriendInviteCode>.IndexKeys.Ascending(c => c.Code),
                new CreateIndexOptions { Unique = true }),
            new CreateIndexModel<FriendInviteCode>(
                Builders<FriendInviteCode>.IndexKeys.Ascending(c => c.OwnerUserId)),
            new CreateIndexModel<FriendInviteCode>(
                Builders<FriendInviteCode>.IndexKeys.Ascending(c => c.ExpiresAt), ttl),
        });

        await PendingEmailInvites.Indexes.CreateManyAsync(new[]
        {
            new CreateIndexModel<PendingEmailInvite>(
                Builders<PendingEmailInvite>.IndexKeys
                    .Ascending(i => i.InviteeEmail).Ascending(i => i.Status)),
            new CreateIndexModel<PendingEmailInvite>(
                Builders<PendingEmailInvite>.IndexKeys.Ascending(i => i.ExpiresAt), ttl),
        });
    }

    public async Task<bool> CanConnectAsync()
    {
        try
        {
            await _database.RunCommandAsync((Command<MongoDB.Bson.BsonDocument>)"{ping:1}");
            return true;
        }
        catch
        {
            return false;
        }
    }

    public IMongoDatabase GetDatabase() => _database;
}
