using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MarriageCalculator.ViewModels;

public partial class MarriageGameViewModel : ObservableObject
{


    public IMarriageGameEngine GameEngine { get; }

    public string CurrencyDescription => GameEngine.SettingsService.Settings.Currency.ToDescriptionString();
    public string DupleeBonus => $"(+{GameEngine.SettingsService.Settings.DubleePointBonus})";
    [ObservableProperty]
    public string gameSequence; 
    public ObservableCollection<PlayerMaal> PlayerMaals { get; } = new ObservableCollection<PlayerMaal>();

    public ObservableCollection<Player> Players { get; set; } = [];

    


    [ObservableProperty]
    public string winner = string.Empty;
    [ObservableProperty]
    public int totalScore;
    private PlayerMaal selectedPlayer;
    public PlayerMaal SelectedPlayer
    {
        get => selectedPlayer;
        set
        {
            if (SetProperty(ref selectedPlayer, value))
            {
                OnPlayerSelected(selectedPlayer);
            }
        }
    }

    [ObservableProperty]
    public int playerCount;

    [ObservableProperty]
    public int gameSetId;

    [ObservableProperty]
    public int gameRoundCount;

    [ObservableProperty]
    public bool isPopupOpen;

    [ObservableProperty]
    public bool enableMaalInput;

    [ObservableProperty]
    public int marriageGameCount;
    [ObservableProperty]
    public bool totalCalculated;
    [ObservableProperty]
    public bool showCalculate = true;
    [ObservableProperty]
    public bool showNewGame = false;
    [ObservableProperty]
    public int setId;
    [ObservableProperty]
    public int roundId;
    [ObservableProperty]
    public int gameId;

    public ICommand ShowPopup { get; set; }
    public ICommand ClosePopup { get; set; }

    
    public MarriageGameViewModel(IMarriageGameEngine gameEngine)
    {
        GameEngine = gameEngine;
         
        ButtonClick = new RelayCommand<string>(OnButtonClick);
        playerCount = GameEngine.PlayerService.Players.Count;
        //Load DynamicGrid with player/score. Columns are MarriageGameID and all the playersIds and rows are MarriageGames (id,Player1score,player2score...) 
        //PlayerId  is available in GameEngine.MarriageGameSet.GameSetPlayers.
        //Score information is available in GameEngine.MarriageGameSet.Rounds.MarriageGames.MarriageGameScores
        LoadPlayerScores();
        LoadIcons();
        LoadIds();
    }
    private void LoadIds()
    {
        SetId = GameEngine.MarriageGameSet?.Id ?? 0;
        RoundId = GameEngine.CurrentMarriageGameRound?.Id ?? 0;
        GameId = GameEngine.CurrentMarriageGame?.Id ?? 0;
        GameSequence=  GameEngine.CurrentMarriageGame?.Sequence.ToString() ?? "1";
    }

    #region IconSource
    public ImageSource SeenIcon { get; private set; }
    public ImageSource UnseenIcon { get; private set; }
    public ImageSource WinnerIcon { get; private set; }
    public ImageSource FoulIcon { get; private set; }
    public ImageSource DupleeIcon { get; private set; }
    public ImageSource IdleIcon { get; private set; }
    public ImageSource BackspaceIcon { get; private set; }
    public ImageSource DoneIcon { get; private set; }

    private void LoadIcons()
    {

        SeenIcon = GetIcon(FontelloCode.Seen);
        UnseenIcon = GetIcon(FontelloCode.Unseen);
        WinnerIcon = GetIcon(FontelloCode.Winner);
        DupleeIcon = GetIcon(FontelloCode.Duplee);
        FoulIcon = GetIcon(FontelloCode.Foul);
        IdleIcon = GetIcon(FontelloCode.Idle);
        BackspaceIcon = GetIcon(FontelloCode.BackSpace);
        DoneIcon = GetIcon(FontelloCode.Done);
        
    }
    private FontImageSource GetIcon(string icon)
    {
        return new FontImageSource
        {
            Glyph = icon,
            FontFamily = "Fontello",
            Color = Color.FromArgb("336699"),
            Size = 18
        };
    }
    #endregion IconSource


