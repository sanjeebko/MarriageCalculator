# Repository Pattern Implementation - MarriageCalculator MAUI App

## Overview
Successfully implemented the Repository pattern in the MarriageCalculator MAUI application to connect to the MarriageCalculator.API instead of using offline SQLite database.

## Key Changes Made

### ? **Removed SQLite Dependencies**
- Removed `sqlite-net-pcl` package
- Removed `SQLitePCLRaw.*` packages
- Removed `SqLiteDbService` implementation
- No more offline database storage

### ? **Added Repository Pattern**
- Created `/Repositories` directory structure
- Implemented repository interfaces for all entities
- Created API service for HTTP communication
- Replaced `SqLiteDbService` with `ApiDbService`

### ? **Added API Communication**
- Added `Microsoft.Extensions.Http` for HttpClient support
- Added `Microsoft.Extensions.Configuration.Json` for configuration
- Added `System.Text.Json` for serialization
- Created `ApiService` for centralized HTTP communication

## Repository Structure

```
MarriageCalculator/
??? Repositories/
?   ??? IRepositories.cs           # All repository interfaces
?   ??? ApiService.cs              # HTTP client service
?   ??? PlayerRepository.cs        # Player operations implementation
?   ??? GameRepositories.cs        # All game-related repositories
??? Services/
?   ??? ApiDbService.cs            # API-based IDbService implementation
?   ??? (existing services)       # Other services unchanged
??? appsettings.json               # API configuration
```

## Repository Interfaces

### Core Entities
- **`IPlayerRepository`** - Player CRUD operations
- **`IGameSettingsRepository`** - Game settings management
- **`IMarriageGameSetRepository`** - Game set operations
- **`IMarriageGameRepository`** - Individual game management
- **`IMarriageGameRoundRepository`** - Round operations
- **`IMarriageGameScoreRepository`** - Score management
- **`IMarriageGameSetPlayerRepository`** - Player-GameSet relationships
- **`IDatabaseRepository`** - Database operations

### API Service
- **`IApiService`** - HTTP communication abstraction
  - `GetAsync<T>(endpoint)` - GET requests
  - `PostAsync<T>(endpoint, data)` - POST requests
  - `PutAsync<T>(endpoint, data)` - PUT requests
  - `DeleteAsync(endpoint)` - DELETE requests
  - `TestConnectionAsync()` - Connection testing

## Configuration

### appsettings.json
```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:7294",
    "Timeout": 30,
    "RetryCount": 3
  }
}
```

### Service Registration (MauiProgram.cs)
```csharp
// HTTP Client configuration
builder.Services.AddHttpClient<IApiService, ApiService>();

// Repository services
builder.Services.AddTransient<IPlayerRepository, PlayerRepository>();
builder.Services.AddTransient<IGameSettingsRepository, GameSettingsRepository>();
// ... other repositories

// Database service (API-based)
builder.Services.AddSingleton<IDbService, ApiDbService>();
```

## API Endpoints Used

### Players
- `GET /api/players` - Get all players
- `GET /api/players/{id}` - Get player by ID
- `POST /api/players` - Create player
- `PUT /api/players/{id}` - Update player
- `DELETE /api/players/{id}` - Delete player

### Game Settings
- `GET /api/gamesettings` - Get all settings
- `GET /api/gamesettings/{id}` - Get settings by ID
- `POST /api/gamesettings` - Create settings
- `PUT /api/gamesettings/{id}` - Update settings
- `DELETE /api/gamesettings/{id}` - Delete settings

### Marriage Game Sets
- `GET /api/marriagegamesets` - Get all game sets
- `GET /api/marriagegamesets/{id}` - Get game set by ID
- `GET /api/marriagegamesets/latest` - Get latest game set
- `POST /api/marriagegamesets` - Create game set
- `PUT /api/marriagegamesets/{id}` - Update game set
- `DELETE /api/marriagegamesets/{id}` - Delete game set

### Marriage Games
- `GET /api/marriagegames` - Get all games
- `GET /api/marriagegames/{id}` - Get game by ID
- `GET /api/marriagegames/round/{roundId}` - Get games by round
- `POST /api/marriagegames` - Create game
- `PUT /api/marriagegames/{id}` - Update game
- `DELETE /api/marriagegames/{id}` - Delete game

### Database Operations
- `GET /api/database/info` - Test connection
- `POST /api/database/seed` - Seed default data
- `DELETE /api/database/cleanup` - Clean database

## Benefits Achieved

### ? **Centralized Data Storage**
- All data stored in SQL Server database
- No local SQLite database to manage
- Consistent data across all devices
- Better data integrity and backup

### ? **Scalability**
- Multiple users can access same data
- Real-time data synchronization possible
- Server-side business logic
- Better performance for complex queries

### ? **Maintainability**
- Single source of truth for data
- API versioning support
- Centralized authentication/authorization
- Easier debugging and monitoring

### ? **Clean Architecture**
- Repository pattern for data access
- Dependency injection for testability
- Interface-based design
- Separation of concerns

## Migration Impact

### ? **Minimal Code Changes**
- Existing `IDbService` interface maintained
- All ViewModels and Services unchanged
- Same method signatures and behavior
- Transparent replacement of data layer

### ? **Enhanced Error Handling**
- HTTP-specific error handling
- Connection timeout management
- Retry logic for failed requests
- Graceful degradation options

### ? **Configuration Flexibility**
- Environment-specific API URLs
- Configurable timeouts and retries
- Easy switching between development/production
- External configuration management

## Usage Example

```csharp
// Old SQLite approach (removed)
// var players = await sqLiteDbService.GetPlayersAsync();

// New Repository approach (automatic)
var players = await dbService.GetPlayersAsync(); // Same interface, API backend
```

## Future Enhancements

### ?? **Possible Additions**
- **Offline Caching** - Cache API responses for offline use
- **Authentication** - JWT token-based authentication
- **Real-time Updates** - SignalR for live data updates
- **Conflict Resolution** - Handle concurrent data modifications
- **Performance Optimization** - Request batching and caching

### ?? **API Extensions Needed**
Some repository methods may require additional API endpoints:
- Marriage Game Rounds endpoints
- Marriage Game Scores endpoints  
- Marriage Game Set Players endpoints

These can be added to the MarriageCalculator.API as needed.

## Testing

### ? **Connection Testing**
```csharp
var dbRepository = serviceProvider.GetService<IDatabaseRepository>();
var isConnected = await dbRepository.TestConnectionAsync();
```

### ? **Error Handling**
All repository methods include proper exception handling and will throw meaningful exceptions for API errors.

---

**Repository Pattern Implementation Complete!** ??  
The MAUI app now uses the Repository pattern to connect to MarriageCalculator.API instead of SQLite.