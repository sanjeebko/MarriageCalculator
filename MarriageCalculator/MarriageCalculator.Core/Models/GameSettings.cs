using SQLite;
namespace MarriageCalculator.Core.Models;

[Table("GameSettings")]
public class GameSettings
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }    
    
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
    public bool Audio { get; set; }

    public static GameSettings Default() {
        return new GameSettings()
        {

            Murder = true,
            Kidnap = false,
            SeenPoint = 3,
            UnseenPoint = 10,
            PointRate = 10,
            Currency = Currency.NPR_Rupee,
            Dublee = true,
            DubleePointLess = true,
            FoulPoint = 15,
            FoulPointBonus = FoulPointBonusType.NEXT_GAME,
            Audio = true
        };
    }
}
