using MarriageCalculator.Core.Models;
using MarriageCalculator.Core.Services;

namespace MarriageCalculator.Core.Tests.Services;

public class ScoringEngineTests
{
    private static GameSettings DefaultSettings() => GameSettings.Default();

    private static MarriageGame CreateGame(string winnerId, List<(string playerId, bool seen, int maal, bool duply)> players)
    {
        var game = new MarriageGame { WinnerId = winnerId };
        foreach (var (playerId, seen, maal, duply) in players)
        {
            game.MarriageGameScores[playerId] = new MarriageGameScore
            {
                PlayerId = playerId,
                Seen = seen || playerId == winnerId,
                Playing = true,
                Maal = maal,
                Duply = duply,
                Winner = playerId == winnerId,
                Score = 0,
                MoneyWon = 0
            };
        }
        return game;
    }

    [Fact]
    public void CalculateScores_ZeroSum_AllModes()
    {
        // Test that scores are always zero-sum in all 3 modes
        var modes = new[] { ("Murder", true, false), ("Kidnap", false, true), ("Normal", false, false) };

        foreach (var (name, murder, kidnap) in modes)
        {
            var settings = DefaultSettings();
            settings.Murder = murder;
            settings.Kidnap = kidnap;

            var game = CreateGame("1", new()
            {
                ("1", true, 20, false),   // Winner, seen, 20 maal
                ("2", true, 10, false),   // Seen, 10 maal
                ("3", false, 5, false),   // Unseen, 5 maal
                ("4", false, 0, false),   // Unseen, 0 maal
            });

            ScoringEngine.CalculateScores(game, settings);
            Assert.True(ScoringEngine.ValidateZeroSum(game), $"{name} mode failed zero-sum");
        }
    }

    [Fact]
    public void CalculateScores_Murder_UnseenMaalZeroed()
    {
        var settings = DefaultSettings();
        settings.Murder = true;
        settings.Kidnap = false;

        var game = CreateGame("1", new()
        {
            ("1", true, 10, false),  // Winner
            ("2", true, 5, false),   // Seen
            ("3", false, 8, false),  // Unseen - maal should be zeroed
        });

        ScoringEngine.CalculateScores(game, settings);

        // Unseen player's maal should be 0 in Murder mode
        Assert.Equal(0, game.MarriageGameScores["3"].Maal);
        Assert.True(ScoringEngine.ValidateZeroSum(game));
    }

    [Fact]
    public void CalculateScores_Kidnap_WinnerStealsMaal()
    {
        var settings = DefaultSettings();
        settings.Murder = false;
        settings.Kidnap = true;

        var game = CreateGame("1", new()
        {
            ("1", true, 10, false),  // Winner, starts with 10 maal
            ("2", true, 5, false),   // Seen
            ("3", false, 8, false),  // Unseen - maal stolen by winner
        });

        ScoringEngine.CalculateScores(game, settings);

        // Winner should have original 10 + stolen 8 = 18
        Assert.Equal(18, game.MarriageGameScores["1"].Maal);
        Assert.Equal(0, game.MarriageGameScores["3"].Maal);
        Assert.True(ScoringEngine.ValidateZeroSum(game));
    }

    [Fact]
    public void CalculateScores_Normal_UnseenKeepsMaal()
    {
        var settings = DefaultSettings();
        settings.Murder = false;
        settings.Kidnap = false;

        var game = CreateGame("1", new()
        {
            ("1", true, 10, false),  // Winner
            ("2", true, 5, false),   // Seen
            ("3", false, 8, false),  // Unseen - keeps maal in Normal mode
        });

        ScoringEngine.CalculateScores(game, settings);

        // Unseen player keeps their maal in Normal mode
        Assert.Equal(8, game.MarriageGameScores["3"].Maal);
        Assert.True(ScoringEngine.ValidateZeroSum(game));
    }

    [Fact]
    public void CalculateScores_TwoPlayers_Simple()
    {
        var settings = DefaultSettings();
        settings.Murder = true;

        var game = CreateGame("1", new()
        {
            ("1", true, 15, false),  // Winner, 15 maal
            ("2", true, 10, false),  // Seen loser, 10 maal
        });

        ScoringEngine.CalculateScores(game, settings);

        // Winner gets: seenPoint (3) from loser + maal diff (15-10=5) = 8
        Assert.Equal(8, game.MarriageGameScores["1"].Score);
        Assert.Equal(-8, game.MarriageGameScores["2"].Score);
        Assert.True(ScoringEngine.ValidateZeroSum(game));
    }

