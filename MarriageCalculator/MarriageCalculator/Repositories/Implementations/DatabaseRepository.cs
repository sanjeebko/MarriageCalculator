using MarriageCalculator.Repositories.Interfaces;
using MarriageCalculator.Services.Interfaces;

namespace MarriageCalculator.Repositories.Implementations;

/// <summary>
/// Database repository implementation using API service
/// </summary>
public class DatabaseRepository : IDatabaseRepository
{
    private readonly IApiService _apiService;

    public DatabaseRepository(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<bool> TestConnectionAsync()
    {
        return await _apiService.TestConnectionAsync();
    }

    public async Task SeedDefaultDataAsync()
    {
        await _apiService.PostAsync<object>("api/database/seed", new { });
    }

    public async Task CleanupDatabaseAsync()
    {
        await _apiService.DeleteAsync("api/database/cleanup");
    }
}
