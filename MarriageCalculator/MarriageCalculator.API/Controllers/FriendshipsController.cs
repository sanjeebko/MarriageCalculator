using MarriageCalculator.Core.DTOs;
using MarriageCalculator.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MarriageCalculator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FriendshipsController : ControllerBase
{
    private readonly IFriendshipService _friendshipService;
    private readonly IFriendInviteService _friendInviteService;
    private readonly ILogger<FriendshipsController> _logger;

    public FriendshipsController(
        IFriendshipService friendshipService,
        IFriendInviteService friendInviteService,
        ILogger<FriendshipsController> logger)
    {
        _friendshipService = friendshipService;
        _friendInviteService = friendInviteService;
        _logger = logger;
    }

    /// <summary>
    /// Get list of accepted friends
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetFriends()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var friends = await _friendshipService.GetFriendsAsync(userId);
            return Ok(friends);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving friends");
            return StatusCode(500, "An error occurred while retrieving friends");
        }
    }

    /// <summary>
    /// Get list of pending requests received
    /// </summary>
    [HttpGet("pending")]
    public async Task<ActionResult<IEnumerable<FriendshipDto>>> GetPendingRequests()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var requests = await _friendshipService.GetPendingRequestsAsync(userId);
            return Ok(requests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending friend requests");
            return StatusCode(500, "An error occurred while retrieving pending requests");
        }
    }

    /// <summary>
    /// Get list of pending requests sent
    /// </summary>
    [HttpGet("sent")]
    public async Task<ActionResult<IEnumerable<FriendshipDto>>> GetSentRequests()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var requests = await _friendshipService.GetSentRequestsAsync(userId);
            return Ok(requests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sent friend requests");
            return StatusCode(500, "An error occurred while retrieving sent requests");
        }
    }

    /// <summary>
    /// Send a friend request by complete email address (requirement §4.4).
    /// If the email is registered a pending request is created; otherwise an invitation
    /// email is sent. The response message is identical in both cases (anti-enumeration).
    /// </summary>
    [HttpPost("request")]
    public async Task<ActionResult<FriendRequestResultDto>> SendFriendRequest([FromBody] SendFriendRequestDto requestDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var result = await _friendshipService.SendFriendRequestAsync(userId, requestDto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending friend request");
            return StatusCode(500, "An error occurred while sending friend request");
        }
    }

    /// <summary>
    /// Get my shareable friend invite code, creating one (valid 7 days, multi-use) if none is active.
    /// </summary>
    [HttpPost("invite-code")]
    public async Task<ActionResult<InviteCodeDto>> GetInviteCode()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var code = await _friendInviteService.GetOrCreateInviteCodeAsync(userId);
            return Ok(code);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating invite code");
            return StatusCode(500, "An error occurred while creating the invite code");
        }
    }

    /// <summary>
    /// Redeem another player's invite code — creates an instant, auto-accepted friendship
    /// (the code is the owner's consent). Rate-limited; wrong and expired codes get the same error.
    /// </summary>
    [HttpPost("invite-code/redeem")]
    public async Task<ActionResult<RedeemInviteCodeResultDto>> RedeemInviteCode([FromBody] RedeemInviteCodeDto redeemDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var result = await _friendInviteService.RedeemInviteCodeAsync(userId, redeemDto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error redeeming invite code");
            return StatusCode(500, "An error occurred while redeeming the invite code");
        }
    }

    /// <summary>
    /// Convert pending email invites addressed to my email into pending friend requests.
    /// The client calls this after login.
    /// </summary>
    [HttpPost("claim-invites")]
    public async Task<ActionResult<ClaimInvitesResultDto>> ClaimInvites()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var result = await _friendInviteService.ClaimPendingInvitesAsync(userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error claiming pending invites");
            return StatusCode(500, "An error occurred while claiming pending invites");
        }
    }

    /// <summary>
    /// Respond to a pending friend request
    /// </summary>
    [HttpPost("respond/{id}")]
    public async Task<ActionResult<FriendshipDto>> RespondFriendRequest(string id, [FromBody] RespondFriendRequestDto respondDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var updated = await _friendshipService.RespondFriendRequestAsync(id, userId, respondDto);
            if (updated == null)
            {
                return NotFound($"Friend request with ID {id} not found");
            }

            return Ok(updated);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error responding to friend request");
            return StatusCode(500, "An error occurred while responding to friend request");
        }
    }

    /// <summary>
    /// Remove a friend or cancel a request
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> RemoveFriend(string id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var deleted = await _friendshipService.RemoveFriendAsync(id, userId);
            if (!deleted)
            {
                return NotFound($"Friendship with ID {id} not found");
            }

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting friendship or cancelling request");
            return StatusCode(500, "An error occurred while removing friendship");
        }
    }
}
