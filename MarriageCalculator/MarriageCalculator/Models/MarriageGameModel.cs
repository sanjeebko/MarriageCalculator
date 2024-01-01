using CommunityToolkit.Mvvm.ComponentModel;
using MarriageCalculator.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace MarriageCalculator.Models;

public partial class MarriageGameModel : ObservableObject
{
    #region private variables

    [ObservableProperty]
    private string? _player1;

    [ObservableProperty]
    private string? _player2;

    [ObservableProperty]
    private string? _player3;

    [ObservableProperty]
    private string? _player4;

    [ObservableProperty]
    private string? _player5;

    [ObservableProperty]
    private string? _player6;

    private string _name;

    #endregion private variables

    public int MarriageGameId { get; set; }

    public string Name
    {
        get => _name;
        set
        {
            if (!string.IsNullOrEmpty(value) && value != _name)
                SetProperty(ref _name, value);
        }
    }

    public DateTime LastPlayed { get; set; }
    public DateTime Created { get; set; }

    [ObservableProperty]
    private int _player1Score;

    [ObservableProperty]
    private int _player2Score;

    [ObservableProperty]
    private int _player3Score;

    [ObservableProperty]
    private int _player4Score;

    [ObservableProperty]
    private int _player5Score;

    [ObservableProperty]
    private int _player6Score;

    public bool IsActive { get; set; }

    public MarriageGameModel()
    {
        _name = $"Game {DateTime.Now:yyyyMMdd HHmmss}";
        Player1 = "Player 1";
        Player2 = "Player 2";
        Player3 = "Player 3";
        Player4 = "Player 4";
        Player5 = "Player 5";
        Player6 = "Player 6";
    }

    [ObservableProperty]
    private string _winnerPlayer;

    public List<GameScore> GameScore { get; set; }
}

public class GameScore
{
    public string Name { get; set; }
    public string Score { get; set; }
    public bool Seen { get; set; }
    public bool Dublee { get; set; }
    public bool Winner { get; set; }
}

public static class MarriageGameModelExtension
{
    public static MarriageGame ToMarriageGame(this MarriageGameModel model) => new()
    {
        Name = model.Name,
        Player1 = model.Player1,
        Player2 = model.Player2,
        Player3 = model.Player3,
        Player4 = model.Player4,
        Player5 = model.Player5,
        Player6 = model.Player6,

        Player1Score = model.Player1Score,
        Player2Score = model.Player2Score,
        Player3Score = model.Player3Score,
        Player4Score = model.Player4Score,
        Player5Score = model.Player5Score,
        Player6Score = model.Player6Score,

        MarriageGameId = model.MarriageGameId,
        LastPlayed = model.LastPlayed,
        Created = model.Created,
        IsActive = model.IsActive
    };
}

public static class MarriageGameExtension
{
    public static MarriageGameModel ToMarriageGameModel(this MarriageGame model) => new()
    {
        Name = model.Name,
        Player1 = model.Player1,
        Player2 = model.Player2,
        Player3 = model.Player3,
        Player4 = model.Player4,
        Player5 = model.Player5,
        Player6 = model.Player6,

        Player1Score = model.Player1Score,
        Player2Score = model.Player2Score,
        Player3Score = model.Player3Score,
        Player4Score = model.Player4Score,
        Player5Score = model.Player5Score,
        Player6Score = model.Player6Score,

        MarriageGameId = model.MarriageGameId,
        LastPlayed = model.LastPlayed,
        Created = model.Created,
        IsActive = model.IsActive
    };
}