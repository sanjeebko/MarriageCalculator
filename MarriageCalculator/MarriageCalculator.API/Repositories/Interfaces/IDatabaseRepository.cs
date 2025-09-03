namespace MarriageCalculator.API.Repositories.Interfaces;

/// <summary>
/// Repository interface for Database operations
/// </summary>
public interface IDatabaseRepository
{
    Task<bool> CanConnectAsync();
    Task<int> GetTableCountAsync();
    Task<string> GetProviderNameAsync();
}