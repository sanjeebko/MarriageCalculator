using System.Globalization;

namespace MarriageCalculator.Core.Extensions;

public static class StringExtension
{
    public static string ToFirstCharUpper(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return char.ToUpper(input[0], CultureInfo.InvariantCulture) + input.Substring(1);
    }
}
