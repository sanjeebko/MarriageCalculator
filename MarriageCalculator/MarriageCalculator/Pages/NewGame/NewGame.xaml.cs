using MarriageCalculator.ViewModels;

namespace MarriageCalculator.Pages.NewGame;

public partial class NewGame : ContentPage
{
    public NewGame(NewGameViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }

    private async void BackButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private void ScrollView_Scrolled(object sender, ScrolledEventArgs e)
    {
    }
}