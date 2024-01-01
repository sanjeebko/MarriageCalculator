using SQLite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarriageCalculator.Core.Models;

public class MarriageGame
{
    [PrimaryKey, AutoIncrement]
    public int MarriageGameId { get; set; }

    [Required]
    [MinLength(2)]
    [System.ComponentModel.DataAnnotations.MaxLength(20)]
    public string Name { get; set; }

    public DateTime LastPlayed { get; set; }
    public DateTime Created { get; set; }

    [Required]
    [MinLength(2)]
    [System.ComponentModel.DataAnnotations.MaxLength(20)]
    public string? Player1 { get; set; }

    [Required]
    [MinLength(2)]
    [System.ComponentModel.DataAnnotations.MaxLength(20)]
    public string? Player2 { get; set; }

    [MinLength(2)]
    [System.ComponentModel.DataAnnotations.MaxLength(20)]
    public string? Player3 { get; set; }

    [MinLength(2)]
    [System.ComponentModel.DataAnnotations.MaxLength(20)]
    public string? Player4 { get; set; }

    [MinLength(2)]
    [System.ComponentModel.DataAnnotations.MaxLength(20)]
    public string? Player5 { get; set; }

    [MinLength(2)]
    [System.ComponentModel.DataAnnotations.MaxLength(20)]
    public string? Player6 { get; set; }

    public int Player1Score { get; set; }
    public int Player2Score { get; set; }
    public int Player3Score { get; set; }
    public int Player4Score { get; set; }
    public int Player5Score { get; set; }
    public int Player6Score { get; set; }
    public bool IsActive { get; set; } = false;

    public MarriageGame()
    {
        Name = $"Game {DateTime.Now:yyyyMMdd HHmmss}";
        Player1 = "Player 1";
        Player2 = "Player 2";
        Player3 = "Player 3";
        Player4 = "Player 4";
        Player5 = "Player 5";
        Player6 = "Player 6";
    }
}

public class MarriageGameRound
{
    [PrimaryKey, AutoIncrement]
    public int RoundId { get; set; }

    [Indexed]
    public int MarriageGameId { get; set; }

    public int RoundSequence { get; set; }

    public int Player1Seat { get; set; }
    public int Player2Seat { get; set; }
    public int Player3Seat { get; set; }
    public int Player4Seat { get; set; }
    public int Player5Seat { get; set; }
    public int Player6Seat { get; set; }

    public int Player1Maal { get; set; }
    public int Player2Maal { get; set; }
    public int Player3Maal { get; set; }
    public int Player4Maal { get; set; }
    public int Player5Maal { get; set; }
    public int Player6Maal { get; set; }
    public bool TotalMaal { get; set; }

    public bool Player1Seen { get; set; }
    public bool Player2Seen { get; set; }
    public bool Player3Seen { get; set; }
    public bool Player4Seen { get; set; }
    public bool Player5Seen { get; set; }
    public bool Player6Seen { get; set; }

    public int Player1Score { get; set; }
    public int Player2Score { get; set; }
    public int Player3Score { get; set; }
    public int Player4Score { get; set; }
    public int Player5Score { get; set; }
    public int Player6Score { get; set; }

    public bool IsPlayingPlayer1 { get; set; }
    public bool IsPlayingPlayer2 { get; set; }
    public bool IsPlayingPlayer3 { get; set; }
    public bool IsPlayingPlayer4 { get; set; }
    public bool IsPlayingPlayer5 { get; set; }
    public bool IsPlayingPlayer6 { get; set; }

    public bool IsDupleePlayer1 { get; set; }
    public bool IsDupleePlayer2 { get; set; }
    public bool IsDupleePlayer3 { get; set; }
    public bool IsDupleePlayer4 { get; set; }
    public bool IsDupleePlayer5 { get; set; }
    public bool IsDupleePlayer6 { get; set; }

    public bool Player1Foul { get; set; }
    public bool Player2Foul { get; set; }
    public bool Player3Foul { get; set; }
    public bool Player4Foul { get; set; }
    public bool Player5Foul { get; set; }
    public bool Player6Foul { get; set; }

    public string WinnerName { get; set; }
    public string DealerName { get; set; }
    public bool ClosedRound { get; set; }
}

public class GameSettings
{
    [PrimaryKey, AutoIncrement]
    public int GameSettingsId { get; set; }

    [Indexed]
    public int MarriageGameId { get; set; }

    public bool Murder { get; set; }
    public bool Kidnap { get; set; }

    public int SeenPoint { get; set; }
    public int UnseenPoint { get; set; }
    public int PointRate { get; set; }
    public string Currency { get; set; }

    public bool Dublee { get; set; }
    public bool DubleePointLess { get; set; }
    public int DubleePointBonus { get; set; }
    public int FoulPoint { get; set; }
    public int FoulPointBonus { get; set; }
}