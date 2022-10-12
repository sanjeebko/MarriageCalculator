namespace MarriageCalculator.Pages.NewGame;

public partial class NewGame : ContentPage
{
    public NewGame()
    {
        InitializeComponent();
    }

    private async void BackButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}