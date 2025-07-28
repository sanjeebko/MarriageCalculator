using MarriageCalculator.API.DTOs;
using MarriageCalculator.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarriageCalculator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    /// Get all marriage games
    /// </summary>
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
    /// Get marriage game by ID
    /// </summary>
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
    /// Get marriage games by round ID
    /// </summary>
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
    /// Create a new marriage game
    /// </summary>
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
    /// Update an existing marriage game
    /// </summary>
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
    /// Delete a marriage game
    /// </summary>
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