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

public class NudgeAndNotificationTests
{
    private static void SetControllerUser(ControllerBase controller, string userId, string email)
    {
        var claims = new[] { 
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task UsersController_RegisterFcmToken_SavesTokenAndReturnsOk()
    {
        // Arrange
        var userServiceMock = new Mock<IUserService>();
        var loggerMock = new Mock<ILogger<UsersController>>();
        var controller = new UsersController(userServiceMock.Object, loggerMock.Object);
        SetControllerUser(controller, "user-alice", "alice@example.com");

        var tokenDto = new RegisterFcmTokenDto { Token = "fcm-token-123" };
        
        userServiceMock.Setup(s => s.UpdateFcmTokenAsync("user-alice", "fcm-token-123"))
            .ReturnsAsync(true);

        // Act
        var result = await controller.RegisterFcmToken(tokenDto);

        // Assert
        Assert.IsType<OkResult>(result);
        userServiceMock.Verify(s => s.UpdateFcmTokenAsync("user-alice", "fcm-token-123"), Times.Once);
    }

    [Fact]
    public async Task MarriageGameSetsController_NudgePlayer_CallerIsHost_ResolvesTargetAndSendsNotification()
    {
        // Arrange
        var gameSetServiceMock = new Mock<IMarriageGameSetService>();
        var loggerMock = new Mock<ILogger<MarriageGameSetsController>>();
        var controller = new MarriageGameSetsController(gameSetServiceMock.Object, loggerMock.Object);
        SetControllerUser(controller, "host-user-id", "host@example.com");

        gameSetServiceMock.Setup(s => s.NudgePlayerAsync("set-1", "host-user-id", "player-2"))
            .ReturnsAsync(true);

        // Act
        var result = await controller.NudgePlayer("set-1", "player-2");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var resData = okResult.Value;
        Assert.NotNull(resData);
        gameSetServiceMock.Verify(s => s.NudgePlayerAsync("set-1", "host-user-id", "player-2"), Times.Once);
    }

    [Fact]
    public async Task MarriageGameSetsController_NudgePlayer_CallerIsNotHost_ReturnsForbid()
    {
        // Arrange
        var gameSetServiceMock = new Mock<IMarriageGameSetService>();
        var loggerMock = new Mock<ILogger<MarriageGameSetsController>>();
        var controller = new MarriageGameSetsController(gameSetServiceMock.Object, loggerMock.Object);
        SetControllerUser(controller, "guest-user-id", "guest@example.com");

        gameSetServiceMock.Setup(s => s.NudgePlayerAsync("set-1", "guest-user-id", "player-2"))
            .ThrowsAsync(new UnauthorizedAccessException("Only the game host can nudge other players."));

        // Act
        var result = await controller.NudgePlayer("set-1", "player-2");

        // Assert
        Assert.IsType<ForbidResult>(result);
    }
}
