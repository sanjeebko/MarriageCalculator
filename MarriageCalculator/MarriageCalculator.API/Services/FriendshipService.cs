using MarriageCalculator.Core.DTOs;
using MarriageCalculator.API.Repositories;
using MarriageCalculator.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarriageCalculator.API.Services;

public class FriendshipService : IFriendshipService
{
    private const int EmailInviteTtlDays = 30;

    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IUserRepository _userRepository;
    private readonly IFcmService _fcmService;
    private readonly IPendingEmailInviteRepository _pendingInviteRepository;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public FriendshipService(
        IFriendshipRepository friendshipRepository,
        IUserRepository userRepository,
        IFcmService fcmService,
        IPendingEmailInviteRepository pendingInviteRepository,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _friendshipRepository = friendshipRepository;
        _userRepository = userRepository;
        _fcmService = fcmService;
        _pendingInviteRepository = pendingInviteRepository;
        _emailService = emailService;
        _configuration = configuration;
    }

    public async Task<IEnumerable<FriendshipDto>> GetPendingRequestsAsync(string userId)
    {
        var friendships = await _friendshipRepository.GetAllForUserAsync(userId);
        var pending = friendships.Where(f => f.ReceiverUserId == userId && f.Status == "Pending");
        
        var list = new List<FriendshipDto>();
        foreach (var f in pending)
        {
            list.Add(await MapToDtoAsync(f));
        }
        return list;
    }

    public async Task<IEnumerable<FriendshipDto>> GetSentRequestsAsync(string userId)
    {
        var friendships = await _friendshipRepository.GetAllForUserAsync(userId);
        var sent = friendships.Where(f => f.RequesterUserId == userId && f.Status == "Pending");
        
        var list = new List<FriendshipDto>();
        foreach (var f in sent)
        {
            list.Add(await MapToDtoAsync(f));
        }
        return list;
    }

    public async Task<IEnumerable<UserDto>> GetFriendsAsync(string userId)
    {
        var friendships = await _friendshipRepository.GetAllForUserAsync(userId);
        var accepted = friendships.Where(f => f.Status == "Accepted");
        
        var friendIds = new List<string>();
        foreach (var f in accepted)
        {
            if (f.RequesterUserId == userId)
                friendIds.Add(f.ReceiverUserId);
            else if (f.ReceiverUserId == userId)
                friendIds.Add(f.RequesterUserId);
        }

        var friendsList = new List<UserDto>();
        foreach (var fId in friendIds)
        {
            var user = await _userRepository.GetByUserIdAsync(fId);
            if (user != null)
            {
                friendsList.Add(new UserDto
                {
                    Id = user.Id,
                    UserId = user.UserId,
                    DisplayName = user.DisplayName,
                    Email = user.Email,
                    CreatedAt = user.CreatedAt
                });
            }
        }
        return friendsList;
    }

    public async Task<FriendRequestResultDto> SendFriendRequestAsync(string requesterUserId, SendFriendRequestDto requestDto)
    {
        // Requirement §4.4 Private Friend Discovery: complete email address only.
        // Display-name / partial search intentionally removed — it allowed harvesting users.
        var email = requestDto.ReceiverEmailOrUsername.Trim().ToLowerInvariant();
        if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
        {
            throw new ArgumentException("Enter the complete email address of the player you want to add.");
        }

        var requesterUser = await _userRepository.GetByUserIdAsync(requesterUserId);
        var requesterName = requesterUser?.DisplayName ?? "Someone";
        if (requesterUser != null && requesterUser.Email.Trim().ToLowerInvariant() == email)
        {
            throw new ArgumentException("Cannot send a friend request to yourself.");
        }

        // Anti-enumeration: both branches below MUST produce this same message so the
        // response never reveals whether an email is registered.
        var genericMessage = $"Request sent to {email}.";

        var receiver = await _userRepository.GetByEmailAsync(requestDto.ReceiverEmailOrUsername.Trim())
                       ?? await _userRepository.GetByEmailAsync(email);

        if (receiver == null)
        {
            // Unknown email → store an invite and email the person; claimed at their first login.
            await CreateEmailInviteAsync(requesterUserId, requesterName, email);
            return new FriendRequestResultDto { Status = "RequestSent", Message = genericMessage };
        }

        if (receiver.UserId == requesterUserId)
        {
            throw new ArgumentException("Cannot send a friend request to yourself.");
        }

        var existing = await _friendshipRepository.GetByUsersAsync(requesterUserId, receiver.UserId);
        if (existing != null)
        {
            if (existing.Status == "Accepted")
            {
                throw new ArgumentException("You are already friends with this user.");
            }
            if (existing.Status == "Pending")
            {
                if (existing.RequesterUserId == requesterUserId)
                {
                    throw new ArgumentException("Friend request is already pending.");
                }

                // Receiver sent request previously, auto-accept it!
                existing.Status = "Accepted";
                existing.ActionAt = DateTime.UtcNow;
                await _friendshipRepository.UpdateAsync(existing.Id, existing);
                await SendFriendAcceptedPushAsync(existing.RequesterUserId, requesterName);
                return new FriendRequestResultDto
                {
                    Status = "AutoAccepted",
                    Message = $"You are now friends with {receiver.DisplayName}.",
                    Friendship = await MapToDtoAsync(existing),
                };
            }

            // Re-open rejected request
            existing.RequesterUserId = requesterUserId;
            existing.ReceiverUserId = receiver.UserId;
            existing.Status = "Pending";
            existing.CreatedAt = DateTime.UtcNow;
            existing.ActionAt = null;
            await _friendshipRepository.UpdateAsync(existing.Id, existing);
            await SendFriendRequestPushAsync(existing.ReceiverUserId, existing.Id, requesterName);
            return new FriendRequestResultDto
            {
                Status = "RequestSent",
                Message = genericMessage,
                Friendship = await MapToDtoAsync(existing),
            };
        }

        var friendship = new Friendship
        {
            RequesterUserId = requesterUserId,
            ReceiverUserId = receiver.UserId,
            Status = "Pending",
            Source = "Email",
            CreatedAt = DateTime.UtcNow
        };

        var created = await _friendshipRepository.CreateAsync(friendship);
        await SendFriendRequestPushAsync(created.ReceiverUserId, created.Id, requesterName);
        return new FriendRequestResultDto
        {
            Status = "RequestSent",
            Message = genericMessage,
            Friendship = await MapToDtoAsync(created),
        };
    }

