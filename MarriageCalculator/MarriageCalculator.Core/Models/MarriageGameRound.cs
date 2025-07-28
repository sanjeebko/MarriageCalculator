using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
 

namespace MarriageCalculator.Core.Models;

[Table("MarriageGameRound")]
public class MarriageGameRound
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public int Sequence { get; set; }
    public int MarriageGameSetId { get; set; }
    public bool Completed { get; set; }
    
    [NotMapped]
    public List<MarriageGame> MarriageGames { get; set; } = [];

    [NotMapped]
    public Dictionary<int, double> TotalScore { get; set; } = [];
}
