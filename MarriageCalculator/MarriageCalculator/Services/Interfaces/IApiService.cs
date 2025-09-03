namespace MarriageCalculator.Services.Interfaces;

/// <summary>
/// Service interface for API communication operations in MAUI client
/// Provides HTTP client abstraction for communicating with the MarriageCalculator.API
/// </summary>
public interface IApiService
{
    Task<T?> GetAsync<T>(string endpoint);
    Task<T?> PostAsync<T>(string endpoint, object data);
    Task<T?> PutAsync<T>(string endpoint, object data);
    Task<bool> DeleteAsync(string endpoint);
    Task<bool> TestConnectionAsync();
    Task SetAuthTokenAsync(string token);
    Task ClearAuthTokenAsync();
}