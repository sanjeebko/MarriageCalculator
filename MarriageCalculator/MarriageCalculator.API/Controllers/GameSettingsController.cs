using MarriageCalculator.Core.DTOs;
using MarriageCalculator.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarriageCalculator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GameSettingsController : ControllerBase
{
    private readonly IGameSettingsService _gameSettingsService;
    private readonly ILogger<GameSettingsController> _logger;

    public GameSettingsController(IGameSettingsService gameSettingsService, ILogger<GameSettingsController> logger)
    {
        _gameSettingsService = gameSettingsService;
        _logger = logger;
    }

    /// <summary>
    /// Get all game settings
    /// Endpoint: GET api/GameSettings
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GameSettingsDto>>> GetGameSettings()
    {
        try
        {
            // Get userId from JWT claims as Guid
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user token");
            }

            var settings = await _gameSettingsService.GetAllGameSettingsAsync(userId);
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving game settings");
            return StatusCode(500, "An error occurred while retrieving game settings");
        }
    }

    /// <summary>
    /// Get game settings by ID
    /// Endpoint: GET api/GameSettings/{id}
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<GameSettingsDto>> GetGameSettings(int id)
    {
        try
        {
            var settings = await _gameSettingsService.GetGameSettingsByIdAsync(id);
            if (settings == null)
            {
                return NotFound($"Game settings with ID {id} not found");
            }

            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving game settings with ID {SettingsId}", id);
            return StatusCode(500, "An error occurred while retrieving the game settings");
        }
    }

    /// <summary>
    /// Create new game settings
    /// Endpoint: POST api/GameSettings
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<GameSettingsDto>> CreateGameSettings([FromBody] CreateGameSettingsDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get userId from JWT claims as Guid
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user token");
            }

            var settings = await _gameSettingsService.CreateGameSettingsAsync(createDto, userId);
            return CreatedAtAction(nameof(GetGameSettings), new { id = settings.Id }, settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating game settings");
            return StatusCode(500, "An error occurred while creating the game settings");
        }
    }

    /// <summary>
    /// Update existing game settings
    /// Endpoint: PUT api/GameSettings/{id}
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<GameSettingsDto>> UpdateGameSettings(int id, [FromBody] CreateGameSettingsDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var settings = await _gameSettingsService.UpdateGameSettingsAsync(id, updateDto);
            if (settings == null)
            {
                return NotFound($"Game settings with ID {id} not found");
            }

            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating game settings with ID {SettingsId}", id);
            return StatusCode(500, "An error occurred while updating the game settings");
        }
    }

    /// <summary>
    /// Delete game settings
    /// Endpoint: DELETE api/GameSettings/{id}
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteGameSettings(int id)
    {
        try
        {
            var deleted = await _gameSettingsService.DeleteGameSettingsAsync(id);
            if (!deleted)
            {
                return NotFound($"Game settings with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting game settings with ID {SettingsId}", id);
            return StatusCode(500, "An error occurred while deleting the game settings");
        }
    }
}