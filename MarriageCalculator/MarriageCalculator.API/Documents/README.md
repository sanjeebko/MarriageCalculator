# MarriageCalculator API Documentation

## Overview
This folder contains comprehensive documentation for the MarriageCalculator API project, covering database setup, architecture, troubleshooting, and implementation details.

## Documentation Files

### ?? [DATABASE_SETUP.md](./DATABASE_SETUP.md)
Complete guide for setting up SQL Server database with Entity Framework.

**Contents:**
- Database configuration and connection strings
- Entity relationships and table structures
- Package updates and automatic table creation
- API endpoint documentation

### ??? [CLEAN_ARCHITECTURE.md](./CLEAN_ARCHITECTURE.md)
Detailed explanation of the clean architecture implementation.

**Contents:**
- Architecture layers (Controllers, Services, Repositories, DTOs)
- Dependency injection configuration
- API endpoint specifications
- Benefits and future enhancements
- Code examples and patterns

### ?? [TROUBLESHOOTING.md](./TROUBLESHOOTING.md)
SQL Server connection troubleshooting guide and solutions.

**Contents:**
- Common error analysis and solutions
- Step-by-step resolution procedures
- Alternative connection string configurations
- Testing and verification commands

### ?? [IMarriageGameServices_Documentation.md](./IMarriageGameServices_Documentation.md)
Interface implementation documentation for the MarriageGameServices.

**Contents:**
- Interface definition and methods
- DatabaseInfo model specification
- Usage examples and patterns
- Benefits of interface implementation
- Future extensibility options

## Quick Navigation

### ?? Getting Started
1. **Database Setup** ? [DATABASE_SETUP.md](./DATABASE_SETUP.md)
2. **Architecture Overview** ? [CLEAN_ARCHITECTURE.md](./CLEAN_ARCHITECTURE.md)
3. **Troubleshooting Issues** ? [TROUBLESHOOTING.md](./TROUBLESHOOTING.md)

### ?? API Endpoints Summary

#### Players Management
- `GET /api/players` - Get all players
- `GET /api/players/{id}` - Get player by ID
- `POST /api/players` - Create new player
- `PUT /api/players/{id}` - Update player
- `DELETE /api/players/{id}` - Delete player

#### Game Settings Management
- `GET /api/gamesettings` - Get all game settings
- `GET /api/gamesettings/{id}` - Get game settings by ID
- `POST /api/gamesettings` - Create new game settings
- `PUT /api/gamesettings/{id}` - Update game settings
- `DELETE /api/gamesettings/{id}` - Delete game settings

#### Marriage Game Sets Management
- `GET /api/marriagegamesets` - Get all game sets
- `GET /api/marriagegamesets/{id}` - Get game set by ID
- `GET /api/marriagegamesets/latest` - Get latest active game set
- `POST /api/marriagegamesets` - Create new game set
- `PUT /api/marriagegamesets/{id}` - Update game set
- `DELETE /api/marriagegamesets/{id}` - Delete game set

#### Marriage Games Management
- `GET /api/marriagegames` - Get all games
- `GET /api/marriagegames/{id}` - Get game by ID
- `GET /api/marriagegames/round/{roundId}` - Get games by round
- `POST /api/marriagegames` - Create new game
- `PUT /api/marriagegames/{id}` - Update game
- `DELETE /api/marriagegames/{id}` - Delete game

#### Database Management
- `GET /api/database/info` - Get database information and connection status
- `POST /api/database/seed` - Seed default data
- `DELETE /api/database/cleanup` - Clean database

### ??? Architecture Summary

```
MarriageCalculator.API/
??? Controllers/          # HTTP request handlers
??? Services/             # Business logic layer
??? Repositories/         # Data access layer
??? DTOs/                # Data transfer objects
??? Data/                # Entity Framework DbContext
??? Documents/           # Documentation (this folder)
```

### ??? Technology Stack
- **.NET 8** - Target framework
- **Entity Framework Core 9.0.7** - ORM with automatic database/table creation
- **SQL Server** - Database provider
- **ASP.NET Core** - Web API framework
- **Swagger/OpenAPI** - API documentation
- **Dependency Injection** - Service registration and lifecycle management

### ?? Development Guidelines

#### Entity Framework Automatic Features
- **Database Creation**: EF automatically creates the database if it doesn't exist
- **Table Creation**: EF automatically creates tables based on DbContext and model definitions
- **Schema Updates**: EF handles schema changes automatically when models change

#### Adding New Entities
1. Create the domain model in `MarriageCalculator.Core`
2. Add DbSet to `MarriageCalculatorDbContext`
3. Create repository interface and implementation
4. Create DTOs for API communication
5. Create service interface and implementation
6. Create controller with CRUD operations
7. **No manual migrations needed** - EF handles everything automatically!

#### Testing Checklist
- [ ] Unit tests for business logic (Services)
- [ ] Integration tests for repositories
- [ ] API endpoint testing
- [ ] Database connection testing
- [ ] Error handling verification

### ?? Related Projects
- **MarriageCalculator** - .NET MAUI mobile application
- **MarriageCalculator.Core** - Shared models and business logic
- **MarriageCalculator.API** - Web API backend (this project)

## Key Simplifications Made

### ? **Removed Manual Migration Management**
- No more `dotnet ef migrations add` commands needed
- No more `dotnet ef database update` commands needed
- Entity Framework handles everything automatically

### ? **Simplified API Endpoints**
- Removed migration status endpoints
- Removed manual migration application endpoints
- Focus on business functionality only

### ? **Streamlined Database Initialization**
- Uses `EnsureCreatedAsync()` for automatic setup
- Handles database and table creation automatically
- Simplified error handling

---

**Last Updated:** January 2024  
**Version:** 2.0 (Simplified)  
**Maintainer:** MarriageCalculator Development Team