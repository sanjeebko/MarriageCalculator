﻿using CommunityToolkit.Maui;
using MarriageCalculator.Pages;
using MarriageCalculator.Services;

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
        builder.Services.AddSingleton<IMarriageGameServices, MarriageGameServices>();

        //views registration
        builder.Services.AddTransient<NewGame>();
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddTransient<Settings>();
        builder.Services.AddTransient<Players>();
        builder.Services.AddTransient<PlayGame>();
        
        builder.Services.AddScoped<PlayerSettingsViewModel>();
        //view models
        builder.Services.AddTransient<NewGameViewModel>();

        return builder.Build();
    }
}