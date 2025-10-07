using FluentAssertions;
using MarriageCalculator.API.Repositories.Interfaces;
using MarriageCalculator.API.Services.Implementations;
using MarriageCalculator.API.Tests.Helpers;
using MarriageCalculator.Core.DTOs;
using MarriageCalculator.Core.Models;
using Moq;
using Xunit;

namespace MarriageCalculator.API.Tests.UnitTests.Services;

/// <summary>
/// Unit tests for PlayerService
/// </summary>
public class PlayerServiceTests : TestBase
{
    private readonly Mock<IPlayerRepository> _mockRepository;
    private readonly PlayerService _service;

    public PlayerServiceTests()
    {
        _mockRepository = new Mock<IPlayerRepository>();
        _service = new PlayerService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetAllPlayersAsync_ShouldReturnAllPlayers()
    {
        // Arrange
        var players = new List<Player>
        {
            TestDataBuilder.Player()
                .WithId(Guid.NewGuid())
                .WithName("Player 1")
                .WithEmail("player1@test.com")
                .WithDeleted(false)
                .Build(),
            TestDataBuilder.Player()
                .WithId(Guid.NewGuid())
                .WithName("Player 2")
                .WithEmail("player2@test.com")
                .WithDeleted(false)
                .Build()
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(players);

        // Act
        var result = await _service.GetAllPlayersAsync();

        // Assert
        result.Should().HaveCount(2);
        var resultList = result.ToList();
        
        resultList[0].Id.Should().Be(players[0].Id);
        resultList[0].Name.Should().Be("Player 1");
        resultList[0].Email.Should().Be("player1@test.com");
        
        resultList[1].Id.Should().Be(players[1].Id);
        resultList[1].Name.Should().Be("Player 2");
        resultList[1].Email.Should().Be("player2@test.com");
    }

    [Fact]
    public async Task GetPlayersByCreatorAsync_ShouldReturnPlayersCreatedByUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var players = new List<Player>
        {
            TestDataBuilder.Player()
                .WithId(Guid.NewGuid())
                .WithName("User's Player 1")
                .WithEmail("userplayer1@test.com")
                .WithCreatedByUserId(userId)
                .Build(),
            TestDataBuilder.Player()
                .WithId(Guid.NewGuid())
                .WithName("User's Player 2")
                .WithEmail("userplayer2@test.com")
                .WithCreatedByUserId(userId)
                .Build()
        };

        _mockRepository.Setup(r => r.GetByCreatorAsync(userId))
            .ReturnsAsync(players);

        // Act
        var result = await _service.GetPlayersByCreatorAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(dto => dto.CreatedByUserId == userId);
    }

    [Fact]
    public async Task GetPlayerByIdAsync_WithValidId_ShouldReturnPlayer()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var player = TestDataBuilder.Player()
            .WithId(playerId)
            .WithName("Test Player")
            .WithEmail("test@example.com")
            .Build();

        _mockRepository.Setup(r => r.GetByIdAsync(playerId))
            .ReturnsAsync(player);

        // Act
        var result = await _service.GetPlayerByIdAsync(playerId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(playerId);
        result.Name.Should().Be("Test Player");
        result.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetPlayerByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var playerId = Guid.NewGuid();

        _mockRepository.Setup(r => r.GetByIdAsync(playerId))
            .ReturnsAsync((Player?)null);

        // Act
        var result = await _service.GetPlayerByIdAsync(playerId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreatePlayerForUserAsync_WithValidData_ShouldCreateAndReturnPlayer()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createDto = TestDataBuilder.CreatePlayerDto()
            .WithId(Guid.NewGuid())
            .WithName("New Player")
            .WithEmail("newplayer@test.com")
            .WithCreatedAt(DateTime.UtcNow)
            .Build();

        var createdPlayer = TestDataBuilder.Player()
            .WithId(createDto.Id)
            .WithName(createDto.Name)
            .WithEmail(createDto.Email)
            .WithCreatedByUserId(userId)
            .WithCreatedAt(createDto.CreatedAt)
            .WithDeleted(false)
            .WithSelected(false)
            .Build();

        _mockRepository.Setup(r => r.CreateForUserAsync(It.IsAny<Player>(), userId))
            .ReturnsAsync(createdPlayer);

        // Act
        var result = await _service.CreatePlayerForUserAsync(createDto, userId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(createDto.Id);
        result.Name.Should().Be(createDto.Name);
        result.Email.Should().Be(createDto.Email);
        result.CreatedByUserId.Should().Be(userId);
        result.CreatedAt.Should().Be(createDto.CreatedAt);
        result.Deleted.Should().BeFalse();

        // Verify repository was called with correct data
        _mockRepository.Verify(r => r.CreateForUserAsync(It.Is<Player>(p =>
            p.Name == createDto.Name &&
            p.Email == createDto.Email &&
            p.CreatedByUserId == userId &&
            p.Deleted == false &&
            p.Selected == false
        ), userId), Times.Once);
    }

    [Fact]
    public async Task EnsureUserPlayerAsync_WithExistingPlayerByEmail_ShouldReturnExistingPlayer()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "existing@test.com";
        var displayName = "Test User";

        var existingPlayer = TestDataBuilder.Player()
            .WithId(Guid.NewGuid())
            .WithName("Existing Player")
            .WithEmail(email)
            .WithCreatedByUserId(userId)
            .Build();

        _mockRepository.Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync(existingPlayer);
        _mockRepository.Setup(r => r.SetCreatorByUserIdAsync(existingPlayer.Id, userId))
            .ReturnsAsync(existingPlayer);

        // Act
        var result = await _service.EnsureUserPlayerAsync(userId, displayName, email);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(existingPlayer.Id);
        result.Email.Should().Be(email);

        // Verify that SetCreatorByUserIdAsync was called
        _mockRepository.Verify(r => r.SetCreatorByUserIdAsync(existingPlayer.Id, userId), Times.Once);
    }

    [Fact]
    public async Task EnsureUserPlayerAsync_WithExistingPlayerByName_ShouldReturnExistingPlayer()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var displayName = "Test User";

        var existingPlayersByCreator = new List<Player>
        {
            TestDataBuilder.Player()
                .WithId(Guid.NewGuid())
                .WithName(displayName)
                .WithEmail("different@test.com")
                .WithCreatedByUserId(userId)
                .Build()
        };

        _mockRepository.Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync((Player?)null);
        _mockRepository.Setup(r => r.GetByCreatorAsync(userId))
            .ReturnsAsync(existingPlayersByCreator);

        // Act
        var result = await _service.EnsureUserPlayerAsync(userId, displayName, email);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(existingPlayersByCreator[0].Id);
        result.Name.Should().Be(displayName);
    }

    [Fact]
    public async Task EnsureUserPlayerAsync_WithNoExistingPlayer_ShouldCreateNewPlayer()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "newuser@test.com";
        var displayName = "New User";

        var createdPlayer = TestDataBuilder.Player()
            .WithId(Guid.NewGuid())
            .WithName(displayName)
            .WithEmail(email)
            .WithCreatedByUserId(userId)
            .WithDeleted(false)
            .Build();

        _mockRepository.Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync((Player?)null);
        _mockRepository.Setup(r => r.GetByCreatorAsync(userId))
            .ReturnsAsync(new List<Player>());
        _mockRepository.Setup(r => r.CreateForUserAsync(It.IsAny<Player>(), userId))
            .ReturnsAsync(createdPlayer);

        // Act
        var result = await _service.EnsureUserPlayerAsync(userId, displayName, email);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(displayName);
        result.Email.Should().Be(email);
        result.CreatedByUserId.Should().Be(userId);

        // Verify repository was called to create new player
        _mockRepository.Verify(r => r.CreateForUserAsync(It.Is<Player>(p =>
            p.Name == displayName &&
            p.Email == email &&
            p.Deleted == false
        ), userId), Times.Once);
    }

