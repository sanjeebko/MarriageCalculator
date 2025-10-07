using FluentAssertions;
using MarriageCalculator.API.Repositories.Implementations;
using MarriageCalculator.API.Tests.Helpers;
using MarriageCalculator.Core.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MarriageCalculator.API.Tests.UnitTests.Repositories;

/// <summary>
/// Unit tests for RefreshTokenRepository
/// </summary>
public class RefreshTokenRepositoryTests : TestBase
{
    private readonly RefreshTokenRepository _repository;

    public RefreshTokenRepositoryTests()
    {
        _repository = new RefreshTokenRepository(DbContext);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateRefreshToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = TestDataBuilder.RefreshToken()
            .WithUserId(userId)
            .WithToken("test-refresh-token")
            .WithExpiresAt(DateTime.UtcNow.AddDays(7))
            .WithCreatedAt(DateTime.UtcNow)
            .WithIsActive(true)
            .WithIsRevoked(false)
            .Build();

        // Act
        var result = await _repository.CreateAsync(token);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.UserId.Should().Be(userId);
        result.Token.Should().Be("test-refresh-token");
        result.IsActive.Should().BeTrue();
        result.IsRevoked.Should().BeFalse();

        // Verify it was saved to the database
        var savedToken = await DbContext.RefreshTokens.FindAsync(result.Id);
        savedToken.Should().NotBeNull();
        savedToken!.Token.Should().Be("test-refresh-token");
    }

    [Fact]
    public async Task GetByTokenAsync_WithValidToken_ShouldReturnRefreshToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenString = "test-refresh-token";
        
        var token = TestDataBuilder.RefreshToken()
            .WithUserId(userId)
            .WithToken(tokenString)
            .WithExpiresAt(DateTime.UtcNow.AddDays(7))
            .WithCreatedAt(DateTime.UtcNow)
            .WithIsActive(true)
            .WithIsRevoked(false)
            .Build();

        await DbContext.RefreshTokens.AddAsync(token);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByTokenAsync(tokenString);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().Be(tokenString);
        result.UserId.Should().Be(userId);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetByTokenAsync_WithNonExistentToken_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByTokenAsync("non-existent-token");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveByUserIdAsync_WithActiveToken_ShouldReturnMostRecentActiveToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        
        // Older active token
        var olderToken = TestDataBuilder.RefreshToken()
            .WithUserId(userId)
            .WithToken("older-token")
            .WithExpiresAt(now.AddDays(7))
            .WithCreatedAt(now.AddMinutes(-10))
            .WithIsActive(true)
            .WithIsRevoked(false)
            .Build();

        // Newer active token (should be returned)
        var newerToken = TestDataBuilder.RefreshToken()
            .WithUserId(userId)
            .WithToken("newer-token")
            .WithExpiresAt(now.AddDays(7))
            .WithCreatedAt(now.AddMinutes(-5))
            .WithIsActive(true)
            .WithIsRevoked(false)
            .Build();

        // Revoked token (should not be returned)
        var revokedToken = TestDataBuilder.RefreshToken()
            .WithUserId(userId)
            .WithToken("revoked-token")
            .WithExpiresAt(now.AddDays(7))
            .WithCreatedAt(now.AddMinutes(-1))
            .WithIsActive(false)
            .WithIsRevoked(true)
            .WithRevokedAt(now.AddMinutes(-1))
            .Build();

        await DbContext.RefreshTokens.AddRangeAsync(olderToken, newerToken, revokedToken);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().Be("newer-token");
        result.IsActive.Should().BeTrue();
        result.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public async Task GetActiveByUserIdAsync_WithExpiredTokens_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        
        var expiredToken = TestDataBuilder.RefreshToken()
            .WithUserId(userId)
            .WithToken("expired-token")
            .WithExpiresAt(now.AddDays(-1)) // Expired
            .WithCreatedAt(now.AddDays(-2))
            .WithIsActive(true)
            .WithIsRevoked(false)
            .Build();

        await DbContext.RefreshTokens.AddAsync(expiredToken);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveByUserIdAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveByUserIdListAsync_ShouldReturnAllActiveTokensOrderedByCreatedAtDescending()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        
        var token1 = TestDataBuilder.RefreshToken()
            .WithUserId(userId)
            .WithToken("token-1")
            .WithExpiresAt(now.AddDays(7))
            .WithCreatedAt(now.AddMinutes(-10))
            .WithIsActive(true)
            .WithIsRevoked(false)
            .Build();

        var token2 = TestDataBuilder.RefreshToken()
            .WithUserId(userId)
            .WithToken("token-2")
            .WithExpiresAt(now.AddDays(7))
            .WithCreatedAt(now.AddMinutes(-5))
            .WithIsActive(true)
            .WithIsRevoked(false)
            .Build();

        // Revoked token (should not be included)
        var revokedToken = TestDataBuilder.RefreshToken()
            .WithUserId(userId)
            .WithToken("revoked-token")
            .WithExpiresAt(now.AddDays(7))
            .WithCreatedAt(now.AddMinutes(-1))
            .WithIsActive(false)
            .WithIsRevoked(true)
            .Build();

