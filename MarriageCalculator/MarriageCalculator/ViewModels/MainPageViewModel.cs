using CommunityToolkit.Mvvm.ComponentModel;
using MarriageCalculator.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MarriageCalculator.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    public IMarriageGameEngine GameEngine { get; set; }
    private readonly IApiService _apiService;
    private readonly IAuthenticationService _authenticationService;
    public ObservableCollection<GameSetDetails> GameSets { get; set; } = new();

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool showNewGame;
     
    [ObservableProperty]
    private bool isServerConnected;

    [ObservableProperty]
    private bool isConnecting;

    [ObservableProperty]
    private string currentUserDisplayName = "User";

    [ObservableProperty]
    private bool isLoggingOut;

    [ObservableProperty]
    private bool? isUserAuthenticated;

    public MainPageViewModel(IApiService apiService, IAuthenticationService authenticationService, IMarriageGameEngine gameEngine)
    {
        _apiService = apiService;
        _authenticationService = authenticationService;
        GameEngine = gameEngine;
         
        isServerConnected = false;
        isConnecting = true;
        isUserAuthenticated = false;
    }

    public async Task InitializeAsync()
    {
        IsConnecting = true;
        IsServerConnected = false;

        // Load current user info for display and check authentication
        await LoadCurrentUserInfoAsync();

        // Set authentication token for API calls BEFORE testing connection
        await SetAuthenticationTokenAsync();

        try
        {
            // If user is authenticated, test the API connection
            if (IsUserAuthenticated == true)
            { 
                System.Diagnostics.Debug.WriteLine("User is authenticated, testing API connection...");

                // Test basic API connectivity with the authentication token
                IsServerConnected = await _apiService.TestConnectionAsync();

                if (IsServerConnected)
                {
                    System.Diagnostics.Debug.WriteLine("User authenticated and API accessible - Connection considered successful");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("User authenticated but API not accessible");

                    // Check if we have user info loaded despite connection failure
                    if (!string.IsNullOrEmpty(CurrentUserDisplayName) && CurrentUserDisplayName != "User")
                    {
                        System.Diagnostics.Debug.WriteLine("User display name is loaded, but API connection failed");
                        System.Diagnostics.Debug.WriteLine("This suggests a network connectivity issue");
                        // Keep IsServerConnected = false to show connection error
                    }
                }

                // If connection is established, initialize the game engine to load game state
                if (IsServerConnected)
                {
                    try
                    {
                        Guid userId = await GetCurrentUserIdAsync();
                        GameEngine.SetUserId(userId);
                        // Set server connected status in game engine
                        GameEngine.SetServerConnectedStatus(true);

                        // IMPORTANT: Initialize game engine AFTER authentication is properly set up
                        await GameEngine.InitializeEngineAsync();
                        System.Diagnostics.Debug.WriteLine("Game engine initialized successfully");
                    }
                    catch (Exception ex)
                    {
                        // Game engine initialization failed - could be authentication or other issues
                        System.Diagnostics.Debug.WriteLine($"Game engine initialization failed: {ex.Message}");

                        // Check if it's an authentication issue
                        if (ex.Message.Contains("Unauthorized") || ex.Message.Contains("401"))
                        {
                            System.Diagnostics.Debug.WriteLine("Authentication issue detected - user may need to login again");
                            IsUserAuthenticated = false;
                            IsServerConnected = false;
                        }
                        else
                        {
                            // Other issues - but API is accessible, so keep connection successful
                            System.Diagnostics.Debug.WriteLine("Non-authentication issue - keeping connection status");
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("API connection failed - game engine will not be initialized");
                    GameEngine.SetServerConnectedStatus(false);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("User not authenticated - connection status false");
                IsServerConnected = false;
                GameEngine.SetServerConnectedStatus(false);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Connection test exception: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Exception type: {ex.GetType().Name}");

            // If we have user info but connection test failed, it might be a network issue  
            if (!string.IsNullOrEmpty(CurrentUserDisplayName) && CurrentUserDisplayName != "User")
            {
                System.Diagnostics.Debug.WriteLine("User info exists despite connection error");
                System.Diagnostics.Debug.WriteLine("This suggests the issue is network connectivity, not authentication");
            }

            IsServerConnected = false;
            GameEngine.SetServerConnectedStatus(IsServerConnected);
        }
        finally
        {
            IsConnecting = false;
        }

        await Refresh();

        // Log final state for debugging
        System.Diagnostics.Debug.WriteLine($"=== Initialize Complete ===");
        System.Diagnostics.Debug.WriteLine($"IsUserAuthenticated: {IsUserAuthenticated}");
        System.Diagnostics.Debug.WriteLine($"IsServerConnected: {IsServerConnected}");
        System.Diagnostics.Debug.WriteLine($"CurrentUserDisplayName: {CurrentUserDisplayName}");
        System.Diagnostics.Debug.WriteLine($"========================");
    }

    private async Task<Guid> GetCurrentUserIdAsync()
    {
        var userId = await _authenticationService.GetCurrentUserIdAsync();
        Guid userIdGuid = userId != null ? Guid.Parse(userId) : Guid.Empty;
        return userIdGuid;
    }

    public async Task Refresh()
    {
        IsBusy = true;
        if (IsServerConnected)
        {
           await LoadGameSets();
        }
        ShowNewGame = IsServerConnected;
        System.Diagnostics.Debug.WriteLine($"Refresh: IsServerConnected={IsServerConnected}, ShowNewGame={ShowNewGame}");

        IsBusy = false;
    }

    private async Task LoadGameSets()
    {
        GameSets.Clear();
        if (GameEngine.MarriageGameSets != null)
        {
            foreach (var set in GameEngine.MarriageGameSets.OrderByDescending(gs => gs.LastPlayed))
            {
                var gameSetPlayers = await GameEngine.GetGameSetPlayersByIdAsync(set.Id);
                GameSets.Add(new GameSetDetails
                {
                    GameSetId = set.Id,
                    GameSetName = set.Name,
                    Created = set.Created,
                    LastPlayed = set.LastPlayed,
                    TotalRounds = set.Rounds?.Count ?? 0,
                    TotalGames = set.Rounds?.Sum(r => r.MarriageGames?.Count ?? 0) ?? 0,
                    TotalPlayers = gameSetPlayers?.Count ?? 0,
                    Players = gameSetPlayers?.Select(gsp => gsp.Player).ToList() ?? new List<Player>(),
                    IsActive = set.IsActive
                });
            }
        }
    }

    [RelayCommand]
    async Task NewGameAsync()
    {
        //Create a new MarriageGameSet. 
        await GameEngine.CreateNewGameSet();

        // Navigate to GameSetup page with new game flag
        await Shell.Current.GoToAsync(nameof(GameSetupPage));
    }

    [RelayCommand]
    public async Task SelectGameSet(GameSetDetails gameSet)
    {
        if (gameSet == null)
            return;

        IsBusy = true;
        try
        {
            // Load the specific game set by ID
            await GameEngine.LoadGameSetAsync(gameSet.GameSetId);
            
            // Navigate to GameSetup page to continue with this game set
            await Shell.Current.GoToAsync($"{nameof(GameSetupPage)}?newgame=false");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error", 
                $"Failed to load game set: {ex.Message}", 
                "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task ResetGame()
    {
        await GameEngine.CleanMarriageGameSet();
        await GameEngine.InitializeEngineAsync();
        Refresh();
    }

    [RelayCommand]
    public async Task GameSettingsPage()
    {
        await Shell.Current.GoToAsync(nameof(SettingsPage));
    }

    [RelayCommand]
    public async Task PlayerSettingsPage()
    {
        await Shell.Current.GoToAsync(nameof(PlayersPage));
    }

    [RelayCommand]
    public void Exit()
    {
        Application.Current.Quit();
    }

    [RelayCommand]
    public async Task RetryConnection()
    {
        IsBusy = true;
        IsConnecting = true;
        IsServerConnected = false;

        System.Diagnostics.Debug.WriteLine("=== RETRY CONNECTION DEBUG ===");

        try
        {
            // First, ensure we have authentication set up properly
            await LoadCurrentUserInfoAsync();
            await SetAuthenticationTokenAsync();

            // Get token for manual verification
            var token = await _authenticationService.GetCurrentUserTokenAsync();
            System.Diagnostics.Debug.WriteLine($"Current token available: {!string.IsNullOrEmpty(token)}");
            if (!string.IsNullOrEmpty(token))
            {
                System.Diagnostics.Debug.WriteLine($"Token length: {token.Length}");
                System.Diagnostics.Debug.WriteLine($"Token preview: {token.Substring(0, Math.Min(50, token.Length))}...");
            }

            // Test basic network connectivity first
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            // Test if we can reach Google (basic internet connectivity)
            try
            {
                var googleResponse = await httpClient.GetAsync("https://www.google.com");
                System.Diagnostics.Debug.WriteLine($"? Google connectivity test: {googleResponse.StatusCode}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? No internet connectivity: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert(
                    "No Internet",
                    "Please check your internet connection and try again.",
                    "OK");
                return;
            }

            // Test if we can reach the API server (basic server reachability)
            try
            {
                var apiBaseResponse = await httpClient.GetAsync("https://mcapi.sanjeebojha.com.np/");
                System.Diagnostics.Debug.WriteLine($"? API server reachable: {apiBaseResponse.StatusCode}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? API server unreachable: {ex.Message}");

                await Application.Current.MainPage.DisplayAlert(
                    "Server Unreachable",
                    $"Cannot reach the game server.\n\n" +
                    $"Please check:\n" +
                    $"• Server might be temporarily down\n" +
                    $"• Firewall or network restrictions\n\n" +
                    $"Technical: {ex.Message}",
                    "OK");
                return;
            }

            // Test the authenticated endpoint manually with token
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var authTestResponse = await httpClient.GetAsync("https://mcapi.sanjeebojha.com.np/api/UserAuth/me");
                    System.Diagnostics.Debug.WriteLine($"Manual auth test result: {authTestResponse.StatusCode}");

                    if (authTestResponse.IsSuccessStatusCode)
                    {
                        var content = await authTestResponse.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine($"? Manual auth test successful");
                        System.Diagnostics.Debug.WriteLine($"Response: {content.Substring(0, Math.Min(200, content.Length))}...");

                        // If manual test works, the issue might be in ApiService
                        System.Diagnostics.Debug.WriteLine("Manual test successful - checking ApiService...");
                    }
                    else
                    {
                        var errorContent = await authTestResponse.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine($"? Manual auth test failed: {authTestResponse.StatusCode}");
                        System.Diagnostics.Debug.WriteLine($"Error: {errorContent}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"? Manual auth test exception: {ex.Message}");
                }
            }
        }
        catch (Exception debugEx)
        {
            System.Diagnostics.Debug.WriteLine($"? Debug testing failed: {debugEx.Message}");
        }

        System.Diagnostics.Debug.WriteLine("=== END MANUAL TESTS ===");

        // Now try the ApiService connection test
        if (IsUserAuthenticated == true)
        {
            System.Diagnostics.Debug.WriteLine("Starting ApiService connection test...");
            IsServerConnected = await _apiService.TestConnectionAsync();

            if (IsServerConnected)
            {
                System.Diagnostics.Debug.WriteLine("? ApiService connection test successful");

                try
                {
                    GameEngine.SetServerConnectedStatus(true);
                    await GameEngine.InitializeEngineAsync();
                    System.Diagnostics.Debug.WriteLine("? Game engine re-initialized successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"? Game engine failed: {ex.Message}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("? ApiService connection test failed");

                // Show detailed error to user
                await Application.Current.MainPage.DisplayAlert(
                    "Connection Failed",
                    "The authentication test failed. This could mean:\n\n" +
                    "• Your login session has expired\n" +
                    "• Server authentication issues\n" +
                    "• Network connectivity problems\n\n" +
                    "Try logging out and logging back in.",
                    "OK");
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("? User not authenticated");
            await Application.Current.MainPage.DisplayAlert(
                "Not Authenticated",
                "You are not logged in. Please log in and try again.",
                "OK");
        }

        IsConnecting = false;
        IsBusy = false;
        Refresh();

        System.Diagnostics.Debug.WriteLine("=== RETRY CONNECTION COMPLETE ===");
    }

    [RelayCommand]
    public async Task LogoutAsync()
    {
        if (IsLoggingOut) return;

        // Add debug information
        System.Diagnostics.Debug.WriteLine($"LogoutAsync called - IsUserAuthenticated: {IsUserAuthenticated}");

        // Show confirmation dialog before logout
        bool userConfirmed = await Application.Current.MainPage.DisplayAlert(
            "Confirm Logout",
            "Are you sure you want to logout?",
            "Yes, Logout",
            "Cancel");

        if (!userConfirmed)
        {
            System.Diagnostics.Debug.WriteLine("User cancelled logout");
            return; // User cancelled, don't proceed with logout
        }

        try
        {
            IsLoggingOut = true;

            // Get current token for API logout call
            var token = await _authenticationService.GetCurrentUserTokenAsync();
            System.Diagnostics.Debug.WriteLine($"Retrieved token: {(!string.IsNullOrEmpty(token) ? "Yes" : "No")}");

            // Call API logout endpoint if we have a token (only if server is connected)
            if (!string.IsNullOrEmpty(token) && IsServerConnected)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("Calling API logout endpoint...");
                    // Note: The API expects the token in the Authorization header
                    // The ApiService should handle this automatically if configured properly
                    var response = await _apiService.PostAsync<ApiResponse>("api/UserAuth/logout", new { });

                    // Log the result but don't block logout if API call fails
                    if (response?.Success != true)
                    {
                        System.Diagnostics.Debug.WriteLine($"API logout failed: {response?.Message}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("API logout successful");
                    }
                }
                catch (Exception ex)
                {
                    // Don't block logout if API call fails
                    System.Diagnostics.Debug.WriteLine($"API logout error: {ex.Message}");
                }
            }

            // Clear local authentication data regardless of API call result
            await _authenticationService.ClearAuthenticationAsync();
            System.Diagnostics.Debug.WriteLine("Local authentication data cleared");

            // Clear API token
            await _apiService.ClearAuthTokenAsync();

            // Update authentication status
            IsUserAuthenticated = false;
            System.Diagnostics.Debug.WriteLine($"IsUserAuthenticated set to: {IsUserAuthenticated}");

            // Show logout confirmation
            await Application.Current.MainPage.DisplayAlert(
                "Logged Out",
                "You have been successfully logged out.",
                "OK");

            // Navigate back to login page
            await Shell.Current.GoToAsync("//LoginPage");
            System.Diagnostics.Debug.WriteLine("Navigated to login page");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Logout error: {ex}");
            // Handle any unexpected errors
            await Application.Current.MainPage.DisplayAlert(
                "Logout Error",
                $"An error occurred during logout: {ex.Message}",
                "OK");
        }
        finally
        {
            IsLoggingOut = false;
            System.Diagnostics.Debug.WriteLine("LogoutAsync completed");
        }
    }

    private async Task LoadCurrentUserInfoAsync()
    {
        try
        {
            
            // Check if user is authenticated
            IsUserAuthenticated = await _authenticationService.IsUserLoggedInAsync();
            System.Diagnostics.Debug.WriteLine($"LoadCurrentUserInfoAsync - IsUserAuthenticated: {IsUserAuthenticated}");
            
            var displayName = await _authenticationService.GetCurrentUserDisplayNameAsync();
            CurrentUserDisplayName = !string.IsNullOrEmpty(displayName) ? displayName : "User";
            System.Diagnostics.Debug.WriteLine($"Current user display name: {CurrentUserDisplayName}");

            // If we successfully got user display name from the API, connection is working
            if (!string.IsNullOrEmpty(displayName) && displayName != "User")
            {
                System.Diagnostics.Debug.WriteLine("User display name retrieved successfully - API connection is working");
                // This will be used as a fallback in InitializeAsync if TestConnectionAsync fails
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadCurrentUserInfoAsync error: {ex.Message}");
            CurrentUserDisplayName = "User";
            IsUserAuthenticated = false;
        }
    }

    private async Task SetAuthenticationTokenAsync()
    {
        try
        {
            // Get the current token directly and ensure it's set in the API service
            var currentToken = await _authenticationService.GetCurrentUserTokenAsync();
            if (!string.IsNullOrEmpty(currentToken))
            {
                await _apiService.SetAuthTokenAsync(currentToken);
                System.Diagnostics.Debug.WriteLine($"Authentication token set in ApiService. Token length: {currentToken.Length}");
                System.Diagnostics.Debug.WriteLine($"Token starts with: {currentToken.Substring(0, Math.Min(10, currentToken.Length))}...");

                // Verify the token is actually set by checking the HttpClient headers
                System.Diagnostics.Debug.WriteLine("Authentication token successfully set in ApiService");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No authentication token found in secure storage");
                System.Diagnostics.Debug.WriteLine("User may need to log in again");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error setting authentication token: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Exception type: {ex.GetType().Name}");
        }
    }
}

public class GameSetDetails
{
    public int GameSetId { get; set; }
    public string GameSetName { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public DateTime LastPlayed { get; set; }
    public int TotalRounds { get; set; }
    public int TotalGames { get; set; }
    public int TotalPlayers { get; set; }
    public List<Player> Players { get; set; } = new();

    public string PlayerNames
    {
        get
        {
            if (Players == null || Players.Count == 0)
                return string.Empty;
            return string.Join(", ", Players.Select(p => p.Name));
        }
    }
    public bool IsActive { get; set; }
}