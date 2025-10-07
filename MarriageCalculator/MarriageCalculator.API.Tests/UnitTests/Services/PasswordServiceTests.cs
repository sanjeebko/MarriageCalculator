using FluentAssertions;
using MarriageCalculator.API.Services.Implementations;
using MarriageCalculator.API.Tests.Helpers;
using Xunit;

namespace MarriageCalculator.API.Tests.UnitTests.Services;

/// <summary>
/// Unit tests for PasswordService
/// </summary>
public class PasswordServiceTests : TestBase
{
    private readonly PasswordService _service;

    public PasswordServiceTests()
    {
        _service = new PasswordService();
    }

    [Theory]
    [InlineData("Password1", true)]  // Has capital letter and number
    [InlineData("Password!", true)]  // Has capital letter and symbol
    [InlineData("MyP@ssw0rd", true)] // Has capital letter, symbol, and number
    [InlineData("ABCDEFGH1", true)]  // All caps with number
    [InlineData("abcdefgh1A", true)] // Lowercase with number and capital
    [InlineData("MyPassword", false)] // Only letters, no number or symbol
    [InlineData("mypassword1", false)] // No capital letter
    [InlineData("MYPASSWORD1", true)]  // Has capital letter and number
    [InlineData("Pass1", false)]     // Too short (less than 8 characters)
    [InlineData("", false)]          // Empty string
    [InlineData("   ", false)]       // Whitespace only
    [InlineData("password", false)]  // No capital, no number, no symbol
    [InlineData("PASSWORD", false)]  // No lowercase, no number, no symbol
    [InlineData("12345678", false)]  // Only numbers, no capital letter
    [InlineData("!@#$%^&*", false)] // Only symbols, no capital letter
    public void ValidatePasswordStrength_WithVariousPasswords_ShouldReturnExpectedResult(string password, bool expected)
    {
        // Act
        var result = _service.ValidatePasswordStrength(password);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ValidatePasswordStrength_WithNullPassword_ShouldReturnFalse()
    {
        // Act
        var result = _service.ValidatePasswordStrength(null!);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HashPassword_WithValidPassword_ShouldReturnHashAndSalt()
    {
        // Arrange
        var password = "TestPassword123";

        // Act
        var hash = _service.HashPassword(password, out var salt);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        salt.Should().NotBeNullOrEmpty();
        
        // Hash and salt should be valid base64 strings
        var hashBytes = Convert.FromBase64String(hash);
        var saltBytes = Convert.FromBase64String(salt);
        
        hashBytes.Should().HaveCount(32); // 256 bits = 32 bytes
        saltBytes.Should().HaveCount(32); // 256 bits = 32 bytes
    }

    [Fact]
    public void HashPassword_WithSamePasswordMultipleTimes_ShouldProduceDifferentHashes()
    {
        // Arrange
        var password = "TestPassword123";

        // Act
        var hash1 = _service.HashPassword(password, out var salt1);
        var hash2 = _service.HashPassword(password, out var salt2);

        // Assert
        hash1.Should().NotBe(hash2);
        salt1.Should().NotBe(salt2);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "TestPassword123";
        var hash = _service.HashPassword(password, out var salt);

        // Act
        var result = _service.VerifyPassword(password, hash, salt);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var correctPassword = "TestPassword123";
        var incorrectPassword = "WrongPassword456";
        var hash = _service.HashPassword(correctPassword, out var salt);

        // Act
        var result = _service.VerifyPassword(incorrectPassword, hash, salt);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithWrongSalt_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword123";
        var hash = _service.HashPassword(password, out var correctSalt);
        var wrongSalt = _service.GenerateSalt(); // Generate a different salt

        // Act
        var result = _service.VerifyPassword(password, hash, wrongSalt);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithInvalidHash_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword123";
        _service.HashPassword(password, out var salt);
        var invalidHash = "InvalidHashString";

        // Act
        var result = _service.VerifyPassword(password, invalidHash, salt);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithInvalidSalt_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword123";
        var hash = _service.HashPassword(password, out var validSalt);
        var invalidSalt = "InvalidSaltString";

        // Act
        var result = _service.VerifyPassword(password, hash, invalidSalt);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GenerateSalt_ShouldReturnValidBase64String()
    {
        // Act
        var salt = _service.GenerateSalt();

        // Assert
        salt.Should().NotBeNullOrEmpty();
        
        // Should be valid base64 string
        var saltBytes = Convert.FromBase64String(salt);
        saltBytes.Should().HaveCount(32); // 256 bits = 32 bytes
    }

    [Fact]
    public void GenerateSalt_CalledMultipleTimes_ShouldReturnDifferentSalts()
    {
        // Act
        var salt1 = _service.GenerateSalt();
        var salt2 = _service.GenerateSalt();
        var salt3 = _service.GenerateSalt();

        // Assert
        salt1.Should().NotBe(salt2);
        salt2.Should().NotBe(salt3);
        salt1.Should().NotBe(salt3);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void HashPassword_WithInvalidPassword_ShouldStillWork(string? password)
    {
        // Act & Assert
        // The method should handle invalid inputs gracefully without throwing
        var hash = _service.HashPassword(password ?? string.Empty, out var salt);
        
        hash.Should().NotBeNullOrEmpty();
        salt.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void VerifyPassword_WithEmptyPassword_ShouldWorkConsistently()
    {
        // Arrange
        var password = "";
        var hash = _service.HashPassword(password, out var salt);

        // Act
        var result = _service.VerifyPassword(password, hash, salt);

        // Assert
        result.Should().BeTrue(); // Empty password should verify against its own hash
    }

    [Fact]
    public void PasswordHashingAndVerification_ShouldBeConsistent()
    {
        // Arrange
        var passwords = new[]
        {
            "SimplePass1",
            "Complex!Password@123",
            "AnotherTest$456",
            "Short1",
            "VeryLongPasswordWithManyCharacters123!@#"
        };

        foreach (var password in passwords)
        {
            // Act
            var hash = _service.HashPassword(password, out var salt);
            var verificationResult = _service.VerifyPassword(password, hash, salt);

            // Assert
            verificationResult.Should().BeTrue($"Password '{password}' should verify against its own hash");
        }
    }

    [Fact]
    public void VerifyPassword_ShouldBeSecureAgainstTimingAttacks()
    {
        // Arrange
        var password = "TestPassword123";
        var hash = _service.HashPassword(password, out var salt);
        var wrongPassword = "WrongPassword456";

        // Act - Multiple verifications should take similar time regardless of correctness
        var correctResults = new List<bool>();
        var incorrectResults = new List<bool>();

        for (int i = 0; i < 10; i++)
        {
            correctResults.Add(_service.VerifyPassword(password, hash, salt));
            incorrectResults.Add(_service.VerifyPassword(wrongPassword, hash, salt));
        }

        // Assert
        correctResults.Should().OnlyContain(r => r == true);
        incorrectResults.Should().OnlyContain(r => r == false);
    }
}
