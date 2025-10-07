using MarriageCalculator.API.Services.Interfaces;
using MarriageCalculator.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarriageCalculator.API.Controllers;

/// <summary>
/// Marriage Games Controller - Manages individual marriage games within rounds
/// 
/// ENDPOINTS SUMMARY:
/// ==================
/// GET    /api/marriagegames              - Get all marriage games
/// GET    /api/marriagegames/{id}         - Get specific marriage game by ID
/// GET    /api/marriagegames/round/{id}   - Get all games for a specific round
/// POST   /api/marriagegames              - Create new marriage game
/// PUT    /api/marriagegames/{id}         - Update existing marriage game by ID
/// DELETE /api/marriagegames/{id}         - Delete marriage game by ID
/// 
/// AUTHENTICATION:
/// - All endpoints require authentication ([Authorize])
/// 
/// KEY FEATURES:
/// - Integer-based game identification
/// - Game-to-round relationship management
/// - Full CRUD operations with proper error handling and logging
/// - Model validation on create and update operations
/// - Filtered retrieval by round ID
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MarriageGamesController : ControllerBase
{
    private readonly IMarriageGameService _gameService;
    private readonly ILogger<MarriageGamesController> _logger;

    public MarriageGamesController(IMarriageGameService gameService, ILogger<MarriageGamesController> logger)
    {
        _gameService = gameService;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/marriagegames - Get all marriage games
    /// Returns a list of all marriage games in the system
    /// </summary>
    /// <returns>200 OK with list of MarriageGameDto objects, or 500 on error</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MarriageGameDto>>> GetMarriageGames()
    {
        try
        {
            var games = await _gameService.GetAllGamesAsync();
            return Ok(games);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving marriage games");
            return StatusCode(500, "An error occurred while retrieving marriage games");
        }
    }

    /// <summary>
    /// GET /api/marriagegames/{id} - Get specific marriage game by ID
    /// Retrieves a specific marriage game by its unique integer identifier
    /// </summary>
    /// <param name="id">The integer ID of the marriage game to retrieve</param>
    /// <returns>200 OK with MarriageGameDto object, 404 Not Found if game doesn't exist, or 500 on error</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<MarriageGameDto>> GetMarriageGame(int id)
    {
        try
        {
            var game = await _gameService.GetGameByIdAsync(id);
            if (game == null)
            {
                return NotFound($"Marriage game with ID {id} not found");
            }

            return Ok(game);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving marriage game with ID {GameId}", id);
            return StatusCode(500, "An error occurred while retrieving the marriage game");
        }
    }

    /// <summary>
    /// GET /api/marriagegames/round/{roundId} - Get all games for a specific round
    /// Retrieves all marriage games that belong to a specific round
    /// </summary>
    /// <param name="roundId">The integer ID of the round to get games for</param>
    /// <returns>200 OK with list of MarriageGameDto objects, or 500 on error</returns>
    [HttpGet("round/{roundId}")]
    public async Task<ActionResult<IEnumerable<MarriageGameDto>>> GetMarriageGamesByRound(int roundId)
    {
        try
        {
            var games = await _gameService.GetGamesByRoundIdAsync(roundId);
            return Ok(games);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving marriage games for round {RoundId}", roundId);
            return StatusCode(500, "An error occurred while retrieving marriage games for the round");
        }
    }

    /// <summary>
    /// POST /api/marriagegames - Create new marriage game
    /// Creates a new marriage game within a round
    /// </summary>
    /// <param name="createDto">The marriage game data to create</param>
    /// <returns>201 Created with MarriageGameDto object and location header, 400 Bad Request if validation fails, or 500 on error</returns>
    [HttpPost]
    public async Task<ActionResult<MarriageGameDto>> CreateMarriageGame([FromBody] CreateMarriageGameDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var game = await _gameService.CreateGameAsync(createDto);
            return CreatedAtAction(nameof(GetMarriageGame), new { id = game.Id }, game);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating marriage game");
            return StatusCode(500, "An error occurred while creating the marriage game");
        }
    }

    /// <summary>
    /// PUT /api/marriagegames/{id} - Update existing marriage game
    /// Updates an existing marriage game identified by integer ID with new data
    /// </summary>
    /// <param name="id">The integer ID of the marriage game to update</param>
    /// <param name="updateDto">The updated marriage game data</param>
    /// <returns>200 OK with updated MarriageGameDto object, 400 Bad Request if validation fails, 404 Not Found if game doesn't exist, or 500 on error</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<MarriageGameDto>> UpdateMarriageGame(int id, [FromBody] CreateMarriageGameDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var game = await _gameService.UpdateGameAsync(id, updateDto);
            if (game == null)
            {
                return NotFound($"Marriage game with ID {id} not found");
            }

            return Ok(game);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating marriage game with ID {GameId}", id);
            return StatusCode(500, "An error occurred while updating the marriage game");
        }
    }

    /// <summary>
    /// DELETE /api/marriagegames/{id} - Delete marriage game
    /// Permanently deletes a marriage game identified by integer ID from the system
    /// </summary>
    /// <param name="id">The integer ID of the marriage game to delete</param>
    /// <returns>204 No Content if successfully deleted, 404 Not Found if game doesn't exist, or 500 on error</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMarriageGame(int id)
    {
        try
        {
            var deleted = await _gameService.DeleteGameAsync(id);
            if (!deleted)
            {
                return NotFound($"Marriage game with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting marriage game with ID {GameId}", id);
            return StatusCode(500, "An error occurred while deleting the marriage game");
        }
    }
}