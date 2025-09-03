using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarriageCalculator.Core.Models;

[Table("MarriageGameScore")]
public class MarriageGameScore
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int MarriageGameId { get; set; }
    public Guid PlayerId { get; set; }

    public bool Seen { get; set; } = false;

    public bool Playing { get; set; } = false;

    public int Maal { get; set; } = 0;

    public int BonusPoint { get; set; } = 0;

    public bool Duply { get; set; } = false;

    public bool Winner { get; set; } = false;

    public int Score { get; set; } = 0;

    public double MoneyWon { get; set; }

    public bool Deal { get; set; } = false;

    public int Position { get; set; } = 0;

    [NotMapped]
    public MarriageGame? MarriageGame { get; set; }
}
