using CommunityToolkit.Mvvm.ComponentModel;
using MarriageCalculator.Services.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MarriageCalculator.ViewModels;

public partial class ScoreBoardViewModel : ObservableObject
{
    public IMarriageGameEngine GameEngine { get; }

    public ObservableCollection<GameResult> GameResults { get; set; }
    public ObservableCollection<GameRowData> GameRowsData { get; set; }
    public List<string> PlayerNames { get; set; }

    public ScoreBoardViewModel(IMarriageGameEngine gameEngine)
    {
        GameEngine = gameEngine;
        GameResults = new ObservableCollection<GameResult>();
        GameRowsData = new ObservableCollection<GameRowData>();
        PlayerNames = new List<string>();

        // Load real game data instead of test data
        InitializeRealData();
        TransformDataForDataGrid();
    }

    private void InitializeRealData()
    {
        GameResults.Clear();

        // Check if we have an active game set with rounds
        if (GameEngine.MarriageGameSet?.Rounds == null || GameEngine.MarriageGameSet.Rounds.Count == 0)
        {
            // If no real data available, fall back to test data for demo purposes
            InitializeTestData();
            return;
        }

        // Process real game data from MarriageGameEngine
        int gameNumber = 1;

        foreach (var round in GameEngine.MarriageGameSet.Rounds.OrderBy(r => r.Sequence))
        {
            if (round.MarriageGames != null && round.MarriageGames.Count > 0)
            {
                foreach (var game in round.MarriageGames.OrderBy(g => g.Sequence))
                {
                    var gameResult = new GameResult(gameNumber++);

                    // Process scores for each player in this game
                    if (game.MarriageGameScores != null && game.MarriageGameScores.Count > 0)
                    {
                        foreach (var score in game.MarriageGameScores)
                        {
                            var player = GameEngine.MarriageGameSet.GameSetPlayers.Values
                                .FirstOrDefault(p => p.PlayerId == score.PlayerId);

                            if (player?.Player != null)
                            {
                                gameResult.PlayerStats.Add(player.Player.Name, new PlayerStats
                                {
                                    Maal = score.Maal,
                                    Seen = score.Seen,
                                    Winner = score.Winner,
                                    Point = CalculatePoints(score).ToString()
                                });
                            }
                        }
                    }

                    if (gameResult.PlayerStats.Count > 0)
                    {
                        GameResults.Add(gameResult);
                    }
                }
            }
        }

        // If no games found in rounds, fall back to test data
        if (GameResults.Count == 0)
        {
            InitializeTestData();
            return;
        }

        // Extract unique player names for column headers from real data
        PlayerNames = GameResults
            .SelectMany(gr => gr.PlayerStats.Keys)
            .Distinct()
            .OrderBy(name => name) // Sort player names alphabetically
            .ToList();
    }

    private int CalculatePoints(MarriageGameScore score)
    {
        // Calculate points based on game settings
        var settings = GameEngine.MarriageGameSet?.GameSettings ?? GameEngine.SettingsService.Settings;
        if (settings == null)
            return 0;

        int points = 0;
        
        // Base points for seen/unseen
        points += score.Seen ? settings.SeenPoint : settings.UnseenPoint;
        
        // Add bonus points
        points += score.BonusPoint;
        
        // Apply point rate if applicable
        if (settings.PointRate > 0)
        {
            points = (int)(points * settings.PointRate);
        }

        return points;
    }

