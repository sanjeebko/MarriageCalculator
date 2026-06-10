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
