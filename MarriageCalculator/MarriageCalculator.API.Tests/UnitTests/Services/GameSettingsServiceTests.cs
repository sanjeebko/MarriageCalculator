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
/// Unit tests for GameSettingsService
/// </summary>
public class GameSettingsServiceTests : TestBase
{
    private readonly Mock<IGameSettingsRepository> _mockRepository;
    private readonly GameSettingsService _service;

    public GameSettingsServiceTests()
    {
        _mockRepository = new Mock<IGameSettingsRepository>();
        _service = new GameSettingsService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetAllGameSettingsAsync_ShouldReturnSettingsForUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var settings = new List<GameSettings>
        {
            TestDataBuilder.GameSettings()
                .WithId(1)
                .WithUserId(userId)
                .WithMurder(true)
                .Build(),
            TestDataBuilder.GameSettings()
                .WithId(2)
                .WithUserId(userId)
                .WithMurder(false)
                .Build()
        };

        _mockRepository.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(settings);

        // Act
        var result = await _service.GetAllGameSettingsAsync(userId);

        // Assert
        var settingsList = result.ToList();
        settingsList.Should().HaveCount(2);
        settingsList.Should().OnlyContain(dto => dto.UserId == userId);
        
        settingsList[0].Id.Should().Be(1);
        settingsList[0].Murder.Should().BeTrue();
        
        settingsList[1].Id.Should().Be(2);
        settingsList[1].Murder.Should().BeFalse();
    }

    [Fact]
    public async Task GetGameSettingsByIdAsync_WithValidId_ShouldReturnGameSettings()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var settings = TestDataBuilder.GameSettings()
            .WithId(1)
            .WithUserId(userId)
            .WithMurder(true)
            .Build();

        _mockRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(settings);

