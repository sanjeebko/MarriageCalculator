namespace MarriageCalculator.Helpers;

/// <summary>
/// Utility class for MarriageGameSetPlayer operations and conversions
/// </summary>
public static class MarriageGameSetPlayerHelper
{
    /// <summary>
    /// Converts a Player to a MarriageGameSetPlayer for a given MarriageGameSet
    /// </summary>
    /// <param name="player">The player to convert</param>
    /// <param name="marriageGameSet">The marriage game set to associate with</param>
    /// <returns>A new MarriageGameSetPlayer instance</returns>
    public static MarriageGameSetPlayer FromPlayer(Player player, MarriageGameSet marriageGameSet)
    {
        return new MarriageGameSetPlayer
        {
            MarriageGameSet = marriageGameSet,
            MarriageGameSetId = marriageGameSet.Id,
            PlayerId = player.Id,
            Player = player
        };
    }
}