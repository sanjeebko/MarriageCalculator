using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;

namespace MarriageCalculator.Core.Models;
public partial class Player : ObservableObject
{

    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    [ObservableProperty]
    public string name = "";
    public string Email { get; set; } = string.Empty;
    public bool Deleted { get; set; } = false;
    [ObservableProperty]
    public bool selected  = false;
    public override bool Equals(object? obj)
    {
        if (obj is not Player player)
            throw new ArgumentException($" {nameof(obj)} must be of type {nameof(Player)}.", nameof(obj));

        return player.Name == this.Name && player.Email == this.Email;
    }
}
   