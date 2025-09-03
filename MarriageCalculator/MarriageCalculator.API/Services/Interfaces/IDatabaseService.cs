using MarriageCalculator.Core.DTOs;

namespace MarriageCalculator.API.Services.Interfaces;

public interface IDatabaseService
{
    Task<DatabaseInfoDto> GetDatabaseInfoAsync();
    Task<ApiResponse> SeedDefaultDataAsync();
    Task<ApiResponse> CleanupDatabaseAsync();
}