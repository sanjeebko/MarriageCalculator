using FluentAssertions;
using MarriageCalculator.API.Repositories.Implementations;
using MarriageCalculator.API.Tests.Helpers;
using MarriageCalculator.Core.Models;
using Xunit;

namespace MarriageCalculator.API.Tests.UnitTests.Repositories;

/// <summary>
/// Unit tests for UserEmailVerificationRepository
/// Tests CRUD operations and business logic for user email verification
/// </summary>
public class UserEmailVerificationRepositoryTests : TestBase
{
    private readonly UserEmailVerificationRepository _repository;

    public UserEmailVerificationRepositoryTests()
    {
        _repository = new UserEmailVerificationRepository(DbContext);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateVerification()
    {
        // Arrange
        var verification = TestDataBuilder.UserEmailVerification().Build();

        // Act
        var result = await _repository.CreateAsync(verification);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BePositive();
        result.UserId.Should().Be(verification.UserId);
        result.VerificationCode.Should().Be(verification.VerificationCode);
        result.IsUsed.Should().Be(verification.IsUsed);
        result.ExpiresAt.Should().Be(verification.ExpiresAt);

        // Verify it was saved to database
        var saved = await DbContext.UserEmailVerifications.FindAsync(result.Id);
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task GetValidVerificationAsync_WithValidCodeAndNotExpired_ShouldReturnVerification()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var code = "123456";
        var verification = TestDataBuilder.UserEmailVerification()
            .WithUserId(userId)
            .WithVerificationCode(code)
            .WithIsUsed(false)
            .WithExpiresAt(DateTime.UtcNow.AddMinutes(10)) // Not expired
            .Build();

        await DbContext.UserEmailVerifications.AddAsync(verification);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetValidVerificationAsync(userId, code);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(verification.Id);
        result.UserId.Should().Be(userId);
        result.VerificationCode.Should().Be(code);
        result.IsUsed.Should().BeFalse();
    }

    [Fact]
    public async Task GetValidVerificationAsync_WithExpiredCode_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var code = "123456";
        var verification = TestDataBuilder.UserEmailVerification()
            .WithUserId(userId)
            .WithVerificationCode(code)
            .WithIsUsed(false)
            .WithExpiresAt(DateTime.UtcNow.AddMinutes(-10)) // Expired
            .Build();

        await DbContext.UserEmailVerifications.AddAsync(verification);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetValidVerificationAsync(userId, code);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetValidVerificationAsync_WithUsedCode_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var code = "123456";
        var verification = TestDataBuilder.UserEmailVerification()
            .WithUserId(userId)
            .WithVerificationCode(code)
            .WithIsUsed(true) // Already used
            .WithExpiresAt(DateTime.UtcNow.AddMinutes(10))
            .Build();

        await DbContext.UserEmailVerifications.AddAsync(verification);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetValidVerificationAsync(userId, code);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetValidVerificationAsync_WithInvalidCode_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var verification = TestDataBuilder.UserEmailVerification()
            .WithUserId(userId)
            .WithVerificationCode("123456")
            .WithIsUsed(false)
            .WithExpiresAt(DateTime.UtcNow.AddMinutes(10))
            .Build();

        await DbContext.UserEmailVerifications.AddAsync(verification);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetValidVerificationAsync(userId, "654321"); // Wrong code

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserIdAndCodeAsync_WithValidUserIdAndCode_ShouldReturnVerification()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var code = "123456";
        var verification = TestDataBuilder.UserEmailVerification()
            .WithUserId(userId)
            .WithVerificationCode(code)
            .Build();

        await DbContext.UserEmailVerifications.AddAsync(verification);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdAndCodeAsync(userId, code);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(verification.Id);
        result.UserId.Should().Be(userId);
        result.VerificationCode.Should().Be(code);
    }

    [Fact]
    public async Task GetByUserIdAndCodeAsync_WithInvalidUserIdOrCode_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var code = "123456";
        var verification = TestDataBuilder.UserEmailVerification()
            .WithUserId(userId)
            .WithVerificationCode(code)
            .Build();

        await DbContext.UserEmailVerifications.AddAsync(verification);
        await DbContext.SaveChangesAsync();

        // Act & Assert
        var resultWrongUserId = await _repository.GetByUserIdAndCodeAsync(Guid.NewGuid(), code);
        resultWrongUserId.Should().BeNull();

