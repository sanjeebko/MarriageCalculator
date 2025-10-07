using FluentAssertions;
using MarriageCalculator.API.Repositories.Implementations;
using MarriageCalculator.API.Tests.Helpers;
using MarriageCalculator.Core.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MarriageCalculator.API.Tests.UnitTests.Repositories;

/// <summary>
/// Unit tests for DatabaseRepository
/// </summary>
public class DatabaseRepositoryTests : TestBase
{
    private readonly DatabaseRepository _repository;

    public DatabaseRepositoryTests()
    {
        _repository = new DatabaseRepository(DbContext);
    }

    [Fact]
    public async Task CanConnectAsync_WithValidContext_ShouldReturnTrue()
    {
        // Act
        var result = await _repository.CanConnectAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetTableCountAsync_WithEmptyDatabase_ShouldReturnZero()
    {
        // Act
        var result = await _repository.GetTableCountAsync();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task GetTableCountAsync_WithDataInTables_ShouldReturnCorrectCount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        // Add test data to different tables
        var player = TestDataBuilder.Player()
            .WithName("Test Player")
            .WithEmail("test@example.com")
            .WithDeleted(false)
            .Build();

        var gameSettings = TestDataBuilder.GameSettings()
            .WithUserId(userId)
            .WithMurder(true)
            .Build();

        var gameSet = TestDataBuilder.GameSet()
            .WithName("Test Game Set")
            .WithGameSettingsId(1)
            .WithIsActive(true)
            .WithCreated(DateTime.UtcNow)
            .WithLastPlayed(DateTime.UtcNow)
            .Build();

        var game = TestDataBuilder.MarriageGame()
            .WithSequence(1)
            .WithMarriageGameRoundId(1)
            .WithTotalMaal(100)
            .WithClosedRound(false)
            .WithCreatedTime(DateTime.UtcNow)
            .Build();

        await DbContext.Players.AddAsync(player);
        await DbContext.GameSettings.AddAsync(gameSettings);
        await DbContext.MarriageGameSets.AddAsync(gameSet);
        await DbContext.MarriageGames.AddAsync(game);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetTableCountAsync();

        // Assert
        result.Should().Be(4); // 1 player + 1 settings + 1 game set + 1 game = 4
    }

    [Fact]
    public async Task GetTableCountAsync_WithMultipleRecordsInSameTable_ShouldCountAll()
    {
        // Arrange
        var players = new[]
        {
            TestDataBuilder.Player()
                .WithName("Player 1")
                .WithEmail("player1@example.com")
                .WithDeleted(false)
                .Build(),
            TestDataBuilder.Player()
                .WithName("Player 2")
                .WithEmail("player2@example.com")
                .WithDeleted(false)
                .Build(),
            TestDataBuilder.Player()
                .WithName("Player 3")
                .WithEmail("player3@example.com")
                .WithDeleted(false)
                .Build()
        };

        await DbContext.Players.AddRangeAsync(players);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetTableCountAsync();

        // Assert
        result.Should().Be(3); // 3 players
    }

    [Fact]
    public async Task GetTableCountAsync_WithDeletedRecords_ShouldStillCountThem()
    {
        // Arrange
        var deletedPlayer = TestDataBuilder.Player()
            .WithName("Deleted Player")
            .WithEmail("deleted@example.com")
            .WithDeleted(true)
            .Build();

        var activePlayer = TestDataBuilder.Player()
            .WithName("Active Player")
            .WithEmail("active@example.com")
            .WithDeleted(false)
            .Build();

        await DbContext.Players.AddRangeAsync(deletedPlayer, activePlayer);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetTableCountAsync();

        // Assert
        result.Should().Be(2); // Both deleted and active players are counted
    }

    [Fact]
    public async Task GetProviderNameAsync_ShouldReturnProviderName()
    {
        // Act
        var result = await _repository.GetProviderNameAsync();

        // Assert
        result.Should().NotBeNullOrEmpty();
        // In-memory database provider name
        result.Should().Contain("InMemory");
    }

    [Fact]
    public async Task GetTableCountAsync_WithMixedData_ShouldReturnCorrectTotal()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        
        // Add 2 players
        var players = new[]
        {
            TestDataBuilder.Player()
                .WithName("Player 1")
                .WithEmail("player1@example.com")
                .WithDeleted(false)
                .Build(),
            TestDataBuilder.Player()
                .WithName("Player 2")
                .WithEmail("player2@example.com")
                .WithDeleted(false)
                .Build()
        };

        // Add 3 game settings
        var gameSettings = new[]
        {
            TestDataBuilder.GameSettings()
                .WithUserId(userId1)
                .WithMurder(true)
                .Build(),
            TestDataBuilder.GameSettings()
                .WithUserId(userId2)
                .WithMurder(false)
                .Build(),
            TestDataBuilder.GameSettings()
                .WithUserId(userId1)
                .WithMurder(true)
                .Build()
        };

        // Add 1 game set
        var gameSet = TestDataBuilder.GameSet()
            .WithName("Test Game Set")
            .WithGameSettingsId(1)
            .WithIsActive(true)
            .WithCreated(DateTime.UtcNow)
            .WithLastPlayed(DateTime.UtcNow)
            .Build();

        // Add 2 games
        var games = new[]
        {
            TestDataBuilder.MarriageGame()
                .WithSequence(1)
                .WithMarriageGameRoundId(1)
                .WithTotalMaal(100)
                .WithCreatedTime(DateTime.UtcNow)
                .Build(),
            TestDataBuilder.MarriageGame()
                .WithSequence(2)
                .WithMarriageGameRoundId(1)
                .WithTotalMaal(200)
                .WithCreatedTime(DateTime.UtcNow)
                .Build()
        };

        await DbContext.Players.AddRangeAsync(players);
        await DbContext.GameSettings.AddRangeAsync(gameSettings);
        await DbContext.MarriageGameSets.AddAsync(gameSet);
        await DbContext.MarriageGames.AddRangeAsync(games);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetTableCountAsync();

        // Assert
        result.Should().Be(8); // 2 players + 3 settings + 1 game set + 2 games = 8
    }
}

