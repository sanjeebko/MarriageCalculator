using MarriageCalculator.API.Data;
using MarriageCalculator.API.Repositories;
using MarriageCalculator.API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure SQL Server with retry logic and enhanced settings
builder.Services.AddDbContext<MarriageCalculatorDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        // Enable retry on failure for transient errors
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: builder.Configuration.GetValue<int>("DatabaseSettings:MaxRetryCount", 3),
            maxRetryDelay: TimeSpan.Parse(builder.Configuration.GetValue<string>("DatabaseSettings:MaxRetryDelay") ?? "00:00:30"),
            errorNumbersToAdd: null);
        
        // Set command timeout
        sqlOptions.CommandTimeout(60);
    });
    
    // Enable detailed errors in development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
    }
});

// Register repositories
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<IGameSettingsRepository, GameSettingsRepository>();
builder.Services.AddScoped<IMarriageGameSetRepository, MarriageGameSetRepository>();
builder.Services.AddScoped<IMarriageGameRepository, MarriageGameRepository>();
builder.Services.AddScoped<IDatabaseRepository, DatabaseRepository>();

// Register services
builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<IGameSettingsService, GameSettingsService>();
builder.Services.AddScoped<IMarriageGameSetService, MarriageGameSetService>();
builder.Services.AddScoped<IMarriageGameService, MarriageGameService>();
builder.Services.AddScoped<IDatabaseService, DatabaseService>();

// Register existing services
builder.Services.AddScoped<IMarriageGameServices, MarriageGameServices>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Simple database initialization - let EF handle everything automatically
await InitializeDatabaseAsync(app);

app.Run();

static async Task InitializeDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Starting database initialization...");
        
        var context = services.GetRequiredService<MarriageCalculatorDbContext>();
        
        // Let Entity Framework handle database and table creation automatically
        logger.LogInformation("Ensuring database is created...");
        await context.Database.EnsureCreatedAsync();
        
        logger.LogInformation("Database initialization completed successfully.");
        
        // Seed default data if needed
        logger.LogInformation("Seeding default data...");
        var dbInitializer = services.GetRequiredService<IMarriageGameServices>();
        await dbInitializer.SeedDefaultData();
        
        logger.LogInformation("Database setup completed successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during database initialization: {Message}", ex.Message);
        logger.LogWarning("Application will start but database may not be properly initialized.");
    }
}