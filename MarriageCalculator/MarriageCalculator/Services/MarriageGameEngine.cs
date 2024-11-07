namespace MarriageCalculator.Services;

public class MarriageGameEngine :IMarriageGameEngine
{  
    public CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();
    public IDbService DbServices { get; }
    public ISettingsService SettingsService { get; }
    public IPlayerService PlayerService { get; }
    public ITextToSpeechService TextToSpeechService { get; }
    public bool Initialized { get; private set; }
    public string LastPageName { get  ; set ; }

    public MarriageGameEngine(IDbService dbServices, ISettingsService settingsService, IPlayerService playerService, ITextToSpeechService textToSpeechService )
    {
        LastPageName = nameof(MarriageGameEngine);
        DbServices = dbServices;
        SettingsService = settingsService;
        PlayerService = playerService;
        TextToSpeechService = textToSpeechService;
        Initialized = false;
    }

    public async Task InitializeEngineAsync()
    {
        await PlayerService.InitializeAsync();
        await SettingsService.InitializeAsync();
        await TextToSpeechService.InitializeAsync();
        Initialized = true;
    }



}
