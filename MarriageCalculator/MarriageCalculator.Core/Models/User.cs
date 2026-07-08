using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MarriageCalculator.Core.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string UserId { get; set; } = string.Empty; // Firebase / Provider unique user ID

    public string DisplayName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? FcmToken { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
