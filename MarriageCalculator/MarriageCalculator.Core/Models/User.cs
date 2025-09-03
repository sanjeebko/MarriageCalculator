using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarriageCalculator.Core.Models;

[Table("User")]
public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string DisplayName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    [Required]
    public string Salt { get; set; } = string.Empty;
    
    public bool IsEmailVerified { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastLoginAt { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<UserEmailVerification> EmailVerifications { get; set; } = new List<UserEmailVerification>();
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public virtual ICollection<GameSettings> GameSettings { get; set; } = new List<GameSettings>();
    
    // Optional: Players created by this user (for auditing)
    public virtual ICollection<Player> CreatedPlayers { get; set; } = new List<Player>();
    
    public override bool Equals(object? obj)
    {
        if (obj is not User user)
            return false;

        return string.Equals(user.Email, this.Email, StringComparison.CurrentCultureIgnoreCase);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Email.ToLower());
    }
}