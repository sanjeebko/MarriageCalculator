using FluentAssertions;
using MarriageCalculator.API.Repositories.Implementations;
using MarriageCalculator.API.Tests.Helpers;
using MarriageCalculator.Core.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MarriageCalculator.API.Tests.UnitTests.Repositories;

/// <summary>
/// Unit tests for PlayerRepository
/// </summary>
public class PlayerRepositoryTests : TestBase
{
    private readonly PlayerRepository _repository;

    public PlayerRepositoryTests()
    {
        _repository = new PlayerRepository(DbContext);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnOnlyNonDeletedPlayersOrderedByName()
    {
        // Arrange
        var player1 = TestDataBuilder.Player()
            .WithName("Charlie")
            .WithEmail("charlie@test.com")
            .WithDeleted(false)
            .Build();

        var player2 = TestDataBuilder.Player()
            .WithName("Alice")
            .WithEmail("alice@test.com")
            .WithDeleted(false)
            .Build();

        var deletedPlayer = TestDataBuilder.Player()
            .WithName("Bob")
            .WithEmail("bob@test.com")
            .WithDeleted(true)
            .Build();

        await DbContext.Players.AddRangeAsync(player1, player2, deletedPlayer);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var playersList = result.ToList();
        playersList.Should().HaveCount(2);
        playersList[0].Name.Should().Be("Alice"); // Ordered alphabetically
        playersList[1].Name.Should().Be("Charlie");
        playersList.Should().NotContain(p => p.Deleted);
    }

    [Fact]
    public async Task GetByCreatorAsync_ShouldReturnPlayersCreatedBySpecificUser()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        var user1Player1 = TestDataBuilder.Player()
            .WithName("User1 Player1")
            .WithEmail("u1p1@test.com")
            .WithCreatedByUserId(userId1)
            .WithDeleted(false)
            .Build();

        var user1Player2 = TestDataBuilder.Player()
            .WithName("User1 Player2")
            .WithEmail("u1p2@test.com")
            .WithCreatedByUserId(userId1)
            .WithDeleted(false)
            .Build();

        var user2Player = TestDataBuilder.Player()
            .WithName("User2 Player")
            .WithEmail("u2p@test.com")
            .WithCreatedByUserId(userId2)
            .WithDeleted(false)
            .Build();

        var deletedPlayer = TestDataBuilder.Player()
            .WithName("Deleted Player")
            .WithEmail("deleted@test.com")
            .WithCreatedByUserId(userId1)
            .WithDeleted(true)
            .Build();

        await DbContext.Players.AddRangeAsync(user1Player1, user1Player2, user2Player, deletedPlayer);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByCreatorAsync(userId1);

        // Assert
        var playersList = result.ToList();
        playersList.Should().HaveCount(2);
        playersList.Should().OnlyContain(p => p.CreatedByUserId == userId1);
        playersList.Should().OnlyContain(p => !p.Deleted);
        playersList.Should().BeInAscendingOrder(p => p.Name);
    }

    [Fact]
    public async Task GetByEmailAsync_WithValidEmail_ShouldReturnPlayer()
    {
        // Arrange
        var email = "test@example.com";
        var player = TestDataBuilder.Player()
            .WithName("Test Player")
            .WithEmail(email)
            .WithDeleted(false)
            .Build();

        await DbContext.Players.AddAsync(player);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(player.Id);
        result.Email.Should().Be(email);
    }

