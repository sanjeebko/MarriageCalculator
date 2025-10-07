using MarriageCalculator.Core.DTOs;
using MarriageCalculator.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarriageCalculator.API.Controllers;

/// <summary>
/// Players Controller - Manages player entities and user-player relationships
/// 
/// ENDPOINTS SUMMARY:
/// ==================
/// GET    /api/players           - Get all players
/// GET    /api/players/my        - Get players created by current authenticated user
/// GET    /api/players/{id}      - Get specific player by GUID
/// POST   /api/players           - Create new player (auto-assigns to current user)
/// POST   /api/players/ensure-me - Ensure current user exists as a player (creates if missing)
/// PUT    /api/players/{id}      - Update existing player by GUID
/// DELETE /api/players/{id}      - Delete player by GUID
/// 
/// AUTHENTICATION:
/// - All endpoints require authentication ([Authorize])
/// - User identity extracted from JWT claims (NameIdentifier, Name, Email)
/// 
/// KEY FEATURES:
/// - GUID-based player identification
/// - User-scoped player management (players tied to creating user)
/// - Auto-creation of user-player relationship via ensure-me endpoint
/// - Full CRUD operations with proper error handling and logging
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlayersController : ControllerBase
{
    private readonly IPlayerService _playerService;
    private readonly ILogger<PlayersController> _logger;

    public PlayersController(IPlayerService playerService, ILogger<PlayersController> logger)
    {
        _playerService = playerService;
        _logger = logger;
    }

     

    /// <summary>
    /// GET /api/players - Get players created by the current authenticated user
    /// Returns only the players that were created by the currently authenticated user
    /// </summary>
    /// <returns>200 OK with list of PlayerDto objects, 401 Unauthorized if invalid token, or 500 on error</returns>
    [HttpGet()]
    public async Task<ActionResult<IEnumerable<PlayerDto>>> GetAllPlayers()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user token");
            }
            var players = await _playerService.GetPlayersByCreatorAsync(userId);
            return Ok(players);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving players for current user");
            return StatusCode(500, "An error occurred while retrieving players");
        }
    }

    /// <summary>
    /// POST /api/players/ensure-me - Ensure the current authenticated user exists as a Player
    /// Creates a player record for the current user if one doesn't exist, otherwise returns existing player
    /// Uses user claims (NameIdentifier, Name, Email) to create/find the player
    /// </summary>
    /// <returns>200 OK with PlayerDto object, 401 Unauthorized if invalid token, or 500 on error</returns>
    [HttpPost("ensure-me")]
    public async Task<ActionResult<PlayerDto>> EnsureMe()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var displayName = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("EnsureMe called with invalid user token. UserIdClaim: {UserIdClaim}", userIdClaim);
                return Unauthorized("Invalid user token");
            }

            _logger.LogInformation("EnsureMe called for user {UserId} with display name '{DisplayName}' and email '{Email}'", userId, displayName, email);

            var player = await _playerService.EnsureUserPlayerAsync(userId, displayName, email);
            _logger.LogInformation("Successfully ensured player {PlayerId} for user {UserId}", player.Id, userId);
            return Ok(player);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("User not found"))
        {
            _logger.LogError(ex, "User not found in database when ensuring player exists");
            return StatusCode(500, "User account not properly initialized. Please try logging out and back in.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring current user exists as player");
            return StatusCode(500, "An error occurred while creating player for current user");
        }
    }

    /// <summary>
    /// GET /api/players/{id} - Get player by ID (GUID)
    /// Retrieves a specific player by their unique GUID identifier
    /// </summary>
    /// <param name="id">The GUID string of the player to retrieve</param>
    /// <returns>200 OK with PlayerDto object, 400 Bad Request if invalid GUID, 404 Not Found if player doesn't exist, or 500 on error</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<PlayerDto>> GetPlayer(string id)
    {
        try
        {
            if (!Guid.TryParse(id, out var playerId))
            {
                return BadRequest("Invalid GUID format");
            }

            var player = await _playerService.GetPlayerByIdAsync(playerId);
            if (player == null)
            {
                return NotFound($"Player with ID {id} not found");
            }

            return Ok(player);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving player with ID {PlayerId}", id);
            return StatusCode(500, "An error occurred while retrieving the player");
        }
    }

    /// <summary>
    /// POST /api/players - Create a new player
    /// Creates a new player and automatically assigns the current authenticated user as the creator
    /// The CreatedByUserId will be set to the current user's ID from JWT claims
    /// </summary>
    /// <param name="createPlayerDto">The player data to create</param>
    /// <returns>201 Created with PlayerDto object and location header, 400 Bad Request if validation fails, 401 Unauthorized if invalid token, or 500 on error</returns>
    [HttpPost]
    public async Task<ActionResult<PlayerDto>> CreatePlayer([FromBody] CreatePlayerDto createPlayerDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user token");
            }

            var player = await _playerService.CreatePlayerForUserAsync(createPlayerDto, userId);
            return CreatedAtAction(nameof(GetPlayer), new { id = player.Id }, player);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating player");
            return StatusCode(500, "An error occurred while creating the player");
        }
    }

    /// <summary>
    /// PUT /api/players/{id} - Update an existing player
    /// Updates an existing player identified by GUID with new data
    /// </summary>
    /// <param name="id">The GUID string of the player to update</param>
    /// <param name="updatePlayerDto">The updated player data</param>
    /// <returns>200 OK with updated PlayerDto object, 400 Bad Request if validation fails or invalid GUID, 404 Not Found if player doesn't exist, or 500 on error</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<PlayerDto>> UpdatePlayer(string id, [FromBody] UpdatePlayerDto updatePlayerDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!Guid.TryParse(id, out var playerId))
            {
                return BadRequest("Invalid GUID format");
            }

            var player = await _playerService.UpdatePlayerAsync(playerId, updatePlayerDto);
            if (player == null)
            {
                return NotFound($"Player with ID {id} not found");
            }

            return Ok(player);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating player with ID {PlayerId}", id);
            return StatusCode(500, "An error occurred while updating the player");
        }
    }

    /// <summary>
    /// DELETE /api/players/{id} - Delete a player
    /// Permanently deletes a player identified by GUID from the system
    /// </summary>
    /// <param name="id">The GUID string of the player to delete</param>
    /// <returns>204 No Content if successfully deleted, 400 Bad Request if invalid GUID, 404 Not Found if player doesn't exist, or 500 on error</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePlayer(string id)
    {
        try
        {
            if (!Guid.TryParse(id, out var playerId))
            {
                return BadRequest("Invalid GUID format");
            }

            var deleted = await _playerService.DeletePlayerAsync(playerId);
            if (!deleted)
            {
                return NotFound($"Player with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting player with ID {PlayerId}", id);
            return StatusCode(500, "An error occurred while deleting the player");
        }
    }
}