        await DbContext.RefreshTokens.AddRangeAsync(token1, token2, revokedToken);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveByUserIdListAsync(userId);

        // Assert
        var tokensList = result.ToList();
        tokensList.Should().HaveCount(2);
        tokensList.Should().OnlyContain(t => t.IsActive && !t.IsRevoked);
        
        // Should be ordered by CreatedAt descending
        tokensList[0].Token.Should().Be("token-2"); // Most recent
        tokensList[1].Token.Should().Be("token-1");
    }

    [Fact]
    public async Task UpdateAsync_WithValidToken_ShouldUpdateRefreshToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenString = "test-token";
        
        var existingToken = TestDataBuilder.RefreshToken()
            .WithUserId(userId)
            .WithToken(tokenString)
            .WithExpiresAt(DateTime.UtcNow.AddDays(7))
            .WithCreatedAt(DateTime.UtcNow)
            .WithIsActive(true)
            .WithIsRevoked(false)
            .Build();

        await DbContext.RefreshTokens.AddAsync(existingToken);
        await DbContext.SaveChangesAsync();

        var updateData = TestDataBuilder.RefreshToken()
            .WithToken(tokenString)
            .WithIsActive(false)
            .WithIsRevoked(true)
            .WithRevokedAt(DateTime.UtcNow)
            .WithRevokedReason("Test revocation")
            .WithReplacedByToken("new-token")
            .Build();

        // Act
        var result = await _repository.UpdateAsync(updateData);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().Be(tokenString);
        result.IsActive.Should().BeFalse();
        result.IsRevoked.Should().BeTrue();
        result.RevokedReason.Should().Be("Test revocation");
        result.ReplacedByToken.Should().Be("new-token");

        // Verify changes were persisted
        var updatedToken = await DbContext.RefreshTokens.FindAsync(existingToken.Id);
        updatedToken!.IsActive.Should().BeFalse();
        updatedToken.RevokedReason.Should().Be("Test revocation");
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentToken_ShouldReturnNull()
    {
        // Arrange
        var updateData = TestDataBuilder.RefreshToken()
            .WithToken("non-existent-token")
            .WithIsActive(false)
            .Build();

        // Act
        var result = await _repository.UpdateAsync(updateData);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task RevokeAsync_WithValidToken_ShouldRevokeToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenString = "test-token";
        
        var token = TestDataBuilder.RefreshToken()
            .WithUserId(userId)
            .WithToken(tokenString)
            .WithExpiresAt(DateTime.UtcNow.AddDays(7))
            .WithCreatedAt(DateTime.UtcNow)
            .WithIsActive(true)
            .WithIsRevoked(false)
            .Build();

        await DbContext.RefreshTokens.AddAsync(token);
        await DbContext.SaveChangesAsync();

        var reason = "Manual revocation";
        var beforeRevoke = DateTime.UtcNow;

        // Act
        var result = await _repository.RevokeAsync(tokenString, reason);

        // Assert
        var afterRevoke = DateTime.UtcNow;
        result.Should().BeTrue();

        var revokedToken = await DbContext.RefreshTokens.FindAsync(token.Id);
        revokedToken!.IsActive.Should().BeFalse();
        revokedToken.RevokedReason.Should().Be(reason);
        revokedToken.RevokedAt.Should().NotBeNull();
        revokedToken.RevokedAt.Should().BeOnOrAfter(beforeRevoke);
        revokedToken.RevokedAt.Should().BeOnOrBefore(afterRevoke);
    }

    [Fact]
    public async Task RevokeAsync_WithNonExistentToken_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.RevokeAsync("non-existent-token", "Test reason");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RevokeAllByUserIdAsync_WithActiveTokens_ShouldRevokeAllUserTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        
        var userToken1 = TestDataBuilder.RefreshToken()
            .WithUserId(userId)
            .WithToken("user-token-1")
            .WithExpiresAt(now.AddDays(7))
            .WithCreatedAt(now)
            .WithIsActive(true)
            .WithIsRevoked(false)
            .Build();

        var userToken2 = TestDataBuilder.RefreshToken()
            .WithUserId(userId)
            .WithToken("user-token-2")
            .WithExpiresAt(now.AddDays(7))
            .WithCreatedAt(now)
            .WithIsActive(true)
            .WithIsRevoked(false)
            .Build();

        var otherUserToken = TestDataBuilder.RefreshToken()
            .WithUserId(otherUserId)
            .WithToken("other-user-token")
            .WithExpiresAt(now.AddDays(7))
            .WithCreatedAt(now)
            .WithIsActive(true)
            .WithIsRevoked(false)
            .Build();

        await DbContext.RefreshTokens.AddRangeAsync(userToken1, userToken2, otherUserToken);
        await DbContext.SaveChangesAsync();

        var reason = "Revoke all tokens";
        var beforeRevoke = DateTime.UtcNow;

        // Act
        var result = await _repository.RevokeAllByUserIdAsync(userId, reason);

        // Assert
        var afterRevoke = DateTime.UtcNow;
        result.Should().BeTrue();

        var revokedToken1 = await DbContext.RefreshTokens.FindAsync(userToken1.Id);
        var revokedToken2 = await DbContext.RefreshTokens.FindAsync(userToken2.Id);
        var otherToken = await DbContext.RefreshTokens.FindAsync(otherUserToken.Id);

        // User tokens should be revoked
        revokedToken1!.IsActive.Should().BeFalse();
        revokedToken1.RevokedReason.Should().Be(reason);
        revokedToken1.RevokedAt.Should().BeOnOrAfter(beforeRevoke);
        
        revokedToken2!.IsActive.Should().BeFalse();
        revokedToken2.RevokedReason.Should().Be(reason);
        revokedToken2.RevokedAt.Should().BeOnOrAfter(beforeRevoke);

        // Other user's token should remain active
        otherToken!.IsActive.Should().BeTrue();
        otherToken.RevokedAt.Should().BeNull();
    }

    [Fact]
    public async Task RevokeAllByUserIdAsync_WithNoActiveTokens_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = await _repository.RevokeAllByUserIdAsync(userId, "Test reason");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteExpiredAsync_ShouldDeleteExpiredAndRevokedTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        
        var activeToken = TestDataBuilder.RefreshToken()
            .WithUserId(userId)
            .WithToken("active-token")
            .WithExpiresAt(now.AddDays(7))
            .WithCreatedAt(now)
            .WithIsActive(true)
            .WithIsRevoked(false)
            .Build();

        var expiredToken = TestDataBuilder.RefreshToken()
            .WithUserId(userId)
            .WithToken("expired-token")
            .WithExpiresAt(now.AddDays(-1)) // Expired
            .WithCreatedAt(now.AddDays(-2))
            .WithIsActive(true)
            .WithIsRevoked(false)
            .Build();

        var revokedToken = TestDataBuilder.RefreshToken()
            .WithUserId(userId)
            .WithToken("revoked-token")
            .WithExpiresAt(now.AddDays(7))
            .WithCreatedAt(now)
            .WithIsActive(false)
            .WithIsRevoked(true)
            .WithRevokedAt(now)
            .Build();

        await DbContext.RefreshTokens.AddRangeAsync(activeToken, expiredToken, revokedToken);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteExpiredAsync();

        // Assert
        result.Should().BeTrue();

        var remainingTokens = await DbContext.RefreshTokens.ToListAsync();
        remainingTokens.Should().HaveCount(1);
        remainingTokens[0].Token.Should().Be("active-token");
    }

    [Fact]
    public async Task DeleteExpiredAsync_WithNoExpiredTokens_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var activeToken = TestDataBuilder.RefreshToken()
            .WithUserId(userId)
            .WithToken("active-token")
            .WithExpiresAt(DateTime.UtcNow.AddDays(7))
            .WithCreatedAt(DateTime.UtcNow)
            .WithIsActive(true)
            .WithIsRevoked(false)
            .Build();

        await DbContext.RefreshTokens.AddAsync(activeToken);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteExpiredAsync();

        // Assert
        result.Should().BeFalse();
        
        var remainingTokens = await DbContext.RefreshTokens.ToListAsync();
        remainingTokens.Should().HaveCount(1);
    }

    [Fact]
    public async Task DeleteByUserIdAsync_ShouldDeleteAllUserTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        
        var userToken1 = TestDataBuilder.RefreshToken()
            .WithUserId(userId)
            .WithToken("user-token-1")
            .WithExpiresAt(now.AddDays(7))
            .WithCreatedAt(now)
            .WithIsActive(true)
            .WithIsRevoked(false)
            .Build();

        var userToken2 = TestDataBuilder.RefreshToken()
            .WithUserId(userId)
            .WithToken("user-token-2")
            .WithExpiresAt(now.AddDays(7))
            .WithCreatedAt(now)
            .WithIsActive(false)
            .WithIsRevoked(true)
            .Build();

        var otherUserToken = TestDataBuilder.RefreshToken()
            .WithUserId(otherUserId)
            .WithToken("other-user-token")
            .WithExpiresAt(now.AddDays(7))
            .WithCreatedAt(now)
            .WithIsActive(true)
            .WithIsRevoked(false)
            .Build();

        await DbContext.RefreshTokens.AddRangeAsync(userToken1, userToken2, otherUserToken);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteByUserIdAsync(userId);

        // Assert
        result.Should().BeTrue();

        var remainingTokens = await DbContext.RefreshTokens.ToListAsync();
        remainingTokens.Should().HaveCount(1);
        remainingTokens[0].Token.Should().Be("other-user-token");
    }

    [Fact]
    public async Task DeleteByUserIdAsync_WithNoUserTokens_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = await _repository.DeleteByUserIdAsync(userId);

        // Assert
        result.Should().BeFalse();
    }
}

