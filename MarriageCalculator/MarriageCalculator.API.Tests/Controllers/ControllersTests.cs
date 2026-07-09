using MarriageCalculator.API.Controllers;
using MarriageCalculator.API.Services;
using MarriageCalculator.Core.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace MarriageCalculator.API.Tests.Controllers;

public class ControllersTests
{
    private static void SetControllerUser(ControllerBase controller, string userId)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task UsersController_Login_ReturnsOkWithUserDto()
    {
        // Arrange
        var serviceMock = new Mock<IUserService>();
        var loggerMock = new Mock<ILogger<UsersController>>();
        var controller = new UsersController(serviceMock.Object, loggerMock.Object);
        SetControllerUser(controller, "mock-sanjeeb");

        var userDto = new UserDto { UserId = "mock-sanjeeb", DisplayName = "Sanjeeb" };
        serviceMock.Setup(s => s.GetOrCreateUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(userDto);

        // Act
        var result = await controller.Login();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedUser = Assert.IsType<UserDto>(okResult.Value);
        Assert.Equal("mock-sanjeeb", returnedUser.UserId);
    }

    [Fact]
    public async Task GameSettingsController_GetGameSettings_FiltersByUserId()
    {
        // Arrange
        var serviceMock = new Mock<IGameSettingsService>();
        var loggerMock = new Mock<ILogger<GameSettingsController>>();
        var controller = new GameSettingsController(serviceMock.Object, loggerMock.Object);
        SetControllerUser(controller, "mock-sanjeeb");

        serviceMock.Setup(s => s.GetAllGameSettingsAsync("mock-sanjeeb"))
            .ReturnsAsync(new List<GameSettingsDto>());

        // Act
        var result = await controller.GetGameSettings();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        serviceMock.Verify(s => s.GetAllGameSettingsAsync("mock-sanjeeb"), Times.Once);
    }

    [Fact]
    public async Task MarriageGameSetsController_GetLatestActiveGameSet_FiltersByHostUserId()
    {
        // Arrange
        var serviceMock = new Mock<IMarriageGameSetService>();
        var loggerMock = new Mock<ILogger<MarriageGameSetsController>>();
        var controller = new MarriageGameSetsController(serviceMock.Object, loggerMock.Object);
        SetControllerUser(controller, "mock-host-456");

        var gameSetDto = new MarriageGameSetDto { Id = "set-1", HostUserId = "mock-host-456", IsActive = true };
        serviceMock.Setup(s => s.GetLatestActiveGameSetAsync("mock-host-456"))
            .ReturnsAsync(gameSetDto);

        // Act
        var result = await controller.GetLatestActiveGameSet();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedSet = Assert.IsType<MarriageGameSetDto>(okResult.Value);
        Assert.Equal("mock-host-456", returnedSet.HostUserId);
        serviceMock.Verify(s => s.GetLatestActiveGameSetAsync("mock-host-456"), Times.Once);
    }

    [Fact]
    public async Task MarriageGameSetsController_SubmitRound_ReturnsOkWithRound()
    {
        // Arrange
        var serviceMock = new Mock<IMarriageGameSetService>();
        var loggerMock = new Mock<ILogger<MarriageGameSetsController>>();
        var controller = new MarriageGameSetsController(serviceMock.Object, loggerMock.Object);
        SetControllerUser(controller, "mock-host-456");

        var submitDto = new SubmitRoundDto
        {
            WinnerId = "p1",
            DealerId = "p2",
            Players = new List<RoundPlayerInputDto>
            {
                new() { PlayerId = "p1", Seen = true, Maal = 10 },
                new() { PlayerId = "p2", Seen = true, Maal = 4 }
            }
        };
        var roundDto = new MarriageGameRoundDto { Id = "round-1", Sequence = 1, MarriageGameSetId = "set-1" };
        serviceMock.Setup(s => s.SubmitRoundAsync("set-1", "mock-host-456", submitDto))
            .ReturnsAsync(roundDto);

        // Act
        var result = await controller.SubmitRound("set-1", submitDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedRound = Assert.IsType<MarriageGameRoundDto>(okResult.Value);
        Assert.Equal("round-1", returnedRound.Id);
    }

    [Fact]
    public async Task MarriageGameSetsController_SubmitRound_NonHost_ReturnsForbid()
    {
        // Arrange
        var serviceMock = new Mock<IMarriageGameSetService>();
        var loggerMock = new Mock<ILogger<MarriageGameSetsController>>();
        var controller = new MarriageGameSetsController(serviceMock.Object, loggerMock.Object);
        SetControllerUser(controller, "not-the-host");

        var submitDto = new SubmitRoundDto { WinnerId = "p1", Players = new List<RoundPlayerInputDto> { new() { PlayerId = "p1" }, new() { PlayerId = "p2" } } };
        serviceMock.Setup(s => s.SubmitRoundAsync("set-1", "not-the-host", submitDto))
            .ThrowsAsync(new UnauthorizedAccessException("Only the game host can add rounds."));

        // Act
        var result = await controller.SubmitRound("set-1", submitDto);

        // Assert
        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public async Task MarriageGameSetsController_SubmitRound_InvalidWinner_ReturnsBadRequest()
    {
        // Arrange
        var serviceMock = new Mock<IMarriageGameSetService>();
        var loggerMock = new Mock<ILogger<MarriageGameSetsController>>();
        var controller = new MarriageGameSetsController(serviceMock.Object, loggerMock.Object);
        SetControllerUser(controller, "mock-host-456");

        var submitDto = new SubmitRoundDto { WinnerId = "not-a-player", Players = new List<RoundPlayerInputDto> { new() { PlayerId = "p1" }, new() { PlayerId = "p2" } } };
        serviceMock.Setup(s => s.SubmitRoundAsync("set-1", "mock-host-456", submitDto))
            .ThrowsAsync(new ArgumentException("Winner must be one of the players in this round."));

        // Act
        var result = await controller.SubmitRound("set-1", submitDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task MarriageGameSetsController_CloseRound_ReturnsOkWithRound()
    {
        // Arrange
        var serviceMock = new Mock<IMarriageGameSetService>();
        var loggerMock = new Mock<ILogger<MarriageGameSetsController>>();
        var controller = new MarriageGameSetsController(serviceMock.Object, loggerMock.Object);
        SetControllerUser(controller, "mock-host-456");

        var roundDto = new MarriageGameRoundDto { Id = "round-1", Sequence = 1, MarriageGameSetId = "set-1", Completed = true };
        serviceMock.Setup(s => s.CloseRoundAsync("set-1", "round-1", "mock-host-456"))
            .ReturnsAsync(roundDto);

        // Act
        var result = await controller.CloseRound("set-1", "round-1");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedRound = Assert.IsType<MarriageGameRoundDto>(okResult.Value);
        Assert.True(returnedRound.Completed);
    }

    [Fact]
    public async Task MarriageGameSetsController_CloseRound_UnknownRound_ReturnsNotFound()
    {
        // Arrange
        var serviceMock = new Mock<IMarriageGameSetService>();
        var loggerMock = new Mock<ILogger<MarriageGameSetsController>>();
        var controller = new MarriageGameSetsController(serviceMock.Object, loggerMock.Object);
        SetControllerUser(controller, "mock-host-456");

        serviceMock.Setup(s => s.CloseRoundAsync("set-1", "missing-round", "mock-host-456"))
            .ReturnsAsync((MarriageGameRoundDto?)null);

        // Act
        var result = await controller.CloseRound("set-1", "missing-round");

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }
}
