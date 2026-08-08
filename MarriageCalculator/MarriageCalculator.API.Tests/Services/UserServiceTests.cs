using MarriageCalculator.API.Repositories;
using MarriageCalculator.API.Services;
using MarriageCalculator.Core.DTOs;
using MarriageCalculator.Core.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace MarriageCalculator.API.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _userService = new UserService(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task GetAllUsersAsync_ReturnsAllUsersAsDtos()
    {
        // Arrange
        var users = new List<User>
        {
            new() { Id = "1", UserId = "mock-1", DisplayName = "User 1", Email = "user1@test.com" },
            new() { Id = "2", UserId = "mock-2", DisplayName = "User 2", Email = "user2@test.com" }
        };

        _userRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

        // Act
        var result = (await _userService.GetAllUsersAsync()).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("User 1", result[0].DisplayName);
        Assert.Equal("mock-2", result[1].UserId);
    }

    [Fact]
    public async Task GetOrCreateUserFromClaimsAsync_UserExists_ReturnsExistingUser()
    {
        // Arrange
        var existingUser = new User
        {
            Id = "1",
            UserId = "mock-sanjeeb",
            DisplayName = "Sanjeeb",
            Email = "sanjeeb@test.com"
        };

        _userRepositoryMock.Setup(r => r.GetByUserIdAsync("mock-sanjeeb")).ReturnsAsync(existingUser);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "mock-sanjeeb"),
            new Claim(ClaimTypes.Name, "Sanjeeb"),
            new Claim(ClaimTypes.Email, "sanjeeb@test.com")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = await _userService.GetOrCreateUserFromClaimsAsync(principal);

        // Assert
        Assert.Equal("mock-sanjeeb", result.UserId);
        Assert.Equal("Sanjeeb", result.DisplayName);
        _userRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task GetOrCreateUserFromClaimsAsync_UserDoesNotExist_RegistersNewUser()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.GetByUserIdAsync("mock-new")).ReturnsAsync((User?)null);
        _userRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "mock-new"),
            new Claim(ClaimTypes.Name, "New User"),
            new Claim(ClaimTypes.Email, "new@test.com")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = await _userService.GetOrCreateUserFromClaimsAsync(principal);

        // Assert
        Assert.Equal("mock-new", result.UserId);
        Assert.Equal("New User", result.DisplayName);
        _userRepositoryMock.Verify(r => r.CreateAsync(It.Is<User>(u => u.UserId == "mock-new")), Times.Once);
    }
}
