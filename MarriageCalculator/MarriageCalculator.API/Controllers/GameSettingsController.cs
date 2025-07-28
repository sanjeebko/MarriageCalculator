using MarriageCalculator.API.DTOs;
using MarriageCalculator.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarriageCalculator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GameSettingsDto>>> GetGameSettings()
    {
        try
        {
            var settings = await _gameSettingsService.GetAllGameSettingsAsync();
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

            var settings = await _gameSettingsService.CreateGameSettingsAsync(createDto);
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