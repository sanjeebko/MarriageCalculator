# Marriage Calculator Database Setup - SQL Server

## Overview
Successfully configured MarriageCalculatorDbContext to work with SQL Server with automatic database and table creation using Entity Framework Core.

## Database Configuration

### Connection String
- **Server**: 192.168.0.214
- **Username**: mcuser
- **Password**: Scorpions18
- **Database**: MarriageCalculatorDB
- **Connection String**: `Server=192.168.0.214;Database=MarriageCalculatorDB;User Id=mcuser;Password=Scorpions18;TrustServerCertificate=true;MultipleActiveResultSets=true`

### Tables Created Automatically
Entity Framework automatically creates these tables based on the DbContext configuration:

1. **Player** - Stores player information
2. **GameSettings** - Game configuration and rules
3. **MarriageGameSet** - Complete game sessions
4. **MarriageGameSetPlayer** - Junction table for players in game sets
5. **MarriageGameRound** - Rounds within game sets
6. **MarriageGame** - Individual games within rounds
7. **MarriageGameScore** - Player scores for each game

### Entity Relationships
- GameSettings ? MarriageGameSet (1:1)
- MarriageGameSet ? MarriageGameRound (1:Many)
- MarriageGameRound ? MarriageGame (1:Many)
- MarriageGame ? MarriageGameScore (1:Many)
- Player ? MarriageGameSetPlayer (1:Many)
- MarriageGameSet ? MarriageGameSetPlayer (1:Many)
- Player ? MarriageGame (Winner/Dealer relationships)

## Package Updates
- **Added**: Microsoft.EntityFrameworkCore.SqlServer (9.0.7)
- **Removed**: Microsoft.EntityFrameworkCore.Sqlite
- **Updated**: Microsoft.EntityFrameworkCore.Design (9.0.7)
- **Updated**: Microsoft.EntityFrameworkCore.Tools (9.0.7)

## Key Features
- **Automatic Database Creation**: EF handles database creation with `EnsureCreatedAsync()`
- **Automatic Table Creation**: Tables created based on DbContext model definitions
- **Proper Entity Framework configurations**: All models properly mapped
- **Foreign key relationships**: Appropriate delete behaviors configured
- **Unique constraints**: Where needed for data integrity
- **Precision specifications**: For decimal fields
- **Enum conversions**: For Currency and FoulPointBonusType

## API Endpoints
- `GET/POST /api/players` - Player management
- `GET/POST /api/gamesettings` - Game settings management
- `GET/POST /api/marriagegamesets` - Game set management
- `GET/POST/PUT/DELETE /api/marriagegames` - Marriage game management
- `GET /api/database/info` - Database statistics and health check
- `POST /api/database/seed` - Seed default data
- `DELETE /api/database/cleanup` - Clean database

## Simplified Setup Process

### No Manual Steps Required!
The application automatically:
1. Creates the database if it doesn't exist
2. Creates all required tables
3. Sets up proper relationships and constraints
4. Seeds default data

### Just Run the Application:
```bash
dotnet run
```

That's it! Entity Framework handles everything automatically.

## Configuration Details

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=192.168.0.214;Database=MarriageCalculatorDB;User Id=mcuser;Password=Scorpions18;TrustServerCertificate=true;MultipleActiveResultSets=true;"
  },
  "DatabaseSettings": {
    "EnableRetryOnFailure": true,
    "MaxRetryCount": 3,
    "MaxRetryDelay": "00:00:30",
    "AutoCreateDatabase": true
  }
}
```

### Database Initialization
```csharp
// In Program.cs - Simple and automatic
await context.Database.EnsureCreatedAsync();
```

## Notes
- **ObservableProperty fields**: From MVVM are properly mapped
- **Navigation properties**: Marked with [Ignore] are excluded from EF mapping
- **Default values**: Use SQL Server syntax (GETUTCDATE())
- **Foreign key relationships**: Preserve referential integrity
- **No manual SQL scripts needed**: Everything is handled by Entity Framework
- **Connection resilience**: Built-in retry logic for transient failures

## Benefits of This Approach
1. **Zero Manual Setup**: No SQL scripts to run
2. **Consistent Development**: Same setup across all environments
3. **Version Control Friendly**: Database schema is defined in code
4. **Easy Testing**: Can easily recreate database for tests
5. **Simplified Deployment**: No separate database setup steps