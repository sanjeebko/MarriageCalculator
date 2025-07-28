using MarriageCalculator.API.DTOs;
using MarriageCalculator.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarriageCalculator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MarriageGameSetsController : ControllerBase
{
    private readonly IMarriageGameSetService _gameSetService;
    private readonly ILogger<MarriageGameSetsController> _logger;

    public MarriageGameSetsController(IMarriageGameSetService gameSetService, ILogger<MarriageGameSetsController> logger)
    {
        _gameSetService = gameSetService;
        _logger = logger;
    }

    /// <summary>
    /// Get all marriage game sets
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MarriageGameSetDto>>> GetMarriageGameSets()
    {
        try
        {
            var gameSets = await _gameSetService.GetAllGameSetsAsync();
            return Ok(gameSets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving marriage game sets");
            return StatusCode(500, "An error occurred while retrieving marriage game sets");
        }
    }

    /// <summary>
    /// Get marriage game set by ID
    /// </summary>
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
    /// Get latest active marriage game set
    /// </summary>
    [HttpGet("latest")]
    public async Task<ActionResult<MarriageGameSetDto>> GetLatestActiveGameSet()
    {
        try
        {
            var gameSet = await _gameSetService.GetLatestActiveGameSetAsync();
            if (gameSet == null)
            {
                return NotFound("No active marriage game set found");
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
    /// Create a new marriage game set
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<MarriageGameSetDto>> CreateMarriageGameSet([FromBody] CreateMarriageGameSetDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
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
    /// Update an existing marriage game set
    /// </summary>
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
    /// Delete a marriage game set
    /// </summary>
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