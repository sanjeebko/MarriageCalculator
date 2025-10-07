using FluentAssertions;
using MarriageCalculator.API.Repositories.Implementations;
using MarriageCalculator.API.Tests.Helpers;
using MarriageCalculator.Core.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MarriageCalculator.API.Tests.UnitTests.Repositories;

/// <summary>
/// Unit tests for MarriageGameSetRepository
/// </summary>
public class MarriageGameSetRepositoryTests : TestBase
{
    private readonly MarriageGameSetRepository _repository;

    public MarriageGameSetRepositoryTests()
    {
        _repository = new MarriageGameSetRepository(DbContext);
    }

    [Fact]
    public async Task GetActiveByGameSettingsIdAsync_WithActiveGameSet_ShouldReturnActiveGameSet()
    {
        // Arrange
        var gameSettingsId = 1;
        var activeGameSet = TestDataBuilder.GameSet()
            .WithName("Active Game Set")
            .WithGameSettingsId(gameSettingsId)
            .WithIsActive(true)
            .WithCreated(DateTime.UtcNow.AddDays(-1))
            .Build();

        var inactiveGameSet = TestDataBuilder.GameSet()
            .WithName("Inactive Game Set")
            .WithGameSettingsId(gameSettingsId)
            .WithIsActive(false)
            .WithCreated(DateTime.UtcNow.AddDays(-2))
            .Build();

        await DbContext.MarriageGameSets.AddRangeAsync(activeGameSet, inactiveGameSet);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveByGameSettingsIdAsync(gameSettingsId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(activeGameSet.Id);
        result.Name.Should().Be("Active Game Set");
        result.IsActive.Should().BeTrue();
        result.GameSettingsId.Should().Be(gameSettingsId);
    }

    [Fact]
    public async Task GetActiveByGameSettingsIdAsync_WithNoActiveGameSet_ShouldReturnNull()
    {
        // Arrange
        var gameSettingsId = 1;
        var inactiveGameSet = TestDataBuilder.GameSet()
            .WithName("Inactive Game Set")
            .WithGameSettingsId(gameSettingsId)
            .WithIsActive(false)
            .Build();

        await DbContext.MarriageGameSets.AddAsync(inactiveGameSet);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveByGameSettingsIdAsync(gameSettingsId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveByGameSettingsIdAsync_WithDifferentGameSettingsId_ShouldReturnNull()
    {
        // Arrange
        var targetGameSettingsId = 1;
        var otherGameSettingsId = 2;

        var activeGameSetForOtherSettings = TestDataBuilder.GameSet()
            .WithName("Active Game Set for Other Settings")
            .WithGameSettingsId(otherGameSettingsId)
            .WithIsActive(true)
            .Build();

        await DbContext.MarriageGameSets.AddAsync(activeGameSetForOtherSettings);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveByGameSettingsIdAsync(targetGameSettingsId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WithValidGameSet_ShouldCreateAndReturnGameSet()
    {
        // Arrange
        var gameSet = TestDataBuilder.GameSet()
            .WithName("New Game Set")
            .WithGameSettingsId(1)
            .WithIsActive(true)
            .WithCreated(DateTime.UtcNow)
            .WithLastPlayed(DateTime.UtcNow)
            .Build();

        // Act
        var result = await _repository.CreateAsync(gameSet);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0); // Should have been assigned an ID
        result.Name.Should().Be("New Game Set");
        result.GameSettingsId.Should().Be(1);
        result.IsActive.Should().BeTrue();

        // Verify it was actually saved to the database
        var savedGameSet = await DbContext.MarriageGameSets.FindAsync(result.Id);
        savedGameSet.Should().NotBeNull();
        savedGameSet!.Name.Should().Be("New Game Set");
    }

    [Fact]
    public async Task GetByGameSettingsIdAsync_WithMultipleGameSets_ShouldReturnOrderedByCreatedDesc()
    {
        // Arrange
        var gameSettingsId = 1;
        var oldGameSet = TestDataBuilder.GameSet()
            .WithName("Old Game Set")
            .WithGameSettingsId(gameSettingsId)
            .WithCreated(DateTime.UtcNow.AddDays(-3))
            .Build();

        var newGameSet = TestDataBuilder.GameSet()
            .WithName("New Game Set")
            .WithGameSettingsId(gameSettingsId)
            .WithCreated(DateTime.UtcNow.AddDays(-1))
            .Build();

        var middleGameSet = TestDataBuilder.GameSet()
            .WithName("Middle Game Set")
            .WithGameSettingsId(gameSettingsId)
            .WithCreated(DateTime.UtcNow.AddDays(-2))
            .Build();

        await DbContext.MarriageGameSets.AddRangeAsync(oldGameSet, newGameSet, middleGameSet);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByGameSettingsIdAsync(gameSettingsId);

        // Assert
        var gameSetsList = result.ToList();
        gameSetsList.Should().HaveCount(3);
        gameSetsList[0].Name.Should().Be("New Game Set");    // Most recent
        gameSetsList[1].Name.Should().Be("Middle Game Set"); // Middle
        gameSetsList[2].Name.Should().Be("Old Game Set");    // Oldest
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnGameSet()
    {
        // Arrange
        var gameSet = TestDataBuilder.GameSet()
            .WithName("Test Game Set")
            .WithGameSettingsId(1)
            .Build();

        await DbContext.MarriageGameSets.AddAsync(gameSet);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(gameSet.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(gameSet.Id);
        result.Name.Should().Be("Test Game Set");
        result.GameSettingsId.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var invalidId = 999;

        // Act
        var result = await _repository.GetByIdAsync(invalidId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_ShouldUpdateGameSet()
    {
        // Arrange
        var existingGameSet = TestDataBuilder.GameSet()
            .WithName("Original Name")
            .WithGameSettingsId(1)
            .WithIsActive(true)
            .WithLastPlayed(DateTime.UtcNow.AddDays(-1))
            .Build();

        await DbContext.MarriageGameSets.AddAsync(existingGameSet);
        await DbContext.SaveChangesAsync();

        var updateData = TestDataBuilder.GameSet()
            .WithName("Updated Name")
            .WithGameSettingsId(2)
            .WithIsActive(false)
            .WithLastPlayed(DateTime.UtcNow)
            .Build();

        // Act
        var result = await _repository.UpdateAsync(existingGameSet.Id, updateData);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(existingGameSet.Id);
        result.Name.Should().Be("Updated Name");
        result.GameSettingsId.Should().Be(2);
        result.IsActive.Should().BeFalse();

        // Verify changes were persisted
        var updatedGameSet = await DbContext.MarriageGameSets.FindAsync(existingGameSet.Id);
        updatedGameSet!.Name.Should().Be("Updated Name");
        updatedGameSet.GameSettingsId.Should().Be(2);
        updatedGameSet.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var invalidId = 999;
        var updateData = TestDataBuilder.GameSet()
            .WithName("Updated Name")
            .Build();

        // Act
        var result = await _repository.UpdateAsync(invalidId, updateData);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldDeleteAndReturnTrue()
    {
        // Arrange
        var gameSet = TestDataBuilder.GameSet()
            .WithName("Game Set to Delete")
            .WithGameSettingsId(1)
            .Build();

        await DbContext.MarriageGameSets.AddAsync(gameSet);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(gameSet.Id);

        // Assert
        result.Should().BeTrue();

        // Verify it was actually deleted
        var deletedGameSet = await DbContext.MarriageGameSets.FindAsync(gameSet.Id);
        deletedGameSet.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        var invalidId = 999;

        // Act
        var result = await _repository.DeleteAsync(invalidId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_WithExistingId_ShouldReturnTrue()
    {
        // Arrange
        var gameSet = TestDataBuilder.GameSet()
            .WithName("Existing Game Set")
            .WithGameSettingsId(1)
            .Build();

        await DbContext.MarriageGameSets.AddAsync(gameSet);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(gameSet.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        var nonExistingId = 999;

        // Act
        var result = await _repository.ExistsAsync(nonExistingId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetLatestActiveAsync_WithActiveGameSets_ShouldReturnMostRecentlyPlayed()
    {
        // Arrange
        var oldActiveGameSet = TestDataBuilder.GameSet()
            .WithName("Old Active Game Set")
            .WithGameSettingsId(1)
            .WithIsActive(true)
            .WithLastPlayed(DateTime.UtcNow.AddHours(-3))
            .Build();

        var recentActiveGameSet = TestDataBuilder.GameSet()
            .WithName("Recent Active Game Set")
            .WithGameSettingsId(2)
            .WithIsActive(true)
            .WithLastPlayed(DateTime.UtcNow.AddHours(-1))
            .Build();

        var inactiveGameSet = TestDataBuilder.GameSet()
            .WithName("Inactive Game Set")
            .WithGameSettingsId(3)
            .WithIsActive(false)
            .WithLastPlayed(DateTime.UtcNow) // Even though this is most recent, it's inactive
            .Build();

        await DbContext.MarriageGameSets.AddRangeAsync(oldActiveGameSet, recentActiveGameSet, inactiveGameSet);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetLatestActiveAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(recentActiveGameSet.Id);
        result.Name.Should().Be("Recent Active Game Set");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetLatestActiveAsync_WithNoActiveGameSets_ShouldReturnNull()
    {
        // Arrange
        var inactiveGameSet = TestDataBuilder.GameSet()
            .WithName("Inactive Game Set")
            .WithGameSettingsId(1)
            .WithIsActive(false)
            .Build();

        await DbContext.MarriageGameSets.AddAsync(inactiveGameSet);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetLatestActiveAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetLatestActiveForUserAsync_WithUserActiveGameSets_ShouldReturnMostRecent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        // Create game settings for both users
        var userGameSettings = TestDataBuilder.GameSettings()
            .WithUserId(userId)
            .Build();
        var otherUserGameSettings = TestDataBuilder.GameSettings()
            .WithUserId(otherUserId)
            .Build();

        await DbContext.GameSettings.AddRangeAsync(userGameSettings, otherUserGameSettings);
        await DbContext.SaveChangesAsync();

        // Create game sets for the current user
        var oldActiveGameSet = TestDataBuilder.GameSet()
            .WithName("Old Active Game Set")
            .WithGameSettingsId(userGameSettings.Id)
            .WithIsActive(true)
            .WithLastPlayed(DateTime.UtcNow.AddDays(-2))
            .Build();

        var recentActiveGameSet = TestDataBuilder.GameSet()
            .WithName("Recent Active Game Set")
            .WithGameSettingsId(userGameSettings.Id)
            .WithIsActive(true)
            .WithLastPlayed(DateTime.UtcNow.AddHours(-1))
            .Build();

        // Create game set for another user (should not be returned)
        var otherUserGameSet = TestDataBuilder.GameSet()
            .WithName("Other User Game Set")
            .WithGameSettingsId(otherUserGameSettings.Id)
            .WithIsActive(true)
            .WithLastPlayed(DateTime.UtcNow) // Most recent, but different user
            .Build();

        await DbContext.MarriageGameSets.AddRangeAsync(oldActiveGameSet, recentActiveGameSet, otherUserGameSet);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetLatestActiveForUserAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(recentActiveGameSet.Id);
        result.Name.Should().Be("Recent Active Game Set");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetLatestActiveForUserAsync_WithNoUserGameSets_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        // Create game settings for another user
        var otherUserGameSettings = TestDataBuilder.GameSettings()
            .WithUserId(otherUserId)
            .Build();

        await DbContext.GameSettings.AddAsync(otherUserGameSettings);
        await DbContext.SaveChangesAsync();

        // Create game set for another user only
        var otherUserGameSet = TestDataBuilder.GameSet()
            .WithName("Other User Game Set")
            .WithGameSettingsId(otherUserGameSettings.Id)
            .WithIsActive(true)
            .Build();

        await DbContext.MarriageGameSets.AddAsync(otherUserGameSet);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetLatestActiveForUserAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetLatestActiveForUserAsync_WithOnlyInactiveUserGameSets_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Create game settings for the user
        var userGameSettings = TestDataBuilder.GameSettings()
            .WithUserId(userId)
            .Build();

        await DbContext.GameSettings.AddAsync(userGameSettings);
        await DbContext.SaveChangesAsync();

        // Create inactive game set for the user
        var inactiveGameSet = TestDataBuilder.GameSet()
            .WithName("Inactive Game Set")
            .WithGameSettingsId(userGameSettings.Id)
            .WithIsActive(false)
            .Build();

        await DbContext.MarriageGameSets.AddAsync(inactiveGameSet);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetLatestActiveForUserAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleGameSets_ShouldReturnAllOrderedByCreatedDesc()
    {
        // Arrange
        var gameSet1 = TestDataBuilder.GameSet()
            .WithName("Game Set 1")
            .WithGameSettingsId(1)
            .WithCreated(DateTime.UtcNow.AddDays(-3))
            .Build();

        var gameSet2 = TestDataBuilder.GameSet()
            .WithName("Game Set 2")
            .WithGameSettingsId(2)
            .WithCreated(DateTime.UtcNow.AddDays(-1))
            .Build();

        await DbContext.MarriageGameSets.AddRangeAsync(gameSet1, gameSet2);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var gameSetsList = result.ToList();
        gameSetsList.Should().HaveCount(2);
        gameSetsList[0].Name.Should().Be("Game Set 2"); // More recent
        gameSetsList[1].Name.Should().Be("Game Set 1"); // Older
    }
}
