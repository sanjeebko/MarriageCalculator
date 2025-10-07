using MarriageCalculator.API.Data;
using Microsoft.EntityFrameworkCore;

namespace MarriageCalculator.API.Tests.Helpers;

/// <summary>
/// Factory class for creating test database contexts with in-memory database
/// </summary>
public static class TestDbContextFactory
{
    /// <summary>
    /// Creates a new in-memory database context for testing
    /// </summary>
    /// <param name="databaseName">Optional database name. If not provided, a unique name will be generated</param>
    /// <returns>A configured MarriageCalculatorDbContext for testing</returns>
    public static MarriageCalculatorDbContext CreateInMemoryContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<MarriageCalculatorDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;

        return new MarriageCalculatorDbContext(options);
    }

    /// <summary>
    /// Creates a new in-memory database context with seed data
    /// </summary>
    /// <param name="databaseName">Optional database name</param>
    /// <returns>A configured MarriageCalculatorDbContext with test data</returns>
    public static MarriageCalculatorDbContext CreateInMemoryContextWithData(string? databaseName = null)
    {
        var context = CreateInMemoryContext(databaseName);
        SeedTestData(context);
        return context;
    }

    /// <summary>
    /// Seeds the database context with test data
    /// </summary>
    /// <param name="context">The database context to seed</param>
    private static void SeedTestData(MarriageCalculatorDbContext context)
    {
        // Add sample test data here if needed
        // This method can be expanded to include common test data
        context.SaveChanges();
    }
}
