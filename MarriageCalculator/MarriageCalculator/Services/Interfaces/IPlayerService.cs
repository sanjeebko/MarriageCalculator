namespace MarriageCalculator.Services.Interfaces;

public interface IPlayerService
{
    bool IsInitialized { get; }
    Dictionary<Guid,Player> AllPlayers { get; }
    Dictionary<Guid, Player> ActivePlayers { get; }

    Task AddPlayerAsync(Player player);

    Task InitializeAsync();
    Task<IReadOnlyList<Player>> RefreshAllPlayers();
    Task DeletePlayerAsync(Player player, bool removeFromDb);
    Player? GetPlayerById(Guid playerId);
    void SelectPlayerByIds(List<Guid> playerIds);

    
}