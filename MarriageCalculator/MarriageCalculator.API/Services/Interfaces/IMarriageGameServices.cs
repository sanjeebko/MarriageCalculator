using MarriageCalculator.Core.DTOs;

namespace MarriageCalculator.API.Services.Interfaces;

/// <summary>
/// Interface for Marriage Game Services
/// Provides database setup, seeding, and information retrieval
/// </summary>
public interface IMarriageGameServices
{
    /// <summary>
    /// Sets up the database, ensuring it's created and seeded with default data
    /// </summary>
    Task SetupDB();
    
    /// <summary>
    /// Seeds the database with default game settings if none exist
    /// </summary>
    Task SeedDefaultData();
    
    /// <summary>
    /// Gets comprehensive database health and statistics information
    /// </summary>
    Task<DatabaseInfo> GetDatabaseInfoAsync();
    
    /// <summary>
    /// Cleans up and resets the entire database
    /// </summary>
    Task CleanupDatabaseAsync();
}

/// <summary>
/// Database information model containing statistics and health data
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
    public DateTime LastChecked { get; set; }
}