    [Fact]
    public async Task EnsureUserPlayerAsync_WithEmptyDisplayNameAndNullEmail_ShouldCreatePlayerWithDefaultName()
    {
        // Arrange
        var userId = Guid.NewGuid();
        string? email = null;
        var displayName = "";

        var createdPlayer = TestDataBuilder.Player()
            .WithId(Guid.NewGuid())
            .WithName("Player")
            .WithEmail("")
            .WithCreatedByUserId(userId)
            .Build();

        _mockRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((Player?)null);
        _mockRepository.Setup(r => r.GetByCreatorAsync(userId))
            .ReturnsAsync(new List<Player>());
        _mockRepository.Setup(r => r.CreateForUserAsync(It.IsAny<Player>(), userId))
            .ReturnsAsync(createdPlayer);

        // Act
        var result = await _service.EnsureUserPlayerAsync(userId, displayName, email!);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Player");
        result.Email.Should().Be("");

        // Verify repository was called with default name
        // When displayName is empty and email is null, Name becomes "Player"
        _mockRepository.Verify(r => r.CreateForUserAsync(It.Is<Player>(p =>
            p.Name == "Player" &&
            p.Email == "" &&
            p.Deleted == false
        ), userId), Times.Once);
    }

    [Fact]
    public async Task EnsureUserPlayerAsync_WithEmptyDisplayNameAndEmptyEmail_ShouldCreatePlayerWithDefaultName()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "";
        var displayName = "";

        var createdPlayer = TestDataBuilder.Player()
            .WithId(Guid.NewGuid())
            .WithName("")
            .WithEmail("")
            .WithCreatedByUserId(userId)
            .Build();

        _mockRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((Player?)null);
        _mockRepository.Setup(r => r.GetByCreatorAsync(userId))
            .ReturnsAsync(new List<Player>());
        _mockRepository.Setup(r => r.CreateForUserAsync(It.IsAny<Player>(), userId))
            .ReturnsAsync(createdPlayer);

