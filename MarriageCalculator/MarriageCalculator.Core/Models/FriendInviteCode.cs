using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MarriageCalculator.Core.Models;

/// <summary>
/// A shareable friend invite code (requirement §4.4 Private Friend Discovery).
/// Multi-use, valid 7 days; redeeming a valid code creates an auto-accepted friendship.
/// </summary>
public class FriendInviteCode
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    /// <summary>6-char uppercase code, unambiguous alphabet (no 0/O, 1/I/L). Unique index.</summary>
    public string Code { get; set; } = string.Empty;

    public string OwnerUserId { get; set; } = string.Empty; // User.UserId of the code owner

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>CreatedAt + 7 days. TTL index removes expired documents.</summary>
    public DateTime ExpiresAt { get; set; }
}
