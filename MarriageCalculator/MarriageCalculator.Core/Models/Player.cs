using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;

namespace MarriageCalculator.Core.Models;
[Table("Player")]
public partial class Player : ObservableObject
{

    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    [ObservableProperty]
    public string name = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Deleted { get; set; } = false;
    [ObservableProperty]
    public bool selected  = false;
    public override bool Equals(object? obj)
    {
        if (obj is not Player player)
            throw new ArgumentException($" {nameof(obj)} must be of type {nameof(Player)}.", nameof(obj));

        return string.Equals( player.Name, this.Name,StringComparison.CurrentCultureIgnoreCase) 
            && string.Equals(player.Email , this.Email,StringComparison.CurrentCultureIgnoreCase);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name.ToLower(), Email.ToLower());
    }
}
   