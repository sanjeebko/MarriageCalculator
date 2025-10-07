using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarriageCalculator.Services.Interfaces;
using MarriageCalculator.Core.DTOs;
using System.ComponentModel.DataAnnotations;

namespace MarriageCalculator.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IApiService _apiService;
    private readonly IAuthenticationService _authenticationService;
    private readonly IAuthenticationManager _authenticationManager;

    public LoginViewModel(IApiService apiService, IAuthenticationService authenticationService, IAuthenticationManager authenticationManager)
    {
        _apiService = apiService;
        _authenticationService = authenticationService;
        _authenticationManager = authenticationManager;
        IsPasswordHidden = true;
        PasswordToggleIcon = "??";
        RememberMe = true;
    }

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private bool rememberMe;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool hasError;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool isPasswordHidden;

    [ObservableProperty]
    private string passwordToggleIcon = string.Empty;

    // Email verification properties
    [ObservableProperty]
    private bool isVerificationDialogVisible;

    [ObservableProperty]
    private string verificationCode = string.Empty;

    [ObservableProperty]
    private bool isVerificationLoading;

    [ObservableProperty]
    private string verificationEmail = string.Empty;

    [ObservableProperty]
    private string verificationMessage = string.Empty;

    [ObservableProperty]
    private bool hasVerificationError;

    public bool IsNotLoading => !IsLoading;
    public bool IsNotVerificationLoading => !IsVerificationLoading;

    [RelayCommand]
    private void TogglePasswordVisibility()
    {
        IsPasswordHidden = !IsPasswordHidden;
        PasswordToggleIcon = IsPasswordHidden ? "??" : "??";
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsLoading) return;

        // Clear previous errors
        HasError = false;
        ErrorMessage = string.Empty;

        // Validate input
        if (string.IsNullOrWhiteSpace(Username))
        {
            ShowError("Please enter your email or username.");
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ShowError("Please enter your password.");
            return;
        }

        try
        {
            IsLoading = true;

            var loginDto = new LoginUserDto
            {
                Email = Username.Trim(),
                Password = Password
            };

            // Call the API login endpoint using ApiService
            var response = await _apiService.PostAsync<ApiResponse<LoginResponseDto>>(
                "api/UserAuth/login", loginDto);

            if (response?.Success == true && response.Data != null)
            {
                // Store authentication token and user info using AuthenticationService
                if (!string.IsNullOrEmpty(response.Data.Token))
                {
                    await _authenticationService.SetAuthenticationTokenAsync(response.Data.Token);
                    await _authenticationService.SetTokenExpirationAsync(response.Data.Expires);
                }

                if (!string.IsNullOrEmpty(response.Data.RefreshToken))
                {
                    await _authenticationService.SetRefreshTokenAsync(response.Data.RefreshToken);
                }

                // Store user info
                if (response.Data.User != null)
                {
                    await SecureStorage.SetAsync("user_id", response.Data.User.Id.ToString());
                    await SecureStorage.SetAsync("user_email", response.Data.User.Email ?? "");
                    await SecureStorage.SetAsync("user_display_name", response.Data.User.DisplayName ?? "");
                    await SecureStorage.SetAsync("user_email_verified", response.Data.User.IsEmailVerified.ToString());

                    // Check if email verification is required
                    if (!response.Data.User.IsEmailVerified)
                    {
                        // Show verification dialog
                        ShowVerificationDialog(response.Data.User.Email);
                        return;
                    }
                }

                // Store remember me preference
                if (RememberMe)
                {
                    await SecureStorage.SetAsync("remember_me", "true");
                    await SecureStorage.SetAsync("saved_username", Username.Trim());
                }
                else
                {
                    SecureStorage.Remove("remember_me");
                    SecureStorage.Remove("saved_username");
                }

                // ? START AUTHENTICATION MANAGER AFTER SUCCESSFUL LOGIN
                System.Diagnostics.Debug.WriteLine("LoginViewModel: Starting AuthenticationManager after successful login");
                await _authenticationManager.StartAsync();

                // Navigate to main page
                await Shell.Current.GoToAsync("//MainPage");
            }
            else
            {
                // Check if it's an email verification issue
                if (response?.Message?.Contains("verify your email") == true || 
                    response?.Message?.Contains("email verification") == true)
                {
                    // Show error with option to verify
                    var userChoice = await Application.Current.MainPage.DisplayAlert(
                        "Email Verification Required",
                        "Please verify your email address before logging in. Would you like to verify now?",
                        "Verify Now",
                        "Cancel");

                    if (userChoice)
                    {
                        // Navigate to verification page
                        await Shell.Current.GoToAsync("//EmailVerificationPage");
                    }
                }
                else
                {
                    ShowError(response?.Message ?? "Login failed. Please check your credentials and try again.");
                }
            }
        }
        catch (HttpRequestException)
        {
            ShowError("Unable to connect to server. Please check your internet connection.");
        }
        catch (TaskCanceledException)
        {
            ShowError("Request timed out. Please try again.");
        }
        catch (Exception ex)
        {
            ShowError($"An unexpected error occurred: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task VerifyEmailAsync()
    {
        if (IsVerificationLoading) return;

        // Clear previous errors
        HasVerificationError = false;
        VerificationMessage = string.Empty;

        // Validate verification code
        if (string.IsNullOrWhiteSpace(VerificationCode))
        {
            ShowVerificationError("Please enter the verification code.");
            return;
        }

        if (VerificationCode.Length != 5)
        {
            ShowVerificationError("Verification code must be 5 digits.");
            return;
        }

        try
        {
            IsVerificationLoading = true;

            var verifyDto = new VerifyEmailDto
            {
                Email = VerificationEmail,
                VerificationCode = VerificationCode
            };

            var response = await _apiService.PostAsync<ApiResponse>(
                "api/UserRegistration/verify-email", verifyDto);

            if (response?.Success == true)
            {
                // Update stored user verification status
                await SecureStorage.SetAsync("user_email_verified", "true");

                // Hide verification dialog
                IsVerificationDialogVisible = false;

                // Show success message
                await Application.Current.MainPage.DisplayAlert(
                    "Success",
                    "Email verified successfully! Welcome to Marriage Calculator.",
                    "OK");

                // Store remember me preference if needed
                if (RememberMe)
                {
                    await SecureStorage.SetAsync("remember_me", "true");
                    await SecureStorage.SetAsync("saved_username", Username.Trim());
                }
                else
                {
                    SecureStorage.Remove("remember_me");
                    SecureStorage.Remove("saved_username");
                }

                // ? START AUTHENTICATION MANAGER AFTER EMAIL VERIFICATION
                System.Diagnostics.Debug.WriteLine("LoginViewModel: Starting AuthenticationManager after email verification");
                await _authenticationManager.StartAsync();

                // Navigate to main page
                await Shell.Current.GoToAsync("//MainPage");
            }
            else
            {
                ShowVerificationError(response?.Message ?? "Verification failed. Please try again.");
            }
        }
        catch (HttpRequestException)
        {
            ShowVerificationError("Unable to connect to server. Please check your internet connection.");
        }
        catch (TaskCanceledException)
        {
            ShowVerificationError("Request timed out. Please try again.");
        }
        catch (Exception ex)
        {
            ShowVerificationError($"An unexpected error occurred: {ex.Message}");
        }
        finally
        {
            IsVerificationLoading = false;
        }
    }

    [RelayCommand]
    private async Task ResendVerificationCodeAsync()
    {
        if (IsVerificationLoading) return;

        try
        {
            IsVerificationLoading = true;

            var resendDto = new ResendVerificationDto
            {
                Email = VerificationEmail
            };

            var response = await _apiService.PostAsync<ApiResponse>(
                "api/UserRegistration/resend-verification", resendDto);

            if (response?.Success == true)
            {
                ShowVerificationMessage("A new verification code has been sent to your email.");
                VerificationCode = string.Empty; // Clear current code
            }
            else
            {
                ShowVerificationError(response?.Message ?? "Failed to resend verification code. Please try again.");
            }
        }
        catch (HttpRequestException)
        {
            ShowVerificationError("Unable to connect to server. Please check your internet connection.");
        }
        catch (TaskCanceledException)
        {
            ShowVerificationError("Request timed out. Please try again.");
        }
        catch (Exception ex)
        {
            ShowVerificationError($"An unexpected error occurred: {ex.Message}");
        }
        finally
        {
            IsVerificationLoading = false;
        }
    }

    [RelayCommand]
    private void CloseVerificationDialog()
    {
        IsVerificationDialogVisible = false;
        VerificationCode = string.Empty;
        VerificationEmail = string.Empty;
        HasVerificationError = false;
        VerificationMessage = string.Empty;
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (IsLoading) return;

        try
        {
            // Navigate to registration page
            await Shell.Current.GoToAsync("//RegisterPage");
        }
        catch (Exception ex)
        {
            ShowError($"Navigation error: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ForgotPasswordAsync()
    {
        if (IsLoading) return;

        try
        {
            // For now, show an alert. Later we can implement a forgot password page
            await Application.Current.MainPage.DisplayAlert(
                "Reset Password",
                "Password reset functionality will be available soon. Please contact support if you need assistance.",
                "OK");
        }
        catch (Exception ex)
        {
            ShowError($"An error occurred: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task NavigateToVerificationAsync()
    {
        if (IsLoading) return;

        try
        {
            await Shell.Current.GoToAsync("//EmailVerificationPage");
        }
        catch (Exception ex)
        {
            ShowError($"Navigation error: {ex.Message}");
        }
    }

    public async Task LoadSavedCredentialsAsync()
    {
        try
        {
            var rememberMe = await SecureStorage.GetAsync("remember_me");
            if (rememberMe == "true")
            {
                RememberMe = true;
                Username = await SecureStorage.GetAsync("saved_username") ?? "";
            }
        }
        catch (Exception)
        {
            // Ignore errors when loading saved credentials
        }
    }

    public async Task<bool> IsUserLoggedInAsync()
    {
        try
        {
            var token = await SecureStorage.GetAsync("access_token");
            var expiresString = await SecureStorage.GetAsync("token_expires");
            var emailVerified = await SecureStorage.GetAsync("user_email_verified");
            
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(expiresString))
                return false;

            // User must be email verified to be considered logged in
            if (emailVerified != "true")
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

    private void ShowVerificationDialog(string email)
    {
        VerificationEmail = email;
        VerificationCode = string.Empty;
        HasVerificationError = false;
        VerificationMessage = string.Empty;
        IsVerificationDialogVisible = true;
    }

    private void ShowError(string message)
    {
        ErrorMessage = message;
        HasError = true;
    }

    private void ShowVerificationError(string message)
    {
        VerificationMessage = message;
        HasVerificationError = true;
    }

    private void ShowVerificationMessage(string message)
    {
        VerificationMessage = message;
        HasVerificationError = false;
    }

    partial void OnIsLoadingChanged(bool value)
    {
        OnPropertyChanged(nameof(IsNotLoading));
    }

    partial void OnIsVerificationLoadingChanged(bool value)
    {
        OnPropertyChanged(nameof(IsNotVerificationLoading));
    }
}