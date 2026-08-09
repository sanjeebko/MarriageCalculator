using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MarriageCalculator.Core.Models;

public class MarriageGameRound
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public int Sequence { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string MarriageGameSetId { get; set; } = string.Empty;
    public bool Completed { get; set; }
    public bool PaymentCleared { get; set; } = false;

    /// <summary>
    /// Seat order snapshotted from the game set when this round started. Reshuffling between
    /// rounds rewrites the game set's order, so each round keeps the seating it was played with.
    /// Empty for legacy rounds created before this field existed (clients fall back to the game
    /// set's current order).
    /// </summary>
    [BsonRepresentation(BsonType.ObjectId)]
    public List<string> PlayerIds { get; set; } = [];

    [BsonIgnore]
    public List<MarriageGame> MarriageGames { get; set; } = [];

    [BsonIgnore]
    public Dictionary<string, double> TotalScore { get; set; } = [];
}
