using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
 

namespace MarriageCalculator.Core.Models;

[Table("MarriageGame")]
public class MarriageGame
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; } 
    
    public int Sequence { get; set; }
    public int MarriageGameRoundId { get; set; }
    public int WinnerId { get; set; }
    public int DealerId { get; set; } 
    public int TotalMaal { get; set; }      
    public bool ClosedRound { get; set; }
    public DateTime CreatedTime { get; set; }
    
    [NotMapped]
    public Dictionary<int, MarriageGameScore> MarriageGameScores { get; set; } = []; //playerId, MarriageGameScore
}
