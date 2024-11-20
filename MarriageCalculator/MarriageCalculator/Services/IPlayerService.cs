
namespace MarriageCalculator.Services;

public interface IPlayerService
{
    Dictionary<int,Player> AllPlayers { get; }
    Dictionary<int, Player> Players { get; }

    Task AddPlayerAsync(Player player);

    Task InitializeAsync();
    Task<List<Player>> RefreshAllPlayers();
    Task DeletePlayerAsync(Player player, bool removeFromDb);
    Player? GetPlayerById(int playerId);
    void SelectPlayerByIds(List<int> playerIds);
}