    private void OnPlayerSelected(PlayerMaal player)
    {
        if (player is not null)
        {
            SelectedPlayer = player;
            EnableMaalInput = player.Score.Seen;   
        }
    }

    private void LoadPlayerScores()
    {
        var playersScoreInOrder = GameEngine.CurrentMarriageGame?.MarriageGameScores.OrderBy(x => x.Value.Position);
        if (playersScoreInOrder is null) return;
        PlayerMaals.Clear();
        Players.Clear();

        foreach (var playerScoreInOrder in playersScoreInOrder)
        {
            var player = GetPlayer(playerScoreInOrder.Key);
            if (player is not null)
                Players.Add(player);
            PlayerMaal playerMaal = new PlayerMaal
            {
                PlayerObject = player ?? throw new Exception($"Player not found {playerScoreInOrder.Key}"),
                playerCount = PlayerCount,
                Score = playerScoreInOrder.Value
            };
            PlayerMaals.Add(playerMaal);
        }


    }

    private Player? GetPlayer(int playerId)
    {
        return GameEngine.PlayerService.GetPlayerById(playerId);
    } 
     
    [RelayCommand]
    public async Task NewGame()
    {
        if (GameEngine.CurrentMarriageGameRound is not null)
        {
            await GameEngine.CreateNewMarriageGameForGivenGameRound(GameEngine.CurrentMarriageGameRound);

            TotalScore = 0;
            GameSequence = GameEngine.CurrentMarriageGame?.Sequence.ToString()??"1";
            ShowCalculate = true;
            ShowNewGame = false;
            SelectedPlayer = null;
            
            LoadPlayerScores();
            LoadIds();
        } 
    }

    [RelayCommand]
    public async Task EndRound() {
    
    }


    public ICommand ButtonClick { get; }
    private void OnButtonClick(string parameter)
    {
        //paramater is the button name. When clicked, the button name is passed to this method.
        //Based on the button name, the action is performed.if the button name is number 0 to 9, the number is concatenated to SelectedPlayer.Score.Maal
        //button name "B" means backspace, i.e. delete the last digit from SelectedPlayer.Score.Maal. 
        //button name "D" means done, i.e call a method called DoneClick() to save the score.

        if (SelectedPlayer == null || SelectedPlayer.Score == null)
        {
            // Handle the null case, e.g., show an error message or log the issue
            Console.WriteLine("SelectedPlayer or SelectedPlayer.Score is null");
            return;
        }

        switch (parameter)
        {
            case "duplee":
                SelectedPlayer.Score.Duply = !SelectedPlayer.Score.Duply;
                SelectedPlayer.Score.Seen = true;
                EnableMaalInput = true;
                break;
            case "winner":
                var previousWinner = PlayerMaals.FirstOrDefault(player => player.Score.Winner);
                if (previousWinner != null)
                {
                    previousWinner.Score.Winner = false;
                }
                SelectedPlayer.Score.Winner = true;
                Winner = SelectedPlayer.PlayerObject.Name;
                SelectedPlayer.Score.Seen = true;
                EnableMaalInput = true;
                break;
            case "seen":
                SelectedPlayer.Score.Seen = true;
                EnableMaalInput = true;
                break;
            case "unseen":
                SelectedPlayer.Score.Seen = false;
                SelectedPlayer.Score.Maal = 0;
                SelectedPlayer.Score.Winner = false;
                EnableMaalInput = false;
                break;
            case "B":
                if (SelectedPlayer.Score.Maal > 9)
                {
                    string maal = SelectedPlayer.Score.Maal.ToString();
                    if (int.TryParse(maal.Substring(0, maal.Length - 1), out int result))
                        SelectedPlayer.Score.Maal = result;
                }
                else
                {
                    selectedPlayer.Score.Maal = 0;
                }
                break;
            case "D":
                CalculateTotal();
                break;

            default:
                if (int.TryParse(parameter, out int number))
                {
                    if (SelectedPlayer.Score.Maal == 0)
                    {
                        SelectedPlayer.Score.Maal = number;
                    }
                    else
                    {
                        var maal = SelectedPlayer.Score.Maal.ToString() + number.ToString();
                        var newMaal = int.Parse(maal);
                        if (newMaal <= 70)
                        {
                            SelectedPlayer.Score.Maal = newMaal;


                        }
                    }
                }
                break;
        }
        CalculateTotalMaal();
    }

