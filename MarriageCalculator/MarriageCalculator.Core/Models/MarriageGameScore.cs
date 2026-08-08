using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MarriageCalculator.Core.Models;

public class MarriageGameScore
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonRepresentation(BsonType.ObjectId)]
    public string MarriageGameId { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.ObjectId)]
    public string PlayerId { get; set; } = string.Empty;

    public bool Seen { get; set; } = false;

    public bool Playing { get; set; } = false;

    public int Maal { get; set; } = 0;

    public int BonusPoint { get; set; } = 0;

    public bool Duply { get; set; } = false;

    public bool Winner { get; set; } = false;

    public int Score { get; set; } = 0;

    public double MoneyWon { get; set; }

    public bool Deal { get; set; } = false;

    public int Position { get; set; } = 0;

    [BsonIgnore]
    public MarriageGame? MarriageGame { get; set; }
}