    [Fact]
    public async Task GetByEmailAsync_WithCaseInsensitiveEmail_ShouldReturnPlayer()
    {
        // Arrange
        var email = "test@example.com";
        var player = TestDataBuilder.Player()
            .WithName("Test Player")
            .WithEmail(email.ToLower())
            .WithDeleted(false)
            .Build();

        await DbContext.Players.AddAsync(player);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync(email.ToUpper());

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email.ToLower());
    }

    [Fact]
    public async Task GetByEmailAsync_WithDeletedPlayer_ShouldReturnNull()
    {
        // Arrange
        var email = "deleted@example.com";
        var deletedPlayer = TestDataBuilder.Player()
            .WithName("Deleted Player")
            .WithEmail(email)
            .WithDeleted(true)
            .Build();

        await DbContext.Players.AddAsync(deletedPlayer);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync(email);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_WithNullOrEmptyEmail_ShouldReturnNull()
    {
        // Act & Assert
        var resultNull = await _repository.GetByEmailAsync(null!);
        var resultEmpty = await _repository.GetByEmailAsync("");
        var resultWhitespace = await _repository.GetByEmailAsync("   ");

        resultNull.Should().BeNull();
        resultEmpty.Should().BeNull();
        resultWhitespace.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnPlayer()
    {
        // Arrange
        var player = TestDataBuilder.Player()
            .WithName("Test Player")
            .WithEmail("test@example.com")
            .WithDeleted(false)
            .Build();

        await DbContext.Players.AddAsync(player);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(player.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(player.Id);
        result.Name.Should().Be("Test Player");
    }

    [Fact]
    public async Task GetByIdAsync_WithDeletedPlayer_ShouldReturnNull()
    {
        // Arrange
        var deletedPlayer = TestDataBuilder.Player()
            .WithName("Deleted Player")
            .WithEmail("deleted@example.com")
            .WithDeleted(true)
            .Build();

        await DbContext.Players.AddAsync(deletedPlayer);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(deletedPlayer.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateForUserAsync_WithValidData_ShouldCreatePlayer()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var player = TestDataBuilder.Player()
            .WithName("New Player")
            .WithEmail("new@example.com")
            .WithDeleted(false)
            .WithSelected(false)
            .Build();

        // Add a user to the context to avoid the warning
        var user = new User
        {
            Id = userId,
            DisplayName = "Test User",
            Email = "testuser@example.com",
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        await DbContext.Users.AddAsync(user);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.CreateForUserAsync(player, userId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("New Player");
        result.Email.Should().Be("new@example.com");
        result.CreatedByUserId.Should().Be(userId);

        // Verify it was saved to the database
        var savedPlayer = await DbContext.Players.FindAsync(result.Id);
        savedPlayer.Should().NotBeNull();
        savedPlayer!.CreatedByUserId.Should().Be(userId);
    }

    [Fact]
    public async Task CreateForUserAsync_WithNonExistentUser_ShouldStillCreatePlayer()
    {
        // Arrange
        var nonExistentUserId = Guid.NewGuid();
        var player = TestDataBuilder.Player()
            .WithName("Player for Non-existent User")
            .WithEmail("player@example.com")
            .Build();

        // Act
        var result = await _repository.CreateForUserAsync(player, nonExistentUserId);

        // Assert
        result.Should().NotBeNull();
        result.CreatedByUserId.Should().Be(nonExistentUserId);

        // Verify it was saved despite user not existing
        var savedPlayer = await DbContext.Players.FindAsync(result.Id);
        savedPlayer.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_ShouldUpdatePlayer()
    {
        // Arrange
        var existingPlayer = TestDataBuilder.Player()
            .WithName("Original Name")
            .WithEmail("original@example.com")
            .WithDeleted(false)
            .Build();

        await DbContext.Players.AddAsync(existingPlayer);
        await DbContext.SaveChangesAsync();

        var updateData = TestDataBuilder.Player()
            .WithName("Updated Name")
            .WithEmail("updated@example.com")
            .Build();

        // Act
        var result = await _repository.UpdateAsync(existingPlayer.Id, updateData);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(existingPlayer.Id);
        result.Name.Should().Be("Updated Name");
        result.Email.Should().Be("updated@example.com");

        // Verify changes were persisted
        var updatedPlayer = await DbContext.Players.FindAsync(existingPlayer.Id);
        updatedPlayer!.Name.Should().Be("Updated Name");
        updatedPlayer.Email.Should().Be("updated@example.com");
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentPlayer_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateData = TestDataBuilder.Player()
            .WithName("Updated Name")
            .Build();

        // Act
        var result = await _repository.UpdateAsync(nonExistentId, updateData);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SetCreatorAsync_WithValidData_ShouldSetCreator()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var oldUserId = Guid.NewGuid();
        var newUserId = Guid.NewGuid();

        var player = TestDataBuilder.Player()
            .WithId(playerId)
            .WithName("Test Player")
            .WithCreatedByUserId(oldUserId)
            .WithDeleted(false)
            .Build();

        var oldUser = new User { Id = oldUserId, DisplayName = "Old User", Email = "old@test.com", IsEmailVerified = true, CreatedAt = DateTime.UtcNow, IsActive = true };
        var newUser = new User { Id = newUserId, DisplayName = "New User", Email = "new@test.com", IsEmailVerified = true, CreatedAt = DateTime.UtcNow, IsActive = true };

        await DbContext.Users.AddRangeAsync(oldUser, newUser);
        await DbContext.Players.AddAsync(player);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.SetCreatorAsync(playerId, newUserId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(playerId);
        result.CreatedByUserId.Should().Be(newUserId);

        // Verify changes were persisted
        var updatedPlayer = await DbContext.Players.FindAsync(playerId);
        updatedPlayer!.CreatedByUserId.Should().Be(newUserId);
    }

    [Fact]
    public async Task SetCreatorAsync_WithNonExistentPlayer_ShouldThrowException()
    {
        // Arrange
        var nonExistentPlayerId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _repository.SetCreatorAsync(nonExistentPlayerId, userId));
    }

    [Fact]
    public async Task SetCreatorAsync_WithNonExistentUser_ShouldThrowException()
    {
        // Arrange
        var player = TestDataBuilder.Player()
            .WithName("Test Player")
            .WithDeleted(false)
            .Build();

        await DbContext.Players.AddAsync(player);
        await DbContext.SaveChangesAsync();

        var nonExistentUserId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _repository.SetCreatorAsync(player.Id, nonExistentUserId));
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldSoftDeletePlayer()
    {
        // Arrange
        var player = TestDataBuilder.Player()
            .WithName("Player to Delete")
            .WithEmail("delete@example.com")
            .WithDeleted(false)
            .Build();

        await DbContext.Players.AddAsync(player);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(player.Id);

        // Assert
        result.Should().BeTrue();

        // Verify soft delete
        var deletedPlayer = await DbContext.Players.FindAsync(player.Id);
        deletedPlayer.Should().NotBeNull();
        deletedPlayer!.Deleted.Should().BeTrue();
        deletedPlayer.Name.Should().StartWith("Player to Delete (Deleted-");
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.DeleteAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WithAlreadyDeletedPlayer_ShouldReturnFalse()
    {
        // Arrange
        var deletedPlayer = TestDataBuilder.Player()
            .WithName("Already Deleted Player")
            .WithDeleted(true)
            .Build();

        await DbContext.Players.AddAsync(deletedPlayer);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(deletedPlayer.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_WithExistingPlayer_ShouldReturnTrue()
    {
        // Arrange
        var player = TestDataBuilder.Player()
            .WithName("Existing Player")
            .WithDeleted(false)
            .Build();

        await DbContext.Players.AddAsync(player);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(player.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithDeletedPlayer_ShouldReturnFalse()
    {
        // Arrange
        var deletedPlayer = TestDataBuilder.Player()
            .WithName("Deleted Player")
            .WithDeleted(true)
            .Build();

        await DbContext.Players.AddAsync(deletedPlayer);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(deletedPlayer.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistentPlayer_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.ExistsAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SetCreatorByUserIdAsync_ShouldCallSetCreatorAsync()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var player = TestDataBuilder.Player()
            .WithId(playerId)
            .WithName("Test Player")
            .WithDeleted(false)
            .Build();

        var user = new User { Id = userId, DisplayName = "Test User", Email = "test@example.com", IsEmailVerified = true, CreatedAt = DateTime.UtcNow, IsActive = true };

        await DbContext.Users.AddAsync(user);
        await DbContext.Players.AddAsync(player);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.SetCreatorByUserIdAsync(playerId, userId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(playerId);
        result.CreatedByUserId.Should().Be(userId);
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
    public async Task GetByCreatorAsync_WithNoMatchingPlayers_ShouldReturnEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var otherUserPlayer = TestDataBuilder.Player()
            .WithName("Other User's Player")
            .WithCreatedByUserId(otherUserId)
            .WithDeleted(false)
            .Build();

        await DbContext.Players.AddAsync(otherUserPlayer);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByCreatorAsync(userId);

        // Assert
        result.Should().BeEmpty();
    }
}

