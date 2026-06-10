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

public class FriendshipsAndPermissionsTests
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
    public async Task FriendshipsController_SendFriendRequest_ReturnsOkWithFriendshipDto()
    {
        // Arrange
        var serviceMock = new Mock<IFriendshipService>();
        var loggerMock = new Mock<ILogger<FriendshipsController>>();
        var controller = new FriendshipsController(serviceMock.Object, loggerMock.Object);
        SetControllerUser(controller, "user-alice", "alice@example.com");

        var requestDto = new SendFriendRequestDto { ReceiverEmailOrUsername = "bob@example.com" };
        var expectedDto = new FriendshipDto 
        { 
            Id = "friendship-1", 
            RequesterUserId = "user-alice", 
            ReceiverUserId = "user-bob", 
            Status = "Pending" 
        };

        serviceMock.Setup(s => s.SendFriendRequestAsync("user-alice", It.IsAny<SendFriendRequestDto>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await controller.SendFriendRequest(requestDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returned = Assert.IsType<FriendshipDto>(okResult.Value);
        Assert.Equal("friendship-1", returned.Id);
        Assert.Equal("Pending", returned.Status);
    }

    [Fact]
    public async Task FriendshipsController_RespondFriendRequest_ReturnsOkWithUpdatedFriendshipDto()
    {
        // Arrange
        var serviceMock = new Mock<IFriendshipService>();
        var loggerMock = new Mock<ILogger<FriendshipsController>>();
        var controller = new FriendshipsController(serviceMock.Object, loggerMock.Object);
        SetControllerUser(controller, "user-bob", "bob@example.com");

        var respondDto = new RespondFriendRequestDto { Accept = true };
        var expectedDto = new FriendshipDto 
        { 
            Id = "friendship-1", 
            RequesterUserId = "user-alice", 
            ReceiverUserId = "user-bob", 
            Status = "Accepted" 
        };

        serviceMock.Setup(s => s.RespondFriendRequestAsync("friendship-1", "user-bob", It.IsAny<RespondFriendRequestDto>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await controller.RespondFriendRequest("friendship-1", respondDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returned = Assert.IsType<FriendshipDto>(okResult.Value);
        Assert.Equal("Accepted", returned.Status);
    }

    [Fact]
    public async Task MarriageGameSetsController_GetMarriageGameSet_ParticipantAccess_ReturnsOk()
    {
        // Arrange
        var serviceMock = new Mock<IMarriageGameSetService>();
        var loggerMock = new Mock<ILogger<MarriageGameSetsController>>();
        var controller = new MarriageGameSetsController(serviceMock.Object, loggerMock.Object);
        // Authenticated user is 'user-bob', who is NOT the host 'user-alice'
        SetControllerUser(controller, "user-bob", "bob@example.com");

        var gameSetDto = new MarriageGameSetDto 
        { 
            Id = "set-1", 
            HostUserId = "user-alice", 
            Name = "Alice's game", 
            PlayerIds = new List<string> { "player-bob-id" } 
        };

        // Service returns the game set since user-bob is authorized
        serviceMock.Setup(s => s.GetGameSetByIdAsync("set-1", "user-bob", "bob@example.com"))
            .ReturnsAsync(gameSetDto);

        // Act
        var result = await controller.GetMarriageGameSet("set-1");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returned = Assert.IsType<MarriageGameSetDto>(okResult.Value);
        Assert.Equal("user-alice", returned.HostUserId);
        Assert.Contains("player-bob-id", returned.PlayerIds);
    }

    [Fact]
    public async Task MarriageGameSetsController_TransferHost_ReturnsOkWithUpdatedGameSet()
    {
        // Arrange
        var serviceMock = new Mock<IMarriageGameSetService>();
        var loggerMock = new Mock<ILogger<MarriageGameSetsController>>();
        var controller = new MarriageGameSetsController(serviceMock.Object, loggerMock.Object);
        SetControllerUser(controller, "user-alice", "alice@example.com");

        var transferDto = new TransferHostDto { NewHostUserId = "user-bob" };
        var expectedDto = new MarriageGameSetDto 
        { 
            Id = "set-1", 
            HostUserId = "user-bob", 
            Name = "Alice's game" 
        };

        serviceMock.Setup(s => s.TransferHostAsync("set-1", "user-alice", "user-bob"))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await controller.TransferHost("set-1", transferDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returned = Assert.IsType<MarriageGameSetDto>(okResult.Value);
        Assert.Equal("user-bob", returned.HostUserId);
    }
}
