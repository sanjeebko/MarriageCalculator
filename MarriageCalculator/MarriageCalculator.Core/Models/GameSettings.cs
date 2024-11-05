using SQLite;

namespace MarriageCalculator.Core.Models;

public class GameSettings
{ 
    public int Id { get; set; }    
    public int MarriageGameId { get; set; }
    public bool Murder { get; set; }
    public bool Kidnap { get; set; }
    public int SeenPoint { get; set; }
    public int UnseenPoint { get; set; }
    public double PointRate { get; set; }
    public Currency Currency { get; set; }
    public bool Dublee { get; set; }
    public bool DubleePointLess { get; set; }
    public int DubleePointBonus { get; set; }
    public int FoulPoint { get; set; }
    public FoulPointBonusType FoulPointBonus { get; set; }      
}
