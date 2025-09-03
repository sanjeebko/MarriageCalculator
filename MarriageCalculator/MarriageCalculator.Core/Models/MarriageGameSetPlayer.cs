using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarriageCalculator.Core.Models;

[Table("MarriageGameSetPlayer")]
public class MarriageGameSetPlayer
{
    // Composite key will be configured in DbContext: (MarriageGameSetId, PlayerId)
    public int MarriageGameSetId { get; set; }
    public Guid PlayerId { get; set; }

    // Navigation properties (mapped)
    public Player Player { get; set; } = default!;
    public MarriageGameSet MarriageGameSet  { get; set; } = default!;
}
