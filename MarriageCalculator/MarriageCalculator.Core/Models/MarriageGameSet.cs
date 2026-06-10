using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MarriageCalculator.Core.Models;

public class MarriageGameSet
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string HostUserId { get; set; } = string.Empty; // Creator/Host of this game set

    public string Name { get; set; }

    public DateTime LastPlayed { get; set; }
    public DateTime Created { get; set; }
    public bool IsActive { get; set; } = true;

    [BsonRepresentation(BsonType.ObjectId)]
    public string GameSettingsId { get; set; } = string.Empty;

    [BsonIgnore]
    public GameSettings GameSettings { get; set; } = GameSettings.Default();

    [BsonIgnore]
    public Dictionary<string, MarriageGameSetPlayer> GameSetPlayers { get; set; } = [];

    [BsonIgnore]
    public List<MarriageGameRound> Rounds { get; set; } = [];

    public MarriageGameSet()
    {
        Name = $"{DateTime.Now:yyyyMMdd HHmmss}";
        Created = DateTime.Now;
    }
}