    /// <summary>Stores a pending email invite (deduplicated) and sends the invitation email.</summary>
    private async Task CreateEmailInviteAsync(string inviterUserId, string inviterName, string email)
    {
        var existing = await _pendingInviteRepository.GetPendingByInviterAndEmailAsync(inviterUserId, email);
        if (existing != null)
        {
            return; // Same inviter already invited this address — don't spam.
        }

        await _pendingInviteRepository.CreateAsync(new PendingEmailInvite
        {
            InviterUserId = inviterUserId,
            InviteeEmail = email,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(EmailInviteTtlDays),
        });

        var downloadUrl = _configuration["App:DownloadUrl"] ?? "https://marriagecalculator.com";
        await _emailService.SendAsync(
            email,
            $"{inviterName} invited you to Marriage Calculator!",
            $"<p><b>{inviterName}</b> wants to add you as a friend on Marriage Calculator, " +
            $"the digital scorer for the Marriage card game.</p>" +
            $"<p><a href=\"{downloadUrl}\">Download the app</a>, then sign in with Google using " +
            $"<b>this email address</b> — the friend request will be waiting for you.</p>");
    }

    public async Task<FriendshipDto?> RespondFriendRequestAsync(string id, string receiverUserId, RespondFriendRequestDto respondDto)
    {
        var friendship = await _friendshipRepository.GetByIdAsync(id);
        if (friendship == null)
        {
            return null;
        }

        if (friendship.ReceiverUserId != receiverUserId)
        {
            throw new UnauthorizedAccessException("Not authorized to respond to this friend request.");
        }

        if (friendship.Status != "Pending")
        {
            throw new ArgumentException("Friend request is already resolved.");
        }

        friendship.Status = respondDto.Accept ? "Accepted" : "Rejected";
        friendship.ActionAt = DateTime.UtcNow;

        var updated = await _friendshipRepository.UpdateAsync(id, friendship);

        if (respondDto.Accept && updated != null)
        {
            var accepter = await _userRepository.GetByUserIdAsync(receiverUserId);
            await SendFriendAcceptedPushAsync(friendship.RequesterUserId, accepter?.DisplayName ?? "Someone");
        }

        return updated != null ? await MapToDtoAsync(updated) : null;
    }

    /// <summary>Notifies the receiver of a new (or reopened) pending friend request.</summary>
    private async Task SendFriendRequestPushAsync(string receiverUserId, string friendshipId, string requesterName)
    {
        var receiver = await _userRepository.GetByUserIdAsync(receiverUserId);
        if (receiver == null || string.IsNullOrEmpty(receiver.FcmToken)) return;

        await _fcmService.SendDataMessageAsync(receiver.FcmToken, new Dictionary<string, string>
        {
            { "type", "FRIEND_REQUEST" },
            { "friendshipId", friendshipId },
            { "requesterName", requesterName }
        });
    }

    /// <summary>Notifies the original requester that their friend request was accepted.</summary>
    private async Task SendFriendAcceptedPushAsync(string notifyUserId, string accepterName)
    {
        var notifyUser = await _userRepository.GetByUserIdAsync(notifyUserId);
        if (notifyUser == null || string.IsNullOrEmpty(notifyUser.FcmToken)) return;

        await _fcmService.SendDataMessageAsync(notifyUser.FcmToken, new Dictionary<string, string>
        {
            { "type", "FRIEND_ACCEPTED" },
            { "requesterName", accepterName }
        });
    }

    public async Task<bool> RemoveFriendAsync(string id, string userId)
    {
        Friendship? friendship = null;
        if (MongoDB.Bson.ObjectId.TryParse(id, out _))
        {
            friendship = await _friendshipRepository.GetByIdAsync(id);
        }

        if (friendship == null)
        {
            friendship = await _friendshipRepository.GetByUsersAsync(userId, id);
        }

        if (friendship == null)
        {
            return false;
        }

        if (friendship.RequesterUserId != userId && friendship.ReceiverUserId != userId)
        {
            throw new UnauthorizedAccessException("Not authorized to remove this friendship.");
        }

        return await _friendshipRepository.DeleteAsync(friendship.Id);
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
            CreatedAt = f.CreatedAt
        };
    }
}
