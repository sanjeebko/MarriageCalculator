using MarriageCalculator.Core.DTOs;
using MarriageCalculator.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarriageCalculator.API.Controllers;

/// <summary>
/// Marriage Game Set Players Controller - Manages player-to-gameset relationships and associations
/// 
/// ENDPOINTS SUMMARY:
/// ==================
/// GET    /api/marriagegamesetplayers                          - Get all game set player relationships
/// GET    /api/marriagegamesetplayers/{gameSetId}/{playerId}   - Get specific player-gameset relationship
/// GET    /api/marriagegamesetplayers/gameset/{id}/players     - Get all players for a specific game set
/// GET    /api/marriagegamesetplayers/player/{id}/gamesets     - Get all game sets for a specific player (GUID)
/// GET    /api/marriagegamesetplayers/{gameSetId}/{playerId}/exists - Check if player exists in game set
/// POST   /api/marriagegamesetplayers                          - Add player to a game set
/// DELETE /api/marriagegamesetplayers/{gameSetId}/{playerId}   - Remove player from a game set
/// DELETE /api/marriagegamesetplayers/gameset/{id}/players     - Remove all players from a game set
/// 
/// AUTHENTICATION:
/// - All endpoints require authentication ([Authorize])
/// 
/// KEY FEATURES:
/// - Composite key identification (GameSetId + PlayerId)
/// - Support for both integer game set IDs and GUID player IDs
/// - Many-to-many relationship management between game sets and players
/// - Conflict detection for duplicate relationships
/// - Bulk operations for removing all players from a game set
/// - Existence checking for player-gameset relationships
/// - Full error handling and logging
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MarriageGameSetPlayersController : ControllerBase
{
    private readonly IMarriageGameSetPlayerService _gameSetPlayerService;
    private readonly ILogger<MarriageGameSetPlayersController> _logger;

    public MarriageGameSetPlayersController(IMarriageGameSetPlayerService gameSetPlayerService, ILogger<MarriageGameSetPlayersController> logger)
    {
        _gameSetPlayerService = gameSetPlayerService;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/marriagegamesetplayers - Get all game set player relationships
    /// Returns a list of all player-to-gameset relationships in the system
    /// </summary>
    /// <returns>200 OK with list of MarriageGameSetPlayerDto objects, or 500 on error</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MarriageGameSetPlayerDto>>> GetAllGameSetPlayers()
    {
        try
        {
            var gameSetPlayers = await _gameSetPlayerService.GetAllGameSetPlayersAsync();
            return Ok(gameSetPlayers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving marriage game set players");
            return StatusCode(500, "An error occurred while retrieving marriage game set players");
        }
    }

    /// <summary>
    /// GET /api/marriagegamesetplayers/{gameSetId}/{playerId} - Get specific player-gameset relationship
    /// Retrieves a specific player-to-gameset relationship using composite key
    /// </summary>
    /// <param name="gameSetId">The integer ID of the game set</param>
    /// <param name="playerId">The GUID of the player</param>
    /// <returns>200 OK with MarriageGameSetPlayerDto object, 404 Not Found if relationship doesn't exist, or 500 on error</returns>
    [HttpGet("{gameSetId}/{playerId}")]
    public async Task<ActionResult<MarriageGameSetPlayerDto>> GetGameSetPlayer(int gameSetId, Guid playerId)
    {
        try
        {
            var gameSetPlayer = await _gameSetPlayerService.GetGameSetPlayerByIdAsync(gameSetId, playerId);
            if (gameSetPlayer == null)
            {
                return NotFound($"Marriage game set player with GameSetId {gameSetId} and PlayerId {playerId} not found");
            }

            return Ok(gameSetPlayer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving marriage game set player with GameSetId {GameSetId} and PlayerId {PlayerId}", gameSetId, playerId);
            return StatusCode(500, "An error occurred while retrieving the marriage game set player");
        }
    }

    /// <summary>
    /// GET /api/marriagegamesetplayers/gameset/{gameSetId}/players - Get all players for a specific game set
    /// Retrieves all players that are associated with a specific game set
    /// </summary>
    /// <param name="gameSetId">The integer ID of the game set to get players for</param>
    /// <returns>200 OK with list of MarriageGameSetPlayerDto objects, or 500 on error</returns>
    [HttpGet("gameset/{gameSetId}/players")]
    public async Task<ActionResult<IEnumerable<MarriageGameSetPlayerDto>>> GetPlayersByGameSetId(int gameSetId)
    {
        try
        {
            var gameSetPlayers = await _gameSetPlayerService.GetPlayersByGameSetIdAsync(gameSetId);
            return Ok(gameSetPlayers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving players for marriage game set with ID {GameSetId}", gameSetId);
            return StatusCode(500, "An error occurred while retrieving players for the marriage game set");
        }
    }

    /// <summary>
    /// GET /api/marriagegamesetplayers/player/{playerId}/gamesets - Get all game sets for a specific player
    /// Retrieves all game sets that a specific player (GUID identifier) is associated with
    /// </summary>
    /// <param name="playerId">The GUID of the player to get game sets for</param>
    /// <returns>200 OK with list of MarriageGameSetPlayerDto objects, or 500 on error</returns>
    [HttpGet("player/{playerId}/gamesets")]
    public async Task<ActionResult<IEnumerable<MarriageGameSetPlayerDto>>> GetGameSetsByPlayerId(Guid playerId)
    {
        try
        {
            var gameSetPlayers = await _gameSetPlayerService.GetGameSetsByPlayerIdAsync(playerId);
            return Ok(gameSetPlayers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving game sets for player with ID {PlayerId}", playerId);
            return StatusCode(500, "An error occurred while retrieving game sets for the player");
        }
    }

    /// <summary>
    /// POST /api/marriagegamesetplayers - Add player to a game set
    /// Creates a new player-to-gameset relationship. Includes conflict detection for duplicate relationships.
    /// </summary>
    /// <param name="createDto">The player-gameset relationship data to create</param>
    /// <returns>201 Created with MarriageGameSetPlayerDto object and location header, 400 Bad Request if validation fails, 409 Conflict if relationship already exists, or 500 on error</returns>
    [HttpPost]
    public async Task<ActionResult<MarriageGameSetPlayerDto>> CreateGameSetPlayer([FromBody] CreateMarriageGameSetPlayerDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if the relationship already exists
            var existingGameSetPlayer = await _gameSetPlayerService.GetGameSetPlayerByIdAsync(createDto.MarriageGameSetId, createDto.PlayerId);
            if (existingGameSetPlayer != null)
            {
                // Return the existing relationship instead of throwing a conflict
                return Ok(existingGameSetPlayer);
            }

            var gameSetPlayer = await _gameSetPlayerService.CreateGameSetPlayerAsync(createDto);
            return CreatedAtAction(nameof(GetGameSetPlayer), 
                new { gameSetId = gameSetPlayer.MarriageGameSetId, playerId = gameSetPlayer.PlayerId }, 
                gameSetPlayer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating marriage game set player");
            return StatusCode(500, "An error occurred while adding the player to the marriage game set");
        }
    }

    /// <summary>
    /// DELETE /api/marriagegamesetplayers/{gameSetId}/{playerId} - Remove player from a game set
    /// Permanently removes a player-to-gameset relationship using composite key
    /// </summary>
    /// <param name="gameSetId">The integer ID of the game set</param>
    /// <param name="playerId">The GUID of the player</param>
    /// <returns>204 No Content if successfully deleted, 404 Not Found if relationship doesn't exist, or 500 on error</returns>
    [HttpDelete("{gameSetId}/{playerId}")]
    public async Task<ActionResult> DeleteGameSetPlayer(int gameSetId, Guid playerId)
    {
        try
        {
            var deleted = await _gameSetPlayerService.DeleteGameSetPlayerAsync(gameSetId, playerId);
            if (!deleted)
            {
                return NotFound($"Marriage game set player with GameSetId {gameSetId} and PlayerId {playerId} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting marriage game set player with GameSetId {GameSetId} and PlayerId {PlayerId}", gameSetId, playerId);
            return StatusCode(500, "An error occurred while removing the player from the marriage game set");
        }
    }

    /// <summary>
    /// DELETE /api/marriagegamesetplayers/gameset/{gameSetId}/players - Remove all players from a game set
    /// Bulk operation that removes all player-to-gameset relationships for a specific game set
    /// </summary>
    /// <param name="gameSetId">The integer ID of the game set to remove all players from</param>
    /// <returns>204 No Content if successfully deleted, 404 Not Found if no players found for game set, or 500 on error</returns>
    [HttpDelete("gameset/{gameSetId}/players")]
    public async Task<ActionResult> DeleteAllPlayersFromGameSet(int gameSetId)
    {
        try
        {
            var deleted = await _gameSetPlayerService.DeletePlayersByGameSetIdAsync(gameSetId);
            if (!deleted)
            {
                return NotFound($"No players found for marriage game set with ID {gameSetId}");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting all players from marriage game set with ID {GameSetId}", gameSetId);
            return StatusCode(500, "An error occurred while removing all players from the marriage game set");
        }
    }

    /// <summary>
    /// GET /api/marriagegamesetplayers/{gameSetId}/{playerId}/exists - Check if player exists in game set
    /// Utility endpoint to check if a player-to-gameset relationship exists without retrieving full data
    /// </summary>
    /// <param name="gameSetId">The integer ID of the game set</param>
    /// <param name="playerId">The GUID of the player</param>
    /// <returns>200 OK with boolean result indicating existence, or 500 on error</returns>
    [HttpGet("{gameSetId}/{playerId}/exists")]
    public async Task<ActionResult<bool>> CheckGameSetPlayerExists(int gameSetId, Guid playerId)
    {
        try
        {
            var exists = await _gameSetPlayerService.GameSetPlayerExistsAsync(gameSetId, playerId);
            return Ok(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if player exists in marriage game set with GameSetId {GameSetId} and PlayerId {PlayerId}", gameSetId, playerId);
            return StatusCode(500, "An error occurred while checking if the player exists in the marriage game set");
        }
    }
}