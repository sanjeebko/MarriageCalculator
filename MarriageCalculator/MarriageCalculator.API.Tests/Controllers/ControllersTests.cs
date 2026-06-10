using MarriageCalculator.API.Controllers;
using MarriageCalculator.API.Services;
using MarriageCalculator.Core.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
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
}
