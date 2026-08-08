using MarriageCalculator.Core.DTOs;
using MarriageCalculator.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarriageCalculator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlayersController : ControllerBase
{
    private readonly IPlayerService _playerService;
    private readonly ILogger<PlayersController> _logger;

    public PlayersController(IPlayerService playerService, ILogger<PlayersController> logger)
    {
        _playerService = playerService;
        _logger = logger;
    }

    /// <summary>
    /// Get all players
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlayerDto>>> GetPlayers()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var players = await _playerService.GetPlayersByCreatedByAsync(userId);
            return Ok(players);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving players");
            return StatusCode(500, "An error occurred while retrieving players");
        }
    }

    /// <summary>
    /// Get player by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PlayerDto>> GetPlayer(string id)
    {
        try
        {
            var player = await _playerService.GetPlayerByIdAsync(id);
            if (player == null)
            {
                return NotFound($"Player with ID {id} not found");
            }

            return Ok(player);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving player with ID {PlayerId}", id);
            return StatusCode(500, "An error occurred while retrieving the player");
        }
    }

    /// <summary>
    /// Create a new player
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PlayerDto>> CreatePlayer([FromBody] CreatePlayerDto createPlayerDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var player = await _playerService.CreatePlayerAsync(createPlayerDto, userId);
            return CreatedAtAction(nameof(GetPlayer), new { id = player.Id }, player);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating player");
            return StatusCode(500, "An error occurred while creating the player");
        }
    }

    /// <summary>
    /// Update an existing player
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<PlayerDto>> UpdatePlayer(string id, [FromBody] UpdatePlayerDto updatePlayerDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var player = await _playerService.UpdatePlayerAsync(id, updatePlayerDto);
            if (player == null)
            {
                return NotFound($"Player with ID {id} not found");
            }

            return Ok(player);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating player with ID {PlayerId}", id);
            return StatusCode(500, "An error occurred while updating the player");
        }
    }

    /// <summary>
    /// Delete a player
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePlayer(string id)
    {
        try
        {
            var deleted = await _playerService.DeletePlayerAsync(id);
            if (!deleted)
            {
                return NotFound($"Player with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting player with ID {PlayerId}", id);
            return StatusCode(500, "An error occurred while deleting the player");
        }
    }
}