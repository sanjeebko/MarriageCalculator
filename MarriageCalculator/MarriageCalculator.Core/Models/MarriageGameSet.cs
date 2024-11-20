using SQLite;
using System.ComponentModel.DataAnnotations;

namespace MarriageCalculator.Core.Models;
[Table("MarriageGameSet")]
public class MarriageGameSet
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }            
    [Required]
    [MinLength(2)]
    [System.ComponentModel.DataAnnotations.MaxLength(20)]
    public string Name { get; set; }
    public DateTime LastPlayed { get; set; }
    public DateTime Created { get; set; }     
    public bool IsActive { get; set; } = true;
    public int GameSettingsId { get; set; }
    [Ignore]
    public GameSettings GameSettings { get; set; } = GameSettings.Default();
    [Ignore]
    public Dictionary<int,MarriageGameSetPlayer> GameSetPlayers { get; set; } = [];
    [Ignore]
    public List<MarriageGameRound> Rounds { get; set; } = [];

   
    public MarriageGameSet()
    {
        Name = $"{DateTime.Now:yyyyMMdd HHmmss}";
        Created = DateTime.Now;
    }
}
