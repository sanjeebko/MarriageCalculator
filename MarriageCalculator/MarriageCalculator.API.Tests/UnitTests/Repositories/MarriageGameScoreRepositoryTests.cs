using FluentAssertions;
using MarriageCalculator.API.Repositories.Implementations;
using MarriageCalculator.API.Tests.Helpers;
using MarriageCalculator.Core.Models;
using Xunit;

namespace MarriageCalculator.API.Tests.UnitTests.Repositories;

/// <summary>
/// Unit tests for MarriageGameScoreRepository
/// Tests CRUD operations and business logic for marriage game scores
/// </summary>
public class MarriageGameScoreRepositoryTests : TestBase
{
    private readonly MarriageGameScoreRepository _repository;

    public MarriageGameScoreRepositoryTests()
    {
        _repository = new MarriageGameScoreRepository(DbContext);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllScores_OrderedByGameIdThenPosition()
    {
        // Arrange - Clear any existing data
        DbContext.MarriageGameScores.RemoveRange(DbContext.MarriageGameScores);
        await DbContext.SaveChangesAsync();

        var score1 = TestDataBuilder.MarriageGameScore().WithMarriageGameId(2).WithPosition(1).Build();
        var score2 = TestDataBuilder.MarriageGameScore().WithMarriageGameId(1).WithPosition(2).Build();
        var score3 = TestDataBuilder.MarriageGameScore().WithMarriageGameId(1).WithPosition(1).Build();

        await DbContext.MarriageGameScores.AddRangeAsync(score1, score2, score3);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var scores = result.ToList();
        scores.Should().HaveCount(3);
        scores[0].MarriageGameId.Should().Be(1);
        scores[0].Position.Should().Be(1);
        scores[1].MarriageGameId.Should().Be(1);
        scores[1].Position.Should().Be(2);
        scores[2].MarriageGameId.Should().Be(2);
        scores[2].Position.Should().Be(1);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoScores_ShouldReturnEmptyList()
    {
        // Arrange - Clear any existing data
        DbContext.MarriageGameScores.RemoveRange(DbContext.MarriageGameScores);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnScore()
    {
        // Arrange
        var score = TestDataBuilder.MarriageGameScore().Build();
        await DbContext.MarriageGameScores.AddAsync(score);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(score.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(score.Id);
        result.MarriageGameId.Should().Be(score.MarriageGameId);
        result.PlayerId.Should().Be(score.PlayerId);
        result.Score.Should().Be(score.Score);
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
    public async Task CreateAsync_ShouldCreateScore()
    {
        // Arrange
        var score = TestDataBuilder.MarriageGameScore().Build();

        // Act
        var result = await _repository.CreateAsync(score);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BePositive();
        result.MarriageGameId.Should().Be(score.MarriageGameId);
        result.PlayerId.Should().Be(score.PlayerId);
        result.Seen.Should().Be(score.Seen);
        result.Playing.Should().Be(score.Playing);
        result.Score.Should().Be(score.Score);

        // Verify it was saved to database
        var saved = await DbContext.MarriageGameScores.FindAsync(result.Id);
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_WithValidId_ShouldUpdateScore()
    {
        // Arrange
        var score = TestDataBuilder.MarriageGameScore()
            .WithSeen(false)
            .WithPlaying(false)
            .WithScore(10)
            .Build();
        await DbContext.MarriageGameScores.AddAsync(score);
        await DbContext.SaveChangesAsync();

        var updateData = TestDataBuilder.MarriageGameScore()
            .WithSeen(true)
            .WithPlaying(true)
            .WithScore(50)
            .Build();

        // Act
        var result = await _repository.UpdateAsync(score.Id, updateData);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(score.Id);
        result.Seen.Should().BeTrue();
        result.Playing.Should().BeTrue();
        result.Score.Should().Be(50);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var updateData = TestDataBuilder.MarriageGameScore().Build();

        // Act
        var result = await _repository.UpdateAsync(999, updateData);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldDeleteScore()
    {
        // Arrange
        var score = TestDataBuilder.MarriageGameScore().Build();
        await DbContext.MarriageGameScores.AddAsync(score);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(score.Id);

        // Assert
        result.Should().BeTrue();

        // Verify it was deleted
        var deleted = await DbContext.MarriageGameScores.FindAsync(score.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        var score = TestDataBuilder.MarriageGameScore().Build();
        await DbContext.MarriageGameScores.AddAsync(score);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(score.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.ExistsAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetByGameIdAsync_ShouldReturnScoresForGame()
    {
        // Arrange - Clear any existing data
        DbContext.MarriageGameScores.RemoveRange(DbContext.MarriageGameScores);
        await DbContext.SaveChangesAsync();

        var gameId = 1;
        var score1 = TestDataBuilder.MarriageGameScore().WithMarriageGameId(gameId).WithPosition(1).Build();
        var score2 = TestDataBuilder.MarriageGameScore().WithMarriageGameId(gameId).WithPosition(2).Build();
        var score3 = TestDataBuilder.MarriageGameScore().WithMarriageGameId(2).WithPosition(1).Build(); // Different game

        await DbContext.MarriageGameScores.AddRangeAsync(score1, score2, score3);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByGameIdAsync(gameId);

        // Assert
        var scores = result.ToList();
        scores.Should().HaveCount(2);
        scores.Should().AllSatisfy(s => s.MarriageGameId.Should().Be(gameId));
        scores.Should().BeInAscendingOrder(s => s.Position);
    }

    [Fact]
    public async Task GetByGameIdAsync_WithNoScores_ShouldReturnEmptyList()
    {
        // Act
        var result = await _repository.GetByGameIdAsync(999);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByPlayerIdAsync_ShouldReturnScoresForPlayer()
    {
        // Arrange - Clear any existing data
        DbContext.MarriageGameScores.RemoveRange(DbContext.MarriageGameScores);
        await DbContext.SaveChangesAsync();

        var playerId = Guid.NewGuid();
        var score1 = TestDataBuilder.MarriageGameScore().WithPlayerId(playerId).WithMarriageGameId(1).Build();
        var score2 = TestDataBuilder.MarriageGameScore().WithPlayerId(playerId).WithMarriageGameId(2).Build();
        var score3 = TestDataBuilder.MarriageGameScore().WithPlayerId(Guid.NewGuid()).WithMarriageGameId(1).Build(); // Different player

        await DbContext.MarriageGameScores.AddRangeAsync(score1, score2, score3);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByPlayerIdAsync(playerId);

        // Assert
        var scores = result.ToList();
        scores.Should().HaveCount(2);
        scores.Should().AllSatisfy(s => s.PlayerId.Should().Be(playerId));
    }

    [Fact]
    public async Task GetByPlayerIdAsync_WithNoScores_ShouldReturnEmptyList()
    {
        // Act
        var result = await _repository.GetByPlayerIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeEmpty();
    }
}
