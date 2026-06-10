using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MarriageCalculator.Core.Models;

public class Player
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool Deleted { get; set; } = false;

    [BsonIgnore]
    public bool Selected { get; set; } = false;

    public override bool Equals(object? obj)
    {
        if (obj is not Player player)
            throw new ArgumentException($" {nameof(obj)} must be of type {nameof(Player)}.", nameof(obj));

        return string.Equals(player.Name, this.Name, StringComparison.CurrentCultureIgnoreCase)
            && string.Equals(player.Email, this.Email, StringComparison.CurrentCultureIgnoreCase);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name.ToLower(), Email.ToLower());
    }
}
