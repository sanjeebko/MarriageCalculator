using MarriageCalculator.Repositories;

namespace MarriageCalculator.Services;

/// <summary>
/// Service for testing and managing API connection
/// </summary>
public interface IConnectionService
{
    Task<bool> TestApiConnectionAsync();
    Task<string> GetApiStatusAsync();
    bool IsApiAvailable { get; }
}

/// <summary>
/// Implementation of connection service for API health checking
/// </summary>
public class ConnectionService : IConnectionService
{
    private readonly IDatabaseRepository _databaseRepository;
    private readonly IApiService _apiService;
    
    public bool IsApiAvailable { get; private set; }

    public ConnectionService(IDatabaseRepository databaseRepository, IApiService apiService)
    {
        _databaseRepository = databaseRepository;
        _apiService = apiService;
    }

    public async Task<bool> TestApiConnectionAsync()
    {
        try
        {
            IsApiAvailable = await _databaseRepository.TestConnectionAsync();
            return IsApiAvailable;
        }
        catch (Exception)
        {
            IsApiAvailable = false;
            return false;
        }
    }

    public async Task<string> GetApiStatusAsync()
    {
        try
        {
            var isConnected = await TestApiConnectionAsync();
            
            if (isConnected)
            {
                return "? API Connection: Active\n?? Status: Connected to MarriageCalculator.API\n?? Data Source: Remote Database";
            }
            else
            {
                return "? API Connection: Failed\n?? Status: Cannot reach MarriageCalculator.API\n?? Please check:\n  • API server is running\n  • Network connection\n  • API URL configuration";
            }
        }
        catch (Exception ex)
        {
            return $"? API Connection: Error\n?? Status: {ex.Message}\n?? Please verify API configuration";
        }
    }
}