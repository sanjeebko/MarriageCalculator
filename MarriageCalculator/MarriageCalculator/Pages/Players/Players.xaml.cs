namespace MarriageCalculator.Pages;

public partial class Players : ContentPage
{
    public Players(PlayerSettingsViewModel viewModel)
    {
        InitializeComponent();
        PlayerSettingsViewModel = viewModel;
        BindingContext = PlayerSettingsViewModel; 
        PlayerSettingsViewModel.OnComplete += PlayerSettingsViewModel_OnComplete;
    }

    private async void PlayerSettingsViewModel_OnComplete(object? sender, EventArgs e)
    {
        await  Shell.Current.GoToAsync("..");
    }

    public PlayerSettingsViewModel PlayerSettingsViewModel { get; }

 
     

    private void Ok_Clicked(object sender, EventArgs e)
    {

    }
     
}

public class Kheladi
{
    public string Name { get; set; }
    public int Maal { get; set; }
    public bool Seen { get; set; }
    public int Score { get; set; }
     
}