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
/// Unit tests for MarriageGameSetService
/// </summary>
public class MarriageGameSetServiceTests : TestBase
{
    private readonly Mock<IMarriageGameSetRepository> _mockRepository;
    private readonly MarriageGameSetService _service;

    public MarriageGameSetServiceTests()
    {
        _mockRepository = new Mock<IMarriageGameSetRepository>();
        _service = new MarriageGameSetService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetActiveGameSetByGameSettingsIdAsync_WithActiveGameSet_ShouldReturnDto()
    {
        // Arrange
        var gameSettingsId = 1;
        var activeGameSet = TestDataBuilder.GameSet()
            .WithId(1)
            .WithName("Active Game Set")
            .WithGameSettingsId(gameSettingsId)
            .WithIsActive(true)
            .Build();

        _mockRepository.Setup(r => r.GetActiveByGameSettingsIdAsync(gameSettingsId))
            .ReturnsAsync(activeGameSet);

        // Act
        var result = await _service.GetActiveGameSetByGameSettingsIdAsync(gameSettingsId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(activeGameSet.Id);
        result.Name.Should().Be(activeGameSet.Name);
        result.GameSettingsId.Should().Be(activeGameSet.GameSettingsId);
        result.IsActive.Should().Be(activeGameSet.IsActive);
    }

    [Fact]
    public async Task GetActiveGameSetByGameSettingsIdAsync_WithNoActiveGameSet_ShouldReturnNull()
    {
        // Arrange
        var gameSettingsId = 1;

        _mockRepository.Setup(r => r.GetActiveByGameSettingsIdAsync(gameSettingsId))
            .ReturnsAsync((MarriageGameSet?)null);

        // Act
        var result = await _service.GetActiveGameSetByGameSettingsIdAsync(gameSettingsId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateGameSetAsync_WithValidDto_ShouldCreateAndReturnGameSet()
    {
        // Arrange
        var createDto = TestDataBuilder.CreateGameSetDto()
            .WithName("New Game Set")
            .WithGameSettingsId(1)
            .Build();

        var createdGameSet = TestDataBuilder.GameSet()
            .WithId(1)
            .WithName(createDto.Name)
            .WithGameSettingsId(createDto.GameSettingsId)
            .WithIsActive(true)
            .WithCreated(DateTime.UtcNow)
            .WithLastPlayed(DateTime.UtcNow)
            .Build();

        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<MarriageGameSet>()))
            .ReturnsAsync(createdGameSet);

        // Act
        var result = await _service.CreateGameSetAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(createdGameSet.Id);
        result.Name.Should().Be(createDto.Name);
        result.GameSettingsId.Should().Be(createDto.GameSettingsId);
        result.IsActive.Should().BeTrue();

        // Verify that repository was called with correct data
        _mockRepository.Verify(r => r.CreateAsync(It.Is<MarriageGameSet>(gs => 
            gs.Name == createDto.Name &&
            gs.GameSettingsId == createDto.GameSettingsId &&
            gs.IsActive == true
        )), Times.Once);
    }

    [Fact]
    public async Task GetAllGameSetsAsync_WithValidGameSettingsId_ShouldReturnGameSets()
    {
        // Arrange
        var gameSettingsId = 1;
        var gameSets = new List<MarriageGameSet>
        {
            TestDataBuilder.GameSet()
                .WithId(1)
                .WithName("Game Set 1")
                .WithGameSettingsId(gameSettingsId)
                .WithIsActive(true)
                .Build(),
            TestDataBuilder.GameSet()
                .WithId(2)
                .WithName("Game Set 2")
                .WithGameSettingsId(gameSettingsId)
                .WithIsActive(false)
                .Build()
        };

        _mockRepository.Setup(r => r.GetByGameSettingsIdAsync(gameSettingsId))
            .ReturnsAsync(gameSets);

        // Act
        var result = await _service.GetAllGameSetsAsync(gameSettingsId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(dto => dto.GameSettingsId == gameSettingsId);
        
        var resultList = result.ToList();
        resultList[0].Id.Should().Be(1);
        resultList[0].Name.Should().Be("Game Set 1");
        resultList[0].IsActive.Should().BeTrue();
        
        resultList[1].Id.Should().Be(2);
        resultList[1].Name.Should().Be("Game Set 2");
        resultList[1].IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task GetGameSetByIdAsync_WithValidId_ShouldReturnGameSet()
    {
        // Arrange
        var gameSetId = 1;
        var gameSet = TestDataBuilder.GameSet()
            .WithId(gameSetId)
            .WithName("Test Game Set")
            .WithGameSettingsId(1)
            .WithIsActive(true)
            .Build();

        _mockRepository.Setup(r => r.GetByIdAsync(gameSetId))
            .ReturnsAsync(gameSet);

        // Act
        var result = await _service.GetGameSetByIdAsync(gameSetId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(gameSetId);
        result.Name.Should().Be("Test Game Set");
        result.GameSettingsId.Should().Be(1);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetGameSetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var gameSetId = 999;

        _mockRepository.Setup(r => r.GetByIdAsync(gameSetId))
            .ReturnsAsync((MarriageGameSet?)null);

        // Act
        var result = await _service.GetGameSetByIdAsync(gameSetId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetLatestActiveGameSetAsync_WithActiveGameSet_ShouldReturnGameSet()
    {
        // Arrange
        var activeGameSet = TestDataBuilder.GameSet()
            .WithId(1)
            .WithName("Latest Active Game Set")
            .WithGameSettingsId(1)
            .WithIsActive(true)
            .WithLastPlayed(DateTime.UtcNow)
            .Build();

        _mockRepository.Setup(r => r.GetLatestActiveAsync())
            .ReturnsAsync(activeGameSet);

        // Act
        var result = await _service.GetLatestActiveGameSetAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(activeGameSet.Id);
        result.Name.Should().Be(activeGameSet.Name);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetLatestActiveGameSetAsync_WithNoActiveGameSet_ShouldReturnNull()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetLatestActiveAsync())
            .ReturnsAsync((MarriageGameSet?)null);

        // Act
        var result = await _service.GetLatestActiveGameSetAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateGameSetAsync_WithValidData_ShouldUpdateAndReturnGameSet()
    {
        // Arrange
        var gameSetId = 1;
        var updateDto = TestDataBuilder.CreateGameSetDto()
            .WithName("Updated Game Set")
            .WithGameSettingsId(2)
            .Build();

        var updatedGameSet = TestDataBuilder.GameSet()
            .WithId(gameSetId)
            .WithName(updateDto.Name)
            .WithGameSettingsId(updateDto.GameSettingsId)
            .WithLastPlayed(DateTime.UtcNow)
            .Build();

        _mockRepository.Setup(r => r.UpdateAsync(gameSetId, It.IsAny<MarriageGameSet>()))
            .ReturnsAsync(updatedGameSet);

        // Act
        var result = await _service.UpdateGameSetAsync(gameSetId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(gameSetId);
        result.Name.Should().Be(updateDto.Name);
        result.GameSettingsId.Should().Be(updateDto.GameSettingsId);
    }

    [Fact]
    public async Task DeleteGameSetAsync_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        var gameSetId = 1;

        _mockRepository.Setup(r => r.DeleteAsync(gameSetId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteGameSetAsync(gameSetId);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.DeleteAsync(gameSetId), Times.Once);
    }

    [Fact]
    public async Task DeleteGameSetAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        var gameSetId = 999;

        _mockRepository.Setup(r => r.DeleteAsync(gameSetId))
            .ReturnsAsync(false);

        // Act
        var result = await _service.DeleteGameSetAsync(gameSetId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GameSetExistsAsync_WithExistingId_ShouldReturnTrue()
    {
        // Arrange
        var gameSetId = 1;

        _mockRepository.Setup(r => r.ExistsAsync(gameSetId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.GameSetExistsAsync(gameSetId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GameSetExistsAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        var gameSetId = 999;

        _mockRepository.Setup(r => r.ExistsAsync(gameSetId))
            .ReturnsAsync(false);

        // Act
        var result = await _service.GameSetExistsAsync(gameSetId);

        // Assert
        result.Should().BeFalse();
    }
}
