namespace MarriageCalculator.Services.Interfaces;

public interface IAuthenticationService
{
    Task<bool> IsUserLoggedInAsync();
    Task<string?> GetCurrentUserTokenAsync();
    Task<string?> GetCurrentUserIdAsync();
    Task<string?> GetCurrentUserEmailAsync();
    Task<string?> GetCurrentUserDisplayNameAsync();
    Task ClearAuthenticationAsync();
    Task SetAuthenticationTokenAsync(string token);
    Task SetRefreshTokenAsync(string refreshToken);
    Task<string?> GetRefreshTokenAsync();
    Task SetTokenExpirationAsync(DateTime expiration);
    Task<DateTime?> GetTokenExpirationAsync();
    Task InitializeAuthenticationAsync();
    Task<bool> RefreshTokenAsync();
    event EventHandler<bool>? AuthenticationStateChanged;
}

public interface IAuthenticationManager
{
    Task StartAsync();
    Task StopAsync();
    event EventHandler<string>? AuthenticationStatusChanged;
}