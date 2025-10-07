using FluentAssertions;
using MarriageCalculator.API.Controllers;
using MarriageCalculator.API.Services.Interfaces;
using MarriageCalculator.API.Tests.Helpers;
using MarriageCalculator.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MarriageCalculator.API.Tests.UnitTests.Controllers;

/// <summary>
/// Unit tests for MarriageGameSetsController
/// </summary>
public class MarriageGameSetsControllerTests : TestBase
{
    private readonly Mock<IMarriageGameSetService> _mockGameSetService;
    private readonly Mock<IGameSettingsService> _mockGameSettingsService;
    private readonly Mock<ILogger<MarriageGameSetsController>> _mockLogger;
    private readonly MarriageGameSetsController _controller;

    public MarriageGameSetsControllerTests()
    {
        _mockGameSetService = new Mock<IMarriageGameSetService>();
        _mockGameSettingsService = new Mock<IGameSettingsService>();
        _mockLogger = CreateMockLogger<MarriageGameSetsController>();
        _controller = new MarriageGameSetsController(_mockGameSetService.Object, _mockGameSettingsService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateMarriageGameSet_WithValidData_AndNoActiveGameSet_ShouldReturnCreatedResult()
    {
        // Arrange
        var createDto = TestDataBuilder.CreateGameSetDto()
            .WithName("Test Game Set")
            .WithGameSettingsId(1)
            .Build();

        var expectedGameSet = new MarriageGameSetDto
        {
            Id = 1,
            Name = "Test Game Set",
            GameSettingsId = 1,
            IsActive = true,
            Created = DateTime.UtcNow,
            LastPlayed = DateTime.UtcNow
        };

        _mockGameSetService.Setup(s => s.GetActiveGameSetByGameSettingsIdAsync(1))
            .ReturnsAsync((MarriageGameSetDto?)null);

        _mockGameSetService.Setup(s => s.CreateGameSetAsync(createDto))
            .ReturnsAsync(expectedGameSet);

        // Act
        var result = await _controller.CreateMarriageGameSet(createDto);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result.Result as CreatedAtActionResult;
        createdResult!.Value.Should().BeEquivalentTo(expectedGameSet);
        createdResult.ActionName.Should().Be(nameof(MarriageGameSetsController.GetMarriageGameSet));
        createdResult.RouteValues!["id"].Should().Be(expectedGameSet.Id);
    }

    [Fact]
    public async Task CreateMarriageGameSet_WithActiveGameSetExists_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = TestDataBuilder.CreateGameSetDto()
            .WithName("Test Game Set")
            .WithGameSettingsId(1)
            .Build();

        var existingActiveGameSet = new MarriageGameSetDto
        {
            Id = 2,
            Name = "Existing Active Game Set",
            GameSettingsId = 1,
            IsActive = true,
            Created = DateTime.UtcNow.AddDays(-1),
            LastPlayed = DateTime.UtcNow.AddHours(-1)
        };

        _mockGameSetService.Setup(s => s.GetActiveGameSetByGameSettingsIdAsync(1))
            .ReturnsAsync(existingActiveGameSet);

        // Act
        var result = await _controller.CreateMarriageGameSet(createDto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("New game can not be created before closing Active gameset.");

        // Verify that CreateGameSetAsync was never called
        _mockGameSetService.Verify(s => s.CreateGameSetAsync(It.IsAny<CreateMarriageGameSetDto>()), Times.Never);
    }

    [Fact]
    public async Task CreateMarriageGameSet_WithInvalidModelState_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = TestDataBuilder.CreateGameSetDto()
            .WithName("")  // Invalid empty name
            .WithGameSettingsId(1)
            .Build();

        _controller.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = await _controller.CreateMarriageGameSet(createDto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        
        // Verify that no service methods were called
        _mockGameSetService.Verify(s => s.GetActiveGameSetByGameSettingsIdAsync(It.IsAny<int>()), Times.Never);
        _mockGameSetService.Verify(s => s.CreateGameSetAsync(It.IsAny<CreateMarriageGameSetDto>()), Times.Never);
    }

    [Fact]
    public async Task CreateMarriageGameSet_WhenServiceThrowsException_ShouldReturnInternalServerError()
    {
        // Arrange
        var createDto = TestDataBuilder.CreateGameSetDto()
            .WithName("Test Game Set")
            .WithGameSettingsId(1)
            .Build();

        _mockGameSetService.Setup(s => s.GetActiveGameSetByGameSettingsIdAsync(1))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.CreateMarriageGameSet(createDto);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
        objectResult.Value.Should().Be("An error occurred while creating the marriage game set");
    }

    [Fact]
    public async Task GetMarriageGameSets_WithValidGameSettingsId_ShouldReturnOkResult()
    {
        // Arrange
        var gameSettingsId = 1;
        var expectedGameSets = new List<MarriageGameSetDto>
        {
            new() { Id = 1, Name = "Game Set 1", GameSettingsId = gameSettingsId, IsActive = true },
            new() { Id = 2, Name = "Game Set 2", GameSettingsId = gameSettingsId, IsActive = false }
        };

        _mockGameSetService.Setup(s => s.GetAllGameSetsAsync(gameSettingsId))
            .ReturnsAsync(expectedGameSets);

        // Act
        var result = await _controller.GetMarriageGameSets();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedGameSets);
    }

    [Fact]
    public async Task GetMarriageGameSet_WithValidId_ShouldReturnOkResult()
    {
        // Arrange
        var gameSetId = 1;
        var expectedGameSet = new MarriageGameSetDto
        {
            Id = gameSetId,
            Name = "Test Game Set",
            GameSettingsId = 1,
            IsActive = true
        };

        _mockGameSetService.Setup(s => s.GetGameSetByIdAsync(gameSetId))
            .ReturnsAsync(expectedGameSet);

        // Act
        var result = await _controller.GetMarriageGameSet(gameSetId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedGameSet);
    }

    [Fact]
    public async Task GetMarriageGameSet_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var gameSetId = 999;

        _mockGameSetService.Setup(s => s.GetGameSetByIdAsync(gameSetId))
            .ReturnsAsync((MarriageGameSetDto?)null);

        // Act
        var result = await _controller.GetMarriageGameSet(gameSetId);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result.Result as NotFoundObjectResult;
        notFoundResult!.Value.Should().Be($"Marriage game set with ID {gameSetId} not found");
    }

