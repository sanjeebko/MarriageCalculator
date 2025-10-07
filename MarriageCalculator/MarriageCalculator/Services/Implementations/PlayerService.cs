using MarriageCalculator.Repositories.Interfaces;
using MarriageCalculator.Services.Interfaces;

namespace MarriageCalculator.Services.Implementations;

public class PlayerService(IPlayerRepository playerRepository) : IPlayerService
{
    private readonly IPlayerRepository _playerRepository = playerRepository;

   
    public Dictionary<Guid, Player> AllPlayers { get; private set; } = new Dictionary<Guid, Player>();
    public bool IsInitialized { get; private set; } = false;

    public async Task InitializeAsync()
    {

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

    public async Task AddPlayerAsync(Player player)
    {
        // if AllPlayers does not contain player, add it to AllPlayers
        if (AllPlayers.TryAdd(player.Id, player))
        {
            await _playerRepository.CreatePlayerAsync(player);
        }
    }

    public async Task DeletePlayerAsync(Player player)
    {
        await _playerRepository.DeletePlayerAsync(player.Id);
    }

    public Player? GetPlayerById(Guid id) 
    {
        AllPlayers.TryGetValue(id, out Player? player);
        return player;
    }


    public async Task<IReadOnlyList<Player>> RefreshAllPlayers()
    {
        var result = await _playerRepository.GetAllPlayersAsync();
        AllPlayers = (result is not null)
            ? result.ToDictionary(p => p.Id)
            : [];

        return result ?? [];
    }

}
