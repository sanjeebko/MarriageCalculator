using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarriageCalculator.Services.Interfaces;
using MarriageCalculator.Core.DTOs;

namespace MarriageCalculator.ViewModels;

public partial class EmailVerificationViewModel : ObservableObject
{
    private readonly IApiService _apiService;

    public EmailVerificationViewModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string verificationCode = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isResendLoading;

    [ObservableProperty]
    private bool hasError;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool hasSuccess;

    [ObservableProperty]
    private string successMessage = string.Empty;

    public bool IsNotLoading => !IsLoading;
    public bool IsNotResendLoading => !IsResendLoading;

    [RelayCommand]
    private async Task VerifyEmailAsync()
    {
        if (IsLoading) return;

        // Clear previous messages
        ClearMessages();

        // Validate input
        if (string.IsNullOrWhiteSpace(Email))
        {
            ShowError("Please enter your email address.");
            return;
        }

        if (string.IsNullOrWhiteSpace(VerificationCode))
        {
            ShowError("Please enter the verification code.");
            return;
        }

        if (VerificationCode.Length != 5)
        {
            ShowError("Verification code must be 5 digits.");
            return;
        }

        try
        {
            IsLoading = true;

            var verifyDto = new VerifyEmailDto
            {
                Email = Email.Trim(),
                VerificationCode = VerificationCode.Trim()
            };

            var response = await _apiService.PostAsync<ApiResponse>(
                "api/UserRegistration/verify-email", verifyDto);

            if (response?.Success == true)
            {
                ShowSuccess("Email verified successfully! You can now sign in.");
                
                // Clear form
                VerificationCode = string.Empty;
                
                // Navigate back to login after a short delay
                await Task.Delay(2000);
                await Shell.Current.GoToAsync("//LoginPage");
            }
            else
            {
                ShowError(response?.Message ?? "Verification failed. Please try again.");
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
    private async Task ResendVerificationCodeAsync()
    {
        if (IsResendLoading) return;

        // Clear previous messages
        ClearMessages();

        // Validate email
        if (string.IsNullOrWhiteSpace(Email))
        {
            ShowError("Please enter your email address.");
            return;
        }

        try
        {
            IsResendLoading = true;

            var resendDto = new ResendVerificationDto
            {
                Email = Email.Trim()
            };

            var response = await _apiService.PostAsync<ApiResponse>(
                "api/UserRegistration/resend-verification", resendDto);

            if (response?.Success == true)
            {
                ShowSuccess("A new verification code has been sent to your email.");
                VerificationCode = string.Empty; // Clear current code
            }
            else
            {
                ShowError(response?.Message ?? "Failed to resend verification code. Please try again.");
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
            IsResendLoading = false;
        }
    }

    [RelayCommand]
    private async Task BackToLoginAsync()
    {
        if (IsLoading || IsResendLoading) return;

        try
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }
        catch (Exception ex)
        {
            ShowError($"Navigation error: {ex.Message}");
        }
    }

    public async Task LoadUserEmailAsync()
    {
        try
        {
            // Try to get email from stored user data
            var userEmail = await SecureStorage.GetAsync("user_email");
            if (!string.IsNullOrWhiteSpace(userEmail))
            {
                Email = userEmail;
            }
        }
        catch (Exception)
        {
            // Ignore errors when loading email
        }
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
    }

    partial void OnIsResendLoadingChanged(bool value)
    {
        OnPropertyChanged(nameof(IsNotResendLoading));
    }
}