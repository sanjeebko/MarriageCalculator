using CommunityToolkit.Maui;
using MarriageCalculator.Mapping;
using MarriageCalculator.Pages.Game;
using MarriageCalculator.Pages.Login;
using MarriageCalculator.Repositories.Implementations;
using MarriageCalculator.Repositories.Interfaces;
using MarriageCalculator.Services.Implementations;
using MarriageCalculator.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using System.Reflection;

namespace MarriageCalculator;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        string syncFusionKey = "Ngo9BigBOggjHTQxAR8/V1JFaF5cXGRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXZccHVSR2JfWUVyW0JWYEg=";
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(syncFusionKey);

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("Segoe-Ui-Bold.ttf", "SegoeBold");
                fonts.AddFont("Segoe-Ui-Semibold.ttf", "SegoeSemibold");
                fonts.AddFont("fontello.ttf", "Fontello");
                
                // Platform-specific emoji font registration
#if ANDROID
                fonts.AddFont("NotoColorEmoji.ttf", "emoji");
#elif IOS
                fonts.AddFont("AppleColorEmoji.ttc", "emoji");
#else
                fonts.AddFont("OpenSans-Regular.ttf", "emoji"); // Fallback
#endif
            });
        builder.ConfigureSyncfusionCore();
#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Add configuration - Check multiple possible locations for appsettings.json
        var configLoaded = false;
        
        // Try different paths for appsettings.json
        var possiblePaths = new[]
        {
            "appsettings.json",
            Path.Combine(AppContext.BaseDirectory, "appsettings.json"),
            Path.Combine(FileSystem.AppDataDirectory, "appsettings.json"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "appsettings.json")
        };

        foreach (var path in possiblePaths)
        {
            try
            {
                if (File.Exists(path))
                {
                    builder.Configuration.AddJsonFile(path, optional: false, reloadOnChange: false);
                    configLoaded = true;
                    break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load config from {path}: {ex.Message}");
            }
        }

        // Fallback: Use in-memory configuration if file not found
        if (!configLoaded)
        {
#if DEBUG
            // For debugging - change this to switch between local and production API
            var useLocalApi = false; // Set to true to use local API for debugging
            var apiUrl = useLocalApi ? "https://localhost:7294/" : "https://mcapi.sanjeebojha.com.np/";
#else
            var apiUrl = "https://mcapi.sanjeebojha.com.np/";
#endif
            
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"ApiSettings:BaseUrl", apiUrl},
                {"ApiSettings:Timeout", "30"},
                {"ApiSettings:RetryCount", "3"},
                {"Logging:LogLevel:Default", "Information"},
                {"Logging:LogLevel:Microsoft", "Warning"},
                {"Logging:LogLevel:Microsoft.Hosting.Lifetime", "Information"}
            });
            
            System.Diagnostics.Debug.WriteLine($"Using API URL: {apiUrl}");
        }

        // AutoMapper configuration
        builder.Services.AddAutoMapper(typeof(MappingProfile));

        // HTTP Client and API Service
        builder.Services.AddHttpClient<IApiService, ApiService>((serviceProvider, client) =>
        {
            var cfg = serviceProvider.GetRequiredService<IConfiguration>();
            var baseUrl = cfg.GetValue<string>("ApiSettings:BaseUrl") ?? "https://localhost:7294";
            var timeout = cfg.GetValue<int>("ApiSettings:Timeout", 30);
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(timeout);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        // Alternative registration for scenarios where you need to create HttpClient manually
        builder.Services.AddSingleton<IApiService>(serviceProvider =>
        {
            var cfg = serviceProvider.GetRequiredService<IConfiguration>();
            var baseUrl = cfg.GetValue<string>("ApiSettings:BaseUrl") ?? "https://localhost:7294";
            var timeout = cfg.GetValue<int>("ApiSettings:Timeout", 30);
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromSeconds(timeout)
            };
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            return new ApiService(httpClient, cfg);
        });

        // Register Repository Services (replaces SQLite)
        builder.Services.AddTransient<IPlayerRepository, PlayerRepository>();
        builder.Services.AddTransient<IGameSettingsRepository, GameSettingsRepository>();
        builder.Services.AddTransient<IMarriageGameSetRepository, MarriageGameSetRepository>();
        builder.Services.AddTransient<IMarriageGameRepository, MarriageGameRepository>();
        builder.Services.AddTransient<IMarriageGameRoundRepository, MarriageGameRoundRepository>();
        builder.Services.AddTransient<IMarriageGameScoreRepository, MarriageGameScoreRepository>();
        builder.Services.AddTransient<IMarriageGameSetPlayerRepository, MarriageGameSetPlayerRepository>();
        builder.Services.AddTransient<IDatabaseRepository, DatabaseRepository>();


        // Connection service for API monitoring
        builder.Services.AddSingleton<IConnectionService, ConnectionService>();

        // Authentication service
        builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
        
        // Background authentication manager
        builder.Services.AddSingleton<IAuthenticationManager, AuthenticationManager>();

        // Other services
        builder.Services.AddSingleton<ISettingsService, SettingsService>();
        builder.Services.AddSingleton<ITextToSpeechService, TextToSpeechService>();
        builder.Services.AddSingleton<IPlayerService, PlayerService>();

        builder.Services.AddSingleton<IMarriageGameEngine>(serviceProvider =>
        {
            var settingsService = serviceProvider.GetRequiredService<ISettingsService>();
            var playerService = serviceProvider.GetRequiredService<IPlayerService>();
            var marriageGameSetRepository = serviceProvider.GetRequiredService<IMarriageGameSetRepository>();
            var marriageGameRoundRepository = serviceProvider.GetRequiredService<IMarriageGameRoundRepository>();
            var marriageGameRepository = serviceProvider.GetRequiredService<IMarriageGameRepository>();
            var marriageGameScoreRepository = serviceProvider.GetRequiredService<IMarriageGameScoreRepository>();
            var marriageGameSetPlayerRepository = serviceProvider.GetRequiredService<IMarriageGameSetPlayerRepository>();
            var textToSpeechService = serviceProvider.GetRequiredService<ITextToSpeechService>();
            
            return new MarriageGameEngine(
                settingsService,
                playerService,
                marriageGameSetRepository,
                marriageGameRoundRepository,
                marriageGameRepository,
                marriageGameScoreRepository,
                marriageGameSetPlayerRepository,
                textToSpeechService);
        });

        // Views registration
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddTransient<NewGame>();
        builder.Services.AddTransient<PlayGame>();
        builder.Services.AddScoped<SettingsPage>();
        builder.Services.AddScoped<PlayersPage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<EmailVerificationPage>();
        builder.Services.AddTransient<GameSetupPage>();
        builder.Services.AddSingleton<ScoreBoardPage>();

        // View models
        builder.Services.AddTransient<MainPageViewModel>();

        builder.Services.AddTransient<MarriageGameViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<PlayerSettingsViewModel>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<EmailVerificationViewModel>();
        builder.Services.AddTransient<GameSetupViewModel>();
        builder.Services.AddTransient<ScoreBoardViewModel>();

        return builder.Build();
    }
}