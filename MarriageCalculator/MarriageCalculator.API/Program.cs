using MarriageCalculator.API.Data;
using MarriageCalculator.API.Repositories.Interfaces;
using MarriageCalculator.API.Repositories.Implementations;
using MarriageCalculator.API.Services.Interfaces;
using MarriageCalculator.API.Services.Implementations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;
using System.Text;
using Serilog;
using Serilog.Events;

// Configure Serilog early to capture startup errors
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: Path.Combine(Directory.GetCurrentDirectory(), "Logs", "log_{Date}.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        fileSizeLimitBytes: 10_000_000,
        rollOnFileSizeLimit: true,
        shared: true,
        flushToDiskInterval: TimeSpan.FromSeconds(1),
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj} {NewLine}{Exception}",
        restrictedToMinimumLevel: LogEventLevel.Information)
    .CreateLogger();

try
{
    Log.Information("Starting MarriageCalculator API application");

    var builder = WebApplication.CreateBuilder(args);

    // Replace default logging with Serilog
    builder.Host.UseSerilog();

    // Add services to the container
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        });
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

        // JWT Authorization for Swagger
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });

        // Enable annotations for better documentation
        c.EnableAnnotations();
        
        // Custom schema IDs to avoid conflicts
        c.CustomSchemaIds(type => type.FullName);
    });

    // Configure JWT Authentication
    var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? 
        throw new InvalidOperationException("JWT SecretKey is required but not configured.");
    var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? 
        throw new InvalidOperationException("JWT Issuer is required but not configured.");
    var jwtAudience = builder.Configuration["Jwt:Audience"] ?? 
        throw new InvalidOperationException("JWT Audience is required but not configured.");

    // Skip authentication setup in Testing environment
    if (!builder.Environment.IsEnvironment("Testing"))
    {
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = !builder.Environment.IsDevelopment(); // Allow HTTP in development
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero, // Remove default 5-minute clock skew
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey))
            };
            
            // Configure JWT events for better error handling
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        context.Response.Headers.Add("Token-Expired", "true");
                    }
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    context.HandleResponse();
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    var result = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        success = false,
                        message = "You are not authorized to access this resource."
                    });
                    return context.Response.WriteAsync(result);
                }
            };
        });

        builder.Services.AddAuthorization();
    }
    else
    {
        // Configure test authentication for Testing environment
        builder.Services.AddAuthentication("Test")
            .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });
        
        builder.Services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                .RequireAssertion(_ => true) // Always allow in tests
                .Build();
        });
    }

    // Configure SQL Server with retry logic and enhanced settings
    builder.Services.AddDbContext<MarriageCalculatorDbContext>(options =>
    {
        // Skip database configuration in Testing environment - let tests handle it
        if (builder.Environment.IsEnvironment("Testing"))
        {
            // Tests will configure their own database
            return;
        }

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
        
        // Configure warnings to suppress pending model changes warning
        options.ConfigureWarnings(warnings =>
            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        
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
    builder.Services.AddScoped<IMarriageGameRoundRepository, MarriageGameRoundRepository>();
    builder.Services.AddScoped<IMarriageGameSetPlayerRepository, MarriageGameSetPlayerRepository>();
    builder.Services.AddScoped<IMarriageGameRepository, MarriageGameRepository>();
    builder.Services.AddScoped<IMarriageGameScoreRepository, MarriageGameScoreRepository>();
    builder.Services.AddScoped<IDatabaseRepository, DatabaseRepository>();

    // Register authentication repositories
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IUserEmailVerificationRepository, UserEmailVerificationRepository>();
    builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

    // Register services
    builder.Services.AddScoped<IPlayerService, PlayerService>();
    builder.Services.AddScoped<IGameSettingsService, GameSettingsService>();
    builder.Services.AddScoped<IMarriageGameSetService, MarriageGameSetService>();
    builder.Services.AddScoped<IMarriageGameRoundService, MarriageGameRoundService>();
    builder.Services.AddScoped<IMarriageGameService, MarriageGameService>();
    builder.Services.AddScoped<IMarriageGameScoreService, MarriageGameScoreService>();
    builder.Services.AddScoped<IDatabaseService, DatabaseService>();
    builder.Services.AddScoped<IMarriageGameSetPlayerService, MarriageGameSetPlayerService>();

    // Register authentication services
    builder.Services.AddScoped<IPasswordService, PasswordService>();
    builder.Services.AddScoped<IEmailService, EmailService>();
    builder.Services.AddScoped<IJwtService, JwtService>();
    builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
    builder.Services.AddScoped<IUserAuthService, UserAuthService>();
    builder.Services.AddScoped<IUserPlayerService, UserPlayerService>();

    // Register existing services
    builder.Services.AddScoped<IMarriageGameServices, MarriageGameServices>();

    var app = builder.Build();

    // Configure Serilog request logging
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.GetLevel = (httpContext, elapsed, ex) => ex != null
            ? LogEventLevel.Error 
            : httpContext.Response.StatusCode > 499 
                ? LogEventLevel.Error 
                : LogEventLevel.Information;
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.FirstOrDefault());
            diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress?.ToString());
        };
    });

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

    // Authentication and Authorization middleware
    app.UseAuthentication();
    app.UseAuthorization();

    // Serve static files for custom Swagger UI assets
    app.UseStaticFiles();

    // Map controllers
    app.MapControllers();

    // Simple database initialization - let EF handle everything automatically
    await InitializeDatabaseAsync(app);

    Log.Information("MarriageCalculator API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

static async Task InitializeDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var environment = services.GetRequiredService<IWebHostEnvironment>();
    
    try
    {
        // Skip database initialization in Testing environment
        if (environment.IsEnvironment("Testing"))
        {
            logger.LogInformation("Skipping database initialization in Testing environment.");
            return;
        }

        logger.LogInformation("Starting database initialization...");
        
        var context = services.GetRequiredService<MarriageCalculatorDbContext>();
        
        // Use Entity Framework migrations to handle database creation and schema
        logger.LogInformation("Running database migrations...");
        await context.Database.MigrateAsync();
        
        logger.LogInformation("Database migration completed successfully.");
        
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

// Make the Program class accessible to tests
public partial class Program { }

/// <summary>
/// Test authentication handler for Testing environment
/// </summary>
public class TestAuthenticationHandler : Microsoft.AspNetCore.Authentication.AuthenticationHandler<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions>
{
#pragma warning disable CS0618 // Type or member is obsolete
    public TestAuthenticationHandler(Microsoft.Extensions.Options.IOptionsMonitor<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions> options,
        Microsoft.Extensions.Logging.ILoggerFactory logger, System.Text.Encodings.Web.UrlEncoder encoder, Microsoft.AspNetCore.Authentication.ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }
#pragma warning restore CS0618 // Type or member is obsolete

    protected override Task<Microsoft.AspNetCore.Authentication.AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "TestUser"),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "12345678-1234-1234-1234-123456789012"),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "test@example.com")
        };

        var identity = new System.Security.Claims.ClaimsIdentity(claims, "Test");
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);
        var ticket = new Microsoft.AspNetCore.Authentication.AuthenticationTicket(principal, "Test");

        return Task.FromResult(Microsoft.AspNetCore.Authentication.AuthenticateResult.Success(ticket));
    }
}