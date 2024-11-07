
namespace MarriageCalculator.Services
{
    public interface IPlayerService
    {
        List<Player> AllPlayers { get; }
        List<Player> Players { get; }

        Task AddPlayerAsync(Player player);

        Task InitializeAsync();
        Task<List<Player>> RefreshAllPlayers();
        Task DeletePlayerAsync(Player player, bool removeFromDb);
    }
}