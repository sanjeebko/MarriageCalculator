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

    private static FriendshipsController CreateFriendshipsController(
        Mock<IFriendshipService> serviceMock,
        Mock<IFriendInviteService>? inviteServiceMock = null)
    {
        inviteServiceMock ??= new Mock<IFriendInviteService>();
        var loggerMock = new Mock<ILogger<FriendshipsController>>();
        return new FriendshipsController(serviceMock.Object, inviteServiceMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task FriendshipsController_SendFriendRequest_ReturnsOkWithResultDto()
    {
        // Arrange
        var serviceMock = new Mock<IFriendshipService>();
        var controller = CreateFriendshipsController(serviceMock);
        SetControllerUser(controller, "user-alice", "alice@example.com");

        var requestDto = new SendFriendRequestDto { ReceiverEmailOrUsername = "bob@example.com" };
        var expectedResult = new FriendRequestResultDto
        {
            Status = "RequestSent",
            Message = "Request sent to bob@example.com.",
            Friendship = new FriendshipDto
            {
                Id = "friendship-1",
                RequesterUserId = "user-alice",
                ReceiverUserId = "user-bob",
                Status = "Pending"
            }
        };

        serviceMock.Setup(s => s.SendFriendRequestAsync("user-alice", It.IsAny<SendFriendRequestDto>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await controller.SendFriendRequest(requestDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returned = Assert.IsType<FriendRequestResultDto>(okResult.Value);
        Assert.Equal("RequestSent", returned.Status);
        Assert.Equal("friendship-1", returned.Friendship!.Id);
    }

    [Fact]
    public async Task FriendshipsController_SendFriendRequest_UnknownEmail_ReturnsSameGenericMessage()
    {
        // Anti-enumeration (requirement §4.4): an unregistered email must produce the
        // same Ok + generic message as a registered one — never an error.
        var serviceMock = new Mock<IFriendshipService>();
        var controller = CreateFriendshipsController(serviceMock);
        SetControllerUser(controller, "user-alice", "alice@example.com");

        serviceMock.Setup(s => s.SendFriendRequestAsync("user-alice", It.IsAny<SendFriendRequestDto>()))
            .ReturnsAsync(new FriendRequestResultDto
            {
                Status = "RequestSent",
                Message = "Request sent to stranger@example.com.",
                Friendship = null
            });

        var result = await controller.SendFriendRequest(
            new SendFriendRequestDto { ReceiverEmailOrUsername = "stranger@example.com" });

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returned = Assert.IsType<FriendRequestResultDto>(okResult.Value);
        Assert.Equal("RequestSent", returned.Status);
        Assert.Null(returned.Friendship);
    }

    [Fact]
    public async Task FriendshipsController_GetInviteCode_ReturnsCode()
    {
        var inviteMock = new Mock<IFriendInviteService>();
        var controller = CreateFriendshipsController(new Mock<IFriendshipService>(), inviteMock);
        SetControllerUser(controller, "user-alice", "alice@example.com");

        inviteMock.Setup(s => s.GetOrCreateInviteCodeAsync("user-alice"))
            .ReturnsAsync(new InviteCodeDto { Code = "K7PMQ4", ExpiresAt = DateTime.UtcNow.AddDays(7) });

        var result = await controller.GetInviteCode();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returned = Assert.IsType<InviteCodeDto>(okResult.Value);
        Assert.Equal("K7PMQ4", returned.Code);
    }

    [Fact]
    public async Task FriendshipsController_RedeemInviteCode_ReturnsAcceptedFriendship()
    {
        var inviteMock = new Mock<IFriendInviteService>();
        var controller = CreateFriendshipsController(new Mock<IFriendshipService>(), inviteMock);
        SetControllerUser(controller, "user-bob", "bob@example.com");

        inviteMock.Setup(s => s.RedeemInviteCodeAsync("user-bob", It.IsAny<RedeemInviteCodeDto>()))
            .ReturnsAsync(new RedeemInviteCodeResultDto
            {
                Message = "Code correct! You are now friends with Alice (a***@e***.com).",
                Friendship = new FriendshipDto { Id = "friendship-2", Status = "Accepted" }
            });

        var result = await controller.RedeemInviteCode(new RedeemInviteCodeDto { Code = "K7PMQ4" });

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returned = Assert.IsType<RedeemInviteCodeResultDto>(okResult.Value);
        Assert.Equal("Accepted", returned.Friendship!.Status);
        Assert.DoesNotContain("alice@example.com", returned.Message); // full email never exposed
    }

    [Fact]
    public async Task FriendshipsController_RedeemInviteCode_InvalidCode_ReturnsBadRequest()
    {
        var inviteMock = new Mock<IFriendInviteService>();
        var controller = CreateFriendshipsController(new Mock<IFriendshipService>(), inviteMock);
        SetControllerUser(controller, "user-bob", "bob@example.com");

        inviteMock.Setup(s => s.RedeemInviteCodeAsync("user-bob", It.IsAny<RedeemInviteCodeDto>()))
            .ThrowsAsync(new ArgumentException("Invalid or expired code."));

        var result = await controller.RedeemInviteCode(new RedeemInviteCodeDto { Code = "WRONG1" });

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task FriendshipsController_ClaimInvites_ReturnsClaimedCount()
    {
        var inviteMock = new Mock<IFriendInviteService>();
        var controller = CreateFriendshipsController(new Mock<IFriendshipService>(), inviteMock);
        SetControllerUser(controller, "user-carol", "carol@example.com");

        inviteMock.Setup(s => s.ClaimPendingInvitesAsync("user-carol"))
            .ReturnsAsync(new ClaimInvitesResultDto { Claimed = 2 });

        var result = await controller.ClaimInvites();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returned = Assert.IsType<ClaimInvitesResultDto>(okResult.Value);
        Assert.Equal(2, returned.Claimed);
    }

    [Fact]
    public void FriendInviteService_MaskEmail_NeverRevealsFullAddress()
    {
        Assert.Equal("s***@g***.com", MarriageCalculator.API.Services.FriendInviteService.MaskEmail("sanjeeb@gmail.com"));
        Assert.Equal("b***@e***.com", MarriageCalculator.API.Services.FriendInviteService.MaskEmail("bob@example.com"));
        Assert.Equal("***", MarriageCalculator.API.Services.FriendInviteService.MaskEmail("not-an-email"));
    }

    [Fact]
    public async Task FriendshipsController_RespondFriendRequest_ReturnsOkWithUpdatedFriendshipDto()
    {
        // Arrange
        var serviceMock = new Mock<IFriendshipService>();
        var controller = CreateFriendshipsController(serviceMock);
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

