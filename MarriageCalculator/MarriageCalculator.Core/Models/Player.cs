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

}
   