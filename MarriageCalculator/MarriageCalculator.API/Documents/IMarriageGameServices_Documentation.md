# IMarriageGameServices Interface Implementation

## Overview
Created a comprehensive interface for the MarriageGameServices class following the existing patterns in your .NET MAUI project.

## Interface Definition: `IMarriageGameServices`

### Methods

#### `Task SetupDB()`
- **Purpose**: Sets up the database, ensuring it's created and seeded with default data
- **Returns**: Task representing the asynchronous operation
- **Usage**: Called during application startup to initialize the database

#### `Task SeedDefaultData()`
- **Purpose**: Seeds the database with default game settings if none exist
- **Returns**: Task representing the asynchronous operation
- **Details**: Creates default GameSettings with predefined values (NPR currency, standard points, etc.)

#### `Task<DatabaseInfo> GetDatabaseInfoAsync()`
- **Purpose**: Gets comprehensive database health and statistics information
- **Returns**: DatabaseInfo object containing counts of all entities and connection status
- **Usage**: Useful for monitoring, debugging, and health checks

#### `Task CleanupDatabaseAsync()`
- **Purpose**: Cleans up and resets the entire database
- **Returns**: Task representing the asynchronous operation
- **Details**: Removes all data in proper order respecting foreign keys, then re-seeds defaults

## DatabaseInfo Model
```csharp
public class DatabaseInfo
{
    public int PlayerCount { get; set; }
    public int GameSettingsCount { get; set; }
    public int MarriageGameSetCount { get; set; }
    public int MarriageGameSetPlayerCount { get; set; }
    public int MarriageGameRoundCount { get; set; }
    public int MarriageGameCount { get; set; }
    public int MarriageGameScoreCount { get; set; }
    public bool DatabaseCreated { get; set; }
    public DateTime LastChecked { get; set; }
}
```

## Dependency Injection Configuration
Updated `Program.cs` to register the interface:
```csharp
builder.Services.AddScoped<IMarriageGameServices, MarriageGameServices>();
```

## Enhanced API Endpoints
Added new endpoints utilizing the service interface:

### `GET /api/database/info`
- Returns comprehensive database statistics
- Uses `GetDatabaseInfoAsync()` method

### `POST /api/database/seed`
- Manually triggers database seeding
- Uses `SeedDefaultData()` method

### `DELETE /api/database/cleanup`
- Cleans and resets the entire database
- Uses `CleanupDatabaseAsync()` method

## Benefits of Interface Implementation

### 1. **Testability**
- Easy to create mock implementations for unit testing
- Interface can be injected into test classes

### 2. **Dependency Inversion**
- Controllers and services depend on abstraction, not concrete implementation
- Follows SOLID principles

### 3. **Flexibility**
- Can easily swap implementations (e.g., different database providers)
- Supports multiple implementations if needed

### 4. **Documentation**
- Interface serves as a contract defining expected behavior
- Clear method signatures with XML documentation

### 5. **Consistency**
- Follows the same pattern as other services in your project (IDbService, IPlayerService, etc.)

## Usage Examples

### In Controllers
```csharp
[ApiController]
public class DatabaseController : ControllerBase
{
    private readonly IMarriageGameServices _marriageGameServices;
    
    public DatabaseController(IMarriageGameServices marriageGameServices)
    {
        _marriageGameServices = marriageGameServices;
    }
    
    [HttpGet("health")]
    public async Task<ActionResult<DatabaseInfo>> GetDatabaseHealth()
    {
        var info = await _marriageGameServices.GetDatabaseInfoAsync();
        return Ok(info);
    }
}
```

### In Unit Tests
```csharp
var mockService = new Mock<IMarriageGameServices>();
mockService.Setup(s => s.GetDatabaseInfoAsync())
           .ReturnsAsync(new DatabaseInfo { DatabaseCreated = true });
```

## Future Extensibility
The interface can be easily extended with additional methods such as:
- `Task<bool> ValidateDatabaseIntegrityAsync()`
- `Task BackupDatabaseAsync(string path)`
- `Task<MigrationStatus> GetMigrationStatusAsync()`
- `Task OptimizeDatabaseAsync()`

This implementation provides a solid foundation for maintaining and extending the MarriageGameServices functionality while following best practices for dependency injection and interface design.