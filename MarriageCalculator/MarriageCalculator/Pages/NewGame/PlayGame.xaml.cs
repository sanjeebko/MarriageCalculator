using MarriageCalculator.Services.Interfaces;
using MarriageCalculator.ViewModels;

namespace MarriageCalculator.Pages;

public partial class PlayGame : ContentPage
{
    public IMarriageGameEngine MarriageGameEngine { get; }
    public MarriageGameViewModel ViewModel { get; }
    private Grid? _expandedDetailsGrid;
    private Button? _toggleButton;

    public PlayGame(IMarriageGameEngine marriageGameEngine, MarriageGameViewModel viewModel)
    {
        InitializeComponent();        
        MarriageGameEngine = marriageGameEngine;
        ViewModel = viewModel;
        BindingContext = ViewModel;
        
        // Find the expanded details grid and toggle button for animation
        _expandedDetailsGrid = this.FindByName<Grid>("ExpandedDetailsGrid");
        
        // Subscribe to the IsExpanded property changes for animation
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        
        // Set initial state
        if (_expandedDetailsGrid != null)
        {
            _expandedDetailsGrid.IsVisible = ViewModel.IsExpanded;
            _expandedDetailsGrid.Opacity = ViewModel.IsExpanded ? 1.0 : 0.0;
            _expandedDetailsGrid.Scale = ViewModel.IsExpanded ? 1.0 : 0.95;
            _expandedDetailsGrid.TranslationY = ViewModel.IsExpanded ? 0 : -10;
        }
    }

    private async void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MarriageGameViewModel.IsExpanded))
        {
            // Animate the expanded details
            if (_expandedDetailsGrid != null)
            {
                if (ViewModel.IsExpanded)
                {
                    await AnimateExpand();
                }
                else
                {
                    await AnimateCollapse();
                }
            }
            
            // Animate button rotation (find the toggle button)
            if (_toggleButton == null)
            {
                _toggleButton = FindToggleButton(this);
            }
            
            if (_toggleButton != null)
            {
                var targetRotation = ViewModel.IsExpanded ? 180.0 : 0.0;
                await _toggleButton.RotateTo(targetRotation, 300, Easing.CubicInOut);
            }
        }
    }

    private Button? FindToggleButton(Element parent)
    {
        if (parent is Button button && button.Command == ViewModel.ToggleExpandCommand)
        {
            return button;
        }

        if (parent is IElementController elementController)
        {
            foreach (var child in elementController.LogicalChildren)
            {
                var result = FindToggleButton(child);
                if (result != null)
                    return result;
            }
        }

        return null;
    }

    private async Task AnimateExpand()
    {
        if (_expandedDetailsGrid == null) return;

        // Make visible first
        _expandedDetailsGrid.IsVisible = true;
        
        // Set initial collapsed state
        _expandedDetailsGrid.Opacity = 0.0;
        _expandedDetailsGrid.Scale = 0.95;
        _expandedDetailsGrid.TranslationY = -10;

        // Animate to expanded state
        var fadeTask = _expandedDetailsGrid.FadeTo(1.0, 300, Easing.CubicOut);
        var scaleTask = _expandedDetailsGrid.ScaleTo(1.0, 300, Easing.CubicOut);
        var slideTask = _expandedDetailsGrid.TranslateTo(0, 0, 300, Easing.CubicOut);

        await Task.WhenAll(fadeTask, scaleTask, slideTask);
    }

    private async Task AnimateCollapse()
    {
        if (_expandedDetailsGrid == null) return;

        // Animate to collapsed state
        var fadeTask = _expandedDetailsGrid.FadeTo(0.0, 200, Easing.CubicIn);
        var scaleTask = _expandedDetailsGrid.ScaleTo(0.95, 200, Easing.CubicIn);
        var slideTask = _expandedDetailsGrid.TranslateTo(0, -10, 200, Easing.CubicIn);

        await Task.WhenAll(fadeTask, scaleTask, slideTask);
        
        // Hide after animation
        _expandedDetailsGrid.IsVisible = false;
    }
    
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Add debug logging to diagnose the issue
        System.Diagnostics.Debug.WriteLine($"PlayGame OnAppearing - CurrentMarriageGame: {MarriageGameEngine.CurrentMarriageGame?.Id}");
        System.Diagnostics.Debug.WriteLine($"PlayGame OnAppearing - MarriageGameScores count: {MarriageGameEngine.CurrentMarriageGame?.MarriageGameScores?.Count ?? 0}");
        System.Diagnostics.Debug.WriteLine($"PlayGame OnAppearing - GameSetPlayers count: {MarriageGameEngine.MarriageGameSet?.GameSetPlayers?.Count ?? 0}");

        // Ensure we have a current marriage game before loading scores
        if (MarriageGameEngine.CurrentMarriageGame == null)
        {
            System.Diagnostics.Debug.WriteLine("PlayGame: No current marriage game found, trying to create one...");

            // If there's no current game, try to create one
            if (MarriageGameEngine.CurrentMarriageGameRound != null)
            {
                await MarriageGameEngine.CreateNewMarriageGame();
            }
            else if (MarriageGameEngine.MarriageGameSet != null)
            {
                // Create a new round and game
                await MarriageGameEngine.CreateNewGameRoundForGivenGameSet(MarriageGameEngine.MarriageGameSet.Id);
            }
        }
        
        ViewModel.LoadPlayerScores();
    }

    protected override void OnDisappearing()
    {
        // Unsubscribe to prevent memory leaks
        if (ViewModel != null)
        {
            ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }
        base.OnDisappearing();
    }
    
    private void Rotate_Clicked(object sender, EventArgs e)
    {

    }
}