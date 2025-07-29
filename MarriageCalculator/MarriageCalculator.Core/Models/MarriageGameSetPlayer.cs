using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarriageCalculator.Core.Models;

[Table("MarriageGameSetPlayer")]
public class MarriageGameSetPlayer
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public int MarriageGameSetId { get; set; }
    public int PlayerId { get; set; }
    
    [NotMapped]
    public Player Player { get; set; }
}
