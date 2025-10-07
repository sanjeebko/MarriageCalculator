using MarriageCalculator.API.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MarriageCalculator.API.Tests.Helpers;

/// <summary>
/// Custom WebApplicationFactory for integration tests that uses in-memory database
/// Authentication is handled by Program.cs in Testing environment
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName;

    // Test user constants for integration tests
    public static readonly Guid TestUserId = new("12345678-1234-1234-1234-123456789012");
    public static readonly string TestUserName = "TestUser";
    public static readonly string TestUserEmail = "test@example.com";

    public TestWebApplicationFactory()
    {
        // Use a single database name for the entire test session
        _databaseName = $"TestDb_{Guid.NewGuid()}";
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set test environment variables to avoid Program.cs validation errors
        Environment.SetEnvironmentVariable("MCDATABASE", "TestServer");
        Environment.SetEnvironmentVariable("MCUSER", "TestUser");
        Environment.SetEnvironmentVariable("MCPASSWORD", "TestPassword");

        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Clear existing configuration sources and add test-specific config
            config.Sources.Clear();
            config.AddJsonFile("appsettings.Testing.json", optional: false);
        });

        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<MarriageCalculatorDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database for testing with consistent name
            services.AddDbContext<MarriageCalculatorDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
                options.EnableSensitiveDataLogging();
            });

            // Ensure database is created and seed test data
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            try
            {
                var context = scope.ServiceProvider.GetRequiredService<MarriageCalculatorDbContext>();
                context.Database.EnsureCreated();
                
                // Seed test user if it doesn't exist
                SeedTestUser(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test database creation warning: {ex.Message}");
            }
        });
    }

    private static void SeedTestUser(MarriageCalculatorDbContext context)
    {
        // Check if test user already exists
        if (context.Users.Any(u => u.Id == TestUserId))
        {
            return;
        }

        // Create test user
        var testUser = new MarriageCalculator.Core.Models.User
        {
            Id = TestUserId,
            DisplayName = TestUserName,
            Email = TestUserEmail,
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        context.Users.Add(testUser);
        context.SaveChanges();
    }
}