    private void CalculateTotalMaal()
    {
        TotalScore = PlayerMaals.Where(p => p.Score.Seen).Sum(a => a.Score.Maal);
        TotalScore += PlayerMaals.Where(p => p.Score.Seen).Sum(a => a.Score.BonusPoint);
    }

    private void CalculateTotal()
    {
        // Implement the logic to save the score
        if (Winner is null)
        {
            return;
        }
        var noOfPlayer = Players.Count;
        var seenPoint = GameEngine.SettingsService.Settings.SeenPoint;
        var unSeenPoint = GameEngine.SettingsService.Settings.UnseenPoint;
        var winnerPlayer = PlayerMaals.FirstOrDefault(a => a.Score.Winner);
        if (winnerPlayer is not null && winnerPlayer.Score.Duply)
        {
            winnerPlayer.Score.BonusPoint = GameEngine.SettingsService.Settings.DubleePointBonus;
            CalculateTotalMaal();
        }

        foreach (var p in PlayerMaals)
        {
            if (p.Score.Winner)
            {


            }
            else if (p.Score.Seen)
            {
                var score = p.Score.Maal * noOfPlayer - (TotalScore + (p.Score.Duply ? 0 : GameEngine.SettingsService.Settings.SeenPoint));
                p.Score.Score = score;
            }
            else
            {
                p.Score.Score = (TotalScore + unSeenPoint) * (-1);
            }
            p.Score.MoneyWon = p.Score.Score * GameEngine.SettingsService.Settings.PointRate;
        }


        if (winnerPlayer is null)
            return;

        var winnerScore = PlayerMaals.Where(a => a.Score.Winner == false).Sum(a => a.Score.Score);

        winnerPlayer.Score.Score = -winnerScore;
        winnerPlayer.Score.MoneyWon = winnerPlayer.Score.Score * GameEngine.SettingsService.Settings.PointRate;

        TotalCalculated = true;
        ShowCalculate = false;
        ShowNewGame = true;

        //Save Marriage Game to Database; 
        GameEngine.SaveCurrentGame();

        CalculateRoundTotal();
    }

    private void CalculateRoundTotal()
    {
        if (GameEngine.CurrentMarriageGameRound == null)
            return;
        var playerTotalMoneyWon = new Dictionary<int, double>();
        for (int i = 0; i < GameEngine.CurrentMarriageGameRound.MarriageGames.Count; i++)
        {
            MarriageGame? marriageGame = GameEngine.CurrentMarriageGameRound.MarriageGames[i];
            foreach (var score in marriageGame.MarriageGameScores)
            {
                if (playerTotalMoneyWon.ContainsKey(score.Key))
                {
                    playerTotalMoneyWon[score.Key] += score.Value.MoneyWon;
                }
                else
                {
                    playerTotalMoneyWon[score.Key] = score.Value.MoneyWon;
                }
            }
        }
        foreach (var playerMaal in PlayerMaals)
        {
            if (playerTotalMoneyWon.TryGetValue(playerMaal.PlayerObject.Id, out double totalMoneyWon))
            {
                playerMaal.TotalForRound = totalMoneyWon;
            }
            else
            {
                playerMaal.TotalForRound = 0;
            }
        }

    }

     

}
public partial class PlayerMaal: ObservableObject
{
    public Player PlayerObject { get; set; }
     
    public int playerCount;
    [ObservableProperty]
    public double totalForRound;
     
    
    public MarriageGameScore Score { get; set; } 
     
}