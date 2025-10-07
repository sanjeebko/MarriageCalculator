using MarriageCalculator.Services.Interfaces;
using System.Timers;

namespace MarriageCalculator.Services.Implementations;

/// <summary>
/// Background service that automatically manages token refresh and authentication state
/// Runs a timer that checks token expiration and refreshes tokens before they expire
/// </summary>
public class AuthenticationManager : IAuthenticationManager, IDisposable
{
    private readonly IAuthenticationService _authenticationService;
    private System.Timers.Timer? _refreshTimer;
    private bool _isStarted = false;
    private readonly object _lockObject = new object();

    // Check every 5 minutes
    private const double CheckIntervalMinutes = 5;
    // Refresh token when it expires in 10 minutes or less
    private const double RefreshThresholdMinutes = 10;

    public event EventHandler<string>? AuthenticationStatusChanged;

    public AuthenticationManager(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
        
        // Subscribe to authentication state changes
        _authenticationService.AuthenticationStateChanged += OnAuthenticationStateChanged;
    }

    private void OnAuthenticationStateChanged(object? sender, bool isAuthenticated)
    {
        var status = isAuthenticated ? "Authenticated" : "Not Authenticated";
        System.Diagnostics.Debug.WriteLine($"AuthenticationManager: Authentication state changed to {status}");
        AuthenticationStatusChanged?.Invoke(this, status);
    }

    public async Task StartAsync()
    {
        lock (_lockObject)
        {
            if (_isStarted)
            {
                System.Diagnostics.Debug.WriteLine("AuthenticationManager: Already started");
                return;
            }

            _isStarted = true;
        }

        System.Diagnostics.Debug.WriteLine("AuthenticationManager: Starting background authentication management");

        // Initialize authentication state
        await _authenticationService.InitializeAuthenticationAsync();

        // Start the refresh timer
        StartRefreshTimer();

        // Perform initial check
        await CheckAndRefreshTokenAsync();

        System.Diagnostics.Debug.WriteLine("AuthenticationManager: Started successfully");
    }

    public async Task StopAsync()
    {
        lock (_lockObject)
        {
            if (!_isStarted)
            {
                return;
            }

            _isStarted = false;
        }

        System.Diagnostics.Debug.WriteLine("AuthenticationManager: Stopping background authentication management");

        // Stop and dispose timer
        _refreshTimer?.Stop();
        _refreshTimer?.Dispose();
        _refreshTimer = null;

        System.Diagnostics.Debug.WriteLine("AuthenticationManager: Stopped");
        await Task.CompletedTask;
    }

    private void StartRefreshTimer()
    {
        _refreshTimer = new System.Timers.Timer(TimeSpan.FromMinutes(CheckIntervalMinutes).TotalMilliseconds);
        _refreshTimer.Elapsed += OnTimerElapsed;
        _refreshTimer.AutoReset = true;
        _refreshTimer.Enabled = true;

        System.Diagnostics.Debug.WriteLine($"AuthenticationManager: Refresh timer started - checking every {CheckIntervalMinutes} minutes");
    }

    private async void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        try
        {
            await CheckAndRefreshTokenAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AuthenticationManager: Error during timer check: {ex.Message}");
        }
    }

    private async Task CheckAndRefreshTokenAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("AuthenticationManager: Checking token status...");

            // Check if user is logged in
            var isLoggedIn = await _authenticationService.IsUserLoggedInAsync();
            if (!isLoggedIn)
            {
                System.Diagnostics.Debug.WriteLine("AuthenticationManager: User not logged in, skipping token check");
                return;
            }

            // Get token expiration
            var tokenExpiration = await _authenticationService.GetTokenExpirationAsync();
            if (tokenExpiration == null)
            {
                System.Diagnostics.Debug.WriteLine("AuthenticationManager: No token expiration found");
                return;
            }

            var timeUntilExpiration = tokenExpiration.Value - DateTime.UtcNow;
            System.Diagnostics.Debug.WriteLine($"AuthenticationManager: Token expires in {timeUntilExpiration.TotalMinutes:F1} minutes");

            // Check if token needs refresh (expires within threshold)
            if (timeUntilExpiration.TotalMinutes <= RefreshThresholdMinutes)
            {
                System.Diagnostics.Debug.WriteLine($"AuthenticationManager: Token expires soon, attempting refresh...");

                var refreshSuccessful = await _authenticationService.RefreshTokenAsync();
                if (refreshSuccessful)
                {
                    System.Diagnostics.Debug.WriteLine("AuthenticationManager: Token refresh successful");
                    AuthenticationStatusChanged?.Invoke(this, "Token Refreshed");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("AuthenticationManager: Token refresh failed");
                    AuthenticationStatusChanged?.Invoke(this, "Token Refresh Failed");
                    
                    // Clear authentication if refresh failed
                    await _authenticationService.ClearAuthenticationAsync();
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("AuthenticationManager: Token is valid, no refresh needed");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AuthenticationManager: Error checking token: {ex.Message}");
        }
    }

    /// <summary>
    /// Force a token refresh check (useful for manual refresh triggers)
    /// </summary>
    public async Task ForceTokenCheckAsync()
    {
        System.Diagnostics.Debug.WriteLine("AuthenticationManager: Force token check requested");
        await CheckAndRefreshTokenAsync();
    }

    /// <summary>
    /// Get current authentication status for debugging
    /// </summary>
    public async Task<string> GetAuthenticationStatusAsync()
    {
        try
        {
            var isLoggedIn = await _authenticationService.IsUserLoggedInAsync();
            if (!isLoggedIn)
            {
                return "Not Authenticated";
            }

            var tokenExpiration = await _authenticationService.GetTokenExpirationAsync();
            if (tokenExpiration == null)
            {
                return "Authenticated (Unknown Expiration)";
            }

            var timeUntilExpiration = tokenExpiration.Value - DateTime.UtcNow;
            if (timeUntilExpiration.TotalMinutes <= 0)
            {
                return "Token Expired";
            }
            else if (timeUntilExpiration.TotalMinutes <= RefreshThresholdMinutes)
            {
                return $"Token Expires Soon ({timeUntilExpiration.TotalMinutes:F1} min)";
            }
            else
            {
                return $"Authenticated ({timeUntilExpiration.TotalMinutes:F1} min remaining)";
            }
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    public void Dispose()
    {
        _refreshTimer?.Stop();
        _refreshTimer?.Dispose();
        
        // Unsubscribe from events
        if (_authenticationService != null)
        {
            _authenticationService.AuthenticationStateChanged -= OnAuthenticationStateChanged;
        }
    }
}