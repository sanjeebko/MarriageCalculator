namespace MarriageCalculator.Services.Interfaces;

public interface IPlayerService
{
    bool IsInitialized { get; }
    Dictionary<Guid,Player> AllPlayers { get; } 

    Task AddPlayerAsync(Player player);

    Task InitializeAsync();
    Task<IReadOnlyList<Player>> RefreshAllPlayers();
    Task DeletePlayerAsync(Player player);
    Player? GetPlayerById(Guid playerId);
     

    
}