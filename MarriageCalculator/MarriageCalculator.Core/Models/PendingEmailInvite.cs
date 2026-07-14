using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MarriageCalculator.Core.Models;

/// <summary>
/// A friend request addressed to an email that has no registered user yet
/// (requirement §4.4 Complete-Email path). Claimed after the invitee signs in
/// with that email, becoming a normal pending Friendship.
/// </summary>
public class PendingEmailInvite
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string InviterUserId { get; set; } = string.Empty; // User.UserId of sender

    public string InviteeEmail { get; set; } = string.Empty;  // stored lowercase

    public string Status { get; set; } = "Pending";           // Pending, Claimed

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>CreatedAt + 30 days. TTL index removes expired documents.</summary>
    public DateTime ExpiresAt { get; set; }
}
