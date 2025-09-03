using MarriageCalculator.Repositories.Interfaces;
using MarriageCalculator.Services.Interfaces;

namespace MarriageCalculator.Services.Implementations;

public class PlayerService(IPlayerRepository playerRepository) : IPlayerService
{
    private readonly IPlayerRepository _playerRepository = playerRepository;

    public Dictionary<Guid, Player> ActivePlayers { get; private set; } = new Dictionary<Guid, Player>();
    public Dictionary<Guid, Player> AllPlayers { get; private set; } = new Dictionary<Guid, Player>();
    public bool IsInitialized { get; private set; } = false;

    public async Task InitializeAsync()
    {
        ActivePlayers.Clear();
        AllPlayers.Clear();
        var playersList = await _playerRepository.GetAllPlayersAsync();
        if (playersList is not null && playersList.Count > 0)
        {
            AllPlayers = playersList.ToDictionary(p => p.Id);
            IsInitialized = true;
            return;
        }
        else
        {
            var player = await _playerRepository.EnsureCurrentUserPlayerAsync();
            if (player is not null)
            {
                AllPlayers = new Dictionary<Guid, Player> { { player.Id, player } };
                IsInitialized = true;
            }
        }
    }

    public void SelectPlayerByIds(List<Guid> players)
    {
        if (AllPlayers.Count == 0)
            return;
        ActivePlayers.Clear();

        // Get selected players from AllPlayers
        foreach (var playerId in players)
        {
            if (AllPlayers.TryGetValue(playerId, out Player? player))
            {
                ActivePlayers.Add(playerId, player);
            }
        }
    }

    public async Task AddPlayerAsync(Player player)
    {
        ActivePlayers[player.Id] = player;

        // if AllPlayers does not contain player, add it to AllPlayers
        if (AllPlayers.TryAdd(player.Id, player))
        {
            await _playerRepository.CreatePlayerAsync(player);
        }
    }

    public async Task DeletePlayerAsync(Player player, bool removeFromDb = false)
    {
        ActivePlayers.Remove(player.Id);
        if (removeFromDb)
            await _playerRepository.DeletePlayerAsync(player.Id);
    }

    public void RemovePlayerById(Guid id)
    {
        var player = ActivePlayers[id];
        if (player is not null)
            ActivePlayers.Remove(id);
    }

    public Player? GetPlayerById(Guid id) => AllPlayers[id];
    public void ClearPlayers() => ActivePlayers.Clear();

    public async Task<IReadOnlyList<Player>> RefreshAllPlayers()
    {
        // FIXED: Don't clear ActivePlayers when refreshing AllPlayers
        // Only refresh the AllPlayers from the API/database
        var result = await _playerRepository.GetAllPlayersAsync();
        AllPlayers = (result is not null)
            ? result.ToDictionary(p => p.Id)
            : new Dictionary<Guid, Player>();

        // Ensure ActivePlayers only contains players that are still in AllPlayers
        SynchronizeActivePlayers();

        return result ?? new List<Player>();
    }

    private void SynchronizeActivePlayers()
    {
        var playersToRemove = ActivePlayers.Keys.Where(id => !AllPlayers.ContainsKey(id)).ToList();
        foreach (var playerId in playersToRemove)
        {
            ActivePlayers.Remove(playerId);
        }
    }
}
