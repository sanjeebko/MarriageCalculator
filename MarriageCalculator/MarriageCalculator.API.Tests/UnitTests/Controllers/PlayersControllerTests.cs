using FluentAssertions;
using MarriageCalculator.API.Controllers;
using MarriageCalculator.API.Services.Interfaces;
using MarriageCalculator.API.Tests.Helpers;
using MarriageCalculator.Core.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace MarriageCalculator.API.Tests.UnitTests.Controllers;

/// <summary>
/// Unit tests for PlayersController
/// </summary>
public class PlayersControllerTests : TestBase
{
    private readonly Mock<IPlayerService> _mockPlayerService;
    private readonly Mock<ILogger<PlayersController>> _mockLogger;
    private readonly PlayersController _controller;
    private readonly Guid _testUserId = Guid.NewGuid();

    public PlayersControllerTests()
    {
        _mockPlayerService = new Mock<IPlayerService>();
        _mockLogger = CreateMockLogger<PlayersController>();
        _controller = new PlayersController(_mockPlayerService.Object, _mockLogger.Object);

        // Setup authenticated user context
        SetupUserContext();
    }

    private void SetupUserContext()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, _testUserId.ToString()),
            new(ClaimTypes.Name, "Test User"),
            new(ClaimTypes.Email, "test@example.com")
        };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task GetAllPlayers_ShouldReturnOkResult_WithPlayerList()
    {
        // Arrange
        var expectedPlayers = new List<PlayerDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Player 1", Email = "player1@test.com" },
            new() { Id = Guid.NewGuid(), Name = "Player 2", Email = "player2@test.com" }
        };

        _mockPlayerService.Setup(s => s.GetPlayersByCreatorAsync(_testUserId))
            .ReturnsAsync(expectedPlayers);

        // Act
        var result = await _controller.GetAllPlayers();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedPlayers);
    }

    [Fact]
    public async Task GetAllPlayers_WhenServiceThrowsException_ShouldReturnInternalServerError()
    {
        // Arrange
        _mockPlayerService.Setup(s => s.GetPlayersByCreatorAsync(_testUserId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetAllPlayers();

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
        objectResult.Value.Should().Be("An error occurred while retrieving players");
    }

    [Fact]
    public async Task GetAllPlayers_WithInvalidUserToken_ShouldReturnUnauthorized()
    {
        // Arrange - Setup controller with no user claims
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
        };

        // Act
        var result = await _controller.GetAllPlayers();

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result.Result as UnauthorizedObjectResult;
        unauthorizedResult!.Value.Should().Be("Invalid user token");
    }

    [Fact]
    public async Task GetPlayer_WithValidId_ShouldReturnOkResult()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var expectedPlayer = new PlayerDto
        {
            Id = playerId,
            Name = "Test Player",
            Email = "test@example.com"
        };

        _mockPlayerService.Setup(s => s.GetPlayerByIdAsync(playerId))
            .ReturnsAsync(expectedPlayer);

        // Act
        var result = await _controller.GetPlayer(playerId.ToString());

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedPlayer);
    }

    [Fact]
    public async Task GetPlayer_WithInvalidGuid_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidId = "invalid-guid";

        // Act
        var result = await _controller.GetPlayer(invalidId);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Invalid GUID format");
    }

    [Fact]
    public async Task GetPlayer_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var playerId = Guid.NewGuid();

        _mockPlayerService.Setup(s => s.GetPlayerByIdAsync(playerId))
            .ReturnsAsync((PlayerDto?)null);

        // Act
        var result = await _controller.GetPlayer(playerId.ToString());

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result.Result as NotFoundObjectResult;
        notFoundResult!.Value.Should().Be($"Player with ID {playerId} not found");
    }

    [Fact]
    public async Task CreatePlayer_WithValidData_ShouldReturnCreatedResult()
    {
        // Arrange
        var createDto = TestDataBuilder.CreatePlayerDto()
            .WithId(Guid.NewGuid())
            .WithName("New Player")
            .WithEmail("newplayer@test.com")
            .WithCreatedAt(DateTime.UtcNow)
            .Build();

        var expectedPlayer = new PlayerDto
        {
            Id = createDto.Id,
            Name = createDto.Name,
            Email = createDto.Email,
            CreatedByUserId = _testUserId,
            CreatedAt = createDto.CreatedAt
        };

        _mockPlayerService.Setup(s => s.CreatePlayerForUserAsync(createDto, _testUserId))
            .ReturnsAsync(expectedPlayer);

        // Act
        var result = await _controller.CreatePlayer(createDto);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result.Result as CreatedAtActionResult;
        createdResult!.Value.Should().BeEquivalentTo(expectedPlayer);
        createdResult.ActionName.Should().Be(nameof(PlayersController.GetPlayer));
        createdResult.RouteValues!["id"].Should().Be(expectedPlayer.Id);
    }

    [Fact]
    public async Task CreatePlayer_WithInvalidModelState_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = TestDataBuilder.CreatePlayerDto()
            .WithName("") // Invalid empty name
            .Build();

        _controller.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = await _controller.CreatePlayer(createDto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();

        // Verify that service method was never called
        _mockPlayerService.Verify(s => s.CreatePlayerForUserAsync(It.IsAny<CreatePlayerDto>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task CreatePlayer_WithInvalidUserToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var createDto = TestDataBuilder.CreatePlayerDto()
            .WithName("Test Player")
            .Build();

        // Setup controller with no user claims
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
        };

        // Act
        var result = await _controller.CreatePlayer(createDto);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task UpdatePlayer_WithValidData_ShouldReturnOkResult()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var updateDto = TestDataBuilder.UpdatePlayerDto()
            .WithName("Updated Player")
            .WithEmail("updated@test.com")
            .Build();

        var expectedPlayer = new PlayerDto
        {
            Id = playerId,
            Name = updateDto.Name,
            Email = updateDto.Email
        };

        _mockPlayerService.Setup(s => s.UpdatePlayerAsync(playerId, updateDto))
            .ReturnsAsync(expectedPlayer);

        // Act
        var result = await _controller.UpdatePlayer(playerId.ToString(), updateDto);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedPlayer);
    }

    [Fact]
    public async Task UpdatePlayer_WithInvalidGuid_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidId = "invalid-guid";
        var updateDto = TestDataBuilder.UpdatePlayerDto()
            .WithName("Updated Player")
            .Build();

        // Act
        var result = await _controller.UpdatePlayer(invalidId, updateDto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Invalid GUID format");
    }

    [Fact]
    public async Task UpdatePlayer_WithNonExistentPlayer_ShouldReturnNotFound()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var updateDto = TestDataBuilder.UpdatePlayerDto()
            .WithName("Updated Player")
            .Build();

        _mockPlayerService.Setup(s => s.UpdatePlayerAsync(playerId, updateDto))
            .ReturnsAsync((PlayerDto?)null);

        // Act
        var result = await _controller.UpdatePlayer(playerId.ToString(), updateDto);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result.Result as NotFoundObjectResult;
        notFoundResult!.Value.Should().Be($"Player with ID {playerId} not found");
    }

    [Fact]
    public async Task DeletePlayer_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        var playerId = Guid.NewGuid();

        _mockPlayerService.Setup(s => s.DeletePlayerAsync(playerId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeletePlayer(playerId.ToString());

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeletePlayer_WithInvalidGuid_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidId = "invalid-guid";

        // Act
        var result = await _controller.DeletePlayer(invalidId);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Invalid GUID format");
    }

    [Fact]
    public async Task DeletePlayer_WithNonExistentPlayer_ShouldReturnNotFound()
    {
        // Arrange
        var playerId = Guid.NewGuid();

        _mockPlayerService.Setup(s => s.DeletePlayerAsync(playerId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeletePlayer(playerId.ToString());

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult!.Value.Should().Be($"Player with ID {playerId} not found");
    }

    [Fact]
    public async Task EnsureMe_WithValidUser_ShouldReturnOkResult()
    {
        // Arrange
        var expectedPlayer = new PlayerDto
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
            Email = "test@example.com",
            CreatedByUserId = _testUserId
        };

        _mockPlayerService.Setup(s => s.EnsureUserPlayerAsync(_testUserId, "Test User", "test@example.com"))
            .ReturnsAsync(expectedPlayer);

        // Act
        var result = await _controller.EnsureMe();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedPlayer);
    }

    [Fact]
    public async Task EnsureMe_WithInvalidUserToken_ShouldReturnUnauthorized()
    {
        // Arrange - Setup controller with no user claims
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
        };

        // Act
        var result = await _controller.EnsureMe();

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result.Result as UnauthorizedObjectResult;
        unauthorizedResult!.Value.Should().Be("Invalid user token");
    }

    [Fact]
    public async Task EnsureMe_WhenUserNotFoundInDatabase_ShouldReturnInternalServerError()
    {
        // Arrange
        _mockPlayerService.Setup(s => s.EnsureUserPlayerAsync(_testUserId, "Test User", "test@example.com"))
            .ThrowsAsync(new InvalidOperationException("User not found in database"));

        // Act
        var result = await _controller.EnsureMe();

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
        objectResult.Value.Should().Be("User account not properly initialized. Please try logging out and back in.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("not-a-guid")]
    [InlineData("12345")]
    public async Task GetPlayer_WithInvalidGuidFormats_ShouldReturnBadRequest(string invalidId)
    {
        // Act
        var result = await _controller.GetPlayer(invalidId);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Invalid GUID format");
    }

    [Fact]
    public async Task CreatePlayer_WhenServiceThrowsException_ShouldReturnInternalServerError()
    {
        // Arrange
        var createDto = TestDataBuilder.CreatePlayerDto()
            .WithName("Test Player")
            .Build();

        _mockPlayerService.Setup(s => s.CreatePlayerForUserAsync(createDto, _testUserId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.CreatePlayer(createDto);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
        objectResult.Value.Should().Be("An error occurred while creating the player");
    }

    [Fact]
    public async Task UpdatePlayer_WhenServiceThrowsException_ShouldReturnInternalServerError()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var updateDto = TestDataBuilder.UpdatePlayerDto()
            .WithName("Updated Player")
            .Build();

        _mockPlayerService.Setup(s => s.UpdatePlayerAsync(playerId, updateDto))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.UpdatePlayer(playerId.ToString(), updateDto);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
        objectResult.Value.Should().Be("An error occurred while updating the player");
    }

    [Fact]
    public async Task DeletePlayer_WhenServiceThrowsException_ShouldReturnInternalServerError()
    {
        // Arrange
        var playerId = Guid.NewGuid();

        _mockPlayerService.Setup(s => s.DeletePlayerAsync(playerId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.DeletePlayer(playerId.ToString());

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
        objectResult.Value.Should().Be("An error occurred while deleting the player");
    }
}