        // Act
        var result = await _service.EnsureUserPlayerAsync(userId, displayName, email);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("");
        result.Email.Should().Be("");

        // Verify repository was called
        // When displayName is empty and email is empty string, Name becomes empty string
        _mockRepository.Verify(r => r.CreateForUserAsync(It.Is<Player>(p =>
            p.Name == "" &&
            p.Email == "" &&
            p.Deleted == false
        ), userId), Times.Once);
    }

    [Fact]
    public async Task EnsureUserPlayerAsync_WithEmptyDisplayNameButValidEmail_ShouldUseEmailAsName()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "user@test.com";
        var displayName = "";

        var createdPlayer = TestDataBuilder.Player()
            .WithId(Guid.NewGuid())
            .WithName(email)
            .WithEmail(email)
            .WithCreatedByUserId(userId)
            .Build();

        _mockRepository.Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync((Player?)null);
        _mockRepository.Setup(r => r.GetByCreatorAsync(userId))
            .ReturnsAsync(new List<Player>());
        _mockRepository.Setup(r => r.CreateForUserAsync(It.IsAny<Player>(), userId))
            .ReturnsAsync(createdPlayer);

        // Act
        var result = await _service.EnsureUserPlayerAsync(userId, displayName, email);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(email);
        result.Email.Should().Be(email);

        // Verify repository was called with email as name
        _mockRepository.Verify(r => r.CreateForUserAsync(It.Is<Player>(p =>
            p.Name == email &&
            p.Email == email
        ), userId), Times.Once);
    }

    [Fact]
    public async Task UpdatePlayerAsync_WithValidData_ShouldUpdateAndReturnPlayer()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var updateDto = TestDataBuilder.UpdatePlayerDto()
            .WithName("Updated Player")
            .WithEmail("updated@test.com")
            .Build();

        var updatedPlayer = TestDataBuilder.Player()
            .WithId(playerId)
            .WithName(updateDto.Name)
            .WithEmail(updateDto.Email)
            .Build();

        _mockRepository.Setup(r => r.UpdateAsync(playerId, It.IsAny<Player>()))
            .ReturnsAsync(updatedPlayer);

        // Act
        var result = await _service.UpdatePlayerAsync(playerId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(playerId);
        result.Name.Should().Be(updateDto.Name);
        result.Email.Should().Be(updateDto.Email);

        // Verify repository was called with correct data
        _mockRepository.Verify(r => r.UpdateAsync(playerId, It.Is<Player>(p =>
            p.Name == updateDto.Name &&
            p.Email == updateDto.Email
        )), Times.Once);
    }

    [Fact]
    public async Task UpdatePlayerAsync_WithNonExistentPlayer_ShouldReturnNull()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var updateDto = TestDataBuilder.UpdatePlayerDto()
            .WithName("Updated Player")
            .Build();

        _mockRepository.Setup(r => r.UpdateAsync(playerId, It.IsAny<Player>()))
            .ReturnsAsync((Player?)null);

        // Act
        var result = await _service.UpdatePlayerAsync(playerId, updateDto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeletePlayerAsync_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        var playerId = Guid.NewGuid();

        _mockRepository.Setup(r => r.DeleteAsync(playerId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeletePlayerAsync(playerId);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.DeleteAsync(playerId), Times.Once);
    }

    [Fact]
    public async Task DeletePlayerAsync_WithNonExistentPlayer_ShouldReturnFalse()
    {
        // Arrange
        var playerId = Guid.NewGuid();

        _mockRepository.Setup(r => r.DeleteAsync(playerId))
            .ReturnsAsync(false);

        // Act
        var result = await _service.DeletePlayerAsync(playerId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task PlayerExistsAsync_WithExistingPlayer_ShouldReturnTrue()
    {
        // Arrange
        var playerId = Guid.NewGuid();

        _mockRepository.Setup(r => r.ExistsAsync(playerId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.PlayerExistsAsync(playerId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task PlayerExistsAsync_WithNonExistentPlayer_ShouldReturnFalse()
    {
        // Arrange
        var playerId = Guid.NewGuid();

        _mockRepository.Setup(r => r.ExistsAsync(playerId))
            .ReturnsAsync(false);

        // Act
        var result = await _service.PlayerExistsAsync(playerId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllPlayersAsync_WithEmptyResult_ShouldReturnEmptyList()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Player>());

        // Act
        var result = await _service.GetAllPlayersAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPlayersByCreatorAsync_WithEmptyResult_ShouldReturnEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockRepository.Setup(r => r.GetByCreatorAsync(userId))
            .ReturnsAsync(new List<Player>());

        // Act
        var result = await _service.GetPlayersByCreatorAsync(userId);

        // Assert
        result.Should().BeEmpty();
    }
}
