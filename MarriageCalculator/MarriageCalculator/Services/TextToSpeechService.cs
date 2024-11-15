

namespace MarriageCalculator.Services;
public class TextToSpeechService: ITextToSpeechService
{
    private Locale? _nepaliLocale;
    private SpeechOptions? _nepaliSpeechOptions;
    public bool Mute { get; set; }
    public async Task InitializeAsync()
    {
        Mute = false;
        // Get available locales
        var locales = await TextToSpeech.Default.GetLocalesAsync();
        // Find the Nepali locale
        _nepaliLocale = locales.FirstOrDefault(locale => locale.Language == "ne" && locale.Country == "NP");

        if (_nepaliLocale is not null)
        {
            _nepaliSpeechOptions = new SpeechOptions
            {
                Locale = _nepaliLocale,
                Pitch = 1.0f,
                Volume = 1.0f
            };
        } 
    }
    public async Task SpeakAsync(Player[] players)
    {
        if (Mute)
            return;
        var numberOfPlayers = players.Length;
        if (numberOfPlayers < 2)
        {
            await SpeakTextInNepali("कृपया २ जना खेलाडीहरुको नाम राख्नु होला। ");
            return;
        }

        string? text = numberOfPlayers switch
        {
            2 => "मेरिज खेल्न तयार हुनु भएका २ जना खेलाडीहरुको नाम हुन् :  ",
            3 => "मेरिज खेल्न तयार हुनु भएका ३ जना खेलाडीहरुको नाम हुन् :  ",
            4 => "४ जना खेलाडीहरुको नाम एस प्रकार छन : ",
            5 => "५ जना खेलाडीहरु मेरिज खेल्न तयार हुनु भएको छ।  वहाहरुको नाम हो :  ",
            6 => "६ जना खेलाडीहरु मेरिज खेल्न तयार हुनु भएको छ।  वहाहरुको नाम हो :  ",
            _ => "खेलाडी हरु को संक्या मिलेन।  कृपया सच्च्याउनु होला। ",
        };

        var names = players.Select(player => player.Name);
        var extraName = " र ";
        //add extraName in the middle of last two names

        var lastTwoNames = names.Skip(numberOfPlayers - 2).ToList();
        lastTwoNames.Insert(1, extraName);
        names = names.Take(numberOfPlayers - 2).Concat(lastTwoNames);

        var namesString = string.Join(",", names);
        if (!string.IsNullOrEmpty(namesString))
            await SpeakTextInNepali(text + namesString);

    }
    public async Task SpeakAsync(string text)
    {
        if (Mute) return;
        if(!string.IsNullOrEmpty(text))
            await SpeakTextInNepali(text);
    }
    private async Task SpeakTextInNepali(string text)
    {
        if (Mute) return;
        if (_nepaliSpeechOptions != null)
        { 
            await TextToSpeech.Default.SpeakAsync(text, _nepaliSpeechOptions);
        }
        else
        {
            // Fallback if Nepali locale is not found
            await TextToSpeech.Default.SpeakAsync("Nepali locale not found.");
        }
    }
}
