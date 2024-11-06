using Toast = CommunityToolkit.Maui.Alerts.Toast;
namespace MarriageCalculator.Pages;

public partial class Settings : ContentPage
{
    private readonly SettingsViewModel _viewModel;
    public Settings(SettingsViewModel viewModel )
    {
        InitializeComponent();
        _viewModel = viewModel;
          
        BindingContext = _viewModel;
    }

    

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
    }
     

    private async void NextButton_Clicked(object sender, EventArgs e)
    {
         await Shell.Current.GoToAsync("..");
    }
 
}