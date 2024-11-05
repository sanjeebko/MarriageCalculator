 
namespace MarriageCalculator.Pages;

public partial class Players : ContentPage
{
    public Players(PlayerSettingsViewModel viewModel)
    {
        InitializeComponent();
        PlayerSettingsViewModel = viewModel;
        BindingContext = PlayerSettingsViewModel; 
        PlayerSettingsViewModel.OnComplete += PlayerSettingsViewModel_OnComplete;
        PlayerSettingsViewModel.OnError += PlayerSettingsViewModel_OnError;
       
    } 

    private void PlayerSettingsViewModel_OnError(object? sender, EventArgs e)
    {
        
    }

    private async void PlayerSettingsViewModel_OnComplete(object? sender, EventArgs e)
    {
        await  Shell.Current.GoToAsync("..");
    }

    public PlayerSettingsViewModel PlayerSettingsViewModel { get; }

     

    protected override void OnAppearing()
    {
        PlayerSettingsViewModel?.RefreshAllPlayers();
    }
}
 