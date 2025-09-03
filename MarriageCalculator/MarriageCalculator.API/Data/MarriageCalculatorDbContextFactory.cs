using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace MarriageCalculator.API.Data;

/// <summary>
/// Design-time factory so EF Tools can create the DbContext without running the web host.
/// Chooses a sensible connection string for development and replaces environment placeholders if provided.
/// </summary>
public class MarriageCalculatorDbContextFactory : IDesignTimeDbContextFactory<MarriageCalculatorDbContext>
{
    public MarriageCalculatorDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        // Try to get preferred connection name from settings; default to LocalSqlServer for design-time
        var preferredName = configuration["DatabaseSettings:UseConnectionString"] ?? "LocalSqlServer";

        // Start with named connection or DefaultConnection
        var connectionString = configuration.GetConnectionString(preferredName)
                               ?? configuration.GetConnectionString("DefaultConnection");

        // If DefaultConnection uses placeholders, try to replace from environment variables
        var dbServer = Environment.GetEnvironmentVariable("MCDATABASE");
        var dbUser = Environment.GetEnvironmentVariable("MCUSER");
        var dbPassword = Environment.GetEnvironmentVariable("MCPASSWORD");

        if (!string.IsNullOrEmpty(dbServer) && !string.IsNullOrEmpty(dbUser) && !string.IsNullOrEmpty(dbPassword) && !string.IsNullOrEmpty(connectionString))
        {
            connectionString = connectionString
                .Replace("{MCDATABASE}", dbServer)
                .Replace("{MCUSER}", dbUser)
                .Replace("{MCPASSWORD}", dbPassword);
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("No connection string could be determined for design-time DbContext creation.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<MarriageCalculatorDbContext>();
        optionsBuilder.UseSqlServer(connectionString, sql =>
        {
            sql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
            sql.CommandTimeout(60);
        });

        return new MarriageCalculatorDbContext(optionsBuilder.Options);
    }
}
