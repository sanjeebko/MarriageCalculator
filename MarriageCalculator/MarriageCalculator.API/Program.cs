using MarriageCalculator.API.Authentication;
using MarriageCalculator.API.Data;
using MarriageCalculator.API.Hubs;
using MarriageCalculator.API.Repositories;
using MarriageCalculator.API.Services;
using MongoDB.Driver;
using Scalar.AspNetCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure OpenAPI and Scalar API Reference UI
builder.Services.AddOpenApi();

// Configure MongoDB
builder.Services.Configure<MongoDbSettings>(options =>
{
    var dbServer = Environment.GetEnvironmentVariable("MCDATABASE")
        ?? throw new InvalidOperationException("MCDATABASE environment variable is required but not set.");
    var dbUser = Environment.GetEnvironmentVariable("MCUSER")
        ?? throw new InvalidOperationException("MCUSER environment variable is required but not set.");
    var dbPassword = Environment.GetEnvironmentVariable("MCPASSWORD")
        ?? throw new InvalidOperationException("MCPASSWORD environment variable is required but not set.");
    var dbName = Environment.GetEnvironmentVariable("MCDATABASENAME")
        ?? throw new InvalidOperationException("MCDATABASENAME environment variable is required but not set.");

    var rawConnectionString = builder.Configuration["MongoDbSettings:ConnectionString"]
        ?? "mongodb://{MCUSER}:{MCPASSWORD}@{MCDATABASE}/{MCDATABASENAME}?authSource={MCDATABASENAME}";

    options.ConnectionString = rawConnectionString
        .Replace("{MCDATABASE}", dbServer)
        .Replace("{MCUSER}", dbUser)
        .Replace("{MCPASSWORD}", dbPassword)
        .Replace("{MCDATABASENAME}", dbName);

    options.DatabaseName = dbName;
});

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MongoDbSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddSingleton<MongoDbSettings>(sp =>
{
    return sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MongoDbSettings>>().Value;
});

builder.Services.AddScoped<MongoDbContext>();

// Register repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<IGameSettingsRepository, GameSettingsRepository>();
builder.Services.AddScoped<IMarriageGameSetRepository, MarriageGameSetRepository>();
builder.Services.AddScoped<IMarriageGameRepository, MarriageGameRepository>();
builder.Services.AddScoped<IDatabaseRepository, DatabaseRepository>();
builder.Services.AddScoped<IFriendshipRepository, FriendshipRepository>();
builder.Services.AddScoped<IFriendInviteCodeRepository, FriendInviteCodeRepository>();
builder.Services.AddScoped<IPendingEmailInviteRepository, PendingEmailInviteRepository>();
builder.Services.AddScoped<IEmailVerificationCodeRepository, EmailVerificationCodeRepository>();

// Register services
builder.Services.AddSingleton<IFcmService, FcmService>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<IGameSettingsService, GameSettingsService>();
builder.Services.AddScoped<IMarriageGameSetService, MarriageGameSetService>();
builder.Services.AddScoped<IMarriageGameService, MarriageGameService>();
builder.Services.AddScoped<IDatabaseService, DatabaseService>();
builder.Services.AddScoped<IFriendshipService, FriendshipService>();
builder.Services.AddScoped<IFriendInviteService, FriendInviteService>();
builder.Services.AddSingleton<IEmailService, SmtpEmailService>();
builder.Services.AddMemoryCache(); // invite-code redemption rate limiting

// Register existing services
builder.Services.AddScoped<IMarriageGameServices, MarriageGameServices>();

// Add SignalR for real-time game updates
builder.Services.AddSignalR();

// Add Firebase/Mock Custom Authentication
builder.Services.AddAuthentication("FirebaseOrMock")
    .AddScheme<FirebaseOrMockAuthenticationOptions, FirebaseOrMockAuthenticationHandler>("FirebaseOrMock", options =>
    {
        options.FirebaseProjectId = builder.Configuration["Firebase:ProjectId"];
        options.GoogleClientId = builder.Configuration["Firebase:GoogleClientId"];
        options.VerifySignature = false;
    });

// Add health checks for Kubernetes probes
builder.Services.AddHealthChecks();

// Add CORS for Android client
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
    options.AddPolicy("SignalR", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Marriage Calculator API Documentation");
        options.WithTheme(ScalarTheme.Purple);
    });
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Serve static files for custom Swagger UI assets
app.UseStaticFiles();

// Map controllers
app.MapControllers();

// Map SignalR hubs
app.MapHub<GameHub>("/hubs/game").RequireCors("SignalR");

// Map health check endpoints for Kubernetes
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false // Liveness: just check app is running, skip DB
});

// MongoDB database initialization - seed default data
await InitializeDatabaseAsync(app);

app.Run();

static async Task InitializeDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Starting MongoDB database initialization...");

        var mongoContext = services.GetRequiredService<MongoDbContext>();

        // Verify connectivity
        var canConnect = await mongoContext.CanConnectAsync();
        if (!canConnect)
        {
            logger.LogError("Cannot connect to MongoDB. Please check connection settings.");
            return;
        }

        logger.LogInformation("MongoDB connection verified successfully.");

        // Ensure indexes for friend discovery (unique codes, TTL cleanup, invite lookups)
        await mongoContext.EnsureIndexesAsync();

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
