using MarriageCalculator.Services.Interfaces;

namespace MarriageCalculator.Services.Implementations;

public class AuthenticationService : IAuthenticationService
{
    private readonly IApiService _apiService;
    public event EventHandler<bool>? AuthenticationStateChanged;

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
            await _apiService.ClearAuthTokenAsync();
            
            // Notify that authentication state changed
            AuthenticationStateChanged?.Invoke(this, false);
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
            await _apiService.SetAuthTokenAsync(token);
        }
        catch
        {
            // Ignore errors when setting token
        }
    }

    public async Task SetRefreshTokenAsync(string refreshToken)
    {
        try
        {
            await SecureStorage.SetAsync("refresh_token", refreshToken);
        }
        catch
        {
            // Ignore errors when setting refresh token
        }
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        try
        {
            return await SecureStorage.GetAsync("refresh_token");
        }
        catch
        {
            return null;
        }
    }

    public async Task SetTokenExpirationAsync(DateTime expiration)
    {
        try
        {
            await SecureStorage.SetAsync("token_expires", expiration.ToString("O"));
        }
        catch
        {
            // Ignore errors when setting expiration
        }
    }

    public async Task<DateTime?> GetTokenExpirationAsync()
    {
        try
        {
            var expiresString = await SecureStorage.GetAsync("token_expires");
            if (DateTime.TryParse(expiresString, out DateTime expires))
            {
                return expires;
            }
            return null;
        }
        catch
        {
            return null;
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
                    
                    // Notify that authentication state is active
                    AuthenticationStateChanged?.Invoke(this, true);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("AuthenticationService: Token expired, trying to refresh...");
                    
                    // Try to refresh the token before clearing authentication
                    var refreshSuccessful = await RefreshTokenAsync();
                    if (!refreshSuccessful)
                    {
                        System.Diagnostics.Debug.WriteLine("AuthenticationService: Token refresh failed, clearing authentication");
                        await ClearAuthenticationAsync();
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("AuthenticationService: No stored token found");
                // No token, make sure API service is cleared
                await _apiService.ClearAuthTokenAsync();
                AuthenticationStateChanged?.Invoke(this, false);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AuthenticationService: Error initializing authentication: {ex.Message}");
            // In case of error, clear the API service token to be safe
            await _apiService.ClearAuthTokenAsync();
            AuthenticationStateChanged?.Invoke(this, false);
        }
    }

    /// <summary>
    /// Refresh the access token using the refresh token
    /// </summary>
    public async Task<bool> RefreshTokenAsync()
    {
        try
        {
            var refreshToken = await GetRefreshTokenAsync();
            if (string.IsNullOrEmpty(refreshToken))
            {
                System.Diagnostics.Debug.WriteLine("AuthenticationService: No refresh token available");
                return false;
            }

            System.Diagnostics.Debug.WriteLine("AuthenticationService: Attempting to refresh token...");

            var refreshRequest = new { refreshToken };
            var response = await _apiService.PostAsync<TokenRefreshResponse>("api/UserAuth/refresh-token", refreshRequest);

            if (response?.Success == true && response.Data != null)
            {
                // Store new tokens
                await SetAuthenticationTokenAsync(response.Data.Token);
                await SetRefreshTokenAsync(response.Data.RefreshToken);
                await SetTokenExpirationAsync(response.Data.Expires);

                System.Diagnostics.Debug.WriteLine("AuthenticationService: Token refresh successful");
                
                // Notify that authentication state is still active
                AuthenticationStateChanged?.Invoke(this, true);
                
                return true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"AuthenticationService: Token refresh failed: {response?.Message}");
                return false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AuthenticationService: Token refresh error: {ex.Message}");
            return false;
        }
    }
}

// DTOs for token refresh
public class TokenRefreshResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public TokenData? Data { get; set; }
}

public class TokenData
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expires { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpires { get; set; }
}