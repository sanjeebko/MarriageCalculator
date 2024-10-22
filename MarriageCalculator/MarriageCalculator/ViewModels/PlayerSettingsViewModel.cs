using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarriageCalculator.Core.Models;
using System.Collections.ObjectModel;

namespace MarriageCalculator;

public partial class PlayerSettingsViewModel : ObservableObject
{

    private Player? _draggedItem;
    public string Message { get; set; } = string.Empty;

    [ObservableProperty]
    private string _playerName =string.Empty;
      
    public const int MaxPlayers = 6;

    [ObservableProperty]
    private int _noOfPlayers ;

    [ObservableProperty]
    private bool _canAddMorePlayer =true;


    public event EventHandler? OnComplete;


    public ObservableCollection<Player> Players { get; set; } = new();
    
     

    public Player? FirstPlayer { get; set; }
    public Player? SelectedPlayer { get; set; }


    public PlayerSettingsViewModel()
    {
        
    }

    [RelayCommand]
    private void AddPlayer()
    {
        AddPlayer(new Player { Name = PlayerName });
        PlayerName = string.Empty;
        NoOfPlayers = Players.Count;
        if (NoOfPlayers == MaxPlayers)
            CanAddMorePlayer = false;

    }
    public RelayCommand<Player> DeletePlayerCommand => new RelayCommand<Player>(RemovePlayer);

    [RelayCommand]
    private void Ok()
    {
        Activate();
        OnComplete?.Invoke(this, EventArgs.Empty);
    }


    public void Activate()
    {
        if(Players.Count < 2)
        {
            Players.Clear();
            Players.Add(new Player { Name = "Player 1" });
            Players.Add(new Player { Name = "Player 2" });
        }

        int id = 0;
        foreach (var player in Players)
        {
            id++;
            player.Seen = false;
            player.Playing = true;
            player.Maal = 0;
            player.Duply = false;
            player.IsWinner = false;
            player.Score = 0;
            player.Deal = false;
            player.Id = id;        
            player.Position = id;
        }

        Message = "ready"; 
    }

    private void SetPlayerSequence(List<int> randomSequence)
    {
        if(randomSequence.Count == 0)
            return;
        var pos = 0;
        foreach(var id in randomSequence)
        {
            var player = Players.FirstOrDefault(p => p.Id == id);
            if (player is null)
                continue;
            pos++;
            player.Position = pos;
            player.Seen = false;
            player.Playing = true;
            player.Maal = 0;
            player.Duply = false;
            player.IsWinner = false;
            player.Score = 0;
            player.Deal = false;
        }

        // swap Player position in Players collection based on position. Position starts from 1
        var sortedPlayers = Players.OrderBy(p => p.Position).ToList();
        Players.Clear();
        foreach (var player in sortedPlayers)
        {
            Players.Add(player);
        }


        //Dictionary<int, Player> temp = new();
        //foreach(var player in Players)
        //{
        //    temp.Add(Players.IndexOf(player), player);
        //}

        //foreach (KeyValuePair<int, Player> kvp in temp.OrderBy(a => a.Value.Position))
        //{
        //    Players.Move(kvp.Key, kvp.Value.Position-1);
        //}
    }

    public void AddPlayer(Player player)
    {
        if (FirstPlayer is null)
        { 
            FirstPlayer = player; 
            SelectedPlayer = player;
        }
        if(Players.Count<MaxPlayers)
            Players.Add(player);

        if (NoOfPlayers == MaxPlayers)
            CanAddMorePlayer = false;
        else
            CanAddMorePlayer = true;
    }
   

    private void RemovePlayer(Player? player)
    {
        if (player is null)
            return;
        if (Players.Contains(player))
        {
            Players.Remove(player);
        }


        NoOfPlayers = Players.Count;

        if (player == FirstPlayer)
        {
            FirstPlayer = null;
            FirstPlayer = Players.FirstOrDefault();
        }
        if (player == SelectedPlayer)
        {
            SelectedPlayer = null;
        }
        if (NoOfPlayers == MaxPlayers)
            CanAddMorePlayer = false;
        else
            CanAddMorePlayer = true;
    }
    public Player? GetNextPlayer()
    {
       if(SelectedPlayer!.Position == MaxPlayers)
        {
            return Players.FirstOrDefault(p => p.Position == 1);
        }

        return Players.FirstOrDefault(a => a.Position == SelectedPlayer!.Position+1);
    }

    public void SelectNextPlayer()
    { 
        SelectedPlayer = GetNextPlayer(); ;
    }

[RelayCommand]
    public void ShufflePlayers()
    {
        var random = new Random();
        var randomSequence = Enumerable.Range(1, MaxPlayers).OrderBy(x => random.Next()).ToList();
        SetPlayerSequence(randomSequence);
    }

    
     

    [RelayCommand]
    public void DragPlayer(Player player)
    {
        _draggedItem = player; 
    }

    [RelayCommand]
    public void DroppedPlayer(Player player)
    {
        if(_draggedItem is null )
            return;

        var draggedIndex = Players.IndexOf(_draggedItem);
        int targetIndex = 0;
        if(player is not null)
            targetIndex=  Players.IndexOf(player);
        Players.Move(draggedIndex, targetIndex);
       // Activate();
    }
}
