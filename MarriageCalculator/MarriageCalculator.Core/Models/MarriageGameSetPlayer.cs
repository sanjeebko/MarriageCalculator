using SQLite;

namespace MarriageCalculator.Core.Models;

[Table("MarriageGameSetPlayer")]
public class MarriageGameSetPlayer
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int MarriageGameSetId { get; set; }
    public int PlayerId { get; set; }
    [Ignore]
    public Player Player { get; set; }
}
