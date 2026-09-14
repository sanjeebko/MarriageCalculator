using System.Linq;
using System.Text.RegularExpressions;

namespace MarriageCalculator.Core.Utilities;

public static partial class UsernameValidator
{
    public const int MaxLength = 15;

    [GeneratedRegex(@"\s{2,}")]
    private static partial Regex ConsecutiveWhitespaceRegex();

    [GeneratedRegex(@"^[a-zA-Z0-9_\- ]+$")]
    private static partial Regex AllowedCharactersRegex();

    /// <summary>
    /// Normalizes a username by trimming leading and trailing whitespace,
    /// and automatically collapsing runs of 2 or more spaces into a single space.
    /// </summary>
    public static string Normalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        var trimmed = raw.Trim();
        return ConsecutiveWhitespaceRegex().Replace(trimmed, " ");
    }

    /// <summary>
    /// Validates a normalized or un-normalized username against the naming rules:
    /// - Max length 15 characters (and at least 1 character).
    /// - Only characters a-z, A-Z, 0-9, _, -, and space.
    /// - Spaces only in the middle (neither leading nor trailing).
    /// - At most 2 spaces total in the middle, and no consecutive spaces.
    /// </summary>
    public static (bool IsValid, string? ErrorMessage) Validate(string? username)
    {
        if (string.IsNullOrEmpty(username))
        {
            return (false, "Username cannot be empty.");
        }

        if (username.StartsWith(' ') || username.EndsWith(' '))
        {
            return (false, "Username cannot start or end with spaces.");
        }

        if (username.Length > MaxLength)
        {
            return (false, $"Username cannot exceed {MaxLength} characters.");
        }

        if (!AllowedCharactersRegex().IsMatch(username))
        {
            return (false, "Username can only contain letters, numbers, underscores, hyphens, and spaces.");
        }

        if (username.Contains("  "))
        {
            return (false, "Username cannot contain consecutive spaces.");
        }

        int spaceCount = username.Count(c => c == ' ');
        if (spaceCount > 2)
        {
            return (false, "Username can have at most 2 spaces in the middle.");
        }

        return (true, null);
    }
}
