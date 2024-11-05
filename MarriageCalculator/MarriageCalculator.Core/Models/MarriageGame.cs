using SQLite;

namespace MarriageCalculator.Core.Models;

public class MarriageGame
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; } 
    public int MarriageGameRoundId { get; set; }
    public int DealerPlayerId { get; set; }
    public int WinnerId { get; set; }
    public int DealerPlayer { get; set; } 
    public bool TotalMaal { get; set; }      
    public bool ClosedRound { get; set; }
}
