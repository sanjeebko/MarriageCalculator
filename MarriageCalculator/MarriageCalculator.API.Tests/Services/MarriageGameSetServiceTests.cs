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

        // Assert: only valid 24-digit hex ObjectIds are passed (email-matched Players plus the
        // caller's own User document id), and the non-ObjectId googleUid is NOT in playerIds
        Assert.NotNull(capturedPlayerIds);
        Assert.Equal(3, capturedPlayerIds.Count);
        Assert.Contains("507f1f77bcf86cd799439011", capturedPlayerIds);
        Assert.Contains("507f191e810c19729de860ea", capturedPlayerIds);
        Assert.Contains("507f1f77bcf86cd799439000", capturedPlayerIds);
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
        Assert.Equal(2, capturedPlayerIds.Count);
        Assert.Contains("507f1f77bcf86cd799439011", capturedPlayerIds);
        Assert.Contains("507f1f77bcf86cd799439000", capturedPlayerIds);
    }

    [Fact]
    public async Task GetJoinedGameSetsAsync_IncludesCallersUserId_ForFriendAddedWithoutPlayerRecord()
    {
        // Arrange: a friend added to a game by the host is stored in PlayerIds under their
        // User document id; they have no Player document of their own (issue #121).
        var friendUid = "222228289240832351249";
        var friendUserDocId = "65a1b2c3d4e5f6a7b8c9d0e1";
        var email = "friend@example.com";

        _userRepoMock.Setup(r => r.GetByUserIdAsync(friendUid))
            .ReturnsAsync(new User { Id = friendUserDocId, UserId = friendUid, Email = email });
        _playerRepoMock.Setup(r => r.GetPlayersByEmailAsync(email))
            .ReturnsAsync(new List<Player>());

        List<string>? capturedPlayerIds = null;
        _gameSetRepoMock.Setup(r => r.GetJoinedForUserAsync(friendUid, It.IsAny<List<string>>()))
            .Callback<string, List<string>>((uid, pIds) => capturedPlayerIds = pIds)
            .ReturnsAsync(new List<MarriageGameSet>());

        // Act
        await _service.GetJoinedGameSetsAsync(friendUid, email);

        // Assert: the friend's own User id is searched for, so the host's game is found
        Assert.NotNull(capturedPlayerIds);
        Assert.Equal(new[] { friendUserDocId }, capturedPlayerIds);
    }

    [Fact]
    public async Task GetGameSetByIdAsync_ReturnsNull_ForUserWhoIsNotAParticipant()
    {
        // Arrange: game hosted by someone else; caller is neither host nor in PlayerIds
        var gameSetId = "65a1b2c3d4e5f6a7b8c9d0ff";
        var strangerUid = "333338289240832351249";
        var email = "stranger@example.com";

        _gameSetRepoMock.Setup(r => r.GetByIdRawAsync(gameSetId))
            .ReturnsAsync(new MarriageGameSet
            {
                Id = gameSetId,
                HostUserId = "host-uid",
                PlayerIds = new List<string> { "65a1b2c3d4e5f6a7b8c9d0e1", "65a1b2c3d4e5f6a7b8c9d0e2" }
            });
        _userRepoMock.Setup(r => r.GetByUserIdAsync(strangerUid))
            .ReturnsAsync(new User { Id = "65a1b2c3d4e5f6a7b8c9d0e9", UserId = strangerUid, Email = email });
        _playerRepoMock.Setup(r => r.GetPlayersByEmailAsync(email))
            .ReturnsAsync(new List<Player>());

        // Act
        var result = await _service.GetGameSetByIdAsync(gameSetId, strangerUid, email);

        // Assert: non-participants still can't read someone else's game
        Assert.Null(result);
    }

    [Fact]
    public async Task TransferHostAsync_ToFriendByUserDocId_StoresFriendsAuthUserId()
    {
        // Arrange: the app sends the friend's id as it appears in PlayerIds (User document id)
        var gameSetId = "65a1b2c3d4e5f6a7b8c9d0ff";
        var hostUid = "host-uid";
        var friendUid = "222228289240832351249";
        var friendDocId = "65a1b2c3d4e5f6a7b8c9d0e1";

        _gameSetRepoMock.Setup(r => r.GetByIdRawAsync(gameSetId))
            .ReturnsAsync(new MarriageGameSet
            {
                Id = gameSetId,
                HostUserId = hostUid,
                PlayerIds = new List<string> { "65a1b2c3d4e5f6a7b8c9d0e0", friendDocId }
            });
        var friend = new User { Id = friendDocId, UserId = friendUid, Email = "friend@example.com" };
        _userRepoMock.Setup(r => r.GetByIdAsync(friendDocId)).ReturnsAsync(friend);
        _userRepoMock.Setup(r => r.GetByUserIdAsync(friendUid)).ReturnsAsync(friend);
        _playerRepoMock.Setup(r => r.GetPlayersByEmailAsync(It.IsAny<string>())).ReturnsAsync(new List<Player>());

        MarriageGameSet? saved = null;
        _gameSetRepoMock.Setup(r => r.UpdateAsync(gameSetId, It.IsAny<MarriageGameSet>(), hostUid))
            .Callback<string, MarriageGameSet, string>((_, gs, _) => saved = gs)
            .ReturnsAsync((MarriageGameSet?)null);

        // Act
        await _service.TransferHostAsync(gameSetId, hostUid, friendDocId);

        // Assert: HostUserId holds the auth id the friend's requests carry, not the doc id
        Assert.NotNull(saved);
        Assert.Equal(friendUid, saved!.HostUserId);
    }

    [Fact]
    public async Task TransferHostAsync_ToUserNotInGame_IsRejected()
    {
        var gameSetId = "65a1b2c3d4e5f6a7b8c9d0ff";
        var outsiderDocId = "65a1b2c3d4e5f6a7b8c9d0e9";
        var outsider = new User { Id = outsiderDocId, UserId = "outsider-uid", Email = "out@example.com" };

        _gameSetRepoMock.Setup(r => r.GetByIdRawAsync(gameSetId))
            .ReturnsAsync(new MarriageGameSet
            {
                Id = gameSetId,
                HostUserId = "host-uid",
                PlayerIds = new List<string> { "65a1b2c3d4e5f6a7b8c9d0e0", "65a1b2c3d4e5f6a7b8c9d0e1" }
            });
        _userRepoMock.Setup(r => r.GetByIdAsync(outsiderDocId)).ReturnsAsync(outsider);
        _userRepoMock.Setup(r => r.GetByUserIdAsync("outsider-uid")).ReturnsAsync(outsider);
        _playerRepoMock.Setup(r => r.GetPlayersByEmailAsync(It.IsAny<string>())).ReturnsAsync(new List<Player>());

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.TransferHostAsync(gameSetId, "host-uid", outsiderDocId));
        _gameSetRepoMock.Verify(
            r => r.UpdateAsync(It.IsAny<string>(), It.IsAny<MarriageGameSet>(), It.IsAny<string>()), Times.Never);
    }
}
