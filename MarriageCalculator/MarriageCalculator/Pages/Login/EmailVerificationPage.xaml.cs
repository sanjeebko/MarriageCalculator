using MarriageCalculator.ViewModels;

namespace MarriageCalculator.Pages.Login;

public partial class EmailVerificationPage : ContentPage
{
    private EmailVerificationViewModel? _viewModel;

    public EmailVerificationPage(EmailVerificationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Load user email if available
        if (_viewModel != null)
        {
            await _viewModel.LoadUserEmailAsync();
            
            // Focus on appropriate field
            if (!_viewModel.IsLoading)
            {
                if (string.IsNullOrEmpty(_viewModel.Email))
                {
                    EmailEntry.Focus();
                }
                else
                {
                    VerificationCodeEntry.Focus();
                }
            }
        }
    }
}