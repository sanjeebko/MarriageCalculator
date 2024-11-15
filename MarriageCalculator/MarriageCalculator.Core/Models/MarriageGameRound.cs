using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;

namespace MarriageCalculator.Core.Models;
[Table("MarriageGameRound")]
public class MarriageGameRound
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int Sequence { get; set; }
    public int MarriageGameSetId { get; set; }
    [Ignore]
    public List<MarriageGame> MarriageGames { get; set; } = [];

    [Ignore]
    public Dictionary<int, double> TotalScore { get; set; } = [];
    
}