        var resultWrongCode = await _repository.GetByUserIdAndCodeAsync(userId, "654321");
        resultWrongCode.Should().BeNull();
    }

    [Fact]
    public async Task MarkAsUsedAsync_WithValidId_ShouldMarkAsUsedAndReturnVerification()
    {
        // Arrange
        var verification = TestDataBuilder.UserEmailVerification()
            .WithIsUsed(false)
            .WithUsedAt(null)
            .Build();

        await DbContext.UserEmailVerifications.AddAsync(verification);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.MarkAsUsedAsync(verification.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(verification.Id);
        result.IsUsed.Should().BeTrue();
        result.UsedAt.Should().NotBeNull();
        result.UsedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task MarkAsUsedAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = await _repository.MarkAsUsedAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteExpiredAsync_ShouldDeleteExpiredVerifications()
    {
        // Arrange - Clear any existing data
        DbContext.UserEmailVerifications.RemoveRange(DbContext.UserEmailVerifications);
        await DbContext.SaveChangesAsync();

        var expiredVerification1 = TestDataBuilder.UserEmailVerification()
            .WithExpiresAt(DateTime.UtcNow.AddMinutes(-10)) // Expired
            .Build();
        var expiredVerification2 = TestDataBuilder.UserEmailVerification()
            .WithExpiresAt(DateTime.UtcNow.AddMinutes(-5)) // Expired
            .Build();
        var validVerification = TestDataBuilder.UserEmailVerification()
            .WithExpiresAt(DateTime.UtcNow.AddMinutes(10)) // Not expired
            .Build();

        await DbContext.UserEmailVerifications.AddRangeAsync(
            expiredVerification1, expiredVerification2, validVerification);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteExpiredAsync();

        // Assert
        result.Should().BeTrue();

        // Verify expired verifications were deleted
        var remainingVerifications = DbContext.UserEmailVerifications.ToList();
        remainingVerifications.Should().HaveCount(1);
        remainingVerifications[0].Id.Should().Be(validVerification.Id);
    }

    [Fact]
    public async Task DeleteExpiredAsync_WithNoExpiredVerifications_ShouldReturnFalse()
    {
        // Arrange - Clear any existing data
        DbContext.UserEmailVerifications.RemoveRange(DbContext.UserEmailVerifications);
        await DbContext.SaveChangesAsync();

        var validVerification = TestDataBuilder.UserEmailVerification()
            .WithExpiresAt(DateTime.UtcNow.AddMinutes(10)) // Not expired
            .Build();

        await DbContext.UserEmailVerifications.AddAsync(validVerification);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteExpiredAsync();

        // Assert
        result.Should().BeFalse(); // Should return false when no expired verifications to delete

        // Verify verification is still there
        var remainingVerifications = DbContext.UserEmailVerifications.ToList();
        remainingVerifications.Should().HaveCount(1);
    }

    [Fact]
    public async Task DeleteByUserIdAsync_ShouldDeleteAllVerificationsForUser()
    {
        // Arrange - Clear any existing data
        DbContext.UserEmailVerifications.RemoveRange(DbContext.UserEmailVerifications);
        await DbContext.SaveChangesAsync();

        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var userVerification1 = TestDataBuilder.UserEmailVerification().WithUserId(userId).Build();
        var userVerification2 = TestDataBuilder.UserEmailVerification().WithUserId(userId).Build();
        var otherUserVerification = TestDataBuilder.UserEmailVerification().WithUserId(otherUserId).Build();

        await DbContext.UserEmailVerifications.AddRangeAsync(
            userVerification1, userVerification2, otherUserVerification);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteByUserIdAsync(userId);

        // Assert
        result.Should().BeTrue();

        // Verify only user's verifications were deleted
        var remainingVerifications = DbContext.UserEmailVerifications.ToList();
        remainingVerifications.Should().HaveCount(1);
        remainingVerifications[0].UserId.Should().Be(otherUserId);
    }

    [Fact]
    public async Task DeleteByUserIdAsync_WithNoVerificationsForUser_ShouldReturnFalse()
    {
        // Arrange - Clear any existing data
        DbContext.UserEmailVerifications.RemoveRange(DbContext.UserEmailVerifications);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteByUserIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse(); // Should return false when no verifications to delete
    }
}
