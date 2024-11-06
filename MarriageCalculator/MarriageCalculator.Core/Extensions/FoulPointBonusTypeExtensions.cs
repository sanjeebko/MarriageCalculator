using MarriageCalculator.Core.Models;

namespace MarriageCalculator.Core.Extensions;

public static class FoulPointBonusTypeExtensions
{
    public static string GetDescription(this FoulPointBonusType bonusType)
    {
        return bonusType switch
        {
            FoulPointBonusType.NO_FOUL_POINT => "Ignore Foul",
            FoulPointBonusType.NEXT_GAME => "Points in the next game",
            FoulPointBonusType.THIS_GAME => "Points in this game",
            _ => "Unknown bonus type"
        };
    }
}
