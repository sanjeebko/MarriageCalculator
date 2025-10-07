using FluentAssertions;
using MarriageCalculator.API.Repositories.Implementations;
using MarriageCalculator.API.Tests.Helpers;
using MarriageCalculator.Core.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MarriageCalculator.API.Tests.UnitTests.Repositories;

/// <summary>
/// Unit tests for MarriageGameRepository
/// </summary>
public class MarriageGameRepositoryTests : TestBase
{
    private readonly MarriageGameRepository _repository;

    public MarriageGameRepositoryTests()
    {
        _repository = new MarriageGameRepository(DbContext);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllGamesOrderedByCreatedTimeDescending()
    {
        // Arrange
        var game1 = TestDataBuilder.MarriageGame()
            .WithId(1)
            .WithSequence(1)
            .WithMarriageGameRoundId(1)
            .WithTotalMaal(100)
            .WithCreatedTime(DateTime.UtcNow.AddMinutes(-10))
            .Build();

        var game2 = TestDataBuilder.MarriageGame()
            .WithId(2)
            .WithSequence(2)
            .WithMarriageGameRoundId(1)
            .WithTotalMaal(200)
            .WithCreatedTime(DateTime.UtcNow.AddMinutes(-5))
            .Build();

        var game3 = TestDataBuilder.MarriageGame()
            .WithId(3)
            .WithSequence(3)
            .WithMarriageGameRoundId(2)
            .WithTotalMaal(150)
            .WithCreatedTime(DateTime.UtcNow.AddMinutes(-1))
            .Build();

        await DbContext.MarriageGames.AddRangeAsync(game1, game2, game3);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var gamesList = result.ToList();
        gamesList.Should().HaveCount(3);
        
        // Should be ordered by CreatedTime descending (most recent first)
        gamesList[0].Id.Should().Be(3); // Most recent
        gamesList[1].Id.Should().Be(2);
        gamesList[2].Id.Should().Be(1); // Oldest
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnGame()
    {
        // Arrange
        var winnerId = Guid.NewGuid();
        var dealerId = Guid.NewGuid();
        
        var game = TestDataBuilder.MarriageGame()
            .WithId(1)
            .WithSequence(1)
            .WithMarriageGameRoundId(10)
            .WithWinnerId(winnerId)
            .WithDealerId(dealerId)
            .WithTotalMaal(250)
            .WithClosedRound(true)
            .WithCreatedTime(DateTime.UtcNow)
            .Build();

        await DbContext.MarriageGames.AddAsync(game);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Sequence.Should().Be(1);
        result.MarriageGameRoundId.Should().Be(10);
        result.WinnerId.Should().Be(winnerId);
        result.DealerId.Should().Be(dealerId);
        result.TotalMaal.Should().Be(250);
        result.ClosedRound.Should().BeTrue();
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
    public async Task CreateAsync_WithValidData_ShouldCreateGame()
    {
        // Arrange
        var winnerId = Guid.NewGuid();
        var dealerId = Guid.NewGuid();
        
        var game = TestDataBuilder.MarriageGame()
            .WithSequence(1)
            .WithMarriageGameRoundId(5)
            .WithWinnerId(winnerId)
            .WithDealerId(dealerId)
            .WithTotalMaal(300)
            .WithClosedRound(false)
            .WithCreatedTime(DateTime.UtcNow)
            .Build();

        // Act
        var result = await _repository.CreateAsync(game);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Sequence.Should().Be(1);
        result.MarriageGameRoundId.Should().Be(5);
        result.WinnerId.Should().Be(winnerId);
        result.DealerId.Should().Be(dealerId);
        result.TotalMaal.Should().Be(300);
        result.ClosedRound.Should().BeFalse();

        // Verify it was saved to the database
        var savedGame = await DbContext.MarriageGames.FindAsync(result.Id);
        savedGame.Should().NotBeNull();
        savedGame!.Sequence.Should().Be(1);
        savedGame.TotalMaal.Should().Be(300);
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_ShouldUpdateGame()
    {
        // Arrange
        var existingGame = TestDataBuilder.MarriageGame()
            .WithId(1)
            .WithSequence(1)
            .WithMarriageGameRoundId(5)
            .WithTotalMaal(100)
            .WithClosedRound(false)
            .WithCreatedTime(DateTime.UtcNow)
            .Build();

        await DbContext.MarriageGames.AddAsync(existingGame);
        await DbContext.SaveChangesAsync();

        var winnerId = Guid.NewGuid();
        var dealerId = Guid.NewGuid();
        
        var updateData = TestDataBuilder.MarriageGame()
            .WithSequence(2)
            .WithMarriageGameRoundId(6)
            .WithWinnerId(winnerId)
            .WithDealerId(dealerId)
            .WithTotalMaal(200)
            .WithClosedRound(true)
            .Build();

        // Act
        var result = await _repository.UpdateAsync(1, updateData);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Sequence.Should().Be(2);
        result.MarriageGameRoundId.Should().Be(6);
        result.WinnerId.Should().Be(winnerId);
        result.DealerId.Should().Be(dealerId);
        result.TotalMaal.Should().Be(200);
        result.ClosedRound.Should().BeTrue();

        // Verify changes were persisted
        var updatedGame = await DbContext.MarriageGames.FindAsync(1);
        updatedGame!.Sequence.Should().Be(2);
        updatedGame.TotalMaal.Should().Be(200);
        updatedGame.ClosedRound.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Arrange
        var updateData = TestDataBuilder.MarriageGame()
            .WithSequence(2)
            .WithTotalMaal(200)
            .Build();

        // Act
        var result = await _repository.UpdateAsync(999, updateData);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteGameAsync_WithValidId_ShouldDeleteGame()
    {
        // Arrange
        var game = TestDataBuilder.MarriageGame()
            .WithId(1)
            .WithSequence(1)
            .WithMarriageGameRoundId(5)
            .WithTotalMaal(100)
            .WithCreatedTime(DateTime.UtcNow)
            .Build();

        await DbContext.MarriageGames.AddAsync(game);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteGameAsync(1);

        // Assert
        result.Should().BeTrue();

        // Verify it was deleted from the database
        var deletedGame = await DbContext.MarriageGames.FindAsync(1);
        deletedGame.Should().BeNull();
    }

    [Fact]
    public async Task DeleteGameAsync_WithNonExistentId_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.DeleteGameAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_WithExistingGame_ShouldReturnTrue()
    {
        // Arrange
        var game = TestDataBuilder.MarriageGame()
            .WithId(1)
            .WithSequence(1)
            .WithMarriageGameRoundId(5)
            .WithTotalMaal(100)
            .WithCreatedTime(DateTime.UtcNow)
            .Build();

        await DbContext.MarriageGames.AddAsync(game);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistentGame_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.ExistsAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetByRoundIdAsync_ShouldReturnGamesForRoundOrderedBySequence()
    {
        // Arrange
        var roundId = 5;
        
        var game1 = TestDataBuilder.MarriageGame()
            .WithId(1)
            .WithSequence(3)
            .WithMarriageGameRoundId(roundId)
            .WithTotalMaal(100)
            .WithCreatedTime(DateTime.UtcNow)
            .Build();

        var game2 = TestDataBuilder.MarriageGame()
            .WithId(2)
            .WithSequence(1)
            .WithMarriageGameRoundId(roundId)
            .WithTotalMaal(200)
            .WithCreatedTime(DateTime.UtcNow)
            .Build();

        var game3 = TestDataBuilder.MarriageGame()
            .WithId(3)
            .WithSequence(2)
            .WithMarriageGameRoundId(roundId)
            .WithTotalMaal(150)
            .WithCreatedTime(DateTime.UtcNow)
            .Build();

        // Game from different round
        var gameOtherRound = TestDataBuilder.MarriageGame()
            .WithId(4)
            .WithSequence(1)
            .WithMarriageGameRoundId(10)
            .WithTotalMaal(300)
            .WithCreatedTime(DateTime.UtcNow)
            .Build();

        await DbContext.MarriageGames.AddRangeAsync(game1, game2, game3, gameOtherRound);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByRoundIdAsync(roundId);

        // Assert
        var gamesList = result.ToList();
        gamesList.Should().HaveCount(3);
        gamesList.Should().OnlyContain(g => g.MarriageGameRoundId == roundId);
        
        // Should be ordered by Sequence ascending
        gamesList[0].Sequence.Should().Be(1);
        gamesList[1].Sequence.Should().Be(2);
        gamesList[2].Sequence.Should().Be(3);
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
    public async Task GetByRoundIdAsync_WithNoMatchingGames_ShouldReturnEmptyList()
    {
        // Arrange
        var game = TestDataBuilder.MarriageGame()
            .WithId(1)
            .WithSequence(1)
            .WithMarriageGameRoundId(5)
            .WithTotalMaal(100)
            .WithCreatedTime(DateTime.UtcNow)
            .Build();

        await DbContext.MarriageGames.AddAsync(game);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByRoundIdAsync(999);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAsync_WithNullableFields_ShouldCreateGameWithNullValues()
    {
        // Arrange
        var game = TestDataBuilder.MarriageGame()
            .WithSequence(1)
            .WithMarriageGameRoundId(5)
            .WithWinnerId(null) // Nullable field
            .WithDealerId(null) // Nullable field
            .WithTotalMaal(0)
            .WithClosedRound(false)
            .WithCreatedTime(DateTime.UtcNow)
            .Build();

        // Act
        var result = await _repository.CreateAsync(game);

        // Assert
        result.Should().NotBeNull();
        result.WinnerId.Should().BeNull();
        result.DealerId.Should().BeNull();
        result.TotalMaal.Should().Be(0);

        // Verify it was saved to the database
        var savedGame = await DbContext.MarriageGames.FindAsync(result.Id);
        savedGame.Should().NotBeNull();
        savedGame!.WinnerId.Should().BeNull();
        savedGame.DealerId.Should().BeNull();
    }
}

