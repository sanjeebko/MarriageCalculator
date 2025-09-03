using MarriageCalculator.Core.DTOs;
using MarriageCalculator.API.Repositories.Interfaces;
using MarriageCalculator.API.Services.Interfaces;

namespace MarriageCalculator.API.Services.Implementations;

public class DatabaseService : IDatabaseService
{
    private readonly IDatabaseRepository _databaseRepository;
    private readonly IMarriageGameServices _marriageGameServices;

    public DatabaseService(IDatabaseRepository databaseRepository, IMarriageGameServices marriageGameServices)
    {
        _databaseRepository = databaseRepository;
        _marriageGameServices = marriageGameServices;
    }

    public async Task<DatabaseInfoDto> GetDatabaseInfoAsync()
    {
        var canConnect = await _databaseRepository.CanConnectAsync();
        var tableCount = canConnect ? await _databaseRepository.GetTableCountAsync() : 0;
        var provider = await _databaseRepository.GetProviderNameAsync();

        return new DatabaseInfoDto
        {
            CanConnect = canConnect,
            Provider = provider,
            TableCount = tableCount,
            Message = canConnect ? "Database connection successful" : "Cannot connect to database",
            Timestamp = DateTime.UtcNow
        };
    }

    public async Task<ApiResponse> SeedDefaultDataAsync()
    {
        try
        {
            await _marriageGameServices.SeedDefaultData();
            return new ApiResponse
            {
                Success = true,
                Message = "Database seeded successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                Success = false,
                Message = $"Seeding failed: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse> CleanupDatabaseAsync()
    {
        try
        {
            await _marriageGameServices.CleanupDatabaseAsync();
            return new ApiResponse
            {
                Success = true,
                Message = "Database cleaned up successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                Success = false,
                Message = $"Cleanup failed: {ex.Message}"
            };
        }
    }
}