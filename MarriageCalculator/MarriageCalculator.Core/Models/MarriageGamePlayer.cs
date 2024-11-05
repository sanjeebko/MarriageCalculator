using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;

namespace MarriageCalculator.Core.Models;

public partial class MarriageGamePlayer : ObservableObject
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
    public bool duply = false;
    
    [ObservableProperty]
    public int score = 0;
    [ObservableProperty]
    public bool deal = false;
    [ObservableProperty]
    public int position = 0;
}
