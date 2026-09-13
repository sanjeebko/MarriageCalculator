using MarriageCalculator.Core.DTOs;
using MarriageCalculator.Core.Models;
using MarriageCalculator.API.Repositories;
using MarriageCalculator.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MarriageCalculator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILoginAuditRepository? _loginAuditRepository;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserService userService, 
        ILogger<UsersController> logger,
        ILoginAuditRepository? loginAuditRepository = null)
    {
        _userService = userService;
        _logger = logger;
        _loginAuditRepository = loginAuditRepository;
    }

    private string GetClientIp()
    {
        try
        {
            if (Request?.Headers != null)
            {
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out var cfIp) && !string.IsNullOrWhiteSpace(cfIp))
                {
                    return cfIp.ToString();
                }
                if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor) && !string.IsNullOrWhiteSpace(forwardedFor))
                {
                    return forwardedFor.ToString().Split(',')[0].Trim();
                }
            }
            return HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    private string GetClientUserAgent()
    {
        try
        {
            return Request?.Headers?.UserAgent.ToString() ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// Authenticate the caller using Bearer token, auto-register them if new, and return their User Profile.
    /// </summary>
    [HttpPost("login")]
    [Authorize]
    public async Task<ActionResult<UserDto>> Login()
    {
        try
        {
            var userDto = await _userService.GetOrCreateUserFromClaimsAsync(User);
            var ip = GetClientIp();
            var userAgent = GetClientUserAgent();
            var now = DateTime.UtcNow;

            _logger.LogInformation(
                "LOGIN AUDIT [Google/Bearer]: User {Email} (UID: {UserId}, Name: {DisplayName}) logged in from IP {Ip} at {TimeUtc:u}. UA: {UserAgent}",
                userDto.Email, userDto.UserId, userDto.DisplayName, ip, now, userAgent);

            if (_loginAuditRepository != null)
            {
                await _loginAuditRepository.RecordLoginAsync(new LoginAudit
                {
                    UserId = userDto.UserId,
                    Email = userDto.Email,
                    DisplayName = userDto.DisplayName,
                    AuthMethod = "Google/Bearer",
                    IpAddress = ip,
                    UserAgent = userAgent,
                    TimestampUtc = now
                });
            }

            await _userService.UpdateLastLoginAsync(userDto.UserId, now, ip);

            return Ok(userDto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Authentication parsing failed.");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing user login/registration");
            return StatusCode(500, "An error occurred during authentication.");
        }
    }

    /// <summary>
    /// Get recent login audit records
    /// </summary>
    [HttpGet("login-audits")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<LoginAuditDto>>> GetRecentLoginAudits([FromQuery] int limit = 50)
    {
        try
        {
            if (_loginAuditRepository == null)
            {
                return Ok(Enumerable.Empty<LoginAuditDto>());
            }

            var audits = await _loginAuditRepository.GetRecentLoginsAsync(limit);
            var dtos = audits.Select(a => new LoginAuditDto
            {
                Id = a.Id,
                UserId = a.UserId,
                Email = a.Email,
                DisplayName = a.DisplayName,
                AuthMethod = a.AuthMethod,
                IpAddress = a.IpAddress,
                UserAgent = a.UserAgent,
                TimestampUtc = a.TimestampUtc
            });
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving login audits");
            return StatusCode(500, "An error occurred while retrieving login audits");
        }
    }

    /// <summary>
    /// Register/update FCM token for the authenticated user
    /// </summary>
    [HttpPost("fcm-token")]
    [Authorize]
    public async Task<ActionResult> RegisterFcmToken([FromBody] RegisterFcmTokenDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID claim missing.");
            }

            var success = await _userService.UpdateFcmTokenAsync(userId, dto.Token);
            if (!success)
            {
                return NotFound("User not found.");
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating FCM token");
            return StatusCode(500, "An error occurred while updating the FCM token.");
        }
    }

    /// <summary>
    /// Search registered users by email or display name
    /// </summary>
    [HttpGet("search")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<UserDto>>> SearchUsers([FromQuery] string query)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Query parameter is required.");
            }
            var users = await _userService.SearchUsersAsync(query);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users with query {Query}", query);
            return StatusCode(500, "An error occurred while searching users");
        }
    }

    /// <summary>
    /// Get all registered users
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return StatusCode(500, "An error occurred while retrieving users");
        }
    }

    /// <summary>
    /// Get user by database ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(string id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found");
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with ID {UserId}", id);
            return StatusCode(500, "An error occurred while retrieving the user");
        }
    }

    /// <summary>
    /// Get user by auth provider UID
    /// </summary>
    [HttpGet("uid/{userId}")]
    public async Task<ActionResult<UserDto>> GetUserByUid(string userId)
    {
        try
        {
            var user = await _userService.GetUserByUserIdAsync(userId);
            if (user == null)
            {
                return NotFound($"User with UID {userId} not found");
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with UID {UserId}", userId);
            return StatusCode(500, "An error occurred while retrieving the user");
        }
    }

    /// <summary>
    /// Update user details
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> UpdateUser(string id, [FromBody] UpdateUserDto updateUserDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userService.UpdateUserAsync(id, updateUserDto);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found");
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user with ID {UserId}", id);
            return StatusCode(500, "An error occurred while updating the user");
        }
    }

    /// <summary>
    /// Delete user account
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUser(string id)
    {
        try
        {
            var deleted = await _userService.DeleteUserAsync(id);
            if (!deleted)
            {
                return NotFound($"User with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user with ID {UserId}", id);
            return StatusCode(500, "An error occurred while deleting the user");
        }
    }
}
