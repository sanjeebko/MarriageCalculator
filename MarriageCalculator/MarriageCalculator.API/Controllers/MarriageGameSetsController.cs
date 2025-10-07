using MarriageCalculator.API.Services.Interfaces;
using MarriageCalculator.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarriageCalculator.API.Controllers;

/// <summary>
/// Marriage Game Sets Controller - Manages game sets and their lifecycle
/// 
/// ENDPOINTS SUMMARY:
/// ==================
/// GET    /api/marriagegamesets           - Get game sets for a specific GameSettingsId (query parameter)
/// GET    /api/marriagegamesets/{id}      - Get specific marriage game set by ID
/// GET    /api/marriagegamesets/latest    - Get latest active marriage game set
/// POST   /api/marriagegamesets           - Create new marriage game set
/// PUT    /api/marriagegamesets/{id}      - Update existing marriage game set by ID
/// DELETE /api/marriagegamesets/{id}      - Delete marriage game set by ID
/// 
/// AUTHENTICATION:
/// - All endpoints require authentication ([Authorize])
/// 
/// KEY FEATURES:
/// - Integer-based game set identification
/// - Game set filtering by game settings ID via query parameter
/// - Latest active game set retrieval for quick access
/// - Full CRUD operations with proper error handling and logging
/// - Model validation on create and update operations
/// - Game set lifecycle management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MarriageGameSetsController : ControllerBase
{
    private readonly IMarriageGameSetService _gameSetService;
    private readonly IGameSettingsService _gameSettingsService;
    private readonly ILogger<MarriageGameSetsController> _logger;

    public MarriageGameSetsController(IMarriageGameSetService gameSetService, IGameSettingsService gameSettingsService, ILogger<MarriageGameSetsController> logger)
    {
        _gameSetService = gameSetService;
        _gameSettingsService = gameSettingsService;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/marriagegamesets - Get all game sets for the current authenticated user
    /// Returns all marriage game sets associated with the current user's game settings
    /// </summary>
    /// <returns>200 OK with list of MarriageGameSetDto objects, 401 Unauthorized if invalid token, or 500 on error</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MarriageGameSetDto>>> GetMarriageGameSets()
    {
        try
        {
            // Get current user ID from JWT claims
            var userIdClaim = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("Invalid or missing user ID in JWT token");
                return Unauthorized("Invalid user token");
            }

            var gameSettings = await _gameSettingsService.GetAllGameSettingsAsync(userId);
            List<MarriageGameSetDto> marriageGameSets = new();
            foreach(var settings in gameSettings)
            {
                var sets = await _gameSetService.GetAllGameSetsAsync(settings.Id);
                marriageGameSets.AddRange(sets);
            }
            return Ok(marriageGameSets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving marriage game sets");
            return StatusCode(500, "An error occurred while retrieving marriage game sets");
        }
    }

    /// <summary>
    /// GET /api/marriagegamesets/{id} - Get specific marriage game set by ID
    /// Retrieves a specific marriage game set by its unique integer identifier
    /// </summary>
    /// <param name="id">The integer ID of the marriage game set to retrieve</param>
    /// <returns>200 OK with MarriageGameSetDto object, 404 Not Found if game set doesn't exist, or 500 on error</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<MarriageGameSetDto>> GetMarriageGameSet(int id)
    {
        try
        {
            var gameSet = await _gameSetService.GetGameSetByIdAsync(id);
            if (gameSet == null)
            {
                return NotFound($"Marriage game set with ID {id} not found");
            }

            return Ok(gameSet);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving marriage game set with ID {GameSetId}", id);
            return StatusCode(500, "An error occurred while retrieving the marriage game set");
        }
    }

    /// <summary>
    /// GET /api/marriagegamesets/latest - Get latest active marriage game set for current user
    /// Retrieves the most recently created active marriage game set for the authenticated user
    /// </summary>
    /// <returns>200 OK with MarriageGameSetDto object, 404 Not Found if no active game set exists, 401 Unauthorized if invalid token, or 500 on error</returns>
    [HttpGet("latest")]
    public async Task<ActionResult<MarriageGameSetDto>> GetLatestActiveGameSet()
    {
        try
        {
            // Get current user ID from JWT claims
            var userIdClaim = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("Invalid or missing user ID in JWT token");
                return Unauthorized("Invalid user token");
            }

            var gameSet = await _gameSetService.GetLatestActiveGameSetForUserAsync(userId);
            if (gameSet == null)
            {
                return NotFound("No active marriage game set found for current user");
            }

            return Ok(gameSet);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving latest active marriage game set");
            return StatusCode(500, "An error occurred while retrieving the latest active marriage game set");
        }
    }

    /// <summary>
    /// POST /api/marriagegamesets - Create new marriage game set
    /// Creates a new marriage game set associated with specific game settings.
    /// Will return an error if there's already an active game set for the same GameSettingsId.
    /// </summary>
    /// <param name="createDto">The marriage game set data to create</param>
    /// <returns>201 Created with MarriageGameSetDto object and location header, 400 Bad Request if validation fails or active game set exists, or 500 on error</returns>
    [HttpPost]
    public async Task<ActionResult<MarriageGameSetDto>> CreateMarriageGameSet([FromBody] CreateMarriageGameSetDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if there's already an active game set for this GameSettingsId
            var existingActiveGameSet = await _gameSetService.GetActiveGameSetByGameSettingsIdAsync(createDto.GameSettingsId);
            if (existingActiveGameSet != null)
            {
                return BadRequest("New game can not be created before closing Active gameset.");
            }

            var gameSet = await _gameSetService.CreateGameSetAsync(createDto);
            return CreatedAtAction(nameof(GetMarriageGameSet), new { id = gameSet.Id }, gameSet);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating marriage game set");
            return StatusCode(500, "An error occurred while creating the marriage game set");
        }
    }

    /// <summary>
    /// PUT /api/marriagegamesets/{id} - Update existing marriage game set
    /// Updates an existing marriage game set identified by integer ID with new data
    /// </summary>
    /// <param name="id">The integer ID of the marriage game set to update</param>
    /// <param name="updateDto">The updated marriage game set data</param>
    /// <returns>200 OK with updated MarriageGameSetDto object, 400 Bad Request if validation fails, 404 Not Found if game set doesn't exist, or 500 on error</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<MarriageGameSetDto>> UpdateMarriageGameSet(int id, [FromBody] CreateMarriageGameSetDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var gameSet = await _gameSetService.UpdateGameSetAsync(id, updateDto);
            if (gameSet == null)
            {
                return NotFound($"Marriage game set with ID {id} not found");
            }

            return Ok(gameSet);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating marriage game set with ID {GameSetId}", id);
            return StatusCode(500, "An error occurred while updating the marriage game set");
        }
    }

    /// <summary>
    /// DELETE /api/marriagegamesets/{id} - Delete marriage game set
    /// Permanently deletes a marriage game set identified by integer ID from the system
    /// </summary>
    /// <param name="id">The integer ID of the marriage game set to delete</param>
    /// <returns>204 No Content if successfully deleted, 404 Not Found if game set doesn't exist, or 500 on error</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMarriageGameSet(int id)
    {
        try
        {
            var deleted = await _gameSetService.DeleteGameSetAsync(id);
            if (!deleted)
            {
                return NotFound($"Marriage game set with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting marriage game set with ID {GameSetId}", id);
            return StatusCode(500, "An error occurred while deleting the marriage game set");
        }
    }
}