using MarriageCalculator.API.Services.Interfaces;
using MarriageCalculator.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarriageCalculator.API.Controllers;

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
    /// Get all marriage game scores
    /// </summary>
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
    /// Get marriage game score by ID
    /// </summary>
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
    /// Get marriage game scores by game ID
    /// </summary>
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
    /// Get marriage game scores by player ID
    /// </summary>
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
    /// Create a new marriage game score
    /// </summary>
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating marriage game score with data: {CreateDto}", System.Text.Json.JsonSerializer.Serialize(createDto));
            return StatusCode(500, $"An error occurred while creating the marriage game score: {ex.Message}");
        }
    }

    /// <summary>
    /// Update an existing marriage game score
    /// </summary>
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
    /// Delete a marriage game score
    /// </summary>
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