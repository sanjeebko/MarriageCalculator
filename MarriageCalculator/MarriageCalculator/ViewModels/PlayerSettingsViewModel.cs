using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarriageCalculator.Core.Models;
using MarriageCalculator.Pages;
using MarriageCalculator.Services;
 
using System.Collections.ObjectModel;

namespace MarriageCalculator;

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
      
    public IMarriageGameServices MarriageGameServices { get; }
    private readonly IServiceProvider _serviceProvider;

    public PlayerSettingsViewModel(IMarriageGameServices marriageGameServices, IServiceProvider serviceProvider)
    {
        MarriageGameServices = marriageGameServices;
        _serviceProvider = serviceProvider;
        var allPlayers = marriageGameServices.GetPlayers().Result;

        //load allplayers to the collectionview PlayerList
        AllPlayers = new ObservableCollection<Player>(allPlayers);
        NoOfPlayers = CurrentPlayers.Count;
    }

    #region RelayCommands
    [RelayCommand]
    public void RefreshAllPlayers()
    {
        IsRefreshing = true;
        var allPlayers = MarriageGameServices.GetPlayers().Result;
        AllPlayers.Clear();

        foreach (var player in allPlayers)
        {
            var playerFound = AllPlayers.FirstOrDefault(p => p.name == player.name);
            if (playerFound is null)
            {
                AllPlayers.Add(player);
            }
        }
        IsRefreshing = false;
    }
    [RelayCommand]
    private void AddPlayer()
    {         
        AddPlayer(new Player { Name = PlayerName.ToFirstCharUpper() });
        PlayerName = string.Empty;
        NoOfPlayers = CurrentPlayers.Count;
        if (NoOfPlayers == MaxPlayers)
            CanAddMorePlayer = false;
    }
    //[RelayCommand]
    //private async Task SelectPlayer()
    //{ 
    //    var listPlayerViewModel = _serviceProvider.GetRequiredService<ListPlayerViewModel>();

    //    var navigationParams = new Dictionary<string, object>
    //        {
    //            {"ListPlayerViewModel", listPlayerViewModel}
    //        };
    //    listPlayerViewModel. OnClosePage += (sender, selectedPlayers) =>
    //    {
    //        System.Diagnostics.Debug.WriteLine("OnClosePage event triggered.");
    //        foreach (var player in selectedPlayers)
    //        {
    //            if (CurrentPlayers.Count < MaxPlayers && !CurrentPlayers.Any(a=>a.Name== player.Name))
    //            {
    //                CurrentPlayers.Add(player);
    //            }
    //        }
    //    };
    //    await Shell.Current.GoToAsync(nameof(ListPlayer), true, navigationParams);

         
    //}

     

    public RelayCommand<Player> DeletePlayerCommand => new RelayCommand<Player>(RemovePlayer);
 public RelayCommand<Player> DeletePlayerFromDbCommand => new RelayCommand<Player>(RemovePlayerFromDb);
    public RelayCommand<Player?> TapPlayerCommand => new RelayCommand<Player?>(TapPlayer);

    public void TapPlayer(Player? player)
    {
        if (player is null)
            return;
        AddPlayer(player);
    }
    [RelayCommand]
    private void Ok()
    {
        Activate();
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

    public async void Activate()
    {
        if(CurrentPlayers.Count < 2)
        {
            Message = "Please add at least 2 players";
            OnError?.Invoke(this, EventArgs.Empty);
            return;
        }
        if(CurrentPlayers.Any(p=>p.Id == 0))
        {
            foreach (var player in CurrentPlayers.Where(a => a.Id == 0))
            {
                await  MarriageGameServices.AddPlayerAsync(player);

                if (player.Id == 0)
                {
                    Message = "Error adding player";
                    OnError?.Invoke(this, EventArgs.Empty);
                    return;
                }                
            }
            return;
        }

        Message = "ready"; 
    }



    public void AddPlayer(Player player)
    {
        if (CurrentPlayers.Contains(player) || CurrentPlayers.Any(a=>a.Name==player.Name))
            return;

        if (CurrentPlayers.Count < MaxPlayers)
            CurrentPlayers.Add(player);

        NoOfPlayers = CurrentPlayers.Count;

        if (NoOfPlayers == MaxPlayers)
            CanAddMorePlayer = false;
        else
            CanAddMorePlayer = true;


    }
    
    private void RemovePlayer(Player? player)
    {
        if (player is null)
            return;
        CurrentPlayers.Remove(player);

        NoOfPlayers = CurrentPlayers.Count;
         
         
        if (NoOfPlayers == MaxPlayers)
            CanAddMorePlayer = false;
        else
            CanAddMorePlayer = true;
    }
    private void RemovePlayerFromDb(Player? player)
    {
        if (player is null)
            return;
       
        //Delete player from database
        MarriageGameServices.DeletePlayer(player);
        CurrentPlayers.Remove(player);
        RefreshAllPlayers();
    }
    
   
}
