namespace MarriageCalculator.Services;

public class PlayerService : IPlayerService
{
    public Dictionary<int,Player> Players { get; private set; }
    public Dictionary<int, Player> AllPlayers { get; private set; }
    private IDbService DbService { get; }

    public PlayerService(IDbService dbService)
    {
        DbService = dbService;
        Players = [];
        AllPlayers = [];         
    }
    public async Task InitializeAsync()
    {

        var playersList = await DbService.GetPlayersAsync();
        if (playersList is not null)
        {
            AllPlayers = playersList.ToDictionary(p => p.Id);
        }
    }

    public void SelectPlayerByIds(List<int> players)
    {
        if (AllPlayers.Count == 0)
            return;
        Players.Clear();

        // Get selected players from AllPlayers  
        foreach (var playerId in players)
        {
            if (AllPlayers.TryGetValue(playerId, out Player? player))
            {
                Players.Add(playerId, player);
            }
        }
    }
    public async Task AddPlayerAsync(Player player)
    {
        Players[player.Id]=player;

        // if AllPlayers does not contain player, add it to AllPlayers
        if (AllPlayers.TryAdd(player.Id, player))
        {
            await DbService.AddPlayerAsync(player);
        }        
    }
    public async Task DeletePlayerAsync(Player player,bool removeFromDb=false)
    {
        Players.Remove(player.Id);
        if(removeFromDb)
            await DbService.DeletePlayerAsync(player);
    }
    public void RemovePlayerById(int id)
    {
        var player = Players[id];
        if (player is not null)
            Players.Remove(id);
    }
    public Player? GetPlayerById(int id) => AllPlayers[id];
    public void ClearPlayers() => Players.Clear();

    public async Task<List<Player>> RefreshAllPlayers()
    {
        var result = await DbService.GetPlayersAsync();
        if (result is not null)
            AllPlayers = result.ToDictionary(p => p.Id);
        return result??[];
    }
}
