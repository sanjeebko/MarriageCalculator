using MarriageCalculator.API.Repositories;
using MarriageCalculator.API.Services;
using MarriageCalculator.Core.Models;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MarriageCalculator.API.Tests.Services;

public class MarriageGameSetServiceTests
{
    private readonly Mock<IMarriageGameSetRepository> _gameSetRepoMock;
    private readonly Mock<IPlayerRepository> _playerRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IFcmService> _fcmServiceMock;
    private readonly MarriageGameSetService _service;

    public MarriageGameSetServiceTests()
    {
        _gameSetRepoMock = new Mock<IMarriageGameSetRepository>();
        _playerRepoMock = new Mock<IPlayerRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _fcmServiceMock = new Mock<IFcmService>();

        _service = new MarriageGameSetService(
            _gameSetRepoMock.Object,
            _playerRepoMock.Object,
            _userRepoMock.Object,
            _fcmServiceMock.Object,
            null!);
    }

    [Fact]
    public async Task GetAllGameSetsAsync_FiltersOutNonObjectIdPlayerIds_AndExcludesHostUserIdFromPlayerIds()
    {
        // Arrange: alphanumeric Google user ID and players with valid and invalid ObjectIds
        var googleUid = "111118289240832351249";
        var email = "player@example.com";

        _userRepoMock.Setup(r => r.GetByUserIdAsync(googleUid))
            .ReturnsAsync(new User { Id = "507f1f77bcf86cd799439000", UserId = googleUid, Email = email });

        _playerRepoMock.Setup(r => r.GetPlayersByEmailAsync(email))
            .ReturnsAsync(new List<Player>
            {
                new() { Id = "507f1f77bcf86cd799439011", Name = "Valid Player 1", Email = email },
                new() { Id = "not-an-objectid-123", Name = "Invalid Player", Email = email },
                new() { Id = "507f191e810c19729de860ea", Name = "Valid Player 2", Email = email }
            });

        List<string>? capturedPlayerIds = null;
        _gameSetRepoMock.Setup(r => r.GetAllForUserAsync(googleUid, It.IsAny<List<string>>()))
            .Callback<string, List<string>>((uid, pIds) => capturedPlayerIds = pIds)
            .ReturnsAsync(new List<MarriageGameSet>());

        // Act
        var result = await _service.GetAllGameSetsAsync(googleUid, string.Empty);

        // Assert: only valid 24-digit hex ObjectIds are passed, and googleUid is NOT in playerIds
        Assert.NotNull(capturedPlayerIds);
        Assert.Equal(2, capturedPlayerIds.Count);
        Assert.Contains("507f1f77bcf86cd799439011", capturedPlayerIds);
        Assert.Contains("507f191e810c19729de860ea", capturedPlayerIds);
        Assert.DoesNotContain(googleUid, capturedPlayerIds);
        Assert.DoesNotContain("not-an-objectid-123", capturedPlayerIds);
    }

    [Fact]
    public async Task GetJoinedGameSetsAsync_FiltersOutNonObjectIdPlayerIds()
    {
        // Arrange
        var googleUid = "111118289240832351249";
        var email = "test@example.com";

        _userRepoMock.Setup(r => r.GetByUserIdAsync(googleUid))
            .ReturnsAsync(new User { Id = "507f1f77bcf86cd799439000", UserId = googleUid, Email = email });

        _playerRepoMock.Setup(r => r.GetPlayersByEmailAsync(email))
            .ReturnsAsync(new List<Player>
            {
                new() { Id = "507f1f77bcf86cd799439011", Name = "Valid Player", Email = email }
            });

        List<string>? capturedPlayerIds = null;
        _gameSetRepoMock.Setup(r => r.GetJoinedForUserAsync(googleUid, It.IsAny<List<string>>()))
            .Callback<string, List<string>>((uid, pIds) => capturedPlayerIds = pIds)
            .ReturnsAsync(new List<MarriageGameSet>());

        // Act
        var result = await _service.GetJoinedGameSetsAsync(googleUid, string.Empty);

        // Assert
        Assert.NotNull(capturedPlayerIds);
        Assert.Single(capturedPlayerIds);
        Assert.Equal("507f1f77bcf86cd799439011", capturedPlayerIds[0]);
    }
}
