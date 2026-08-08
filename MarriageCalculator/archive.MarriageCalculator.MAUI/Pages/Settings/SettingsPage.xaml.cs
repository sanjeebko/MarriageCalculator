using CommunityToolkit.Mvvm.Messaging;
using Toast = CommunityToolkit.Maui.Alerts.Toast;
namespace MarriageCalculator.Pages;

public partial class SettingsPage : ContentPage
{
    private readonly SettingsViewModel _viewModel;
    public SettingsPage(SettingsViewModel viewModel )
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