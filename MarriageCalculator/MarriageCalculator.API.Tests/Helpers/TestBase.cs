using MarriageCalculator.API.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace MarriageCalculator.API.Tests.Helpers;

/// <summary>
/// Base class for unit tests providing common setup and utilities
/// </summary>
public abstract class TestBase : IDisposable
{
    protected MarriageCalculatorDbContext DbContext { get; private set; }
    protected Mock<ILogger<T>> CreateMockLogger<T>() => new Mock<ILogger<T>>();

    protected TestBase()
    {
        DbContext = TestDbContextFactory.CreateInMemoryContext();
    }

    /// <summary>
    /// Sets up user claims for a controller to simulate authenticated user
    /// </summary>
    /// <param name="controller">The controller to setup claims for</param>
    /// <param name="userId">The user ID to set in claims</param>
    /// <param name="displayName">Optional display name</param>
    /// <param name="email">Optional email</param>
    protected static void SetupUserClaims(ControllerBase controller, Guid userId, string displayName = "Test User", string email = "test@example.com")
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, displayName),
            new(ClaimTypes.Email, email)
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    protected TestBase(string databaseName)
    {
        DbContext = TestDbContextFactory.CreateInMemoryContext(databaseName);
    }

    public virtual void Dispose()
    {
        DbContext?.Dispose();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Base class for integration tests
/// </summary>
public abstract class IntegrationTestBase : IDisposable
{
    protected MarriageCalculatorDbContext DbContext { get; private set; }

    protected IntegrationTestBase()
    {
        DbContext = TestDbContextFactory.CreateInMemoryContext();
    }

    public virtual void Dispose()
    {
        DbContext?.Dispose();
        GC.SuppressFinalize(this);
    }
}
