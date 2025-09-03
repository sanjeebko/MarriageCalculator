using MarriageCalculator.Services.Interfaces;

namespace MarriageCalculator.Services.Implementations;

public interface IAuthenticationService
{
    Task<bool> IsUserLoggedInAsync();
    Task<string?> GetCurrentUserTokenAsync();
    Task<string?> GetCurrentUserIdAsync();
    Task<string?> GetCurrentUserEmailAsync();
    Task<string?> GetCurrentUserDisplayNameAsync();
    Task ClearAuthenticationAsync();
    Task SetAuthenticationTokenAsync(string token);
    Task InitializeAuthenticationAsync(); // New method to initialize auth state
}

public class AuthenticationService : IAuthenticationService
{
    private readonly IApiService _apiService;

    public AuthenticationService(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<bool> IsUserLoggedInAsync()
    {
        try
        {
            var token = await SecureStorage.GetAsync("access_token");
            var expiresString = await SecureStorage.GetAsync("token_expires");
            
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(expiresString))
                return false;

            if (DateTime.TryParse(expiresString, out DateTime expires))
            {
                return DateTime.UtcNow < expires;
            }
            
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string?> GetCurrentUserTokenAsync()
    {
        try
        {
            return await SecureStorage.GetAsync("access_token");
        }
        catch
        {
            return null;
        }
    }

    public async Task<string?> GetCurrentUserIdAsync()
    {
        try
        {
            return await SecureStorage.GetAsync("user_id");
        }
        catch
        {
            return null;
        }
    }

    public async Task<string?> GetCurrentUserEmailAsync()
    {
        try
        {
            return await SecureStorage.GetAsync("user_email");
        }
        catch
        {
            return null;
        }
    }

    public async Task<string?> GetCurrentUserDisplayNameAsync()
    {
        try
        {
            return await SecureStorage.GetAsync("user_display_name");
        }
        catch
        {
            return null;
        }
    }

    public async Task ClearAuthenticationAsync()
    {
        try
        {
            SecureStorage.RemoveAll();
            // Also clear the API service token
            await _apiService.ClearAuthTokenAsync();
        }
        catch
        {
            // Ignore errors when clearing storage
        }
    }

    public async Task SetAuthenticationTokenAsync(string token)
    {
        try
        {
            await SecureStorage.SetAsync("access_token", token);
            // Also set it in the API service
            await _apiService.SetAuthTokenAsync(token);
        }
        catch
        {
            // Ignore errors when setting token
        }
    }

    /// <summary>
    /// Initialize authentication state on app startup
    /// This will restore the JWT token to the ApiService if the user is logged in
    /// </summary>
    public async Task InitializeAuthenticationAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("AuthenticationService: Initializing authentication...");
            
            var token = await GetCurrentUserTokenAsync();
            
            if (!string.IsNullOrEmpty(token))
            {
                System.Diagnostics.Debug.WriteLine($"AuthenticationService: Found stored token (length: {token.Length})");
                
                if (await IsUserLoggedInAsync())
                {
                    // User has a valid token, set it in the API service
                    await _apiService.SetAuthTokenAsync(token);
                    System.Diagnostics.Debug.WriteLine("AuthenticationService: Valid token set in ApiService");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("AuthenticationService: Token expired, clearing authentication");
                    await ClearAuthenticationAsync();
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("AuthenticationService: No stored token found");
                // No token, make sure API service is cleared
                await _apiService.ClearAuthTokenAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AuthenticationService: Error initializing authentication: {ex.Message}");
            // In case of error, clear the API service token to be safe
            await _apiService.ClearAuthTokenAsync();
        }
    }
}