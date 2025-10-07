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
/// Simple authentication test to verify our test setup works
/// </summary>
public class AuthenticationTests : IClassFixture<TestWebApplicationFactory>, IDisposable
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthenticationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task TestAPI_ShouldNotReturn401()
    {
        // Arrange
        var createDto = new CreateMarriageGameSetDto
        {
            Name = "Auth Test Game Set",
            GameSettingsId = 999
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/marriagegamesets", createDto);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized, 
            $"Expected any status except 401, but got {response.StatusCode}. Response: {await response.Content.ReadAsStringAsync()}");
    }

    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }
}