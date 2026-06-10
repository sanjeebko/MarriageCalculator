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

    [BsonIgnore]
    public List<MarriageGame> MarriageGames { get; set; } = [];

    [BsonIgnore]
    public Dictionary<string, double> TotalScore { get; set; } = [];
}
