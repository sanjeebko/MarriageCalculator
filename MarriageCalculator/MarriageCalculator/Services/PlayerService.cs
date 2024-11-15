
using System.Collections.ObjectModel;

namespace MarriageCalculator.Services;

public class PlayerService : IPlayerService
{
    public List<Player> Players { get; private set; }
    public List<Player> AllPlayers { get; private set; }
    private IDbService DbService { get; }

    public PlayerService(IDbService dbService)
    {
        DbService = dbService;
        Players = new List<Player>();
        AllPlayers = new List<Player>();
         
    }
    public async Task InitializeAsync()
    {
        AllPlayers = await DbService.GetPlayersAsync();
    }

    public async Task AddPlayerAsync(Player player)
    {
        if (Players.Contains(player) || Players.Any(a => a.Equals(player)))
            return;
        
        Players.Add(player);
        if(!AllPlayers.Contains(player))
            await DbService.AddPlayerAsync(player);
    }
    public async Task DeletePlayerAsync(Player player,bool removeFromDb=false)
    {
        Players.Remove(player);
        if(removeFromDb)
            await DbService.DeletePlayerAsync(player);
    }
    public void RemovePlayerById(int id)
    {
        var player = Players.FirstOrDefault(p => p.Id == id);
        if (player is not null)
            Players.Remove(player);
    }
    public Player? GetPlayerById(int id) => AllPlayers.FirstOrDefault(p => p.Id == id);
    public void ClearPlayers() => Players.Clear();

    public async Task<List<Player>> RefreshAllPlayers()
    {
        var result = await DbService.GetPlayersAsync();
        if (result is not null)
            AllPlayers = result;
        return AllPlayers;
    }
}
