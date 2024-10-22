using SQLite;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;


namespace MarriageCalculator.Core.Models;

public partial class Player   : ObservableObject
{

    [ObservableProperty]
    public int id;
    [ObservableProperty]
    public string name  = "";
    [ObservableProperty]
    public bool seen   = false;
    [ObservableProperty]
    public bool playing   = false;
    [ObservableProperty]
    public int maal   = 0;
    [ObservableProperty]
    public bool duply   = false;
    [ObservableProperty]
    public bool isWinner  = false;
    [ObservableProperty]
    public int score   = 0;
    [ObservableProperty]
    public bool deal   = false;
    [ObservableProperty]
    public int position   = 0;

    
}
public class PlayerModel
{
    [PrimaryKey, AutoIncrement]
    public int PlayerId { get; set; }

    public int Position { get; set; }

    public string Name { get; set; }

    public string Email { get; set; }
}

public class GamePlayerMapModel
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int PlayerId { get; set; }

    [Indexed]
    public int MarriageGameId { get; set; }
}