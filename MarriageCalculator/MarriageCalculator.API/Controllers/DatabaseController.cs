using MarriageCalculator.API.DTOs;
using MarriageCalculator.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarriageCalculator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DatabaseController : ControllerBase
{
    private readonly IDatabaseService _databaseService;
    private readonly ILogger<DatabaseController> _logger;

    public DatabaseController(IDatabaseService databaseService, ILogger<DatabaseController> logger)
    {
        _databaseService = databaseService;
        _logger = logger;
    }

    /// <summary>
    /// Test database connection and get basic information
    /// </summary>
    [HttpGet("info")]
    public async Task<ActionResult<DatabaseInfoDto>> GetDatabaseInfo()
    {
        try
        {
            var info = await _databaseService.GetDatabaseInfoAsync();
            return Ok(info);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting database info");
            return StatusCode(500, "An error occurred while getting database information");
        }
    }

    /// <summary>
    /// Seed database with default data
    /// </summary>
    [HttpPost("seed")]
    public async Task<ActionResult<ApiResponse>> SeedDefaultData()
    {
        try
        {
            _logger.LogInformation("Database seeding requested");
            var result = await _databaseService.SeedDefaultDataAsync();
            
            if (result.Success)
            {
                _logger.LogInformation("Database seeded successfully");
                return Ok(result);
            }
            else
            {
                _logger.LogError("Database seeding failed: {Message}", result.Message);
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding database");
            return StatusCode(500, new ApiResponse 
            { 
                Success = false, 
                Message = "An error occurred while seeding the database" 
            });
        }
    }

    /// <summary>
    /// Clean up database (remove all data)
    /// </summary>
    [HttpDelete("cleanup")]
    public async Task<ActionResult<ApiResponse>> CleanupDatabase()
    {
        try
        {
            _logger.LogInformation("Database cleanup requested");
            var result = await _databaseService.CleanupDatabaseAsync();
            
            if (result.Success)
            {
                _logger.LogInformation("Database cleaned up successfully");
                return Ok(result);
            }
            else
            {
                _logger.LogError("Database cleanup failed: {Message}", result.Message);
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up database");
            return StatusCode(500, new ApiResponse 
            { 
                Success = false, 
                Message = "An error occurred while cleaning up the database" 
            });
        }
    }
}