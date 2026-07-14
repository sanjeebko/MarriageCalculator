using MarriageCalculator.API.Repositories;
using MarriageCalculator.Core.DTOs;
using MarriageCalculator.Core.Models;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace MarriageCalculator.API.Services;

/// <summary>
/// Private friend discovery via invite codes, plus claiming of pending email invites
/// (requirement §4.4). Redeeming a valid code creates an auto-accepted friendship —
/// sharing the code IS the consent, so the owner never approves.
/// </summary>
public class FriendInviteService : IFriendInviteService
{
    // No 0/O, 1/I/L — codes are read aloud / typed from another phone.
    private const string CodeAlphabet = "23456789ABCDEFGHJKMNPQRSTUVWXYZ";
    private const int CodeLength = 6;
    private const int CodeTtlDays = 7;
    private const int MaxRedeemAttempts = 5;
    private static readonly TimeSpan RedeemAttemptWindow = TimeSpan.FromMinutes(10);

    private readonly IFriendInviteCodeRepository _inviteCodeRepository;
    private readonly IPendingEmailInviteRepository _pendingInviteRepository;
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IUserRepository _userRepository;
    private readonly IFcmService _fcmService;
    private readonly IMemoryCache _cache;

    public FriendInviteService(
        IFriendInviteCodeRepository inviteCodeRepository,
        IPendingEmailInviteRepository pendingInviteRepository,
        IFriendshipRepository friendshipRepository,
        IUserRepository userRepository,
        IFcmService fcmService,
        IMemoryCache cache)
    {
        _inviteCodeRepository = inviteCodeRepository;
        _pendingInviteRepository = pendingInviteRepository;
        _friendshipRepository = friendshipRepository;
        _userRepository = userRepository;
        _fcmService = fcmService;
        _cache = cache;
    }

    public async Task<InviteCodeDto> GetOrCreateInviteCodeAsync(string userId)
    {
        var existing = await _inviteCodeRepository.GetActiveByOwnerAsync(userId);
        if (existing != null)
        {
            return new InviteCodeDto { Code = existing.Code, ExpiresAt = existing.ExpiresAt };
        }

        // Retry on the (rare) collision with an existing code — Code has a unique index.
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var code = GenerateCode();
            if (await _inviteCodeRepository.GetByCodeAsync(code) != null)
            {
                continue;
            }

            var created = await _inviteCodeRepository.CreateAsync(new FriendInviteCode
            {
                Code = code,
                OwnerUserId = userId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(CodeTtlDays),
            });
            return new InviteCodeDto { Code = created.Code, ExpiresAt = created.ExpiresAt };
        }

