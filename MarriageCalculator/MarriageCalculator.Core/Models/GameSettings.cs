using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace MarriageCalculator.Core.Models;

[Table("GameSettings")]
public class GameSettings
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }    
    
    /// <summary>
    /// Foreign key to User - each GameSetting belongs to a specific user
    /// </summary>
    public Guid UserId { get; set; }
    
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
    
    /// <summary>
    /// When this settings record was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public virtual User? User { get; set; }

    public static GameSettings Default(Guid userId = default)
    {
        return new GameSettings()
        {
            UserId = userId,
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
