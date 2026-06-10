using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Xunit;
using MarriageCalculator.API.Authentication;

namespace MarriageCalculator.API.Tests.Authentication;

public class AuthenticationTests
{
    private static FirebaseOrMockAuthenticationHandler CreateHandler(
        HttpContext context, 
        FirebaseOrMockAuthenticationOptions options)
    {
        var optionsMonitorMock = new Mock<IOptionsMonitor<FirebaseOrMockAuthenticationOptions>>();
        optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);

        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(l => l.CreateLogger(It.IsAny<string>())).Returns(new Mock<ILogger>().Object);

        var handler = new FirebaseOrMockAuthenticationHandler(
            optionsMonitorMock.Object,
            loggerFactoryMock.Object,
            UrlEncoder.Default
        );

        handler.InitializeAsync(new AuthenticationScheme("FirebaseOrMock", null, typeof(FirebaseOrMockAuthenticationHandler)), context).Wait();
        return handler;
    }

    [Fact]
    public async Task Authenticate_NoHeader_ReturnsNoResult()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var handler = CreateHandler(context, new FirebaseOrMockAuthenticationOptions());

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.True(result.None);
    }

    [Fact]
    public async Task Authenticate_InvalidHeaderFormat_ReturnsNoResult()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["Authorization"] = "InvalidFormat xyz";
        var handler = CreateHandler(context, new FirebaseOrMockAuthenticationOptions());

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.True(result.None);
    }

    [Fact]
    public async Task Authenticate_MockToken_SucceedsWithClaims()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["Authorization"] = "Bearer mock-sanjeeb";
        var handler = CreateHandler(context, new FirebaseOrMockAuthenticationOptions());

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Principal);
        
        var userIdClaim = result.Principal.FindFirst(ClaimTypes.NameIdentifier);
        var nameClaim = result.Principal.FindFirst(ClaimTypes.Name);
        var emailClaim = result.Principal.FindFirst(ClaimTypes.Email);

        Assert.Equal("mock-sanjeeb", userIdClaim?.Value);
        Assert.Equal("sanjeeb", nameClaim?.Value);
        Assert.Equal("sanjeeb@marriagecalculator.local", emailClaim?.Value);
    }
}
