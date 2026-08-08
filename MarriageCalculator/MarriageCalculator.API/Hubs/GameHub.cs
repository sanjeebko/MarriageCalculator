using Microsoft.AspNetCore.SignalR;

namespace MarriageCalculator.API.Hubs;

/// <summary>
/// SignalR hub for real-time game score updates.
/// Clients join a group by gameSetId and receive score broadcasts.
/// </summary>
public class GameHub : Hub
{
    private readonly ILogger<GameHub> _logger;

    public GameHub(ILogger<GameHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Join a game room to receive real-time updates.
    /// </summary>
    public async Task JoinGame(string gameSetId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, gameSetId);
        _logger.LogInformation("Client {ConnectionId} joined game {GameSetId}", Context.ConnectionId, gameSetId);
        await Clients.Caller.SendAsync("JoinedGame", gameSetId);
    }

    /// <summary>
    /// Leave a game room.
    /// </summary>
    public async Task LeaveGame(string gameSetId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameSetId);
        _logger.LogInformation("Client {ConnectionId} left game {GameSetId}", Context.ConnectionId, gameSetId);
    }

    /// <summary>
    /// Broadcast score update to all clients in a game room.
    /// </summary>
    public async Task BroadcastScoreUpdate(string gameSetId, object scoreData)
    {
        await Clients.Group(gameSetId).SendAsync("ScoreUpdated", scoreData);
    }

    /// <summary>
    /// Notify all clients in a game that a new round has been added.
    /// </summary>
    public async Task BroadcastNewRound(string gameSetId, object roundData)
    {
        await Clients.Group(gameSetId).SendAsync("NewRound", roundData);
    }

    /// <summary>
    /// Notify all clients that game settings have changed.
    /// </summary>
    public async Task BroadcastSettingsChange(string gameSetId, object settingsData)
    {
        await Clients.Group(gameSetId).SendAsync("SettingsChanged", settingsData);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client {ConnectionId} disconnected", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
