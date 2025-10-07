# MarriageCalculator.API.Tests

This is the test project for the MarriageCalculator API, built using xUnit and following best practices for .NET testing.

## 🏗️ Project Structure

```
MarriageCalculator.API.Tests/
├── Helpers/                    # Test infrastructure and utilities
│   ├── TestBase.cs            # Base class for unit tests
│   ├── TestDataBuilder.cs     # Fluent builders for test data
│   ├── TestDbContextFactory.cs # In-memory database factory
│   └── TestWebApplicationFactory.cs # Integration test factory
├── UnitTests/                 # Unit tests organized by layer
│   ├── Controllers/           # Controller tests
│   ├── Services/             # Service tests
│   └── Repositories/         # Repository tests
├── IntegrationTests/         # End-to-end API tests
└── GlobalUsings.cs           # Global using statements
```

## 🧪 Test Categories

### Unit Tests (111 tests - All Passing ✅)

**Controller Tests** - Test the API controllers in isolation:
- `MarriageGameSetsControllerTests` - Tests for the MarriageGameSets controller including:
  - ✅ **New Validation Logic**: Prevents creating new game sets when an active one exists
  - ✅ Model validation
  - ✅ Error handling
  - ✅ CRUD operations
- `PlayersControllerTests` (29 tests) - Tests for the Players controller including:
  - ✅ **Complete CRUD operations** (GET, POST, PUT, DELETE)
  - ✅ **Authentication & Authorization** testing with JWT claims
  - ✅ **GUID validation** and error handling
  - ✅ **User context** and claims parsing
  - ✅ **EnsureMe endpoint** functionality
  - ✅ **Error scenarios** and exception handling

**Service Tests** - Test business logic:
- `MarriageGameSetServiceTests` - Tests for the service layer including:
  - ✅ **Active game set checking** by GameSettingsId
  - ✅ Data mapping
  - ✅ Service orchestration
- `PlayerServiceTests` (26 tests) - Tests for player business logic including:
  - ✅ **Player creation** and management
  - ✅ **User-player relationships** handling
  - ✅ **EnsureUserPlayerAsync** complex logic with multiple scenarios
  - ✅ **Edge cases** for empty/null values
  - ✅ **DTO mapping** validation
  - ✅ **Email-based player lookup**

**Repository Tests** - Test data access:
- `MarriageGameSetRepositoryTests` - Tests for database operations including:
  - ✅ **New repository method**: `GetActiveByGameSettingsIdAsync`
  - ✅ CRUD operations with in-memory database
  - ✅ Query filtering and ordering
- `PlayerRepositoryTests` (41 tests) - Tests for player data access including:
  - ✅ **Complete CRUD operations** with in-memory database
  - ✅ **Soft delete functionality** testing
  - ✅ **User-creator relationships** management
  - ✅ **Email-based queries** (case-insensitive)
  - ✅ **Data filtering and ordering** by name
  - ✅ **Creator assignment** and validation

### Integration Tests

- `MarriageGameSetsIntegrationTests` - Full HTTP request/response cycle tests
- **Note**: Integration tests require additional setup for authentication and database configuration

## 🚀 Running Tests

### Run All Unit Tests
```bash
dotnet test --filter "FullyQualifiedName~UnitTests"
```

### Run Specific Test Class
```bash
dotnet test --filter "MarriageGameSetsControllerTests"
dotnet test --filter "PlayersControllerTests"
```

### Run All Player-Related Tests
```bash
dotnet test --filter "FullyQualifiedName~Player"
```

### Run Tests with Detailed Output
```bash
dotnet test --verbosity normal
```

## 📦 Dependencies

- **xUnit** - Testing framework
- **Moq** - Mocking library for isolating dependencies
- **FluentAssertions** - Fluent assertion library for readable tests
- **Microsoft.AspNetCore.Mvc.Testing** - ASP.NET Core testing utilities
- **Microsoft.EntityFrameworkCore.InMemory** - In-memory database for testing

## 🔧 Key Features Tested

### New Validation Logic ✅
The tests specifically verify the new business rule implemented in the API:

**Rule**: *"CreateMarriageGameSet should first check if there is an active game for same GameSettingsId. If there's an active gameset, then it should not create new gameset. It should return error message saying 'New game can not be created before closing Active gameset.'"*

**Tests covering this:**
- `CreateMarriageGameSet_WithActiveGameSetExists_ShouldReturnBadRequest`
- `CreateMarriageGameSet_WithValidData_AndNoActiveGameSet_ShouldReturnCreated`
- `GetActiveByGameSettingsIdAsync_WithActiveGameSet_ShouldReturnActiveGameSet`

## 🏛️ Test Architecture

### Test Infrastructure
- **TestBase**: Provides common setup for unit tests with in-memory database
- **TestDataBuilder**: Fluent builder pattern for creating test data
- **TestDbContextFactory**: Factory for creating isolated test databases

### Mocking Strategy
- Controllers are tested with mocked services
- Services are tested with mocked repositories
- Repositories are tested with real in-memory databases

### Assertions
Uses FluentAssertions for readable and maintainable test assertions:
```csharp
result.Result.Should().BeOfType<BadRequestObjectResult>();
badRequestResult!.Value.Should().Be("New game can not be created before closing Active gameset.");
```

## 📊 Test Coverage

The test suite covers:
- ✅ **Happy path scenarios** - Normal operations work correctly
- ✅ **Error scenarios** - Proper error handling and responses
- ✅ **Edge cases** - Boundary conditions and special cases
- ✅ **Business rules** - New validation logic is properly enforced
- ✅ **Data persistence** - Repository operations work correctly

## 🔍 Key Test Examples

### Testing the New Validation Logic
```csharp
[Fact]
public async Task CreateMarriageGameSet_WithActiveGameSetExists_ShouldReturnBadRequest()
{
    // Arrange: Set up an existing active game set
    var existingActiveGameSet = new MarriageGameSetDto { /* ... */ };
    _mockGameSetService.Setup(s => s.GetActiveGameSetByGameSettingsIdAsync(1))
        .ReturnsAsync(existingActiveGameSet);

    // Act: Try to create a new game set
    var result = await _controller.CreateMarriageGameSet(createDto);

    // Assert: Should return BadRequest with specific message
    result.Result.Should().BeOfType<BadRequestObjectResult>();
    var badRequestResult = result.Result as BadRequestObjectResult;
    badRequestResult!.Value.Should().Be("New game can not be created before closing Active gameset.");
}
```

## 🎯 Benefits

1. **Confidence**: Ensures the new validation logic works correctly
2. **Regression Prevention**: Catches breaking changes early
3. **Documentation**: Tests serve as living documentation of the API behavior
4. **Maintainability**: Well-structured tests are easy to maintain and extend
5. **Quality**: Comprehensive test coverage ensures high code quality

---

**Total Tests**: 122 (111 Unit Tests ✅ + 11 Integration Tests)
**Status**: All unit tests passing, integration tests need authentication setup
**Coverage**: Controllers, Services, Repositories, Players functionality, and new validation logic
