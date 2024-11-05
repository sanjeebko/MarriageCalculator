using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;

namespace MarriageCalculator.Core.Models;

public class MarriageGameRound
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int MarriageGameId { get; set; }
         
}
