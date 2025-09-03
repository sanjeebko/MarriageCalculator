using CommunityToolkit.Mvvm.Messaging;
using MarriageCalculator.DataServices;
using MarriageCalculator.ViewModels;

namespace MarriageCalculator.Pages;

public partial class GameSetupPage : ContentPage
{
    private readonly GameSetupViewModel _viewModel;

    public GameSetupPage(GameSetupViewModel viewModel)
    {
        System.Diagnostics.Debug.WriteLine("GameSetupPage constructor starting");
        
        try
        {
            InitializeComponent();
            System.Diagnostics.Debug.WriteLine("GameSetupPage InitializeComponent completed");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GameSetupPage InitializeComponent FAILED: {ex}");
            throw; // Re-throw to see the error
        }

        _viewModel = viewModel;
        BindingContext = _viewModel;
        System.Diagnostics.Debug.WriteLine("GameSetupPage BindingContext set");

        // Register for navigation return messages to refresh the page
        WeakReferenceMessenger.Default.Register<NavigationReturnMessage>(this, async (sender, message) =>
        {
            await _viewModel.RefreshAsync();
        });
        
        System.Diagnostics.Debug.WriteLine("GameSetupPage constructor completed");
    }

    protected override async void OnAppearing()
    {
        System.Diagnostics.Debug.WriteLine("GameSetupPage OnAppearing started");
        base.OnAppearing();
        
        try
        {
            await _viewModel.InitializeAsync();
            System.Diagnostics.Debug.WriteLine("GameSetupPage ViewModel initialized successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GameSetupPage OnAppearing error: {ex}");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Unregister to prevent memory leaks
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }
}