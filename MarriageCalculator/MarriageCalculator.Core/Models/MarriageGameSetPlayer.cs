using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MarriageCalculator.Core.Models;

public class MarriageGameSetPlayer
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonRepresentation(BsonType.ObjectId)]
    public string MarriageGameSetId { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.ObjectId)]
    public string PlayerId { get; set; } = string.Empty;

    [BsonIgnore]
    public Player Player { get; set; } = null!;
}
