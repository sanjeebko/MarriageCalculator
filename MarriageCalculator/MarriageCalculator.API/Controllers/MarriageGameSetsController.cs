using MarriageCalculator.Core.DTOs;
using MarriageCalculator.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarriageCalculator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
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
            var hostUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
            var gameSets = await _gameSetService.GetAllGameSetsAsync(hostUserId, email);
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
    public async Task<ActionResult<MarriageGameSetDto>> GetMarriageGameSet(string id)
    {
        try
        {
            var hostUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
            var gameSet = await _gameSetService.GetGameSetByIdAsync(id, hostUserId, email);
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
            var hostUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var gameSet = await _gameSetService.GetLatestActiveGameSetAsync(hostUserId);
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

            var hostUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            createDto.HostUserId = hostUserId; // Set the host to be the authenticated user

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
    public async Task<ActionResult<MarriageGameSetDto>> UpdateMarriageGameSet(string id, [FromBody] CreateMarriageGameSetDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var hostUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            updateDto.HostUserId = hostUserId;

            var gameSet = await _gameSetService.UpdateGameSetAsync(id, updateDto, hostUserId);
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
    public async Task<ActionResult> DeleteMarriageGameSet(string id)
    {
        try
        {
            var hostUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var deleted = await _gameSetService.DeleteGameSetAsync(id, hostUserId);
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

    /// <summary>
    /// Transfer host ownership of a game set to another user
    /// </summary>
    [HttpPost("{id}/transfer-host")]
    public async Task<ActionResult<MarriageGameSetDto>> TransferHost(string id, [FromBody] TransferHostDto transferDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var hostUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var result = await _gameSetService.TransferHostAsync(id, hostUserId, transferDto.NewHostUserId);
            if (result == null)
            {
                return NotFound($"Marriage game set with ID {id} not found");
            }

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transferring host of game set {GameSetId} to user {NewHostUserId}", id, transferDto.NewHostUserId);
            return StatusCode(500, "An error occurred while transferring host ownership.");
        }
    }

    /// <summary>
    /// Record one round's result: winner, dealer, and per-player seen/dublee/maal.
    /// Scores are always computed server-side, never trusted from the client.
    /// </summary>
    [HttpPost("{id}/rounds")]
    public async Task<ActionResult<MarriageGameRoundDto>> SubmitRound(string id, [FromBody] SubmitRoundDto submitDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var hostUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var round = await _gameSetService.SubmitRoundAsync(id, hostUserId, submitDto);
            return Ok(round);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting round for game set {GameSetId}", id);
            return StatusCode(500, "An error occurred while submitting the round.");
        }
    }

    /// <summary>
    /// Nudge a player in a game set via FCM push notification
    /// </summary>
    [HttpPost("{id}/nudge/{playerId}")]
    public async Task<ActionResult> NudgePlayer(string id, string playerId)
    {
        try
        {
            var hostUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var success = await _gameSetService.NudgePlayerAsync(id, hostUserId, playerId);
            if (!success)
            {
                return BadRequest("Failed to nudge player. Make sure the player exists and has linked their registered account (having a registered device token).");
            }

            return Ok(new { Message = "Nudge notification sent successfully." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error nudging player {PlayerId} in game set {GameSetId}", playerId, id);
            return StatusCode(500, "An error occurred while sending the nudge notification.");
        }
    }
}