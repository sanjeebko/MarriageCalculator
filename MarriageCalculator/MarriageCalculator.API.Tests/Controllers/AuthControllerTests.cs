using MarriageCalculator.API.Controllers;
using MarriageCalculator.API.Services;
using MarriageCalculator.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MarriageCalculator.API.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _loggerMock = new Mock<ILogger<AuthController>>();
        _controller = new AuthController(_authServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task SendVerificationCode_Success_ReturnsOk()
    {
        // Arrange
        var request = new SendVerificationCodeRequestDto { Email = "test@example.com" };
        var expectedResult = new SendVerificationCodeResultDto { Success = true, Message = "Code sent." };
        _authServiceMock.Setup(s => s.SendVerificationCodeAsync("test@example.com"))
            .ReturnsAsync(expectedResult);

        // Act
        var actionResult = await _controller.SendVerificationCode(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var resultDto = Assert.IsType<SendVerificationCodeResultDto>(okResult.Value);
        Assert.True(resultDto.Success);
    }

    [Fact]
    public async Task Register_Success_ReturnsAuthToken()
    {
        // Arrange
        var request = new RegisterUserDto
        {
            Email = "newuser@example.com",
            VerificationCode = "123456",
            Username = "newuser",
            Password = "Password123!",
            DisplayName = "New User"
        };

        var expectedResult = new AuthTokenResultDto
        {
            Token = "mock.jwt.token",
            UserId = "user_123",
            Username = "newuser",
            Email = "newuser@example.com",
            DisplayName = "New User",
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _authServiceMock.Setup(s => s.RegisterAsync(request))
            .ReturnsAsync(expectedResult);

        // Act
        var actionResult = await _controller.Register(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var tokenDto = Assert.IsType<AuthTokenResultDto>(okResult.Value);
        Assert.Equal("mock.jwt.token", tokenDto.Token);
        Assert.Equal("newuser", tokenDto.Username);
    }

    [Fact]
    public async Task Login_Success_ReturnsAuthToken()
    {
        // Arrange
        var request = new LoginDto
        {
            UsernameOrEmail = "newuser",
            Password = "Password123!"
        };

        var expectedResult = new AuthTokenResultDto
        {
            Token = "mock.jwt.token",
            UserId = "user_123",
            Username = "newuser",
            Email = "newuser@example.com",
            DisplayName = "New User",
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _authServiceMock.Setup(s => s.LoginAsync(request))
            .ReturnsAsync(expectedResult);

        // Act
        var actionResult = await _controller.Login(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var tokenDto = Assert.IsType<AuthTokenResultDto>(okResult.Value);
        Assert.Equal("mock.jwt.token", tokenDto.Token);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginDto
        {
            UsernameOrEmail = "wronguser",
            Password = "WrongPassword"
        };

        _authServiceMock.Setup(s => s.LoginAsync(request))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid credentials."));

        // Act
        var actionResult = await _controller.Login(request);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(actionResult.Result);
    }
}
