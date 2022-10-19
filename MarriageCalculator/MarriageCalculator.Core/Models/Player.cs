using SQLite;

namespace MarriageCalculator.Core.Models;

public class PlayerModel
{
    [PrimaryKey, AutoIncrement]
    public int PlayerId { get; set; }

    public int Position { get; set; }

    public string Name { get; set; }

    public string Email { get; set; }
}

public class GamePlayerMapModel
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int PlayerId { get; set; }

    [Indexed]
    public int MarriageGameId { get; set; }
}