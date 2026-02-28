using MarriageCalculator.API.Data;
using MarriageCalculator.API.Hubs;
using MarriageCalculator.API.Repositories;
using MarriageCalculator.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger/OpenAPI with enhanced documentation
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Marriage Calculator API",
        Version = "v1",
        Description = "A comprehensive API for managing marriage card game calculations, player management, and game statistics.",
        Contact = new OpenApiContact
        {
            Name = "Marriage Calculator Team",
            Email = "support@marriagecalculator.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Include XML documentation for better API docs
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Add authorization definitions if needed in the future
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Enable annotations for better documentation
    c.EnableAnnotations();
    
    // Custom schema IDs to avoid conflicts
    c.CustomSchemaIds(type => type.FullName);
});

// Configure SQL Server with retry logic and enhanced settings
builder.Services.AddDbContext<MarriageCalculatorDbContext>(options =>
{
    // Get base connection string from configuration
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    // Get database configuration from environment variables (no fallback values)
    var dbServer = Environment.GetEnvironmentVariable("MCDATABASE");
    var dbUser = Environment.GetEnvironmentVariable("MCUSER");
    var dbPassword = Environment.GetEnvironmentVariable("MCPASSWORD");
    
    // Validate that all required environment variables are set
    if (string.IsNullOrEmpty(dbServer))
        throw new InvalidOperationException("MCDATABASE environment variable is required but not set.");
    
    if (string.IsNullOrEmpty(dbUser))
        throw new InvalidOperationException("MCUSER environment variable is required but not set.");
    
    if (string.IsNullOrEmpty(dbPassword))
        throw new InvalidOperationException("MCPASSWORD environment variable is required but not set.");
    
    // Replace placeholders with actual environment values
    connectionString = connectionString?
        .Replace("{MCDATABASE}", dbServer)
        .Replace("{MCUSER}", dbUser)
        .Replace("{MCPASSWORD}", dbPassword);
    
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

// Add SignalR for real-time game updates
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    
    // Enhanced Swagger UI Configuration
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Marriage Calculator API v1");
        c.RoutePrefix = string.Empty; // Makes Swagger UI available at the app's root URL
        c.DocumentTitle = "Marriage Calculator API Documentation";
        c.DisplayRequestDuration();
        c.EnableTryItOutByDefault();
        c.EnableDeepLinking();
        c.EnableFilter();
        c.MaxDisplayedTags(10);
        c.ShowExtensions();
        c.ShowCommonExtensions();
        c.EnableValidator();
        
        // Configure supported HTTP methods
        c.SupportedSubmitMethods(SubmitMethod.Get, SubmitMethod.Post, SubmitMethod.Put, SubmitMethod.Delete, SubmitMethod.Patch);
        
        // Custom CSS for better appearance
        c.InjectStylesheet("/swagger-ui/custom.css");
        
        // Custom JavaScript for enhanced functionality
        c.InjectJavascript("/swagger-ui/custom.js");
        
        // Default model expansion
        c.DefaultModelExpandDepth(2);
        c.DefaultModelsExpandDepth(1);
        
        // Configure API explorer settings
        c.DocExpansion(DocExpansion.List);
        c.DefaultModelRendering(ModelRendering.Example);
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Serve static files for custom Swagger UI assets
app.UseStaticFiles();

// Map controllers
app.MapControllers();

// Map SignalR hubs
app.MapHub<GameHub>("/hubs/game");

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