using Microsoft.Maui.Devices;
using System.Diagnostics;

namespace MarriageCalculator.Pages;

public partial class PlayGame : ContentPage
{
    public PlayGame(NewGameViewModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }

    private void Rotate_Clicked(object sender, EventArgs e)
    {
    }
}