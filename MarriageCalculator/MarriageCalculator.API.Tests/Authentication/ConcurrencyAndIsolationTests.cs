using MarriageCalculator.API.Controllers;
using MarriageCalculator.API.Services;
using MarriageCalculator.Core.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace MarriageCalculator.API.Tests.Authentication;

public class ConcurrencyAndIsolationTests
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
    public async Task MarriageGameSetsController_ConcurrentRequests_IsolateUserDataWithoutCrossTalk()
    {
        // Arrange
        var serviceMock = new Mock<IMarriageGameSetService>();
        var loggerMock = new Mock<ILogger<MarriageGameSetsController>>();
        var controller = new MarriageGameSetsController(serviceMock.Object, loggerMock.Object);

        // Define simulated users and their private game sets
        var users = new[] { "mock-user-alice", "mock-user-bob", "mock-user-charlie", "mock-user-dave" };
        var userGameSets = new Dictionary<string, List<MarriageGameSetDto>>
        {
            ["mock-user-alice"] = new() { new() { Id = "set-a1", HostUserId = "mock-user-alice", Name = "Alice Set" } },
            ["mock-user-bob"] = new() { new() { Id = "set-b1", HostUserId = "mock-user-bob", Name = "Bob Set" } },
            ["mock-user-charlie"] = new() { new() { Id = "set-c1", HostUserId = "mock-user-charlie", Name = "Charlie Set" } },
            ["mock-user-dave"] = new() { new() { Id = "set-d1", HostUserId = "mock-user-dave", Name = "Dave Set" } }
        };

        // Set up the mocked service to filter returns strictly based on hostUserId
        serviceMock.Setup(s => s.GetAllGameSetsAsync(It.IsAny<string>()))
            .ReturnsAsync((string hostUserId) => userGameSets.ContainsKey(hostUserId) ? userGameSets[hostUserId] : new List<MarriageGameSetDto>());

        var concurrentResults = new ConcurrentDictionary<string, List<MarriageGameSetDto>>();

        // Act - Simulate concurrent execution of requests from multiple users
        var tasks = users.Select(async userId =>
        {
            // Create a controller instance per thread/request to match web framework behavior
            var requestController = new MarriageGameSetsController(serviceMock.Object, loggerMock.Object);
            SetControllerUser(requestController, userId);

            var actionResult = await requestController.GetMarriageGameSets();
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedSets = Assert.IsType<List<MarriageGameSetDto>>(okResult.Value);
            
            concurrentResults[userId] = returnedSets;
        });

        await Task.WhenAll(tasks);

        // Assert - Verify complete isolation of results
        foreach (var userId in users)
        {
            var results = concurrentResults[userId];
            Assert.Single(results);
            Assert.Equal(userId, results.First().HostUserId);
        }
    }

    [Fact]
    public async Task GameSettingsController_ConcurrentRequests_IsolateSettingsByUserId()
    {
        // Arrange
        var serviceMock = new Mock<IGameSettingsService>();
        var loggerMock = new Mock<ILogger<GameSettingsController>>();

        var users = new[] { "mock-user-1", "mock-user-2", "mock-user-3" };
        var settingsStore = new Dictionary<string, GameSettingsDto>
        {
            ["mock-user-1"] = new() { Id = "settings-1", UserId = "mock-user-1", Currency = "NPR" },
            ["mock-user-2"] = new() { Id = "settings-2", UserId = "mock-user-2", Currency = "USD" },
            ["mock-user-3"] = new() { Id = "settings-3", UserId = "mock-user-3", Currency = "INR" }
        };

        serviceMock.Setup(s => s.GetAllGameSettingsAsync(It.IsAny<string>()))
            .ReturnsAsync((string userId) => settingsStore.ContainsKey(userId) ? new List<GameSettingsDto> { settingsStore[userId] } : new List<GameSettingsDto>());

        var concurrentResults = new ConcurrentDictionary<string, List<GameSettingsDto>>();

        // Act
        var tasks = users.Select(async userId =>
        {
            var requestController = new GameSettingsController(serviceMock.Object, loggerMock.Object);
            SetControllerUser(requestController, userId);

            var actionResult = await requestController.GetGameSettings();
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedSettings = Assert.IsType<List<GameSettingsDto>>(okResult.Value);

            concurrentResults[userId] = returnedSettings;
        });

        await Task.WhenAll(tasks);

        // Assert
        foreach (var userId in users)
        {
            var results = concurrentResults[userId];
            Assert.Single(results);
            Assert.Equal(userId, results.First().UserId);
            Assert.Equal(settingsStore[userId].Currency, results.First().Currency);
        }
    }
}
