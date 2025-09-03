using MarriageCalculator.Core.DTOs;
using MarriageCalculator.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarriageCalculator.API.Controllers;

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
    /// Get all marriage game set players
    /// </summary>
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
    /// Get marriage game set player by game set ID and player ID
    /// </summary>
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
    /// Get all players for a specific marriage game set
    /// </summary>
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
    /// Get all game sets for a specific player
    /// </summary>
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
    /// Add a player to a marriage game set
    /// </summary>
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
                return Conflict($"Player {createDto.PlayerId} is already part of game set {createDto.MarriageGameSetId}");
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
    /// Remove a player from a marriage game set
    /// </summary>
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
    /// Remove all players from a marriage game set
    /// </summary>
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
    /// Check if a player exists in a marriage game set
    /// </summary>
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