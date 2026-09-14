using MarriageCalculator.Core.Utilities;
using Xunit;

namespace MarriageCalculator.Core.Tests;

public class UsernameValidatorTests
{
    [Theory]
    [InlineData("  John  ", "John")]
    [InlineData("John   Doe", "John Doe")]
    [InlineData("  John    B   Doe  ", "John B Doe")]
    [InlineData("Alice_99", "Alice_99")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    [InlineData(null, "")]
    public void Normalize_TrimsAndCollapsesSpaces(string? input, string expected)
    {
        var result = UsernameValidator.Normalize(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("John")]
    [InlineData("user_1")]
    [InlineData("player-10")]
    [InlineData("12345")]
    [InlineData("John Doe")]
    [InlineData("John B Doe")]
    [InlineData("A B C")]
    [InlineData("123456789012345")] // exactly 15 chars
    [InlineData("A 1234567890123")] // exactly 15 chars with space
    public void Validate_ValidUsernames_ReturnTrue(string username)
    {
        var (isValid, errorMessage) = UsernameValidator.Validate(username);
        Assert.True(isValid);
        Assert.Null(errorMessage);
    }

    [Theory]
    [InlineData("", "Username cannot be empty.")]
    [InlineData(null, "Username cannot be empty.")]
    [InlineData(" 123", "Username cannot start or end with spaces.")]
    [InlineData("123 ", "Username cannot start or end with spaces.")]
    [InlineData("1234567890123456", "Username cannot exceed 15 characters.")] // 16 chars
    [InlineData("A B C D", "Username can have at most 2 spaces in the middle.")] // 3 spaces
    [InlineData("A  B", "Username cannot contain consecutive spaces.")] // double space if un-normalized
    [InlineData("John@Doe", "Username can only contain letters, numbers, underscores, hyphens, and spaces.")]
    [InlineData("User.Name", "Username can only contain letters, numbers, underscores, hyphens, and spaces.")]
    [InlineData("User#1", "Username can only contain letters, numbers, underscores, hyphens, and spaces.")]
    [InlineData("Player!1", "Username can only contain letters, numbers, underscores, hyphens, and spaces.")]
    public void Validate_InvalidUsernames_ReturnFalseWithReason(string? username, string expectedErrorSubstring)
    {
        var (isValid, errorMessage) = UsernameValidator.Validate(username);
        Assert.False(isValid);
        Assert.NotNull(errorMessage);
        Assert.Contains(expectedErrorSubstring, errorMessage);
    }

    [Theory]
    [InlineData("  John   Doe  ", true, "John Doe")]
    [InlineData("  John  B  Doe  ", true, "John B Doe")]
    [InlineData("   123456789012345   ", true, "123456789012345")]
    [InlineData("   1234567890123456   ", false, null)] // 16 chars
    [InlineData("  A   B   C   D  ", false, null)] // 3 spaces
    public void NormalizeThenValidate_Workflow(string rawInput, bool expectedValid, string? expectedNormalized)
    {
        var normalized = UsernameValidator.Normalize(rawInput);
        var (isValid, _) = UsernameValidator.Validate(normalized);

        Assert.Equal(expectedValid, isValid);
        if (expectedValid)
        {
            Assert.Equal(expectedNormalized, normalized);
        }
    }
}
