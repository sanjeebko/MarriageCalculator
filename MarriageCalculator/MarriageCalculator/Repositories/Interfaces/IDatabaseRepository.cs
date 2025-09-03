namespace MarriageCalculator.Repositories.Interfaces;

/// <summary>
/// Repository interface for Database operations in MAUI client
/// </summary>
public interface IDatabaseRepository
{
    Task<bool> TestConnectionAsync();
    Task SeedDefaultDataAsync();
    Task CleanupDatabaseAsync();
}