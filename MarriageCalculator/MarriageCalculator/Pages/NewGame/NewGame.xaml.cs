using MarriageCalculator.ViewModels;

namespace MarriageCalculator.Pages;

public partial class NewGame : ContentPage
{
    public readonly NewGameViewModel _viewModel;

    public NewGame(NewGameViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    private void ScrollView_Scrolled(object sender, ScrolledEventArgs e)
    {
    }

    private async void NextButton_Clicked(object sender, EventArgs e)
    {

        
        await Shell.Current.GoToAsync(nameof(Settings), true, new Dictionary<string, object>()
        {
            [nameof(_viewModel.MarriageGameModel)] = _viewModel.MarriageGameModel,
            [nameof(_viewModel.GameSettingsModel)] = _viewModel.GameSettingsModel
        });
    }
}