using MarriageCalculator.API.Services.Interfaces;
using MarriageCalculator.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarriageCalculator.API.Controllers;

/// <summary>
/// Marriage Game Rounds Controller - Manages individual game rounds within game sets
/// 
/// ENDPOINTS SUMMARY:
/// ==================
/// GET    /api/marriagegamerounds                - Get all marriage game rounds
/// GET    /api/marriagegamerounds/{id}           - Get specific marriage game round by ID
/// GET    /api/marriagegamerounds/gameset/{id}   - Get all rounds for a specific game set
/// POST   /api/marriagegamerounds                - Create new marriage game round
/// PUT    /api/marriagegamerounds/{id}           - Update existing marriage game round by ID
/// DELETE /api/marriagegamerounds/{id}           - Delete marriage game round by ID
/// 
/// AUTHENTICATION:
/// - All endpoints require authentication ([Authorize])
/// 
/// KEY FEATURES:
/// - Integer-based round identification
/// - Round-to-gameset relationship management
/// - Full CRUD operations with proper error handling and logging
/// - Model validation on create and update operations
/// - Filtered retrieval by game set ID
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MarriageGameRoundsController : ControllerBase
{
    private readonly IMarriageGameRoundService _roundService;
    private readonly ILogger<MarriageGameRoundsController> _logger;

    public MarriageGameRoundsController(IMarriageGameRoundService roundService, ILogger<MarriageGameRoundsController> logger)
    {
        _roundService = roundService;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/marriagegamerounds - Get all marriage game rounds
    /// Returns a list of all marriage game rounds in the system
    /// </summary>
    /// <returns>200 OK with list of MarriageGameRoundDto objects, or 500 on error</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MarriageGameRoundDto>>> GetMarriageGameRounds()
    {
        try
        {
            var rounds = await _roundService.GetAllRoundsAsync();
            return Ok(rounds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving marriage game rounds");
            return StatusCode(500, "An error occurred while retrieving marriage game rounds");
        }
    }

    /// <summary>
    /// GET /api/marriagegamerounds/{id} - Get specific marriage game round by ID
    /// Retrieves a specific marriage game round by its unique integer identifier
    /// </summary>
    /// <param name="id">The integer ID of the marriage game round to retrieve</param>
    /// <returns>200 OK with MarriageGameRoundDto object, 404 Not Found if round doesn't exist, or 500 on error</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<MarriageGameRoundDto>> GetMarriageGameRound(int id)
    {
        try
        {
            var round = await _roundService.GetRoundByIdAsync(id);
            if (round == null)
            {
                return NotFound($"Marriage game round with ID {id} not found");
            }

            return Ok(round);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving marriage game round with ID {RoundId}", id);
            return StatusCode(500, "An error occurred while retrieving the marriage game round");
        }
    }

    /// <summary>
    /// GET /api/marriagegamerounds/gameset/{gameSetId} - Get all rounds for a specific game set
    /// Retrieves all marriage game rounds that belong to a specific game set
    /// </summary>
    /// <param name="gameSetId">The integer ID of the game set to get rounds for</param>
    /// <returns>200 OK with list of MarriageGameRoundDto objects, or 500 on error</returns>
    [HttpGet("gameset/{gameSetId}")]
    public async Task<ActionResult<IEnumerable<MarriageGameRoundDto>>> GetMarriageGameRoundsByGameSet(int gameSetId)
    {
        try
        {
            var rounds = await _roundService.GetRoundsByGameSetIdAsync(gameSetId);
            return Ok(rounds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving marriage game rounds for game set {GameSetId}", gameSetId);
            return StatusCode(500, "An error occurred while retrieving marriage game rounds for the game set");
        }
    }

    /// <summary>
    /// POST /api/marriagegamerounds - Create new marriage game round
    /// Creates a new marriage game round within a game set
    /// </summary>
    /// <param name="createDto">The marriage game round data to create</param>
    /// <returns>201 Created with MarriageGameRoundDto object and location header, 400 Bad Request if validation fails, or 500 on error</returns>
    [HttpPost]
    public async Task<ActionResult<MarriageGameRoundDto>> CreateMarriageGameRound([FromBody] CreateMarriageGameRoundDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var round = await _roundService.CreateRoundAsync(createDto);
            return CreatedAtAction(nameof(GetMarriageGameRound), new { id = round.Id }, round);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating marriage game round");
            return StatusCode(500, "An error occurred while creating the marriage game round");
        }
    }

    /// <summary>
    /// PUT /api/marriagegamerounds/{id} - Update existing marriage game round
    /// Updates an existing marriage game round identified by integer ID with new data
    /// </summary>
    /// <param name="id">The integer ID of the marriage game round to update</param>
    /// <param name="updateDto">The updated marriage game round data</param>
    /// <returns>200 OK with updated MarriageGameRoundDto object, 400 Bad Request if validation fails, 404 Not Found if round doesn't exist, or 500 on error</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<MarriageGameRoundDto>> UpdateMarriageGameRound(int id, [FromBody] CreateMarriageGameRoundDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var round = await _roundService.UpdateRoundAsync(id, updateDto);
            if (round == null)
            {
                return NotFound($"Marriage game round with ID {id} not found");
            }

            return Ok(round);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating marriage game round with ID {RoundId}", id);
            return StatusCode(500, "An error occurred while updating the marriage game round");
        }
    }

    /// <summary>
    /// DELETE /api/marriagegamerounds/{id} - Delete marriage game round
    /// Permanently deletes a marriage game round identified by integer ID from the system
    /// </summary>
    /// <param name="id">The integer ID of the marriage game round to delete</param>
    /// <returns>204 No Content if successfully deleted, 404 Not Found if round doesn't exist, or 500 on error</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMarriageGameRound(int id)
    {
        try
        {
            var deleted = await _roundService.DeleteRoundAsync(id);
            if (!deleted)
            {
                return NotFound($"Marriage game round with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting marriage game round with ID {RoundId}", id);
            return StatusCode(500, "An error occurred while deleting the marriage game round");
        }
    }
}