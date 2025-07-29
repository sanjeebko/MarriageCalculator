using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarriageCalculator.Core.Models;

[Table("Player")]
public class Player
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    public bool Deleted { get; set; } = false;
     
    public bool Selected { get; set; } = false;
    
    public override bool Equals(object? obj)
    {
        if (obj is not Player player)
            throw new ArgumentException($" {nameof(obj)} must be of type {nameof(Player)}.", nameof(obj));

        return string.Equals(player.Name, this.Name, StringComparison.CurrentCultureIgnoreCase) 
            && string.Equals(player.Email, this.Email, StringComparison.CurrentCultureIgnoreCase);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name.ToLower(), Email.ToLower());
    }
}