    private void InitializeTestData()
    {
        // Keep original test data as fallback
        for (int i = 1; i <= 5; i++)
        {
            var gameResult = new GameResult(i);
            gameResult.PlayerStats.Add("Rajeev", new PlayerStats { Maal = i * 5, Seen = i % 2 == 0, Winner = i == 2, Point = (i * 5).ToString() });
            gameResult.PlayerStats.Add("Neha", new PlayerStats { Maal = i * 10, Seen = i % 2 == 0, Winner = i == 1, Point = (i * 2).ToString() });
            gameResult.PlayerStats.Add("Sanjeeb", new PlayerStats { Maal = i * 15, Seen = i % 3 == 0, Winner = i == 4, Point = (i * 3).ToString() });
            gameResult.PlayerStats.Add("Sushma", new PlayerStats { Maal = i * 20, Seen = i % 4 == 0, Winner = i == 3, Point = (i * 4).ToString() });
            gameResult.PlayerStats.Add("Amit", new PlayerStats { Maal = i * 25, Seen = i % 5 == 0, Winner = i == 5, Point = (i * 6).ToString() });
            gameResult.PlayerStats.Add("Priya", new PlayerStats { Maal = i * 30, Seen = i % 6 == 0, Winner = false, Point = (i * 7).ToString() });

            GameResults.Add(gameResult);
        }

        // Extract unique player names for column headers
        PlayerNames = GameResults
            .SelectMany(gr => gr.PlayerStats.Keys)
            .Distinct()
            .OrderBy(name => name) // Sort player names alphabetically
            .ToList();
    }

    private void TransformDataForDataGrid()
    {
        GameRowsData.Clear();
        
        foreach (var gameResult in GameResults)
        {
            var rowData = new GameRowData
            {
                GameNumber = gameResult.GameNumber
            };

            // Create player data for each player
            foreach (var playerName in PlayerNames)
            {
                if (gameResult.PlayerStats.TryGetValue(playerName, out var stats))
                {
                    var displayValue = FormatPlayerStats(stats);
                    rowData.PlayerData[playerName] = displayValue;
                    // Store winner status for styling
                    rowData.WinnerData[playerName] = stats.Winner;
                    // Store seen status for styling
                    rowData.SeenData[playerName] = stats.Seen;
                }
                else
                {
                    rowData.PlayerData[playerName] = "-";
                    rowData.WinnerData[playerName] = false;
                    rowData.SeenData[playerName] = false;
                }
            }

            GameRowsData.Add(rowData);
        }
    }

    private string FormatPlayerStats(PlayerStats stats)
    { 
        // Format as "Maal: X\nPoints: Y" to work with the capsule converters
        return $"Maal: {stats.Maal}\nPoints: {stats.Point}";
    }

    // Method to refresh data from current game state
    public void RefreshData()
    {
        InitializeRealData();
        TransformDataForDataGrid();
    }

    // Method to refresh data when new games are added
    public async Task RefreshFromGameEngineAsync()
    {
        // Ensure we have the latest data from the game engine
        if (GameEngine.MarriageGameSet != null)
        {
            await GameEngine.RefreshPlayers();
        }
        
        RefreshData();
    }
}

public class GameResult
{
    public int GameNumber { get; set; }
    public Dictionary<string, PlayerStats> PlayerStats { get; set; } = new Dictionary<string, PlayerStats>();

    public GameResult(int gameNumber)
    {
        GameNumber = gameNumber;
        PlayerStats = new Dictionary<string, PlayerStats>();
    }
}

public class PlayerStats
{
    public int Maal { get; set; }
    public bool Seen { get; set; }
    public bool Winner { get; set; }
    public string Point { get; set; } = string.Empty;
}

// Data structure for SfDataGrid binding
public class GameRowData : INotifyPropertyChanged
{
    public int GameNumber { get; set; }
    public Dictionary<string, string> PlayerData { get; set; } = new Dictionary<string, string>();
    public Dictionary<string, bool> WinnerData { get; set; } = new Dictionary<string, bool>();
    public Dictionary<string, bool> SeenData { get; set; } = new Dictionary<string, bool>();

    // Indexer to access player data dynamically
    public string this[string playerName]
    {
        get => PlayerData.TryGetValue(playerName, out var value) ? value : "-";
        set
        {
            PlayerData[playerName] = value;
            OnPropertyChanged($"[{playerName}]");
        }
    }

    // Method to check if a player is winner for this game
    public bool IsWinner(string playerName)
    {
        return WinnerData.TryGetValue(playerName, out var isWinner) && isWinner;
    }

    // Method to check if a player has seen for this game
    public bool HasSeen(string playerName)
    {
        return SeenData.TryGetValue(playerName, out var hasSeen) && hasSeen;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}