    [Fact]
    public async Task GetLatestActiveGameSet_WithActiveGameSet_ShouldReturnOkResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedGameSet = new MarriageGameSetDto
        {
            Id = 1,
            Name = "Latest Active Game Set",
            GameSettingsId = 1,
            IsActive = true
        };

        // Mock the user claims
        SetupUserClaims(_controller, userId);

        _mockGameSetService.Setup(s => s.GetLatestActiveGameSetForUserAsync(userId))
            .ReturnsAsync(expectedGameSet);

        // Act
        var result = await _controller.GetLatestActiveGameSet();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedGameSet);
    }

    [Fact]
    public async Task GetLatestActiveGameSet_WithNoActiveGameSet_ShouldReturnNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        // Mock the user claims
        SetupUserClaims(_controller, userId);

        _mockGameSetService.Setup(s => s.GetLatestActiveGameSetForUserAsync(userId))
            .ReturnsAsync((MarriageGameSetDto?)null);

        // Act
        var result = await _controller.GetLatestActiveGameSet();

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result.Result as NotFoundObjectResult;
        notFoundResult!.Value.Should().Be("No active marriage game set found for current user");
    }

    [Fact]
    public async Task GetLatestActiveGameSet_WithInvalidUserToken_ShouldReturnUnauthorized()
    {
        // Arrange
        // Explicitly set up controller context with no user claims to simulate invalid token
        _controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
        };

        // Act
        var result = await _controller.GetLatestActiveGameSet();

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result.Result as UnauthorizedObjectResult;
        unauthorizedResult!.Value.Should().Be("Invalid user token");
    }

    [Fact]
    public async Task GetLatestActiveGameSet_WithInvalidUserIdFormat_ShouldReturnUnauthorized()
    {
        // Arrange
        // Setup user claims with invalid GUID format
        var claims = new List<System.Security.Claims.Claim>
        {
            new(System.Security.Claims.ClaimTypes.NameIdentifier, "invalid-guid-format")
        };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);

        _controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = principal
            }
        };

        // Act
        var result = await _controller.GetLatestActiveGameSet();

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result.Result as UnauthorizedObjectResult;
        unauthorizedResult!.Value.Should().Be("Invalid user token");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(999)]
    public async Task CreateMarriageGameSet_WithDifferentGameSettingsIds_ChecksCorrectGameSettingsId(int gameSettingsId)
    {
        // Arrange
        var createDto = TestDataBuilder.CreateGameSetDto()
            .WithName("Test Game Set")
            .WithGameSettingsId(gameSettingsId)
            .Build();

        _mockGameSetService.Setup(s => s.GetActiveGameSetByGameSettingsIdAsync(gameSettingsId))
            .ReturnsAsync((MarriageGameSetDto?)null);

        _mockGameSetService.Setup(s => s.CreateGameSetAsync(createDto))
            .ReturnsAsync(new MarriageGameSetDto { Id = 1, GameSettingsId = gameSettingsId });

        // Act
        await _controller.CreateMarriageGameSet(createDto);

        // Assert
        _mockGameSetService.Verify(s => s.GetActiveGameSetByGameSettingsIdAsync(gameSettingsId), Times.Once);
    }
}
