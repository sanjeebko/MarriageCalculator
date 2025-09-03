using MarriageCalculator.Extensions;
using MarriageCalculator.Services.Interfaces;
using System.Collections.ObjectModel;
using MvvmHelpers;
using CommunityToolkit.Mvvm.ComponentModel;
using ObservableObject = CommunityToolkit.Mvvm.ComponentModel.ObservableObject;

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
    
    [ObservableProperty]
    private string activePlayerNames = string.Empty;
    
    public event EventHandler? OnComplete;
    public event EventHandler? OnError;
    

    public ObservableRangeCollection<Player> ActivePlayers { get; set; } = new();
    public ObservableRangeCollection<Player> AllPlayers { get; set; } = new();
    public ObservableRangeCollection<PlayerWithStatus> AllPlayersWithStatus { get; set; } = new();
      
    public IMarriageGameEngine MarriageGameEngine { get; }

    public PlayerSettingsViewModel( IMarriageGameEngine marriageGameEngine)
    {
        MarriageGameEngine = marriageGameEngine;
        MarriageGameEngine.LastPageName = nameof(PlayerSettingsViewModel);
         
        LoadAllPlayersAsync().ConfigureAwait(false);
        ActivePlayers.SafeLoad(MarriageGameEngine.PlayerService.ActivePlayers.Values.ToList());

        ActivePlayers.CollectionChanged += CurrentPlayers_CollectionChanged;
    }

    public async Task InitializeAsync()
    {
        await RefreshAllPlayersAsync();
        NoOfPlayers = ActivePlayers.Count;
        UpdateAddPlayerButtonState();
        
        // Initialize ActivePlayerNames
        ActivePlayerNames = string.Join(",", ActivePlayers.Select(p => p.Name));
    }

    private void CurrentPlayers_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        NoOfPlayers = ActivePlayers.Count;
        UpdateAddPlayerButtonState();
        
        // Update the active player names string for the converter
        ActivePlayerNames = string.Join(",", ActivePlayers.Select(p => p.Name));
        
        // Force a refresh of all players to update their selection status
        RefreshAllPlayersVisualState();
    }

    private void RefreshAllPlayersVisualState()
    {
        // Update selection status for all existing PlayerWithStatus objects
        UpdateAllPlayersSelectionStatus();
    }

    private async Task LoadAllPlayersAsync()
    {
        var players = MarriageGameEngine.PlayerService.AllPlayers.Values.ToList();
        
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            AllPlayers.ReplaceRange(players);
            
            // Create PlayerWithStatus objects for the UI
            var playersWithStatus = players.Select(p => new PlayerWithStatus 
            { 
                Player = p
            }).ToList();
            
            AllPlayersWithStatus.ReplaceRange(playersWithStatus);
            
            // Update selection status for all players
            UpdateAllPlayersSelectionStatus();
        });
    }

    private void UpdateAllPlayersSelectionStatus()
    {
        foreach (var playerWithStatus in AllPlayersWithStatus)
        {
            bool isSelected = ActivePlayers.Any(ap => ap.Id == playerWithStatus.Player.Id);
            playerWithStatus.UpdateSelectionStatus(isSelected);
        }
    }

    private async Task LoadActivePlayersAsync() {
        var players = MarriageGameEngine.PlayerService.ActivePlayers.Values.ToList();

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            ActivePlayers.ReplaceRange(players);
        });
    }
    public async Task RefreshCurrentPlayerAsync()
    {
        IsRefreshing = true;
        // FIXED: Don't call RefreshAllPlayers here as it was clearing ActivePlayers
        // Just reload the ActivePlayers from the GameEngine without clearing them
        await LoadActivePlayersAsync();
        IsRefreshing = false;
    }
     

    #region RelayCommands
    [RelayCommand]
    public async Task RefreshAllPlayersAsync()
    {
        IsRefreshing = true;
        await MarriageGameEngine.PlayerService.RefreshAllPlayers();
        await MarriageGameEngine.AddMarriageGameSetPlayerAsync();
        await LoadAllPlayersAsync();

        IsRefreshing = false;
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
    public AsyncRelayCommand<PlayerWithStatus> DeletePlayerWithStatusFromDbCommand => new AsyncRelayCommand<PlayerWithStatus>(RemovePlayerWithStatusFromDbAsync);
    public RelayCommand<Player?> TapPlayerCommand => new RelayCommand<Player?>(TapPlayer);
    public RelayCommand<PlayerWithStatus?> TapPlayerWithStatusCommand => new RelayCommand<PlayerWithStatus?>(TapPlayerWithStatus);

    public void TapPlayer(Player? player)
    {
        if (player is null)
            return;
        AddPlayer(player);
    }

    public void TapPlayerWithStatus(PlayerWithStatus? playerWithStatus)
    {
        if (playerWithStatus?.Player is null)
            return;
        AddPlayer(playerWithStatus.Player);
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

        var draggedIndex = ActivePlayers.IndexOf(_draggedItem);
        int targetIndex = 0;
        if (player is not null)
            targetIndex = ActivePlayers.IndexOf(player);
        ActivePlayers.Move(draggedIndex, targetIndex);
        // Activate();
    }
    #endregion RelayCommands

    public async Task Activate()
    {
        if (ActivePlayers.Count < 2)
        {
            Message = "Please add at least 2 players";
            OnError?.Invoke(this, EventArgs.Empty);
            return;
        }
        
        // FIXED: No need to clear and re-add since we're syncing in real-time
        // The GameEngine's PlayerService.ActivePlayers should already be in sync
        // Just refresh the AllPlayers list to ensure database is up to date
        await RefreshAllPlayersAsync();

        return;
    }



    public async void AddPlayer(Player player)
    {
        if (ActivePlayers.Contains(player) || ActivePlayers.Any(a => a.Name == player.Name))
            return;

        if (ActivePlayers.Count >= MaxPlayers)
            return;

        // Add to local UI collection
        ActivePlayers.Add(player);
        
        // FIXED: Immediately sync with GameEngine's PlayerService
        await MarriageGameEngine.PlayerService.AddPlayerAsync(player);
        
        UpdateAddPlayerButtonState();
    }

    private async void RemovePlayer(Player? player)
    {
        if (player is null)
            return;
            
        // Remove from local UI collection
        ActivePlayers.Remove(player);
        
        // FIXED: Immediately sync with GameEngine's PlayerService
        await MarriageGameEngine.PlayerService.DeletePlayerAsync(player, false);

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
    
    private async Task RemovePlayerWithStatusFromDbAsync(PlayerWithStatus? playerWithStatus)
    {
        if (playerWithStatus?.Player is null)
            return;
        
        await RemovePlayerFromDbAsync(playerWithStatus.Player);
    }

    /// <summary>
    /// Method to check if a player is selected (for direct binding)
    /// </summary>
    public bool IsPlayerSelected(Player player)
    {
        return ActivePlayers.Any(p => p.Id == player.Id);
    }

    /// <summary>
    /// Method to get background color for a player (for direct binding)
    /// </summary>
    public Color GetPlayerBackgroundColor(Player player)
    {
        bool isSelected = ActivePlayers.Any(p => p.Id == player.Id);
        
        return isSelected 
            ? Color.FromArgb("#2ECC71") // Green background for selected players
            : Color.FromArgb("#4A4E69"); // Default background for unselected players
    }
}

/// <summary>
/// Wrapper class to track player with selection status for UI binding
/// </summary>
public partial class PlayerWithStatus : ObservableObject
{
    public required Player Player { get; set; }
    
    [ObservableProperty]
    private bool isSelected;
    
    [ObservableProperty]
    private Color backgroundColor = Color.FromArgb("#4A4E69");

    public void UpdateSelectionStatus(bool selected)
    {
        IsSelected = selected;
        BackgroundColor = selected 
            ? Color.FromArgb("#2ECC71") // Green for selected
            : Color.FromArgb("#4A4E69"); // Default for unselected
    }
}
