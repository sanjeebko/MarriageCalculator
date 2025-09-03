using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarriageCalculator.Core.Models;

[Table("Player")]
public class Player
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    public bool Deleted { get; set; } = false;
     
    public bool Selected { get; set; } = false;
    
    /// <summary>
    /// When this player was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Optional: Who created this player (for auditing purposes) by User.Id (Guid)
    /// This doesn't mean the player "belongs" to this user
    /// </summary>
    public Guid? CreatedByUserId { get; set; }
    
    // Navigation property (optional for auditing)
    public virtual User? CreatedByUser { get; set; }
    
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