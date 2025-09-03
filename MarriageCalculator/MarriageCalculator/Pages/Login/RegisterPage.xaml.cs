using MarriageCalculator.ViewModels;

namespace MarriageCalculator.Pages.Login;

public partial class RegisterPage : ContentPage
{
    public RegisterPage(RegisterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // Focus on display name field when page appears
        if (BindingContext is RegisterViewModel viewModel && !viewModel.IsLoading)
        {
            DisplayNameEntry.Focus();
        }
    }
}