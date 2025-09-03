using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarriageCalculator.Core.Models;

[Table("MarriageGameSet")]
public class MarriageGameSet
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }            
    
    [Required]
    [MinLength(2)]
    [MaxLength(20)]
    public string Name { get; set; }
    
    public DateTime LastPlayed { get; set; }
    public DateTime Created { get; set; }     
    public bool IsActive { get; set; } = true;
    public int GameSettingsId { get; set; }
    
    [NotMapped]
    public GameSettings GameSettings { get; set; } = GameSettings.Default();
    
    [NotMapped]
    public Dictionary<Guid,MarriageGameSetPlayer> GameSetPlayers { get; set; } = [];
    
    [NotMapped]
    public List<MarriageGameRound> Rounds { get; set; } = [];

    public MarriageGameSet()
    {
        Name = $"{DateTime.Now:yyyyMMdd HHmmss}";
        Created = DateTime.Now;
    }
}