        throw new InvalidOperationException("Could not generate an invite code. Please retry.");
    }

    public async Task<RedeemInviteCodeResultDto> RedeemInviteCodeAsync(string userId, RedeemInviteCodeDto redeemDto)
    {
        var code = redeemDto.Code?.Trim().ToUpperInvariant() ?? string.Empty;
        if (code.Length == 0)
        {
            throw new ArgumentException("Code is required.");
        }

        CheckRedeemRateLimit(userId);

        var inviteCode = await _inviteCodeRepository.GetByCodeAsync(code);
        // Same message for wrong and expired codes — no oracle for guessing (requirement §4.4).
        if (inviteCode == null || inviteCode.ExpiresAt < DateTime.UtcNow)
        {
            throw new ArgumentException("Invalid or expired code.");
        }

        if (inviteCode.OwnerUserId == userId)
        {
            throw new ArgumentException("You cannot redeem your own invite code.");
        }

        var owner = await _userRepository.GetByUserIdAsync(inviteCode.OwnerUserId)
            ?? throw new ArgumentException("Invalid or expired code.");

        Friendship friendship;
        var existing = await _friendshipRepository.GetByUsersAsync(userId, owner.UserId);
        if (existing != null)
        {
            if (existing.Status == "Accepted")
            {
                throw new ArgumentException("You are already friends with this user.");
            }

            // Pending or Rejected: the code proves both sides' consent — accept it.
            existing.Status = "Accepted";
            existing.ActionAt = DateTime.UtcNow;
            friendship = await _friendshipRepository.UpdateAsync(existing.Id, existing) ?? existing;
        }
        else
        {
            friendship = await _friendshipRepository.CreateAsync(new Friendship
            {
                RequesterUserId = userId,
                ReceiverUserId = owner.UserId,
                Status = "Accepted",
                Source = "Code",
                CreatedAt = DateTime.UtcNow,
                ActionAt = DateTime.UtcNow,
            });
        }

        var redeemer = await _userRepository.GetByUserIdAsync(userId);
        await NotifyOwnerAsync(owner, redeemer?.DisplayName ?? "Someone");

        var ownerLabel = string.IsNullOrEmpty(owner.DisplayName) ? "your friend" : owner.DisplayName;
        return new RedeemInviteCodeResultDto
        {
            Message = $"Code correct! You are now friends with {ownerLabel} ({MaskEmail(owner.Email)}).",
            Friendship = await MapToDtoAsync(friendship),
        };
    }

    public async Task<ClaimInvitesResultDto> ClaimPendingInvitesAsync(string userId)
    {
        var user = await _userRepository.GetByUserIdAsync(userId);
        if (user == null || string.IsNullOrEmpty(user.Email))
        {
            return new ClaimInvitesResultDto { Claimed = 0 };
        }

        var invites = await _pendingInviteRepository.GetPendingByEmailAsync(user.Email.Trim().ToLowerInvariant());
        var claimed = 0;

        foreach (var invite in invites)
        {
            var existing = await _friendshipRepository.GetByUsersAsync(invite.InviterUserId, userId);
            if (existing == null)
            {
                var friendship = await _friendshipRepository.CreateAsync(new Friendship
                {
                    RequesterUserId = invite.InviterUserId,
                    ReceiverUserId = userId,
                    Status = "Pending",
                    Source = "Email",
                    CreatedAt = DateTime.UtcNow,
                });

                var inviter = await _userRepository.GetByUserIdAsync(invite.InviterUserId);
                await NotifyReceiverOfRequestAsync(user, friendship.Id, inviter?.DisplayName ?? "Someone");
                claimed++;
            }

            await _pendingInviteRepository.MarkClaimedAsync(invite.Id);
        }

        return new ClaimInvitesResultDto { Claimed = claimed };
    }

    // ---------- helpers ----------

    private sealed class AttemptCounter { public int Count; }

    /// <summary>5 redemption attempts per 10 minutes per user — codes are short, so brute force must be expensive.</summary>
    private void CheckRedeemRateLimit(string userId)
    {
        var counter = _cache.GetOrCreate($"invite_redeem_attempts:{userId}", entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = RedeemAttemptWindow;
            return new AttemptCounter();
        })!;

        if (Interlocked.Increment(ref counter.Count) > MaxRedeemAttempts)
        {
            throw new ArgumentException("Too many attempts. Please try again in a few minutes.");
        }
    }

    private static string GenerateCode()
    {
        var bytes = RandomNumberGenerator.GetBytes(CodeLength);
        var chars = new char[CodeLength];
        for (var i = 0; i < CodeLength; i++)
        {
            chars[i] = CodeAlphabet[bytes[i] % CodeAlphabet.Length];
        }
        return new string(chars);
    }

    /// <summary>sanjeeb@gmail.com → s***@g***.com — never expose the full address to a code redeemer.</summary>
    public static string MaskEmail(string email)
    {
        var at = email.IndexOf('@');
        var dot = email.LastIndexOf('.');
        if (at < 1 || dot <= at)
        {
            return "***";
        }
        return $"{email[0]}***@{email[at + 1]}***{email[dot..]}";
    }

    private async Task NotifyOwnerAsync(User owner, string redeemerName)
    {
        if (string.IsNullOrEmpty(owner.FcmToken)) return;

        await _fcmService.SendDataMessageAsync(owner.FcmToken, new Dictionary<string, string>
        {
            { "type", "FRIEND_ADDED_VIA_CODE" },
            { "friendName", redeemerName },
        });
    }

    private async Task NotifyReceiverOfRequestAsync(User receiver, string friendshipId, string requesterName)
    {
        if (string.IsNullOrEmpty(receiver.FcmToken)) return;

        await _fcmService.SendDataMessageAsync(receiver.FcmToken, new Dictionary<string, string>
        {
            { "type", "FRIEND_REQUEST" },
            { "friendshipId", friendshipId },
            { "requesterName", requesterName },
        });
    }

    private async Task<FriendshipDto> MapToDtoAsync(Friendship f)
    {
        var requester = await _userRepository.GetByUserIdAsync(f.RequesterUserId);
        var receiver = await _userRepository.GetByUserIdAsync(f.ReceiverUserId);

        return new FriendshipDto
        {
            Id = f.Id,
            RequesterUserId = f.RequesterUserId,
            RequesterName = requester?.DisplayName ?? "Unknown User",
            RequesterEmail = requester?.Email ?? string.Empty,
            ReceiverUserId = f.ReceiverUserId,
            ReceiverName = receiver?.DisplayName ?? "Unknown User",
            ReceiverEmail = receiver?.Email ?? string.Empty,
            Status = f.Status,
            CreatedAt = f.CreatedAt,
        };
    }
}
