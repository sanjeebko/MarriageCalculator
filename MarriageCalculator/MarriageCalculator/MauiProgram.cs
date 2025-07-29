using CommunityToolkit.Maui;
using Syncfusion.Maui.Core.Hosting;
using System.Reflection;
using MarriageCalculator.Mapping;

namespace MarriageCalculator;

//Repository pattern implementation for connecting to MarriageCalculator.API
//No more SQLite offline database - all data comes from API
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureSyncfusionCore()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("Poppins-Regular.ttf", "PoppinsRegular");
                fonts.AddFont("Poppins-Semibold.ttf", "PoppinsSemibold");
                fonts.AddFont("fontello.ttf", "Fontello");
            });

        // Configuration from embedded resources
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("MarriageCalculator.appsettings.json");
        
        var configurationBuilder = new ConfigurationBuilder();
        if (stream != null)
        {
            configurationBuilder.AddJsonStream(stream);
        }
        
#if DEBUG
        using var devStream = assembly.GetManifestResourceStream("MarriageCalculator.appsettings.Development.json");
        if (devStream != null)
        {
            configurationBuilder.AddJsonStream(devStream);
        }
#endif
        
        var configuration = configurationBuilder.Build();
        builder.Services.AddSingleton<IConfiguration>(configuration);

        // AutoMapper configuration - use explicit assembly method to avoid ambiguity
        builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // HTTP Client for API communication
        builder.Services.AddHttpClient<IApiService, ApiService>(client =>
        {
            var baseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "https://localhost:7294";
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(configuration.GetValue<int>("ApiSettings:Timeout", 30));
        });

        // Register Repository Services (replaces SQLite)
        builder.Services.AddTransient<IApiService, ApiService>();
        builder.Services.AddTransient<IPlayerRepository, PlayerRepository>();
        builder.Services.AddTransient<IGameSettingsRepository, GameSettingsRepository>();
        builder.Services.AddTransient<IMarriageGameSetRepository, MarriageGameSetRepository>();
        builder.Services.AddTransient<IMarriageGameRepository, MarriageGameRepository>();
        builder.Services.AddTransient<IMarriageGameRoundRepository, MarriageGameRoundRepository>();
        builder.Services.AddTransient<IMarriageGameScoreRepository, MarriageGameScoreRepository>();
        builder.Services.AddTransient<IMarriageGameSetPlayerRepository, MarriageGameSetPlayerRepository>();
        builder.Services.AddTransient<IDatabaseRepository, DatabaseRepository>();

        // Register Database Service (API-based instead of SQLite)
        builder.Services.AddSingleton<IDbService, ApiDbService>();

        // Connection service for API monitoring
        builder.Services.AddSingleton<IConnectionService, ConnectionService>();

        // Other services
        builder.Services.AddSingleton<ISettingsService, SettingsService>();
        builder.Services.AddSingleton<ITextToSpeechService, TextToSpeechService>();
        builder.Services.AddSingleton<IPlayerService, PlayerService>();
        builder.Services.AddSingleton<IMarriageGameEngine, MarriageGameEngine>();

        // Views registration
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddTransient<NewGame>();
        builder.Services.AddTransient<PlayGame>();
        builder.Services.AddScoped<SettingsPage>();
        builder.Services.AddScoped<PlayersPage>();

        // View models
        builder.Services.AddTransient<MainPageViewModel>();
        builder.Services.AddScoped<SettingsViewModel>();
        builder.Services.AddScoped<MarriageGameViewModel>();
        builder.Services.AddScoped<PlayerSettingsViewModel>();

        return builder.Build();
    }
}