using CommunityToolkit.Mvvm.ComponentModel;

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

   

    public bool IsActive { get; set; }

    public MarriageGameModel()
    {
        _name = $"{RandomString}";         
    }

    [ObservableProperty]
    private int _winnerPlayer;

     

    //create a function to return random string of length 6
    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        Random random = new ();
        return new string(Enumerable.Repeat(chars, length)
          .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
 

  