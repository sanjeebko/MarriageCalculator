using MarriageCalculator.Core.DTOs;
using MarriageCalculator.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarriageCalculator.API.Controllers;

/// <summary>
/// Game Settings Controller - Manages game configuration and settings
/// 
/// ENDPOINTS SUMMARY:
/// ==================
/// GET    /api/gamesettings       - Get all game settings for current user
/// GET    /api/gamesettings/{id}  - Get specific game settings by ID
/// POST   /api/gamesettings       - Create new game settings (auto-assigns to current user)
/// PUT    /api/gamesettings/{id}  - Update existing game settings by ID
/// DELETE /api/gamesettings/{id}  - Delete game settings by ID
/// 
/// AUTHENTICATION:
/// - All endpoints require authentication ([Authorize])
/// - User identity extracted from JWT claims (NameIdentifier)
/// - GET all endpoint filters by current user's settings
/// 
/// KEY FEATURES:
/// - Integer-based game settings identification
/// - User-scoped settings management (settings tied to creating user)
/// - Full CRUD operations with proper error handling and logging
/// - Model validation on create and update operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GameSettingsController : ControllerBase
{
    private readonly IGameSettingsService _gameSettingsService;
    private readonly ILogger<GameSettingsController> _logger;

    public GameSettingsController(IGameSettingsService gameSettingsService, ILogger<GameSettingsController> logger)
    {
        _gameSettingsService = gameSettingsService;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/gamesettings - Get all game settings for current user
    /// Returns game settings created by the currently authenticated user
    /// </summary>
    /// <returns>200 OK with list of GameSettingsDto objects, 401 Unauthorized if invalid token, or 500 on error</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GameSettingsDto>>> GetGameSettings()
    {
        try
        {
            // Get userId from JWT claims as Guid
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user token");
            }

            var settings = await _gameSettingsService.GetAllGameSettingsAsync(userId);
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving game settings");
            return StatusCode(500, "An error occurred while retrieving game settings");
        }
    }

    /// <summary>
    /// GET /api/gamesettings/{id} - Get specific game settings by ID
    /// Retrieves game settings by their unique integer identifier
    /// </summary>
    /// <param name="id">The integer ID of the game settings to retrieve</param>
    /// <returns>200 OK with GameSettingsDto object, 404 Not Found if settings don't exist, or 500 on error</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<GameSettingsDto>> GetGameSettings(int id)
    {
        try
        {
            var settings = await _gameSettingsService.GetGameSettingsByIdAsync(id);
            if (settings == null)
            {
                return NotFound($"Game settings with ID {id} not found");
            }

            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving game settings with ID {SettingsId}", id);
            return StatusCode(500, "An error occurred while retrieving the game settings");
        }
    }

    /// <summary>
    /// POST /api/gamesettings - Create new game settings
    /// Creates new game settings and automatically assigns the current authenticated user as the creator
    /// The CreatedByUserId will be set to the current user's ID from JWT claims
    /// </summary>
    /// <param name="createDto">The game settings data to create</param>
    /// <returns>201 Created with GameSettingsDto object and location header, 400 Bad Request if validation fails, 401 Unauthorized if invalid token, or 500 on error</returns>
    [HttpPost]
    public async Task<ActionResult<GameSettingsDto>> CreateGameSettings([FromBody] CreateGameSettingsDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get userId from JWT claims as Guid
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user token");
            }

            var settings = await _gameSettingsService.CreateGameSettingsAsync(createDto, userId);
            return CreatedAtAction(nameof(GetGameSettings), new { id = settings.Id }, settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating game settings");
            return StatusCode(500, "An error occurred while creating the game settings");
        }
    }

    /// <summary>
    /// PUT /api/gamesettings/{id} - Update existing game settings
    /// Updates existing game settings identified by integer ID with new data
    /// </summary>
    /// <param name="id">The integer ID of the game settings to update</param>
    /// <param name="updateDto">The updated game settings data</param>
    /// <returns>200 OK with updated GameSettingsDto object, 400 Bad Request if validation fails, 404 Not Found if settings don't exist, or 500 on error</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<GameSettingsDto>> UpdateGameSettings(int id, [FromBody] CreateGameSettingsDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var settings = await _gameSettingsService.UpdateGameSettingsAsync(id, updateDto);
            if (settings == null)
            {
                return NotFound($"Game settings with ID {id} not found");
            }

            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating game settings with ID {SettingsId}", id);
            return StatusCode(500, "An error occurred while updating the game settings");
        }
    }

    /// <summary>
    /// DELETE /api/gamesettings/{id} - Delete game settings
    /// Permanently deletes game settings identified by integer ID from the system
    /// </summary>
    /// <param name="id">The integer ID of the game settings to delete</param>
    /// <returns>204 No Content if successfully deleted, 404 Not Found if settings don't exist, or 500 on error</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteGameSettings(int id)
    {
        try
        {
            var deleted = await _gameSettingsService.DeleteGameSettingsAsync(id);
            if (!deleted)
            {
                return NotFound($"Game settings with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting game settings with ID {SettingsId}", id);
            return StatusCode(500, "An error occurred while deleting the game settings");
        }
    }
}