using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MarriageCalculator.Core.Models;

public class Friendship
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string RequesterUserId { get; set; } = string.Empty; // User.UserId of sender

    public string ReceiverUserId { get; set; } = string.Empty;  // User.UserId of receiver

    public string Status { get; set; } = "Pending";             // Pending, Accepted, Rejected

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ActionAt { get; set; }
}
