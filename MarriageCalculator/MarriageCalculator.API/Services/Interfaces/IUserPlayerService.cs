using MarriageCalculator.Core.Models;

namespace MarriageCalculator.API.Services.Interfaces;

public interface IUserPlayerService
{
    Task CreateDefaultPlayerForUserAsync(User user);
    Task<List<Player>> GetAllAvailablePlayersAsync();
    Task<Player?> FindPlayerByEmailAsync(string email);
    Task<List<Player>> GetGuestPlayersAsync();
    Task<List<Player>> GetAppUsersAsync();
    Task<Player> CreatePlayerAsync(string name, string email, Guid createdByUserId);
}