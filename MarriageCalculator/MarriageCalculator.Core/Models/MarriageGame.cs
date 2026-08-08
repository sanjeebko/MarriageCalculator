using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MarriageCalculator.Core.Models;

public class MarriageGame
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public int Sequence { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string MarriageGameRoundId { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.ObjectId)]
    public string WinnerId { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.ObjectId)]
    public string DealerId { get; set; } = string.Empty;

    public int TotalMaal { get; set; }
    public bool ClosedRound { get; set; }
    public DateTime CreatedTime { get; set; }

    [BsonIgnore]
    public Dictionary<string, MarriageGameScore> MarriageGameScores { get; set; } = []; //playerId, MarriageGameScore
}
