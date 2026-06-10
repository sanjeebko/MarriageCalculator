using MarriageCalculator.Core.DTOs;
using MarriageCalculator.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarriageCalculator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
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
