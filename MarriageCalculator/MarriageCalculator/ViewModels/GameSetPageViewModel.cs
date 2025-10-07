using MarriageCalculator.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;

namespace MarriageCalculator.ViewModels;

public class GameSetPageViewModel
{
    public IMarriageGameEngine MarriageGameEngine { get; }
    
    public ObservableCollection<MarriageGameRound> Rounds { get; } = new ObservableCollection<MarriageGameRound>();

    public GameSetPageViewModel(IMarriageGameEngine marriageGameEngine)
    {
        MarriageGameEngine = marriageGameEngine;
    }
    public async Task InitializeAsync()
    {
        if (!MarriageGameEngine.Initialized)
        {
            await MarriageGameEngine.InitializeEngineAsync();
        }
        
        if (MarriageGameEngine.MarriageGameSet == null)
        {
            // Handle the case where there is no active game set
            return;
        }
         
    }

    
}

