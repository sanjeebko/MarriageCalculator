using MarriageCalculator.Core.DTOs;
using MarriageCalculator.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarriageCalculator.API.Controllers;

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
    /// Get all players
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlayerDto>>> GetPlayers()
    {
        try
        {
            var players = await _playerService.GetAllPlayersAsync();
            return Ok(players);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving players");
            return StatusCode(500, "An error occurred while retrieving players");
        }
    }

    /// <summary>
    /// Get players created by the current authenticated user
    /// </summary>
    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<PlayerDto>>> GetMyPlayers()
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
    /// Ensure the current authenticated user exists as a Player; creates if missing
    /// </summary>
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
    /// Get player by ID (GUID)
    /// </summary>
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
    /// Create a new player (will set CreatedByUserId to current user)
    /// </summary>
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
    /// Update an existing player (GUID-based)
    /// </summary>
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
    /// Delete a player (GUID-based)
    /// </summary>
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