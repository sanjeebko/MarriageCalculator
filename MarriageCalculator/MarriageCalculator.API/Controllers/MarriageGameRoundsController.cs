using MarriageCalculator.API.Services.Interfaces;
using MarriageCalculator.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarriageCalculator.API.Controllers;

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
    /// Get all marriage game rounds
    /// </summary>
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
    /// Get marriage game round by ID
    /// </summary>
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
    /// Get marriage game rounds by game set ID
    /// </summary>
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
    /// Create a new marriage game round
    /// </summary>
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
    /// Update an existing marriage game round
    /// </summary>
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
    /// Delete a marriage game round
    /// </summary>
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