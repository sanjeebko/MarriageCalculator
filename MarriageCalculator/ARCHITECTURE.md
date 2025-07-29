# MarriageCalculator Solution Architecture

## ?? Project Structure

```
MarriageCalculator/
??? MarriageCalculator/                    # ?? MAUI Mobile App
?   ??? Platforms/                         # Platform-specific code
?   ??? Pages/                            # MAUI Pages/Views
?   ??? Resources/                        # Images, fonts, etc.
?   ??? MarriageCalculator.csproj         # MAUI project file
??? MarriageCalculator.API/               # ?? Web API Backend
?   ??? Controllers/                      # API endpoints
?   ??? Services/                         # Business logic
?   ??? Repositories/                     # Data access
?   ??? MarriageCalculator.API.csproj     # Web API project file
??? MarriageCalculator.Core/              # ?? Shared Library
    ??? Models/                           # Entity models
    ??? Extensions/                       # Helper extensions
    ??? MarriageCalculator.Core.csproj    # Simple .NET 8 library
```

## ?? Project Dependencies

### MarriageCalculator.Core (Shared Library) - ? CLEANED
- **Purpose**: Contains shared models, business logic, and utilities
- **Target Framework**: `net8.0` (Simple .NET 8 class library)
- **Dependencies**: NONE - Pure .NET 8 with no MAUI dependencies
- **Used By**: Both MAUI app and Web API

### ? Changes Made - Clean Architecture

1. **Removed MAUI Dependencies**: Core is now a pure .NET 8 library
2. **Removed ObservableObject**: Models use simple properties instead of MVVM patterns
3. **Removed IValueConverter**: No UI-specific converters in shared library
4. **Removed Multi-targeting**: Single target framework (net8.0)

### MarriageCalculator (MAUI App)
- **Purpose**: Cross-platform mobile application
- **Dependencies**: References `MarriageCalculator.Core`
- **API Communication**: Connects to `MarriageCalculator.API` via HTTP
- **MVVM Implementation**: Uses its own MVVM code, not shared

### MarriageCalculator.API (Web API)
- **Purpose**: Backend API for data management
- **Dependencies**: References `MarriageCalculator.Core`
- **Database**: Uses Entity Framework Core with SQL Server

## ?? Docker Build Simplified

### No MAUI Workload Required
The Dockerfile no longer needs MAUI workload installation:

```dockerfile
# Simple .NET 8 build - no MAUI workload needed
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
RUN dotnet restore
RUN dotnet build
RUN dotnet publish
```

### Benefits of Clean Architecture
1. **Faster Docker builds**: No MAUI workload installation needed
2. **Smaller images**: No unnecessary MAUI dependencies
3. **Cleaner separation**: Core library is truly shared without UI concerns
4. **Better testability**: Core models can be unit tested without MAUI dependencies

## ?? Core Library Contents

### ? What's Included (Clean)
- **Entity Models**: `Player`, `GameSettings`, `MarriageGame`, etc.
- **Enums**: `Currency`, `FoulPointBonusType`
- **Extensions**: Helper methods for business logic
- **Pure .NET 8**: No platform-specific dependencies

### ? What's Removed (MAUI-specific)
- **ObservableObject**: Removed from models
- **IValueConverter**: UI converters moved to MAUI project
- **Multi-targeting**: Single framework target
- **MVVM Attributes**: No `[ObservableProperty]` annotations

## ?? Benefits of Clean Architecture

? **Simplified Docker Build**: No MAUI workload installation required  
? **Pure Shared Library**: Core contains only business logic and models  
? **Better Separation**: UI concerns separated from business logic  
? **Easier Testing**: Unit tests don't require MAUI dependencies  
? **Faster Builds**: Reduced complexity and dependencies  
? **Clear Responsibilities**: Each project has a single, well-defined purpose  

## ?? Migration Notes

### For MAUI App
- MAUI app now implements its own MVVM patterns
- UI converters moved to MAUI project
- Observable properties implemented in MAUI-specific ViewModels

### For API
- Uses simple POCO models from Core
- Entity Framework works with clean models
- No UI-related dependencies

## ?? Recommended Usage

This clean architecture is optimal because:
1. **Clear separation of concerns**: UI, business logic, and data access are properly separated
2. **Technology independence**: Core library can be used by any .NET application
3. **Docker efficiency**: Simple builds without complex workload requirements
4. **Maintainability**: Changes to UI don't affect business logic and vice versa

The MarriageCalculator solution now follows clean architecture principles with a truly shared, technology-agnostic core library.