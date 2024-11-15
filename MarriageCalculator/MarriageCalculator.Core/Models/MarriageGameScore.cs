using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;

namespace MarriageCalculator.Core.Models;
[Table("MarriageGameScore")]
public partial class MarriageGameScore : ObservableObject
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int MarriageGameId { get; set; }
    public int PlayerId { get; set; }

    [ObservableProperty]
    public bool seen = false;
    [ObservableProperty]
    public bool playing = false;
    [ObservableProperty]
    public int maal = 0;
    [ObservableProperty]
    public int bonusPoint = 0;
    [ObservableProperty]
    public bool duply = false;
    [ObservableProperty]
    public bool winner = false;

    [ObservableProperty]
    public int score = 0;
    [ObservableProperty]
    public double moneyWon;
    [ObservableProperty]
    public bool deal = false;
    [ObservableProperty]
    public int position = 0;
    [Ignore]
    public MarriageGame? MarriageGame { get; set; }
}
