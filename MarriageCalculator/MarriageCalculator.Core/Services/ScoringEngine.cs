namespace MarriageCalculator.Core.Services;

using MarriageCalculator.Core.Models;

/// <summary>
/// Marriage game scoring engine implementing the Central Collection algorithm.
/// Handles Normal, Kidnap, and Murder game modes with Seen/Unseen/Dublee states.
/// </summary>
public class ScoringEngine
{
    /// <summary>
    /// Calculates scores for all players in a game using the Central Collection technique.
    /// Winner collects all penalties, then distributes Maal points.
    /// </summary>
    public static void CalculateScores(MarriageGame game, GameSettings settings)
    {
        var scores = game.MarriageGameScores.Values.Where(s => s.Playing).ToList();
        if (scores.Count < 2) return;

        var winner = scores.FirstOrDefault(s => s.Winner);
        if (winner == null) return;

        int playerCount = scores.Count;

        // Adjust Maal based on game mode
        ApplyGameMode(scores, winner, settings);

        // Step 1: Winner collects fixed penalties from all losers
        int totalCollected = CollectPenalties(scores, winner, settings, playerCount);

        // Step 2: Distribute Maal - compare each pair of seen players
        DistributeMaal(scores, winner, settings);

        // Calculate money
        foreach (var score in scores)
        {
            score.MoneyWon = score.Score * settings.PointRate;
        }

        // Update total Maal on game
        game.TotalMaal = scores.Where(s => s.Seen || s.Winner).Sum(s => s.Maal);
    }

    /// <summary>
    /// Apply game mode rules to Maal values before scoring.
    /// Normal: Unseen players keep their Maal (reduces what they owe).
    /// Kidnap: Winner steals unseen players' Maal.
    /// Murder: Unseen players' Maal is zeroed (voided).
    /// </summary>
    private static void ApplyGameMode(List<MarriageGameScore> scores, MarriageGameScore winner, GameSettings settings)
    {
        if (settings.Kidnap)
        {
            // Winner steals Maal from all unseen players
            foreach (var score in scores.Where(s => !s.Seen && !s.Winner))
            {
                winner.Maal += score.Maal;
                score.Maal = 0;
            }
        }
        else if (settings.Murder)
        {
            // Unseen players' Maal is voided (zeroed)
            foreach (var score in scores.Where(s => !s.Seen && !s.Winner))
            {
                score.Maal = 0;
            }
        }
        // Normal mode: unseen players keep their Maal (it reduces their penalty)
    }

    /// <summary>
    /// Winner collects fixed game points from all losers.
    /// Unseen pay unseenPoint, Seen pay seenPoint.
    /// Dublee winner gets bonus from all players.
    /// </summary>
    private static int CollectPenalties(List<MarriageGameScore> scores, MarriageGameScore winner, GameSettings settings, int playerCount)
    {
        int totalCollected = 0;

        foreach (var loser in scores.Where(s => !s.Winner))
        {
            int penalty;
            if (!loser.Seen)
            {
                penalty = settings.UnseenPoint;
            }
            else
            {
                penalty = settings.SeenPoint;
            }

            // Add dublee bonus if winner won with dublee
            if (winner.Duply && settings.Dublee)
            {
                penalty += settings.DubleePointBonus;
            }

            loser.Score -= penalty;
            totalCollected += penalty;
        }

        winner.Score += totalCollected;
        return totalCollected;
    }

    /// <summary>
    /// Distribute Maal points using Central Collection.
    /// Each seen player (and winner) compares Maal with every other seen player.
    /// Unseen players pay their full effective Maal to the winner.
    /// </summary>
    private static void DistributeMaal(List<MarriageGameScore> scores, MarriageGameScore winner, GameSettings settings)
    {
        var seenPlayers = scores.Where(s => s.Seen || s.Winner).ToList();
        var unseenPlayers = scores.Where(s => !s.Seen && !s.Winner).ToList();

        // Unseen players pay Maal difference to each seen player
        // In Normal mode, unseen still have their Maal which reduces what they owe
        // In Kidnap/Murder mode, unseen Maal is already 0
        foreach (var unseen in unseenPlayers)
        {
            foreach (var seen in seenPlayers)
            {
                int diff = seen.Maal - unseen.Maal;
                // Unseen pays the full Maal of each seen player (minus their own if Normal mode)
                seen.Score += diff;
                unseen.Score -= diff;
            }
        }

        // Seen players compare Maal with each other (including winner)
        for (int i = 0; i < seenPlayers.Count; i++)
        {
            for (int j = i + 1; j < seenPlayers.Count; j++)
            {
                int diff = seenPlayers[i].Maal - seenPlayers[j].Maal;
                seenPlayers[i].Score += diff;
                seenPlayers[j].Score -= diff;
            }
        }
    }

    /// <summary>
    /// Validates that scores are balanced (zero-sum game).
    /// Total of all scores should be zero.
    /// </summary>
    public static bool ValidateZeroSum(MarriageGame game)
    {
        var totalScore = game.MarriageGameScores.Values
            .Where(s => s.Playing)
            .Sum(s => s.Score);
        return totalScore == 0;
    }
}
