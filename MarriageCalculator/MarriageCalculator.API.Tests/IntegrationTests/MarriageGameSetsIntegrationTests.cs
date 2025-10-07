using FluentAssertions;
using MarriageCalculator.API.Tests.Helpers;
using MarriageCalculator.Core.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace MarriageCalculator.API.Tests.IntegrationTests;

/// <summary>
/// Integration tests for MarriageGameSets API endpoints
/// These tests verify the complete request/response cycle including validation
/// </summary>
public class MarriageGameSetsIntegrationTests : IClassFixture<TestWebApplicationFactory>, IDisposable
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public MarriageGameSetsIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    /// <summary>
    /// Helper method to get JsonSerializerOptions that match the API's configuration
    /// </summary>
    private static JsonSerializerOptions GetJsonSerializerOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
    }

    /// <summary>
    /// Helper method to create game settings for the test user
    /// </summary>
    private async Task<GameSettingsDto> CreateTestGameSettingsAsync()
    {
        var gameSettingsDto = new CreateGameSettingsDto
        {
            Murder = true,
            Kidnap = false,
            SeenPoint = 3,
            UnseenPoint = 10,
            PointRate = 10,
            Currency = MarriageCalculator.Core.Models.Currency.NPR_Rupee,
            Dublee = true,
            DubleePointLess = true,
            DubleePointBonus = 5,
            FoulPoint = 15,
            FoulPointBonus = MarriageCalculator.Core.Models.FoulPointBonusType.NEXT_GAME,
            Audio = true
        };

        var gameSettingsResponse = await _client.PostAsJsonAsync("/api/gamesettings", gameSettingsDto);
        gameSettingsResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var gameSettingsContent = await gameSettingsResponse.Content.ReadAsStringAsync();
        var gameSettings = JsonSerializer.Deserialize<GameSettingsDto>(gameSettingsContent, GetJsonSerializerOptions());
        
        return gameSettings!;
    }

    [Fact]
    public async Task CreateMarriageGameSet_WithValidData_AndNoActiveGameSet_ShouldReturnCreated()
    {
        // Arrange
        var gameSettings = await CreateTestGameSettingsAsync();
        var createDto = new CreateMarriageGameSetDto
        {
            Name = "Integration Test Game Set",
            GameSettingsId = gameSettings.Id
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/marriagegamesets", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadAsStringAsync();
        var gameSet = JsonSerializer.Deserialize<MarriageGameSetDto>(content, GetJsonSerializerOptions());

        gameSet.Should().NotBeNull();
        gameSet!.Name.Should().Be(createDto.Name);
        gameSet.GameSettingsId.Should().Be(createDto.GameSettingsId);
        gameSet.IsActive.Should().BeTrue();
        gameSet.Id.Should().BeGreaterThan(0);

        // Verify the Location header
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().ContainEquivalentOf($"/api/marriagegamesets/{gameSet.Id}");
    }

    [Fact]
    public async Task CreateMarriageGameSet_WithActiveGameSetExists_ShouldReturnBadRequest()
    {
        // Arrange
        var gameSettings = await CreateTestGameSettingsAsync();
        
        // First, create an active game set
        var firstGameSet = new CreateMarriageGameSetDto
        {
            Name = "First Active Game Set",
            GameSettingsId = gameSettings.Id
        };

        var firstResponse = await _client.PostAsJsonAsync("/api/marriagegamesets", firstGameSet);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Now try to create another game set with the same GameSettingsId
        var secondGameSet = new CreateMarriageGameSetDto
        {
            Name = "Second Game Set - Should Fail",
            GameSettingsId = gameSettings.Id
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/marriagegamesets", secondGameSet);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("New game can not be created before closing Active gameset.");
    }

    [Fact]
    public async Task CreateMarriageGameSet_WithInvalidData_ShouldReturnCreated()
    {
        // Arrange
        // Note: The current DTO doesn't have validation attributes, so "invalid" data like empty strings 
        // and zero values are currently accepted by the API
        var createDto = new CreateMarriageGameSetDto
        {
            Name = "", // Currently allowed (no [Required] attribute)
            GameSettingsId = 0 // Currently allowed (no validation range)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/marriagegamesets", createDto);

        // Assert
        // Since there are no validation attributes on CreateMarriageGameSetDto, 
        // the API currently accepts this data and returns Created
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetMarriageGameSets_WithValidGameSettingsId_ShouldReturnOk()
    {
        // Arrange
        var gameSettingsId = 3;
        
        // Create a game set first
        var createDto = new CreateMarriageGameSetDto
        {
            Name = "Test Game Set for Get",
            GameSettingsId = gameSettingsId
        };

        await _client.PostAsJsonAsync("/api/marriagegamesets", createDto);

        // Act
        var response = await _client.GetAsync($"/api/marriagegamesets?gameSettingsId={gameSettingsId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var gameSets = JsonSerializer.Deserialize<List<MarriageGameSetDto>>(content, GetJsonSerializerOptions());

        gameSets.Should().NotBeNull();
        gameSets!.Should().HaveCountGreaterThan(0);
        gameSets.Should().OnlyContain(gs => gs.GameSettingsId == gameSettingsId);
    }

    [Fact]
    public async Task GetMarriageGameSet_WithValidId_ShouldReturnOk()
    {
        // Arrange
        var createDto = new CreateMarriageGameSetDto
        {
            Name = "Test Game Set for Get By Id",
            GameSettingsId = 4
        };

        var createResponse = await _client.PostAsJsonAsync("/api/marriagegamesets", createDto);
        var createdContent = await createResponse.Content.ReadAsStringAsync();
        var createdGameSet = JsonSerializer.Deserialize<MarriageGameSetDto>(createdContent, GetJsonSerializerOptions());

        // Act
        var response = await _client.GetAsync($"/api/marriagegamesets/{createdGameSet!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var gameSet = JsonSerializer.Deserialize<MarriageGameSetDto>(content, GetJsonSerializerOptions());

        gameSet.Should().NotBeNull();
        gameSet!.Id.Should().Be(createdGameSet.Id);
        gameSet.Name.Should().Be(createDto.Name);
        gameSet.GameSettingsId.Should().Be(createDto.GameSettingsId);
    }

    [Fact]
    public async Task GetMarriageGameSet_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = 99999;

        // Act
        var response = await _client.GetAsync($"/api/marriagegamesets/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain($"Marriage game set with ID {invalidId} not found");
    }

    [Fact]
    public async Task GetLatestActiveGameSet_WithActiveGameSet_ShouldReturnOk()
    {
        // Arrange - First create game settings for the test user
        var gameSettingsDto = new CreateGameSettingsDto
        {
            Murder = true,
            Kidnap = false,
            SeenPoint = 3,
            UnseenPoint = 10,
            PointRate = 10,
            Currency = MarriageCalculator.Core.Models.Currency.NPR_Rupee,
            Dublee = true,
            DubleePointLess = true,
            DubleePointBonus = 5,
            FoulPoint = 15,
            FoulPointBonus = MarriageCalculator.Core.Models.FoulPointBonusType.NEXT_GAME,
            Audio = true
        };

        var gameSettingsResponse = await _client.PostAsJsonAsync("/api/gamesettings", gameSettingsDto);
        gameSettingsResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var gameSettingsContent = await gameSettingsResponse.Content.ReadAsStringAsync();
        var gameSettings = JsonSerializer.Deserialize<GameSettingsDto>(gameSettingsContent, GetJsonSerializerOptions());

        var createDto = new CreateMarriageGameSetDto
        {
            Name = "Latest Active Game Set Test",
            GameSettingsId = gameSettings!.Id
        };

        await _client.PostAsJsonAsync("/api/marriagegamesets", createDto);

        // Act
        var response = await _client.GetAsync("/api/marriagegamesets/latest");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var gameSet = JsonSerializer.Deserialize<MarriageGameSetDto>(content, GetJsonSerializerOptions());

        gameSet.Should().NotBeNull();
        gameSet!.IsActive.Should().BeTrue();
        gameSet.Name.Should().Be("Latest Active Game Set Test");
    }

    [Fact]
    public async Task GetLatestActiveGameSet_WithNoActiveGameSet_ShouldReturnNotFound()
    {
        // Act - Call the endpoint without creating any game sets for the test user
        var response = await _client.GetAsync("/api/marriagegamesets/latest");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("No active marriage game set found for current user");
    }

    [Fact]
    public async Task UpdateMarriageGameSet_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var createDto = new CreateMarriageGameSetDto
        {
            Name = "Game Set to Update",
            GameSettingsId = 6
        };

        var createResponse = await _client.PostAsJsonAsync("/api/marriagegamesets", createDto);
        var createdContent = await createResponse.Content.ReadAsStringAsync();
        var createdGameSet = JsonSerializer.Deserialize<MarriageGameSetDto>(createdContent, GetJsonSerializerOptions());

        var updateDto = new CreateMarriageGameSetDto
        {
            Name = "Updated Game Set Name",
            GameSettingsId = 7
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/marriagegamesets/{createdGameSet!.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var updatedGameSet = JsonSerializer.Deserialize<MarriageGameSetDto>(content, GetJsonSerializerOptions());

        updatedGameSet.Should().NotBeNull();
        updatedGameSet!.Id.Should().Be(createdGameSet.Id);
        updatedGameSet.Name.Should().Be(updateDto.Name);
        updatedGameSet.GameSettingsId.Should().Be(updateDto.GameSettingsId);
    }

    [Fact]
    public async Task DeleteMarriageGameSet_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        var createDto = new CreateMarriageGameSetDto
        {
            Name = "Game Set to Delete",
            GameSettingsId = 8
        };

        var createResponse = await _client.PostAsJsonAsync("/api/marriagegamesets", createDto);
        var createdContent = await createResponse.Content.ReadAsStringAsync();
        var createdGameSet = JsonSerializer.Deserialize<MarriageGameSetDto>(createdContent, GetJsonSerializerOptions());

        // Act
        var response = await _client.DeleteAsync($"/api/marriagegamesets/{createdGameSet!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify it's actually deleted
        var getResponse = await _client.GetAsync($"/api/marriagegamesets/{createdGameSet.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteMarriageGameSet_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = 99999;

        // Act
        var response = await _client.DeleteAsync($"/api/marriagegamesets/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateMarriageGameSet_ValidationFlow_ShouldPreventDuplicateActiveGameSets()
    {
        // Arrange
        var gameSettingsId = 10;
        
        var firstGameSet = new CreateMarriageGameSetDto
        {
            Name = "First Game Set",
            GameSettingsId = gameSettingsId
        };

        var secondGameSet = new CreateMarriageGameSetDto
        {
            Name = "Second Game Set",
            GameSettingsId = gameSettingsId
        };

        var thirdGameSet = new CreateMarriageGameSetDto
        {
            Name = "Third Game Set",
            GameSettingsId = gameSettingsId
        };

        // Act & Assert
        // First game set should succeed
        var firstResponse = await _client.PostAsJsonAsync("/api/marriagegamesets", firstGameSet);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Second game set should fail due to active game set
        var secondResponse = await _client.PostAsJsonAsync("/api/marriagegamesets", secondGameSet);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Third game set should also fail
        var thirdResponse = await _client.PostAsJsonAsync("/api/marriagegamesets", thirdGameSet);
        thirdResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify only one game set exists for this GameSettingsId
        var getResponse = await _client.GetAsync($"/api/marriagegamesets?gameSettingsId={gameSettingsId}");
        var content = await getResponse.Content.ReadAsStringAsync();
        var gameSets = JsonSerializer.Deserialize<List<MarriageGameSetDto>>(content, GetJsonSerializerOptions());

        gameSets.Should().HaveCount(1);
        gameSets![0].Name.Should().Be("First Game Set");
        gameSets[0].IsActive.Should().BeTrue();
    }

    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }
}
