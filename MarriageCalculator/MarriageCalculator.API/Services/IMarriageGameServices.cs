using MarriageCalculator.Core.Models;

namespace MarriageCalculator.API.Services;

/// <summary>
/// Interface for Marriage Game Services providing database setup and data seeding functionality
/// </summary>
public interface IMarriageGameServices
{
    /// <summary>
    /// Sets up the database, ensuring it's created and seeded with default data
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    Task SetupDB();

    /// <summary>
    /// Seeds the database with default game settings if none exist
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    Task SeedDefaultData();

    /// <summary>
    /// Gets database health and statistics information
    /// </summary>
    /// <returns>A task containing database information</returns>
    Task<DatabaseInfo> GetDatabaseInfoAsync();

    /// <summary>
    /// Cleans up and resets the database
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    Task CleanupDatabaseAsync();
}

/// <summary>
/// Database information model for health checks and statistics
/// </summary>
public class DatabaseInfo
{
    public int PlayerCount { get; set; }
    public int GameSettingsCount { get; set; }
    public int MarriageGameSetCount { get; set; }
    public int MarriageGameSetPlayerCount { get; set; }
    public int MarriageGameRoundCount { get; set; }
    public int MarriageGameCount { get; set; }
    public int MarriageGameScoreCount { get; set; }
    public bool DatabaseCreated { get; set; }
    public DateTime LastChecked { get; set; } = DateTime.UtcNow;
}