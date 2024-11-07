using CommunityToolkit.Mvvm.ComponentModel;
using MarriageCalculator.Extensions;
using System.Collections.ObjectModel;

namespace MarriageCalculator.ViewModels;

public partial class PlayerSettingsViewModel : ObservableObject
{
    public const int MaxPlayers = 6;

    private Player? _draggedItem;
    public string Message { get; set; } = string.Empty;

    [ObservableProperty]
    private string _playerName =string.Empty;          

    [ObservableProperty]
    private int _noOfPlayers ;

    [ObservableProperty]
    private bool _canAddMorePlayer =true;
    [ObservableProperty]
    private bool isRefreshing;
    public event EventHandler? OnComplete;
    public event EventHandler? OnError;
    

    public ObservableCollection<Player> CurrentPlayers { get; set; } = new();
    public ObservableCollection<Player> AllPlayers { get; set; } = new();
      
    public IMarriageGameEngine MarriageGameEngine { get; }

    public PlayerSettingsViewModel( IMarriageGameEngine marriageGameEngine)
    {
        MarriageGameEngine = marriageGameEngine;
        Console.WriteLine("PlayerSettingsViewModel: Previous Page: " + marriageGameEngine.LastPageName);
        MarriageGameEngine.LastPageName = nameof(PlayerSettingsViewModel);
        AllPlayers.Load(MarriageGameEngine.PlayerService.AllPlayers);
        CurrentPlayers.Load(MarriageGameEngine.PlayerService.Players);

        CurrentPlayers.CollectionChanged += CurrentPlayers_CollectionChanged;
    }

    private void CurrentPlayers_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        NoOfPlayers = CurrentPlayers.Count;
        UpdateAddPlayerButtonState();
    }

    private async Task RefreshAllPlayersAsync()
    {
        IsRefreshing = true;
        await MarriageGameEngine.PlayerService.RefreshAllPlayers();         
        AllPlayers.Load(MarriageGameEngine.PlayerService.AllPlayers);         
        IsRefreshing = false;
    }
     

    #region RelayCommands
    [RelayCommand]
    public async Task RefreshAllPlayers()
    {        
        await RefreshAllPlayersAsync();        
    }
    [RelayCommand]
    private void AddPlayer()
    {
        string[] seperators = [",", " ", "|", ";", "-", "_", "."];
        var players = PlayerName.Split(seperators,StringSplitOptions.TrimEntries);
        foreach(var player in players.Where(a=>a.Length>1))
        {
            AddPlayer(new Player { Name = player.ToFirstCharUpper() });
        }
         
        PlayerName = string.Empty;         
    }

    private void UpdateAddPlayerButtonState()
    {
        CanAddMorePlayer = NoOfPlayers < MaxPlayers;
    }


    public RelayCommand<Player> DeletePlayerCommand => new RelayCommand<Player>(RemovePlayer);
    public AsyncRelayCommand<Player> DeletePlayerFromDbCommand => new AsyncRelayCommand<Player>(RemovePlayerFromDbAsync);
    public RelayCommand<Player?> TapPlayerCommand => new RelayCommand<Player?>(TapPlayer);

    public void TapPlayer(Player? player)
    {
        if (player is null)
            return;
        AddPlayer(player);
    }
    [RelayCommand]
    private async Task Ok()
    {
       await  Activate(); 
        OnComplete?.Invoke(this, EventArgs.Empty);        
    }
      

    [RelayCommand]
    public void DragPlayer(Player player)
    {
        _draggedItem = player;
    }

    [RelayCommand]
    public void DroppedPlayer(Player player)
    {
        if (_draggedItem is null)
            return;

        var draggedIndex = CurrentPlayers.IndexOf(_draggedItem);
        int targetIndex = 0;
        if (player is not null)
            targetIndex = CurrentPlayers.IndexOf(player);
        CurrentPlayers.Move(draggedIndex, targetIndex);
        // Activate();
    }
    #endregion RelayCommands

    public async Task Activate()
    {
        if (CurrentPlayers.Count < 2)
        {
            Message = "Please add at least 2 players";
            OnError?.Invoke(this, EventArgs.Empty);
            return;
        }
        var oldPlayers = MarriageGameEngine.PlayerService.Players.ToArray();
        foreach (var player in oldPlayers.Where(a => !CurrentPlayers.Contains(a)))
        {
            await MarriageGameEngine.PlayerService.DeletePlayerAsync(player, false);
        }
        foreach (var player in CurrentPlayers)
        {
            await MarriageGameEngine.PlayerService.AddPlayerAsync(player);
        }
        await RefreshAllPlayersAsync();

        return;
    }



    public void AddPlayer(Player player)
    {
        if (CurrentPlayers.Contains(player) || CurrentPlayers.Any(a => a.Name == player.Name))
            return;

        if (CurrentPlayers.Count >= MaxPlayers)
            return;

        CurrentPlayers.Add(player);
        UpdateAddPlayerButtonState();
    }

    private void RemovePlayer(Player? player)
    {
        if (player is null)
            return;
        CurrentPlayers.Remove(player);

        UpdateAddPlayerButtonState();
    }
    private async Task RemovePlayerFromDbAsync(Player? player)
    {
        if (player is null)
            return;
        
        await MarriageGameEngine.PlayerService.DeletePlayerAsync(player,true);
        RemovePlayer(player);
         
        await RefreshAllPlayersAsync();
    }

   
}
