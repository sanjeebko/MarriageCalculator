using MarriageCalculator.ViewModels;

namespace MarriageCalculator.Pages;

public partial class NewGame : ContentPage
{
    public readonly MarriageGameViewModel _viewModel;

    public NewGame(MarriageGameViewModel viewModel)
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

        
       
    }
}