using FluentAssertions;
using MarriageCalculator.API.Repositories.Implementations;
using MarriageCalculator.API.Tests.Helpers;
using MarriageCalculator.Core.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MarriageCalculator.API.Tests.UnitTests.Repositories;

/// <summary>
/// Unit tests for GameSettingsRepository
/// </summary>
public class GameSettingsRepositoryTests : TestBase
{
    private readonly GameSettingsRepository _repository;

    public GameSettingsRepositoryTests()
    {
        _repository = new GameSettingsRepository(DbContext);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllGameSettings()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        
        var settings1 = TestDataBuilder.GameSettings()
            .WithId(1)
            .WithUserId(userId1)
            .WithMurder(true)
            .Build();

        var settings2 = TestDataBuilder.GameSettings()
            .WithId(2)
            .WithUserId(userId2)
            .WithMurder(false)
            .Build();

        await DbContext.GameSettings.AddRangeAsync(settings1, settings2);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var settingsList = result.ToList();
        settingsList.Should().HaveCount(2);
        settingsList.Should().Contain(s => s.Id == 1 && s.UserId == userId1);
        settingsList.Should().Contain(s => s.Id == 2 && s.UserId == userId2);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnSettingsForSpecificUser()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        
        var user1Settings1 = TestDataBuilder.GameSettings()
            .WithId(1)
            .WithUserId(userId1)
            .WithMurder(true)
            .Build();

        var user1Settings2 = TestDataBuilder.GameSettings()
            .WithId(2)
            .WithUserId(userId1)
            .WithMurder(false)
            .Build();

        var user2Settings = TestDataBuilder.GameSettings()
            .WithId(3)
            .WithUserId(userId2)
            .WithMurder(true)
            .Build();

        await DbContext.GameSettings.AddRangeAsync(user1Settings1, user1Settings2, user2Settings);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdAsync(userId1);

        // Assert
        var settingsList = result.ToList();
        settingsList.Should().HaveCount(2);
        settingsList.Should().OnlyContain(s => s.UserId == userId1);
        settingsList.Should().Contain(s => s.Id == 1);
        settingsList.Should().Contain(s => s.Id == 2);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnGameSettings()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var settings = TestDataBuilder.GameSettings()
            .WithId(1)
            .WithUserId(userId)
            .WithMurder(true)
            .Build();

        await DbContext.GameSettings.AddAsync(settings);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.UserId.Should().Be(userId);
        result.Murder.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateGameSettings()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var settings = TestDataBuilder.GameSettings()
            .WithUserId(userId)
            .WithMurder(true)
            .Build();

        // Act
        var result = await _repository.CreateAsync(settings);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.UserId.Should().Be(userId);
        result.Murder.Should().BeTrue();

        // Verify it was saved to the database
        var savedSettings = await DbContext.GameSettings.FindAsync(result.Id);
        savedSettings.Should().NotBeNull();
        savedSettings!.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_ShouldUpdateGameSettings()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingSettings = TestDataBuilder.GameSettings()
            .WithId(1)
            .WithUserId(userId)
            .WithMurder(false)
            .Build();

        await DbContext.GameSettings.AddAsync(existingSettings);
        await DbContext.SaveChangesAsync();

        var updateData = TestDataBuilder.GameSettings()
            .WithMurder(true)
            .Build();

        // Act
        var result = await _repository.UpdateAsync(1, updateData);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Murder.Should().BeTrue();

        // Verify changes were persisted
        var updatedSettings = await DbContext.GameSettings.FindAsync(1);
        updatedSettings!.Murder.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Arrange
        var updateData = TestDataBuilder.GameSettings()
            .WithMurder(true)
            .Build();

        // Act
        var result = await _repository.UpdateAsync(999, updateData);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldDeleteGameSettings()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var settings = TestDataBuilder.GameSettings()
            .WithId(1)
            .WithUserId(userId)
            .WithMurder(true)
            .Build();

        await DbContext.GameSettings.AddAsync(settings);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(1);

        // Assert
        result.Should().BeTrue();

        // Verify it was deleted from the database
        var deletedSettings = await DbContext.GameSettings.FindAsync(1);
        deletedSettings.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_WithExistingSettings_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var settings = TestDataBuilder.GameSettings()
            .WithId(1)
            .WithUserId(userId)
            .Build();

        await DbContext.GameSettings.AddAsync(settings);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistentSettings_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.ExistsAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllAsync_WithEmptyDatabase_ShouldReturnEmptyList()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByUserIdAsync_WithNoMatchingSettings_ShouldReturnEmptyList()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        var otherUserSettings = TestDataBuilder.GameSettings()
            .WithId(1)
            .WithUserId(userId2)
            .Build();

        await DbContext.GameSettings.AddAsync(otherUserSettings);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdAsync(userId1);

        // Assert
        result.Should().BeEmpty();
    }
}

