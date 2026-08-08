using System.ComponentModel;

namespace MarriageCalculator.Core.Models;

public enum Currency
{
    [Description("British Pound Pence")]
    GBP_Pence,
    [Description("US Dollar Cent")]
    USD_Cent,
    [Description("Nepalese Rupee")]
    NPR_Rupee,
    [Description("Indian Rupee")]
    INR_Rupee,
    [Description("Euro Cent")]
    EUR_Cent,
    [Description("Australian Dollar Cent")]
    AUD_Cent
}

public static class CurrencyExtensions
{
    public static string ToDescriptionString(this Currency val)
    {
        var field = val.GetType().GetField(val.ToString());
        if (field is null) return val.ToString();
        var attribute = (DescriptionAttribute?)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
        return attribute?.Description ?? val.ToString();
    }
}
