using MarriageCalculator.ViewModels;

namespace MarriageCalculator.Pages.Login;

public partial class LoginPage : ContentPage
{
    private LoginViewModel? _viewModel;

    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
        
        // Subscribe to property changes
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Load saved credentials if available
        if (BindingContext is LoginViewModel viewModel)
        {
            await viewModel.LoadSavedCredentialsAsync();
            
            // Focus on appropriate field
            if (!viewModel.IsLoading)
            {
                if (string.IsNullOrEmpty(viewModel.Username))
                {
                    UsernameEntry.Focus();
                }
                else
                {
                    PasswordEntry.Focus();
                }
            }
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        // Unsubscribe from property changes
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LoginViewModel.IsVerificationDialogVisible))
        {
            // Focus on verification code entry when dialog becomes visible
            if (_viewModel?.IsVerificationDialogVisible == true)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    // Small delay to ensure the UI is rendered
                    await Task.Delay(100);
                    VerificationCodeEntry.Focus();
                });
            }
        }
    }
}