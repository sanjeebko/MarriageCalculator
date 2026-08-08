using MarriageCalculator.API.Repositories;
using MarriageCalculator.API.Services;
using MarriageCalculator.Core.DTOs;
using MarriageCalculator.Core.Models;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MarriageCalculator.API.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IEmailVerificationCodeRepository> _verificationCodeRepositoryMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _verificationCodeRepositoryMock = new Mock<IEmailVerificationCodeRepository>();
        _emailServiceMock = new Mock<IEmailService>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _loggerMock = new Mock<ILogger<AuthService>>();

        _service = new AuthService(
            _userRepositoryMock.Object,
            _verificationCodeRepositoryMock.Object,
            _emailServiceMock.Object,
            _jwtTokenServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task SendVerificationCodeAsync_ValidEmail_CreatesCodeAndSendsEmail()
    {
        // Act
        var result = await _service.SendVerificationCodeAsync("test@example.com");

        // Assert
        Assert.True(result.Success);
        _verificationCodeRepositoryMock.Verify(r => r.CreateCodeAsync(It.Is<EmailVerificationCode>(c => c.Email == "test@example.com")), Times.Once);
        _emailServiceMock.Verify(e => e.SendAsync("test@example.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ValidOTP_RegistersUserAndReturnsToken()
    {
        // Arrange
        var registerDto = new RegisterUserDto
        {
            Email = "user@example.com",
            VerificationCode = "654321",
            Username = "testuser",
            Password = "SecurePassword123!",
            DisplayName = "Test User"
        };

        var validCode = new EmailVerificationCode
        {
            Id = "code_1",
            Email = "user@example.com",
            Code = "654321",
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false
        };

        _verificationCodeRepositoryMock.Setup(r => r.GetLatestValidCodeAsync("user@example.com"))
            .ReturnsAsync(validCode);

        _userRepositoryMock.Setup(r => r.GetByUsernameAsync("testuser"))
            .ReturnsAsync((User?)null);

        _userRepositoryMock.Setup(r => r.GetByEmailAsync("user@example.com"))
            .ReturnsAsync((User?)null);

        _jwtTokenServiceMock.Setup(j => j.GenerateToken(It.IsAny<User>()))
            .Returns(("jwt.token.valid", DateTime.UtcNow.AddDays(7)));

        // Act
        var result = await _service.RegisterAsync(registerDto);

        // Assert
        Assert.Equal("jwt.token.valid", result.Token);
        Assert.Equal("testuser", result.Username);
        _verificationCodeRepositoryMock.Verify(r => r.MarkCodeAsUsedAsync("code_1"), Times.Once);
        _userRepositoryMock.Verify(r => r.CreateAsync(It.Is<User>(u => u.Username == "testuser")), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_InvalidOTP_ThrowsInvalidOperationException()
    {
        // Arrange
        var registerDto = new RegisterUserDto
        {
            Email = "user@example.com",
            VerificationCode = "000000",
            Username = "testuser",
            Password = "SecurePassword123!"
        };

        _verificationCodeRepositoryMock.Setup(r => r.GetLatestValidCodeAsync("user@example.com"))
            .ReturnsAsync((EmailVerificationCode?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RegisterAsync(registerDto));
    }
}
