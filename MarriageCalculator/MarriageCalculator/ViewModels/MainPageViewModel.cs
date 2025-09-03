using CommunityToolkit.Mvvm.ComponentModel;
using MarriageCalculator.Services.Interfaces;
using MarriageCalculator.Services.Implementations;

namespace MarriageCalculator.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    public IMarriageGameEngine GameEngine { get; set; }
    private readonly IApiService _apiService;
    private readonly IAuthenticationService _authenticationService;
    
    [ObservableProperty]
    private bool isBusy;
    
    [ObservableProperty]
    private bool showResumeGame;
    
    [ObservableProperty]
    private bool showNewGame;

    [ObservableProperty]
    private bool showSettings;
      
    [ObservableProperty]
    private bool showPlayer;

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
        showSettings = true;
        isServerConnected = false;
        isConnecting = true;  
        isUserAuthenticated = false;
    }
     

    public void Refresh()
    {
        IsBusy = true;
        
        // Connection status is now based on authentication, not game engine state
        // Show game options based on both connection AND game engine readiness
        //bool gameEngineReady = GameEngine?.Initialized == true;
        
        ShowNewGame = IsServerConnected;
        ShowResumeGame = IsServerConnected && GameEngine.IsActiveGame; 
        //ShowSettings = IsServerConnected && (gameEngineReady ? !GameEngine.IsActiveGame : true);
       // ShowPlayer = IsServerConnected && (gameEngineReady ? !GameEngine.IsActiveGame : true);
        
        System.Diagnostics.Debug.WriteLine($"Refresh: IsServerConnected={IsServerConnected}, ShowNewGame={ShowNewGame}, ShowResumeGame={ShowResumeGame}, ShowSettings={ShowSettings}, ShowPlayer={ShowPlayer}");
        
        IsBusy = false;
    }

    [RelayCommand]     
    async Task NewGameAsync()
    {
        // Navigate to GameSetup page with new game flag
        await Shell.Current.GoToAsync($"{nameof(GameSetupPage)}?newgame=true");
    }

    [RelayCommand]
    public async Task ResumeGame()
    {
        if(GameEngine.MarriageGameSet is null)
        {
            Refresh();
            return;
        }
        
        // Navigate to GameSetup page with resume game flag
        await Shell.Current.GoToAsync($"{nameof(GameSetupPage)}?newgame=false");
    }
    
    [RelayCommand]
    public async Task ResetGame()
    {
       await GameEngine.CleanMarriageGameSet();
       await GameEngine.InitializeEngineAsync();
       Refresh();
    }

    [RelayCommand] 
    public async Task  GameSettingsPage() {
        await Shell.Current.GoToAsync(nameof(SettingsPage));
    }

    [RelayCommand]
    public async Task  PlayerSettingsPage()
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
        
        try
        {
            // Re-check authentication status
            await LoadCurrentUserInfoAsync();
            await SetAuthenticationTokenAsync();
            
            if (IsUserAuthenticated == true)
            {
                // Test basic API connectivity
                IsServerConnected = await _apiService.TestConnectionAsync();
                
                if (IsServerConnected)
                {
                    System.Diagnostics.Debug.WriteLine("Retry: User authenticated and API accessible");
                    
                    try
                    {
                        // Try to re-initialize game engine
                        await GameEngine.InitializeEngineAsync();
                        System.Diagnostics.Debug.WriteLine("Retry: Game engine re-initialized successfully");
                    }
                    catch (Exception ex)
                    {
                        // Game engine failed but keep connection successful since API works
                        System.Diagnostics.Debug.WriteLine($"Retry: Game engine failed but API accessible: {ex.Message}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Retry: User authenticated but API not accessible");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Retry: User not authenticated");
                IsServerConnected = false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Retry connection exception: {ex.Message}");
            IsServerConnected = false;
        }
        finally
        {
            IsConnecting = false;
            IsBusy = false;
            Refresh();
        }
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
            // If user is authenticated, consider the connection as successful for UI purposes
            // The connection dot should be green if the user is logged in
            if (IsUserAuthenticated == true)
            {
                // Test basic API connectivity (not full game engine initialization)
                IsServerConnected = await _apiService.TestConnectionAsync();
                
                if (IsServerConnected)
                {
                    System.Diagnostics.Debug.WriteLine("User authenticated and API accessible - Connection considered successful");
                    
                    try
                    {
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
                            // Other issues - keep connection as successful since API is accessible
                            System.Diagnostics.Debug.WriteLine("Non-authentication issue - keeping connection status");
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("User authenticated but API not accessible");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("User not authenticated - connection status false");
                IsServerConnected = false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Connection test exception: {ex.Message}");
            IsServerConnected = false;
        }
        finally
        {
            IsConnecting = false;
        }

        Refresh();
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
            // Use the AuthenticationService's InitializeAuthenticationAsync method
            // which will properly synchronize tokens between SecureStorage and ApiService
            await _authenticationService.InitializeAuthenticationAsync();
            System.Diagnostics.Debug.WriteLine("Authentication initialized successfully for API calls");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing authentication: {ex.Message}");
        }
    }
}

