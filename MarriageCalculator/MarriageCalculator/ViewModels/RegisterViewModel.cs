using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarriageCalculator.Services.Interfaces;
using MarriageCalculator.Core.DTOs;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace MarriageCalculator.ViewModels;

public partial class RegisterViewModel : ObservableObject
{
    private readonly IApiService _apiService;

    public RegisterViewModel(IApiService apiService)
    {
        _apiService = apiService;
        IsPasswordHidden = true;
        PasswordToggleIcon = "???";
        AgreeToTerms = false;
    }

    [ObservableProperty]
    private string displayName = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private bool agreeToTerms;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool hasError;

    [ObservableProperty]
    private bool hasSuccess;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private string successMessage = string.Empty;

    [ObservableProperty]
    private bool isPasswordHidden;

    [ObservableProperty]
    private string passwordToggleIcon = string.Empty;

    public bool IsNotLoading => !IsLoading;
    public bool CanRegister => !IsLoading && AgreeToTerms && IsFormValid();

    [RelayCommand]
    private void TogglePasswordVisibility()
    {
        IsPasswordHidden = !IsPasswordHidden;
        PasswordToggleIcon = IsPasswordHidden ? "???" : "??";
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (IsLoading) return;

        // Clear previous messages
        ClearMessages();

        // Validate input
        if (!ValidateInput())
            return;

        try
        {
            IsLoading = true;

            var registerDto = new RegisterUserDto
            {
                DisplayName = DisplayName.Trim(),
                Email = Email.Trim(),
                Password = Password
            };

            // Call the API registration endpoint
            var response = await _apiService.PostAsync<ApiResponse<UserDto>>(
                "api/UserRegistration/register", registerDto);

            if (response?.Success == true && response.Data != null)
            {
                // Show success message
                ShowSuccess("Registration successful! Please check your email for a verification code.");

                // Wait a moment to show the success message
                await Task.Delay(2000);

                // Navigate back to login page
                await Shell.Current.GoToAsync("//LoginPage");

                // Show additional message on login page
                await Application.Current.MainPage.DisplayAlert(
                    "Email Verification Required",
                    "A verification code has been sent to your email address. Please check your email and use the verification code to complete your registration.",
                    "OK");
            }
            else
            {
                ShowError(response?.Message ?? "Registration failed. Please try again.");
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
    private async Task BackToLoginAsync()
    {
        if (IsLoading) return;

        try
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }
        catch (Exception ex)
        {
            ShowError($"Navigation error: {ex.Message}");
        }
    }

    private bool ValidateInput()
    {
        // Validate display name
        if (string.IsNullOrWhiteSpace(DisplayName))
        {
            ShowError("Please enter a display name.");
            return false;
        }

        if (DisplayName.Trim().Length < 2)
        {
            ShowError("Display name must be at least 2 characters long.");
            return false;
        }

        if (DisplayName.Trim().Length > 100)
        {
            ShowError("Display name must be less than 100 characters.");
            return false;
        }

        // Validate email
        if (string.IsNullOrWhiteSpace(Email))
        {
            ShowError("Please enter an email address.");
            return false;
        }

        if (!IsValidEmail(Email))
        {
            ShowError("Please enter a valid email address.");
            return false;
        }

        // Validate password
        if (string.IsNullOrWhiteSpace(Password))
        {
            ShowError("Please enter a password.");
            return false;
        }

        if (!IsValidPassword(Password))
        {
            ShowError("Password must be at least 8 characters with 1 capital letter and 1 number or symbol.");
            return false;
        }

        // Validate terms agreement
        if (!AgreeToTerms)
        {
            ShowError("Please agree to the Terms and Conditions and Privacy Policy.");
            return false;
        }

        return true;
    }

    private bool IsFormValid()
    {
        return !string.IsNullOrWhiteSpace(DisplayName) &&
               !string.IsNullOrWhiteSpace(Email) &&
               !string.IsNullOrWhiteSpace(Password) &&
               IsValidEmail(Email) &&
               IsValidPassword(Password) &&
               AgreeToTerms;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var emailAttribute = new EmailAddressAttribute();
            return emailAttribute.IsValid(email);
        }
        catch
        {
            return false;
        }
    }

    private static bool IsValidPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            return false;

        // Must have at least one capital letter
        if (!Regex.IsMatch(password, @"[A-Z]"))
            return false;

        // Must have at least one number OR one symbol
        bool hasNumber = Regex.IsMatch(password, @"[0-9]");
        bool hasSymbol = Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{}|;':"",./<>?]");

        return hasNumber || hasSymbol;
    }

    private void ShowError(string message)
    {
        ErrorMessage = message;
        HasError = true;
        HasSuccess = false;
    }

    private void ShowSuccess(string message)
    {
        SuccessMessage = message;
        HasSuccess = true;
        HasError = false;
    }

    private void ClearMessages()
    {
        HasError = false;
        HasSuccess = false;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
    }

    partial void OnIsLoadingChanged(bool value)
    {
        OnPropertyChanged(nameof(IsNotLoading));
        OnPropertyChanged(nameof(CanRegister));
    }

    partial void OnAgreeToTermsChanged(bool value)
    {
        OnPropertyChanged(nameof(CanRegister));
    }

    partial void OnDisplayNameChanged(string value)
    {
        OnPropertyChanged(nameof(CanRegister));
    }

    partial void OnEmailChanged(string value)
    {
        OnPropertyChanged(nameof(CanRegister));
    }

    partial void OnPasswordChanged(string value)
    {
        OnPropertyChanged(nameof(CanRegister));
    }
}