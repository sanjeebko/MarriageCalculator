using MarriageCalculator.Models;

namespace MarriageCalculator.Pages.NewGame;

public partial class NewGamePlayers : ContentPage
{
    private readonly NewGameViewModel _viewModel;

    public NewGamePlayers(NewGameViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
    }

    private void ScrollView_Scrolled(object sender, ScrolledEventArgs e)
    {
    }

    private async void NextButton_Clicked(object sender, EventArgs e)
    {
        if (_viewModel.MarriageGameModel.Player1?.Length > 2 && _viewModel.MarriageGameModel.Player2?.Length > 2)
        {
            _viewModel.MarriageGameModel.IsActive = true;
            await Shell.Current.GoToAsync("..");
        }
    }

    private void SwipeGestureRecognizer_Swiped(object sender, SwipedEventArgs e)
    {
        _viewModel.marriageGameModel.Player6 += "-Disabled";
        Player6Border.BackgroundColor = Colors.Grey;
    }

    private void SwipeGestureRecognizer_Swiped_Right(object sender, SwipedEventArgs e)
    {
        _viewModel.marriageGameModel.Player6 = _viewModel.marriageGameModel.Player6?.Replace("-Disabled", "");
        Player6Border.BackgroundColor = Colors.Black;
    }
}