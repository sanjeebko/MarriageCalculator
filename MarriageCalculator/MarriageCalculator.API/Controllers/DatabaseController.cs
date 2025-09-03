using MarriageCalculator.Core.DTOs;
using MarriageCalculator.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarriageCalculator.API.Controllers;

/// <summary>
/// Manages database operations, connectivity testing, and data seeding
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Database Management")]
[Authorize]
public class DatabaseController : ControllerBase
{
    private readonly IDatabaseService _databaseService;
    private readonly ILogger<DatabaseController> _logger;

    /// <summary>
    /// Initializes a new instance of the DatabaseController
    /// </summary>
    /// <param name="databaseService">Database service for operations</param>
    /// <param name="logger">Logger for tracking operations</param>
    public DatabaseController(IDatabaseService databaseService, ILogger<DatabaseController> logger)
    {
        _databaseService = databaseService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves comprehensive database connection and status information
    /// </summary>
    /// <remarks>
    /// This endpoint provides detailed information about the database connection status,
    /// including provider information, table count, and connectivity status.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/database/info
    /// 
    /// </remarks>
    /// <returns>Database information including connection status and table count</returns>
    /// <response code="200">Returns database information successfully</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("info")]
    [ProducesResponseType(typeof(DatabaseInfoDto), 200)]
    [ProducesResponseType(typeof(string), 500)]
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
    /// Checks if the database is connected and operational
    /// </summary>
    /// <remarks>
    /// This endpoint provides a simple health check to verify database connectivity.
    /// Returns true if the database is accessible and operational, false otherwise.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/database/health
    /// 
    /// </remarks>
    /// <returns>Boolean indicating if database is operational</returns>
    /// <response code="200">Returns database health status</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("health")]
    [ProducesResponseType(typeof(bool), 200)]
    [ProducesResponseType(typeof(string), 500)]
    public async Task<ActionResult<bool>> GetDatabaseHealth()
    {
        try
        {
            var info = await _databaseService.GetDatabaseInfoAsync();
            return Ok(info.CanConnect);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking database health");
            return StatusCode(500, "An error occurred while checking database health");
        }
    }

    /// <summary>
    /// Seeds the database with default game settings and initial data
    /// </summary>
    /// <remarks>
    /// This endpoint populates the database with default game settings if none exist.
    /// It's safe to call multiple times as it only adds data if the database is empty.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/database/seed
    /// 
    /// </remarks>
    /// <returns>Result of the seeding operation</returns>
    /// <response code="200">Database seeded successfully</response>
    /// <response code="400">If seeding failed due to validation or business logic errors</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("seed")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
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
    /// Removes all data from the database and resets it to initial state
    /// </summary>
    /// <remarks>
    /// **WARNING**: This operation removes ALL data from the database and cannot be undone.
    /// Use with extreme caution, especially in production environments.
    /// 
    /// The operation will:
    /// - Delete all game data
    /// - Delete all player data  
    /// - Delete all settings
    /// - Re-seed with default settings
    /// 
    /// Sample request:
    /// 
    ///     DELETE /api/database/cleanup
    /// 
    /// </remarks>
    /// <returns>Result of the cleanup operation</returns>
    /// <response code="200">Database cleaned up successfully</response>
    /// <response code="400">If cleanup failed due to validation or business logic errors</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpDelete("cleanup")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 500)]
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