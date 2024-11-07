using CommunityToolkit.Maui;
using MarriageCalculator.Core.Models;
using MarriageCalculator.Pages;
using MarriageCalculator.Services;
using MarriageCalculator.ViewModels;

namespace MarriageCalculator;

//simple sqlite tutorial: https://www.youtube.com/watch?v=JRNwjsywrWM&t=100s

//Tutorials used for api part
//https://www.youtube.com/watch?v=LrZwd-f0M4I
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        //services
        builder.Services.AddSingleton<IDbService, SqLiteDbService>();
        builder.Services.AddSingleton<ISettingsService,SettingsService>();
        builder.Services.AddSingleton<ITextToSpeechService, TextToSpeechService>();
        builder.Services.AddSingleton<IPlayerService, PlayerService>();
        builder.Services.AddSingleton<IMarriageGameEngine, MarriageGameEngine>();
        //views registration
        
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddTransient<NewGame>();
        builder.Services.AddTransient<PlayGame>();     
        builder.Services.AddScoped<SettingsPage>();
        builder.Services.AddScoped<PlayersPage>();
         

        //view models
        builder.Services.AddScoped<SettingsViewModel>();
        builder.Services.AddScoped<NewGameViewModel>(); 
        builder.Services.AddScoped<PlayerSettingsViewModel>();
          

        return builder.Build();
    }
}