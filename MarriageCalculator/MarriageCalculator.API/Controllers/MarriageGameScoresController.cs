using MarriageCalculator.API.Services.Interfaces;
using MarriageCalculator.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarriageCalculator.API.Controllers;

/// <summary>
/// Marriage Game Scores Controller - Manages player scores within individual games
/// 
/// ENDPOINTS SUMMARY:
/// ==================
/// GET    /api/marriagegamescores               - Get all marriage game scores
/// GET    /api/marriagegamescores/{id}          - Get specific marriage game score by ID
/// GET    /api/marriagegamescores/game/{id}     - Get all scores for a specific game
/// GET    /api/marriagegamescores/player/{id}   - Get all scores for a specific player (GUID)
/// POST   /api/marriagegamescores               - Create new marriage game score
/// PUT    /api/marriagegamescores/{id}          - Update existing marriage game score by ID
/// DELETE /api/marriagegamescores/{id}         - Delete marriage game score by ID
/// 
/// AUTHENTICATION:
/// - All endpoints require authentication ([Authorize])
/// 
/// KEY FEATURES:
/// - Integer-based score identification
/// - Score-to-game and score-to-player relationship management
/// - Support for both integer game IDs and GUID player IDs
/// - Full CRUD operations with proper error handling and logging
/// - Enhanced logging for debugging and monitoring
/// - Model validation on create and update operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MarriageGameScoresController : ControllerBase
{
    private readonly IMarriageGameScoreService _scoreService;
    private readonly ILogger<MarriageGameScoresController> _logger;

    public MarriageGameScoresController(IMarriageGameScoreService scoreService, ILogger<MarriageGameScoresController> logger)
    {
        _scoreService = scoreService;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/marriagegamescores - Get all marriage game scores
    /// Returns a list of all marriage game scores in the system
    /// </summary>
    /// <returns>200 OK with list of MarriageGameScoreDto objects, or 500 on error</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MarriageGameScoreDto>>> GetMarriageGameScores()
    {
        try
        {
            var scores = await _scoreService.GetAllScoresAsync();
            return Ok(scores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving marriage game scores");
            return StatusCode(500, "An error occurred while retrieving marriage game scores");
        }
    }

    /// <summary>
    /// GET /api/marriagegamescores/{id} - Get specific marriage game score by ID
    /// Retrieves a specific marriage game score by its unique integer identifier
    /// </summary>
    /// <param name="id">The integer ID of the marriage game score to retrieve</param>
    /// <returns>200 OK with MarriageGameScoreDto object, 404 Not Found if score doesn't exist, or 500 on error</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<MarriageGameScoreDto>> GetMarriageGameScore(int id)
    {
        try
        {
            var score = await _scoreService.GetScoreByIdAsync(id);
            if (score == null)
            {
                return NotFound($"Marriage game score with ID {id} not found");
            }

            return Ok(score);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving marriage game score with ID {ScoreId}", id);
            return StatusCode(500, "An error occurred while retrieving the marriage game score");
        }
    }

    /// <summary>
    /// GET /api/marriagegamescores/game/{gameId} - Get all scores for a specific game
    /// Retrieves all marriage game scores that belong to a specific game
    /// </summary>
    /// <param name="gameId">The integer ID of the game to get scores for</param>
    /// <returns>200 OK with list of MarriageGameScoreDto objects, or 500 on error</returns>
    [HttpGet("game/{gameId}")]
    public async Task<ActionResult<IEnumerable<MarriageGameScoreDto>>> GetMarriageGameScoresByGame(int gameId)
    {
        try
        {
            _logger.LogInformation("Getting marriage game scores for game ID: {GameId}", gameId);
            var scores = await _scoreService.GetScoresByGameIdAsync(gameId);
            _logger.LogInformation("Found {Count} scores for game ID: {GameId}", scores.Count(), gameId);
            return Ok(scores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving marriage game scores for game {GameId}", gameId);
            return StatusCode(500, "An error occurred while retrieving marriage game scores for the game");
        }
    }

    /// <summary>
    /// GET /api/marriagegamescores/player/{playerId} - Get all scores for a specific player
    /// Retrieves all marriage game scores that belong to a specific player (GUID identifier)
    /// </summary>
    /// <param name="playerId">The GUID of the player to get scores for</param>
    /// <returns>200 OK with list of MarriageGameScoreDto objects, or 500 on error</returns>
    [HttpGet("player/{playerId}")]
    public async Task<ActionResult<IEnumerable<MarriageGameScoreDto>>> GetMarriageGameScoresByPlayer(Guid playerId)
    {
        try
        {
            _logger.LogInformation("Getting marriage game scores for player ID: {PlayerId}", playerId);
            var scores = await _scoreService.GetScoresByPlayerIdAsync(playerId);
            _logger.LogInformation("Found {Count} scores for player ID: {PlayerId}", scores.Count(), playerId);
            return Ok(scores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving marriage game scores for player {PlayerId}", playerId);
            return StatusCode(500, "An error occurred while retrieving marriage game scores for the player");
        }
    }

    /// <summary>
    /// POST /api/marriagegamescores - Create new marriage game score
    /// Creates a new marriage game score for a player in a specific game
    /// </summary>
    /// <param name="createDto">The marriage game score data to create</param>
    /// <returns>201 Created with MarriageGameScoreDto object and location header, 400 Bad Request if validation fails, or 500 on error</returns>
    [HttpPost]
    public async Task<ActionResult<MarriageGameScoreDto>> CreateMarriageGameScore([FromBody] CreateMarriageGameScoreDto createDto)
    {
        try
        {
            _logger.LogInformation("Creating marriage game score with data: {CreateDto}", System.Text.Json.JsonSerializer.Serialize(createDto));
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for marriage game score creation: {ModelState}", 
                    string.Join(", ", ModelState.SelectMany(x => x.Value.Errors.Select(e => $"{x.Key}: {e.ErrorMessage}"))));
                return BadRequest(ModelState);
            }

            var score = await _scoreService.CreateScoreAsync(createDto);
            _logger.LogInformation("Successfully created marriage game score with ID: {ScoreId}", score.Id);
            return CreatedAtAction(nameof(GetMarriageGameScore), new { id = score.Id }, score);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            _logger.LogWarning("Attempted to create duplicate marriage game score: {Message}", ex.Message);
            return Conflict(new { message = ex.Message, playerId = createDto.PlayerId, gameId = createDto.MarriageGameId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating marriage game score with data: {CreateDto}", System.Text.Json.JsonSerializer.Serialize(createDto));
            return StatusCode(500, $"An error occurred while creating the marriage game score: {ex.Message}");
        }
    }

    /// <summary>
    /// PUT /api/marriagegamescores/{id} - Update existing marriage game score
    /// Updates an existing marriage game score identified by integer ID with new data
    /// </summary>
    /// <param name="id">The integer ID of the marriage game score to update</param>
    /// <param name="updateDto">The updated marriage game score data</param>
    /// <returns>200 OK with updated MarriageGameScoreDto object, 400 Bad Request if validation fails, 404 Not Found if score doesn't exist, or 500 on error</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<MarriageGameScoreDto>> UpdateMarriageGameScore(int id, [FromBody] CreateMarriageGameScoreDto updateDto)
    {
        try
        {
            _logger.LogInformation("Updating marriage game score ID {ScoreId} with data: {UpdateDto}", id, System.Text.Json.JsonSerializer.Serialize(updateDto));
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for marriage game score update: {ModelState}", 
                    string.Join(", ", ModelState.SelectMany(x => x.Value.Errors.Select(e => $"{x.Key}: {e.ErrorMessage}"))));
                return BadRequest(ModelState);
            }

            var score = await _scoreService.UpdateScoreAsync(id, updateDto);
            if (score == null)
            {
                return NotFound($"Marriage game score with ID {id} not found");
            }

            _logger.LogInformation("Successfully updated marriage game score with ID: {ScoreId}", id);
            return Ok(score);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating marriage game score with ID {ScoreId}", id);
            return StatusCode(500, $"An error occurred while updating the marriage game score: {ex.Message}");
        }
    }

    /// <summary>
    /// DELETE /api/marriagegamescores/{id} - Delete marriage game score
    /// Permanently deletes a marriage game score identified by integer ID from the system
    /// </summary>
    /// <param name="id">The integer ID of the marriage game score to delete</param>
    /// <returns>204 No Content if successfully deleted, 404 Not Found if score doesn't exist, or 500 on error</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMarriageGameScore(int id)
    {
        try
        {
            _logger.LogInformation("Deleting marriage game score with ID: {ScoreId}", id);
            
            var deleted = await _scoreService.DeleteScoreAsync(id);
            if (!deleted)
            {
                return NotFound($"Marriage game score with ID {id} not found");
            }

            _logger.LogInformation("Successfully deleted marriage game score with ID: {ScoreId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting marriage game score with ID {ScoreId}", id);
            return StatusCode(500, $"An error occurred while deleting the marriage game score: {ex.Message}");
        }
    }
}