        // Act
        var result = await _service.GetGameSettingsByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.UserId.Should().Be(userId);
        result.Murder.Should().BeTrue();
    }

    [Fact]
    public async Task GetGameSettingsByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((GameSettings?)null);

        // Act
        var result = await _service.GetGameSettingsByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateGameSettingsAsync_WithoutUserId_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var createDto = TestDataBuilder.CreateGameSettingsDto()
            .WithMurder(true)
            .WithKidnap(false)
            .Build();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateGameSettingsAsync(createDto));
    }

    [Fact]
    public async Task CreateGameSettingsAsync_WithUserId_ShouldCreateAndReturnGameSettings()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createDto = TestDataBuilder.CreateGameSettingsDto()
            .WithMurder(true)
            .WithKidnap(false)
            .WithSeenPoint(10)
            .WithUnseenPoint(5)
            .WithPointRate(1.5)
            .WithAudio(true)
            .Build();

        var createdSettings = TestDataBuilder.GameSettings()
            .WithId(1)
            .WithUserId(userId)
            .WithMurder(createDto.Murder)
            .WithKidnap(createDto.Kidnap)
            .WithSeenPoint(createDto.SeenPoint)
            .WithUnseenPoint(createDto.UnseenPoint)
            .WithPointRate(createDto.PointRate)
            .WithAudio(createDto.Audio)
            .Build();

        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<GameSettings>()))
            .ReturnsAsync(createdSettings);

        // Act
        var result = await _service.CreateGameSettingsAsync(createDto, userId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.UserId.Should().Be(userId);
        result.Murder.Should().BeTrue();
        result.Kidnap.Should().BeFalse();
        result.SeenPoint.Should().Be(10);
        result.UnseenPoint.Should().Be(5);
        result.PointRate.Should().Be(1.5);
        result.Audio.Should().BeTrue();

        // Verify repository was called with correct data
        _mockRepository.Verify(r => r.CreateAsync(It.Is<GameSettings>(s =>
            s.UserId == userId &&
            s.Murder == createDto.Murder &&
            s.Kidnap == createDto.Kidnap &&
            s.SeenPoint == createDto.SeenPoint &&
            s.UnseenPoint == createDto.UnseenPoint &&
            s.PointRate == createDto.PointRate &&
            s.Audio == createDto.Audio
        )), Times.Once);
    }

    [Fact]
    public async Task UpdateGameSettingsAsync_WithValidId_ShouldUpdateAndReturnGameSettings()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingSettings = TestDataBuilder.GameSettings()
            .WithId(1)
            .WithUserId(userId)
            .WithMurder(false)
            .Build();

        var updateDto = TestDataBuilder.CreateGameSettingsDto()
            .WithMurder(true)
            .WithKidnap(true)
            .WithSeenPoint(15)
            .Build();

        var updatedSettings = TestDataBuilder.GameSettings()
            .WithId(1)
            .WithUserId(userId)
            .WithMurder(updateDto.Murder)
            .WithKidnap(updateDto.Kidnap)
            .WithSeenPoint(updateDto.SeenPoint)
            .Build();

        _mockRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingSettings);
        _mockRepository.Setup(r => r.UpdateAsync(1, It.IsAny<GameSettings>()))
            .ReturnsAsync(updatedSettings);

        // Act
        var result = await _service.UpdateGameSettingsAsync(1, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.UserId.Should().Be(userId);
        result.Murder.Should().BeTrue();
        result.Kidnap.Should().BeTrue();
        result.SeenPoint.Should().Be(15);

        // Verify repository was called with correct data
        _mockRepository.Verify(r => r.UpdateAsync(1, It.Is<GameSettings>(s =>
            s.UserId == userId &&
            s.Murder == updateDto.Murder &&
            s.Kidnap == updateDto.Kidnap &&
            s.SeenPoint == updateDto.SeenPoint
        )), Times.Once);
    }

    [Fact]
    public async Task UpdateGameSettingsAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Arrange
        var updateDto = TestDataBuilder.CreateGameSettingsDto()
            .WithMurder(true)
            .Build();

        _mockRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((GameSettings?)null);

        // Act
        var result = await _service.UpdateGameSettingsAsync(999, updateDto);

        // Assert
        result.Should().BeNull();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<GameSettings>()), Times.Never);
    }

    [Fact]
    public async Task UpdateGameSettingsAsync_WithRepositoryUpdateFailure_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingSettings = TestDataBuilder.GameSettings()
            .WithId(1)
            .WithUserId(userId)
            .WithMurder(false)
            .Build();

        var updateDto = TestDataBuilder.CreateGameSettingsDto()
            .WithMurder(true)
            .Build();

        _mockRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingSettings);
        _mockRepository.Setup(r => r.UpdateAsync(1, It.IsAny<GameSettings>()))
            .ReturnsAsync((GameSettings?)null);

        // Act
        var result = await _service.UpdateGameSettingsAsync(1, updateDto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteGameSettingsAsync_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        _mockRepository.Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteGameSettingsAsync(1);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteGameSettingsAsync_WithNonExistentId_ShouldReturnFalse()
    {
        // Arrange
        _mockRepository.Setup(r => r.DeleteAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _service.DeleteGameSettingsAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GameSettingsExistsAsync_WithExistingSettings_ShouldReturnTrue()
    {
        // Arrange
        _mockRepository.Setup(r => r.ExistsAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _service.GameSettingsExistsAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GameSettingsExistsAsync_WithNonExistentSettings_ShouldReturnFalse()
    {
        // Arrange
        _mockRepository.Setup(r => r.ExistsAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _service.GameSettingsExistsAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllGameSettingsAsync_WithEmptyResult_ShouldReturnEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(new List<GameSettings>());

        // Act
        var result = await _service.GetAllGameSettingsAsync(userId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateGameSettingsAsync_ShouldSetCreatedAtToCurrentTime()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createDto = TestDataBuilder.CreateGameSettingsDto()
            .WithMurder(true)
            .Build();

        var beforeCreate = DateTime.UtcNow;
        
        var createdSettings = TestDataBuilder.GameSettings()
            .WithId(1)
            .WithUserId(userId)
            .WithMurder(true)
            .Build();

        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<GameSettings>()))
            .ReturnsAsync(createdSettings);

        // Act
        await _service.CreateGameSettingsAsync(createDto, userId);

        var afterCreate = DateTime.UtcNow;

        // Assert
        _mockRepository.Verify(r => r.CreateAsync(It.Is<GameSettings>(s =>
            s.CreatedAt >= beforeCreate && s.CreatedAt <= afterCreate
        )), Times.Once);
    }

    [Fact]
    public async Task UpdateGameSettingsAsync_ShouldPreserveOriginalUserId()
    {
        // Arrange
        var originalUserId = Guid.NewGuid();
        var existingSettings = TestDataBuilder.GameSettings()
            .WithId(1)
            .WithUserId(originalUserId)
            .WithMurder(false)
            .Build();

        var updateDto = TestDataBuilder.CreateGameSettingsDto()
            .WithMurder(true)
            .Build();

        var updatedSettings = TestDataBuilder.GameSettings()
            .WithId(1)
            .WithUserId(originalUserId)
            .WithMurder(true)
            .Build();

        _mockRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingSettings);
        _mockRepository.Setup(r => r.UpdateAsync(1, It.IsAny<GameSettings>()))
            .ReturnsAsync(updatedSettings);

        // Act
        var result = await _service.UpdateGameSettingsAsync(1, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(originalUserId);

        // Verify the update preserves the original UserId
        _mockRepository.Verify(r => r.UpdateAsync(1, It.Is<GameSettings>(s =>
            s.UserId == originalUserId
        )), Times.Once);
    }
}
