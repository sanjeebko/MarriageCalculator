using SQLite;
using System.ComponentModel.DataAnnotations;

namespace MarriageCalculator.Core.Models;

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
    public bool IsActive { get; set; } = false;

    public MarriageGameSet()
    {
        Name = $"{DateTime.Now:yyyyMMdd HHmmss}";
        Created = DateTime.Now;
    }
}
