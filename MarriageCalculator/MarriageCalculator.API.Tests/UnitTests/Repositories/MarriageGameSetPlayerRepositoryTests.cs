using FluentAssertions;
using MarriageCalculator.API.Repositories.Implementations;
using MarriageCalculator.API.Tests.Helpers;
using MarriageCalculator.Core.Models;
using Xunit;

namespace MarriageCalculator.API.Tests.UnitTests.Repositories;

/// <summary>
/// Unit tests for MarriageGameSetPlayerRepository
/// Tests CRUD operations and business logic for marriage game set players
/// </summary>
public class MarriageGameSetPlayerRepositoryTests : TestBase
{
    private readonly MarriageGameSetPlayerRepository _repository;

    public MarriageGameSetPlayerRepositoryTests()
    {
        _repository = new MarriageGameSetPlayerRepository(DbContext);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllGameSetPlayers()
    {
        // Arrange - Clear any existing data
        DbContext.MarriageGameSetPlayers.RemoveRange(DbContext.MarriageGameSetPlayers);
        DbContext.Players.RemoveRange(DbContext.Players);
        await DbContext.SaveChangesAsync();

        var gameSetPlayer1 = TestDataBuilder.MarriageGameSetPlayer().Build();
        var gameSetPlayer2 = TestDataBuilder.MarriageGameSetPlayer().Build();
        var gameSetPlayer3 = TestDataBuilder.MarriageGameSetPlayer().Build();

        await DbContext.MarriageGameSetPlayers.AddRangeAsync(gameSetPlayer1, gameSetPlayer2, gameSetPlayer3);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoGameSetPlayers_ShouldReturnEmptyList()
    {
        // Arrange - Clear any existing data
        DbContext.MarriageGameSetPlayers.RemoveRange(DbContext.MarriageGameSetPlayers);
        DbContext.Players.RemoveRange(DbContext.Players);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_WithValidIds_ShouldReturnGameSetPlayer()
    {
        // Arrange
        var gameSetPlayer = TestDataBuilder.MarriageGameSetPlayer().Build();
        await DbContext.MarriageGameSetPlayers.AddAsync(gameSetPlayer);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(gameSetPlayer.MarriageGameSetId, gameSetPlayer.PlayerId);

        // Assert
        result.Should().NotBeNull();
        result!.MarriageGameSetId.Should().Be(gameSetPlayer.MarriageGameSetId);
        result.PlayerId.Should().Be(gameSetPlayer.PlayerId);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidIds_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999, Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByGameSetIdAsync_ShouldReturnPlayersForGameSet()
    {
        // Arrange
        var gameSetId = 1;
        var player1 = TestDataBuilder.Player().Build();
        var player2 = TestDataBuilder.Player().Build();
        var player3 = TestDataBuilder.Player().Build();

        await DbContext.Players.AddRangeAsync(player1, player2, player3);
        await DbContext.SaveChangesAsync();

        var gameSetPlayer1 = TestDataBuilder.MarriageGameSetPlayer()
            .WithMarriageGameSetId(gameSetId)
            .WithPlayerId(player1.Id)
            .Build();
        var gameSetPlayer2 = TestDataBuilder.MarriageGameSetPlayer()
            .WithMarriageGameSetId(gameSetId)
            .WithPlayerId(player2.Id)
            .Build();
        var gameSetPlayer3 = TestDataBuilder.MarriageGameSetPlayer()
            .WithMarriageGameSetId(2) // Different game set
            .WithPlayerId(player3.Id)
            .Build();

        await DbContext.MarriageGameSetPlayers.AddRangeAsync(gameSetPlayer1, gameSetPlayer2, gameSetPlayer3);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByGameSetIdAsync(gameSetId);

        // Assert
        var gameSetPlayers = result.ToList();
        gameSetPlayers.Should().HaveCount(2);
        gameSetPlayers.Should().AllSatisfy(gsp => gsp.MarriageGameSetId.Should().Be(gameSetId));
        gameSetPlayers.Should().AllSatisfy(gsp => gsp.Player.Should().NotBeNull());
    }

    [Fact]
    public async Task GetByGameSetIdAsync_WithNoPlayers_ShouldReturnEmptyList()
    {
        // Act
        var result = await _repository.GetByGameSetIdAsync(999);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByPlayerIdAsync_ShouldReturnGameSetsForPlayer()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var gameSetPlayer1 = TestDataBuilder.MarriageGameSetPlayer()
            .WithPlayerId(playerId)
            .WithMarriageGameSetId(1)
            .Build();
        var gameSetPlayer2 = TestDataBuilder.MarriageGameSetPlayer()
            .WithPlayerId(playerId)
            .WithMarriageGameSetId(2)
            .Build();
        var gameSetPlayer3 = TestDataBuilder.MarriageGameSetPlayer()
            .WithPlayerId(Guid.NewGuid()) // Different player
            .WithMarriageGameSetId(1)
            .Build();

        await DbContext.MarriageGameSetPlayers.AddRangeAsync(gameSetPlayer1, gameSetPlayer2, gameSetPlayer3);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByPlayerIdAsync(playerId);

        // Assert
        var gameSetPlayers = result.ToList();
        gameSetPlayers.Should().HaveCount(2);
        gameSetPlayers.Should().AllSatisfy(gsp => gsp.PlayerId.Should().Be(playerId));
    }

    [Fact]
    public async Task GetByPlayerIdAsync_WithNoGameSets_ShouldReturnEmptyList()
    {
        // Act
        var result = await _repository.GetByPlayerIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateGameSetPlayer()
    {
        // Arrange
        var gameSetPlayer = TestDataBuilder.MarriageGameSetPlayer().Build();

        // Act
        var result = await _repository.CreateAsync(gameSetPlayer);

        // Assert
        result.Should().NotBeNull();
        result.MarriageGameSetId.Should().Be(gameSetPlayer.MarriageGameSetId);
        result.PlayerId.Should().Be(gameSetPlayer.PlayerId);

        // Verify it was saved to database
        var saved = await DbContext.MarriageGameSetPlayers
            .FindAsync(result.MarriageGameSetId, result.PlayerId);
        saved.Should().NotBeNull();
    }


    [Fact]
    public async Task DeleteAsync_WithValidIds_ShouldDeleteGameSetPlayer()
    {
        // Arrange
        var gameSetPlayer = TestDataBuilder.MarriageGameSetPlayer().Build();
        await DbContext.MarriageGameSetPlayers.AddAsync(gameSetPlayer);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(gameSetPlayer.MarriageGameSetId, gameSetPlayer.PlayerId);

        // Assert
        result.Should().BeTrue();

        // Verify it was deleted
        var deleted = await DbContext.MarriageGameSetPlayers
            .FindAsync(gameSetPlayer.MarriageGameSetId, gameSetPlayer.PlayerId);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidIds_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.DeleteAsync(999, Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_WithValidIds_ShouldReturnTrue()
    {
        // Arrange
        var gameSetPlayer = TestDataBuilder.MarriageGameSetPlayer().Build();
        await DbContext.MarriageGameSetPlayers.AddAsync(gameSetPlayer);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(gameSetPlayer.MarriageGameSetId, gameSetPlayer.PlayerId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithInvalidIds_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.ExistsAsync(999, Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }
}
