namespace MarriageCalculator.Pages;

public partial class PlayGame : ContentPage
{
    public IMarriageGameEngine MarriageGameEngine { get; }
    public MarriageGameViewModel ViewModel { get; }

    public PlayGame(IMarriageGameEngine marriageGameEngine, MarriageGameViewModel viewModel)
    {
        InitializeComponent();        
        MarriageGameEngine = marriageGameEngine;
        ViewModel = viewModel;
        BindingContext = ViewModel;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        ViewModel.LoadPlayerScores();
    }
    private void Rotate_Clicked(object sender, EventArgs e)
    {

    }
}