    [Fact]
    public void CalculateScores_SixPlayers_ZeroSum()
    {
        var settings = DefaultSettings();
        settings.Murder = true;

        var game = CreateGame("1", new()
        {
            ("1", true, 25, false),  // Winner
            ("2", true, 15, false),  // Seen
            ("3", true, 10, false),  // Seen
            ("4", false, 8, false),  // Unseen
            ("5", false, 3, false),  // Unseen
            ("6", false, 0, false),  // Unseen
        });

        ScoringEngine.CalculateScores(game, settings);
        Assert.True(ScoringEngine.ValidateZeroSum(game));
    }

    [Fact]
    public void CalculateScores_WinnerCanBeNegative()
    {
        var settings = DefaultSettings();
        settings.Murder = false;
        settings.Kidnap = false;

        // Winner has very low maal, others have very high
        var game = CreateGame("1", new()
        {
            ("1", true, 0, false),   // Winner with 0 maal
            ("2", true, 30, false),  // Seen with 30 maal
            ("3", true, 25, false),  // Seen with 25 maal
        });

        ScoringEngine.CalculateScores(game, settings);

        // Winner collects seenPoint*2=6, but pays (30+25)=55 in maal diffs
        // Winner should be negative
        Assert.True(game.MarriageGameScores["1"].Score < 0, "Winner should have negative score");
        Assert.True(ScoringEngine.ValidateZeroSum(game));
    }

    [Fact]
    public void CalculateScores_DubleeBonus()
    {
        var settings = DefaultSettings();
        settings.Murder = true;
        settings.Dublee = true;
        settings.DubleePointBonus = 5;

        var game = CreateGame("1", new()
        {
            ("1", true, 10, true),   // Winner with dublee
            ("2", true, 5, false),   // Seen
            ("3", false, 0, false),  // Unseen
        });

        ScoringEngine.CalculateScores(game, settings);
        Assert.True(ScoringEngine.ValidateZeroSum(game));

        // Winner should get bonus from each loser
        // Seen loser pays: seenPoint(3) + dubleeBonus(5) = 8
        // Unseen loser pays: unseenPoint(10) + dubleeBonus(5) = 15
    }

    [Fact]
    public void CalculateScores_MoneyCalculation()
    {
        var settings = DefaultSettings();
        settings.Murder = true;
        settings.PointRate = 5.0;

        var game = CreateGame("1", new()
        {
            ("1", true, 10, false),
            ("2", true, 5, false),
        });

        ScoringEngine.CalculateScores(game, settings);

        // MoneyWon should be Score * PointRate
        Assert.Equal(
            game.MarriageGameScores["1"].Score * 5.0,
            game.MarriageGameScores["1"].MoneyWon
        );
    }

    [Fact]
    public void CalculateScores_LessThanTwoPlayers_NoEffect()
    {
        var settings = DefaultSettings();
        var game = CreateGame("1", new()
        {
            ("1", true, 10, false),
        });

        ScoringEngine.CalculateScores(game, settings);
        Assert.Equal(0, game.MarriageGameScores["1"].Score);
    }

    [Fact]
    public void ValidateZeroSum_CorrectlyDetectsImbalance()
    {
        var game = new MarriageGame();
        game.MarriageGameScores["1"] = new MarriageGameScore { PlayerId = "1", Playing = true, Score = 10 };
        game.MarriageGameScores["2"] = new MarriageGameScore { PlayerId = "2", Playing = true, Score = -5 };

        Assert.False(ScoringEngine.ValidateZeroSum(game));
    }

    [Fact]
    public void CalculateScores_AllUnseenExceptWinner()
    {
        var settings = DefaultSettings();
        settings.Murder = true;

        var game = CreateGame("1", new()
        {
            ("1", true, 20, false),  // Winner (seen)
            ("2", false, 5, false),  // Unseen
            ("3", false, 3, false),  // Unseen
            ("4", false, 0, false),  // Unseen
        });

        ScoringEngine.CalculateScores(game, settings);
        Assert.True(ScoringEngine.ValidateZeroSum(game));

        // Winner should collect unseenPoint from each unseen = 10*3 = 30
        // Plus maal from each unseen (all zeroed in murder) = 20*3 = 60
        // Total winner score = 90
        Assert.Equal(90, game.MarriageGameScores["1"].Score);
    }
}

