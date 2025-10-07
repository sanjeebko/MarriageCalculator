using FluentAssertions;
using MarriageCalculator.API.Repositories.Implementations;
using MarriageCalculator.API.Tests.Helpers;
using MarriageCalculator.Core.Models;
using Xunit;

namespace MarriageCalculator.API.Tests.UnitTests.Repositories;

/// <summary>
/// Unit tests for MarriageGameRoundRepository
/// Tests CRUD operations and business logic for marriage game rounds
/// </summary>
public class MarriageGameRoundRepositoryTests : TestBase
{
    private readonly MarriageGameRoundRepository _repository;

    public MarriageGameRoundRepositoryTests()
    {
        _repository = new MarriageGameRoundRepository(DbContext);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllRounds_OrderedBySequence()
    {
        // Arrange - Clear any existing data
        DbContext.MarriageGameRounds.RemoveRange(DbContext.MarriageGameRounds);
        await DbContext.SaveChangesAsync();

        var round1 = TestDataBuilder.MarriageGameRound().WithSequence(2).Build();
        var round2 = TestDataBuilder.MarriageGameRound().WithSequence(1).Build();
        var round3 = TestDataBuilder.MarriageGameRound().WithSequence(3).Build();

        await DbContext.MarriageGameRounds.AddRangeAsync(round1, round2, round3);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var rounds = result.ToList();
        rounds.Should().HaveCount(3);
        rounds[0].Sequence.Should().Be(1);
        rounds[1].Sequence.Should().Be(2);
        rounds[2].Sequence.Should().Be(3);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoRounds_ShouldReturnEmptyList()
    {
        // Arrange - Clear any existing data
        DbContext.MarriageGameRounds.RemoveRange(DbContext.MarriageGameRounds);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnRound()
    {
        // Arrange
        var round = TestDataBuilder.MarriageGameRound().Build();
        await DbContext.MarriageGameRounds.AddAsync(round);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(round.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(round.Id);
        result.Sequence.Should().Be(round.Sequence);
        result.MarriageGameSetId.Should().Be(round.MarriageGameSetId);
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
    public async Task CreateAsync_ShouldCreateRound()
    {
        // Arrange
        var round = TestDataBuilder.MarriageGameRound().Build();

        // Act
        var result = await _repository.CreateAsync(round);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BePositive();
        result.Sequence.Should().Be(round.Sequence);
        result.MarriageGameSetId.Should().Be(round.MarriageGameSetId);
        result.Completed.Should().Be(round.Completed);

        // Verify it was saved to database
        var saved = await DbContext.MarriageGameRounds.FindAsync(result.Id);
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_WithValidId_ShouldUpdateRound()
    {
        // Arrange
        var round = TestDataBuilder.MarriageGameRound()
            .WithSequence(1)
            .WithCompleted(false)
            .Build();
        await DbContext.MarriageGameRounds.AddAsync(round);
        await DbContext.SaveChangesAsync();

        var updateData = TestDataBuilder.MarriageGameRound()
            .WithSequence(2)
            .WithCompleted(true)
            .Build();

        // Act
        var result = await _repository.UpdateAsync(round.Id, updateData);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(round.Id);
        result.Sequence.Should().Be(2);
        result.Completed.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var updateData = TestDataBuilder.MarriageGameRound().Build();

        // Act
        var result = await _repository.UpdateAsync(999, updateData);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldDeleteRound()
    {
        // Arrange
        var round = TestDataBuilder.MarriageGameRound().Build();
        await DbContext.MarriageGameRounds.AddAsync(round);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(round.Id);

        // Assert
        result.Should().BeTrue();

        // Verify it was deleted
        var deleted = await DbContext.MarriageGameRounds.FindAsync(round.Id);
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
        var round = TestDataBuilder.MarriageGameRound().Build();
        await DbContext.MarriageGameRounds.AddAsync(round);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(round.Id);

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
    public async Task GetByGameSetIdAsync_ShouldReturnRoundsForGameSet()
    {
        // Arrange - Clear any existing data
        DbContext.MarriageGameRounds.RemoveRange(DbContext.MarriageGameRounds);
        await DbContext.SaveChangesAsync();

        var gameSetId = 1;
        var round1 = TestDataBuilder.MarriageGameRound().WithMarriageGameSetId(gameSetId).WithSequence(1).Build();
        var round2 = TestDataBuilder.MarriageGameRound().WithMarriageGameSetId(gameSetId).WithSequence(2).Build();
        var round3 = TestDataBuilder.MarriageGameRound().WithMarriageGameSetId(2).WithSequence(1).Build(); // Different game set

        await DbContext.MarriageGameRounds.AddRangeAsync(round1, round2, round3);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByGameSetIdAsync(gameSetId);

        // Assert
        var rounds = result.ToList();
        rounds.Should().HaveCount(2);
        rounds.Should().AllSatisfy(r => r.MarriageGameSetId.Should().Be(gameSetId));
        rounds.Should().BeInAscendingOrder(r => r.Sequence);
    }

    [Fact]
    public async Task GetByGameSetIdAsync_WithNoRounds_ShouldReturnEmptyList()
    {
        // Act
        var result = await _repository.GetByGameSetIdAsync(999);

        // Assert
        result.Should().BeEmpty();
    }
}
