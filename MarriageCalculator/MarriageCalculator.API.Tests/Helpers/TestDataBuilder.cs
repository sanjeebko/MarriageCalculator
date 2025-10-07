using MarriageCalculator.Core.DTOs;
using MarriageCalculator.Core.Models;

namespace MarriageCalculator.API.Tests.Helpers;

/// <summary>
/// Builder class for creating test data objects with fluent interface
/// </summary>
public static class TestDataBuilder
{
    /// <summary>
    /// Creates a builder for MarriageGameSet test data
    /// </summary>
    public static MarriageGameSetBuilder GameSet() => new();

    /// <summary>
    /// Creates a builder for CreateMarriageGameSetDto test data
    /// </summary>
    public static CreateMarriageGameSetDtoBuilder CreateGameSetDto() => new();

    /// <summary>
    /// Creates a builder for GameSettings test data
    /// </summary>
    public static GameSettingsBuilder GameSettings() => new();

    /// <summary>
    /// Creates a builder for Player test data
    /// </summary>
    public static PlayerBuilder Player() => new();

    /// <summary>
    /// Creates a builder for CreatePlayerDto test data
    /// </summary>
    public static CreatePlayerDtoBuilder CreatePlayerDto() => new();

    /// <summary>
    /// Creates a builder for UpdatePlayerDto test data
    /// </summary>
    public static UpdatePlayerDtoBuilder UpdatePlayerDto() => new();

    /// <summary>
    /// Creates a builder for MarriageGame test data
    /// </summary>
    public static MarriageGameBuilder MarriageGame() => new();

    /// <summary>
    /// Creates a builder for MarriageGameRound test data
    /// </summary>
    public static MarriageGameRoundBuilder MarriageGameRound() => new();

    /// <summary>
    /// Creates a builder for MarriageGameScore test data
    /// </summary>
    public static MarriageGameScoreBuilder MarriageGameScore() => new();

    /// <summary>
    /// Creates a builder for MarriageGameSetPlayer test data
    /// </summary>
    public static MarriageGameSetPlayerBuilder MarriageGameSetPlayer() => new();

    /// <summary>
    /// Creates a builder for UserEmailVerification test data
    /// </summary>
    public static UserEmailVerificationBuilder UserEmailVerification() => new();

    /// <summary>
    /// Creates a builder for RefreshToken test data
    /// </summary>
    public static RefreshTokenBuilder RefreshToken() => new();

    /// <summary>
    /// Creates a builder for CreateGameSettingsDto test data
    /// </summary>
    public static CreateGameSettingsDtoBuilder CreateGameSettingsDto() => new();
}

/// <summary>
/// Builder for MarriageGameSet objects
/// </summary>
public class MarriageGameSetBuilder
{
    private readonly MarriageGameSet _gameSet = new();

    public MarriageGameSetBuilder WithId(int id)
    {
        _gameSet.Id = id;
        return this;
    }

    public MarriageGameSetBuilder WithName(string name)
    {
        _gameSet.Name = name;
        return this;
    }

    public MarriageGameSetBuilder WithGameSettingsId(int gameSettingsId)
    {
        _gameSet.GameSettingsId = gameSettingsId;
        return this;
    }

    public MarriageGameSetBuilder WithIsActive(bool isActive)
    {
        _gameSet.IsActive = isActive;
        return this;
    }

    public MarriageGameSetBuilder WithCreated(DateTime created)
    {
        _gameSet.Created = created;
        return this;
    }

    public MarriageGameSetBuilder WithLastPlayed(DateTime lastPlayed)
    {
        _gameSet.LastPlayed = lastPlayed;
        return this;
    }

    public MarriageGameSet Build() => _gameSet;
}

/// <summary>
/// Builder for CreateMarriageGameSetDto objects
/// </summary>
public class CreateMarriageGameSetDtoBuilder
{
    private readonly CreateMarriageGameSetDto _dto = new();

    public CreateMarriageGameSetDtoBuilder WithName(string name)
    {
        _dto.Name = name;
        return this;
    }

    public CreateMarriageGameSetDtoBuilder WithGameSettingsId(int gameSettingsId)
    {
        _dto.GameSettingsId = gameSettingsId;
        return this;
    }

    public CreateMarriageGameSetDto Build() => _dto;
}

/// <summary>
/// Builder for GameSettings objects
/// </summary>
public class GameSettingsBuilder
{
    private readonly GameSettings _settings = new();

    public GameSettingsBuilder WithId(int id)
    {
        _settings.Id = id;
        return this;
    }

    public GameSettingsBuilder WithUserId(Guid userId)
    {
        _settings.UserId = userId;
        return this;
    }

    public GameSettingsBuilder WithMurder(bool murder)
    {
        _settings.Murder = murder;
        return this;
    }

    public GameSettingsBuilder WithKidnap(bool kidnap)
    {
        _settings.Kidnap = kidnap;
        return this;
    }

    public GameSettingsBuilder WithSeenPoint(int seenPoint)
    {
        _settings.SeenPoint = seenPoint;
        return this;
    }

    public GameSettingsBuilder WithUnseenPoint(int unseenPoint)
    {
        _settings.UnseenPoint = unseenPoint;
        return this;
    }

    public GameSettingsBuilder WithPointRate(double pointRate)
    {
        _settings.PointRate = pointRate;
        return this;
    }

    public GameSettingsBuilder WithAudio(bool audio)
    {
        _settings.Audio = audio;
        return this;
    }

    public GameSettings Build() => _settings;
}

/// <summary>
/// Builder for Player objects
/// </summary>
public class PlayerBuilder
{
    private readonly Player _player = new();

    public PlayerBuilder WithId(Guid id)
    {
        _player.Id = id;
        return this;
    }

    public PlayerBuilder WithName(string name)
    {
        _player.Name = name;
        return this;
    }

    public PlayerBuilder WithEmail(string email)
    {
        _player.Email = email;
        return this;
    }

    public PlayerBuilder WithDeleted(bool deleted)
    {
        _player.Deleted = deleted;
        return this;
    }

    public PlayerBuilder WithSelected(bool selected)
    {
        _player.Selected = selected;
        return this;
    }

    public PlayerBuilder WithCreatedByUserId(Guid? userId)
    {
        _player.CreatedByUserId = userId;
        return this;
    }

    public PlayerBuilder WithCreatedAt(DateTime createdAt)
    {
        _player.CreatedAt = createdAt;
        return this;
    }

    public Player Build() => _player;
}

/// <summary>
/// Builder for CreatePlayerDto objects
/// </summary>
public class CreatePlayerDtoBuilder
{
    private readonly CreatePlayerDto _dto = new();

    public CreatePlayerDtoBuilder WithId(Guid id)
    {
        _dto.Id = id;
        return this;
    }

    public CreatePlayerDtoBuilder WithName(string name)
    {
        _dto.Name = name;
        return this;
    }

    public CreatePlayerDtoBuilder WithEmail(string email)
    {
        _dto.Email = email;
        return this;
    }

    public CreatePlayerDtoBuilder WithCreatedAt(DateTime createdAt)
    {
        _dto.CreatedAt = createdAt;
        return this;
    }

    public CreatePlayerDto Build() => _dto;
}

/// <summary>
/// Builder for UpdatePlayerDto objects
/// </summary>
public class UpdatePlayerDtoBuilder
{
    private readonly UpdatePlayerDto _dto = new();

    public UpdatePlayerDtoBuilder WithName(string name)
    {
        _dto.Name = name;
        return this;
    }

    public UpdatePlayerDtoBuilder WithEmail(string email)
    {
        _dto.Email = email;
        return this;
    }

    public UpdatePlayerDto Build() => _dto;
}

/// <summary>
/// Builder for MarriageGame objects
/// </summary>
public class MarriageGameBuilder
{
    private readonly MarriageGame _game = new();

    public MarriageGameBuilder WithId(int id)
    {
        _game.Id = id;
        return this;
    }

    public MarriageGameBuilder WithSequence(int sequence)
    {
        _game.Sequence = sequence;
        return this;
    }

    public MarriageGameBuilder WithMarriageGameRoundId(int roundId)
    {
        _game.MarriageGameRoundId = roundId;
        return this;
    }

    public MarriageGameBuilder WithWinnerId(Guid? winnerId)
    {
        _game.WinnerId = winnerId;
        return this;
    }

    public MarriageGameBuilder WithDealerId(Guid? dealerId)
    {
        _game.DealerId = dealerId;
        return this;
    }

    public MarriageGameBuilder WithTotalMaal(int totalMaal)
    {
        _game.TotalMaal = totalMaal;
        return this;
    }

    public MarriageGameBuilder WithClosedRound(bool closedRound)
    {
        _game.ClosedRound = closedRound;
        return this;
    }

    public MarriageGameBuilder WithCreatedTime(DateTime createdTime)
    {
        _game.CreatedTime = createdTime;
        return this;
    }

    public MarriageGame Build() => _game;
}

/// <summary>
/// Builder for RefreshToken objects
/// </summary>
public class RefreshTokenBuilder
{
    private readonly RefreshToken _token = new();

    public RefreshTokenBuilder WithId(int id)
    {
        _token.Id = id;
        return this;
    }

    public RefreshTokenBuilder WithUserId(Guid userId)
    {
        _token.UserId = userId;
        return this;
    }

    public RefreshTokenBuilder WithToken(string token)
    {
        _token.Token = token;
        return this;
    }

    public RefreshTokenBuilder WithExpiresAt(DateTime expiresAt)
    {
        _token.ExpiresAt = expiresAt;
        return this;
    }

    public RefreshTokenBuilder WithCreatedAt(DateTime createdAt)
    {
        _token.CreatedAt = createdAt;
        return this;
    }

    public RefreshTokenBuilder WithIsActive(bool isActive)
    {
        _token.IsActive = isActive;
        return this;
    }

    public RefreshTokenBuilder WithIsRevoked(bool isRevoked)
    {
        // IsRevoked is a computed property based on RevokedAt
        // Set RevokedAt to simulate revoked state
        _token.RevokedAt = isRevoked ? DateTime.UtcNow : null;
        return this;
    }

    public RefreshTokenBuilder WithRevokedAt(DateTime? revokedAt)
    {
        _token.RevokedAt = revokedAt;
        return this;
    }

    public RefreshTokenBuilder WithRevokedReason(string? revokedReason)
    {
        _token.RevokedReason = revokedReason;
        return this;
    }

    public RefreshTokenBuilder WithReplacedByToken(string? replacedByToken)
    {
        _token.ReplacedByToken = replacedByToken;
        return this;
    }

    public RefreshToken Build() => _token;
}

/// <summary>
/// Builder for CreateGameSettingsDto objects
/// </summary>
public class CreateGameSettingsDtoBuilder
{
    private readonly CreateGameSettingsDto _dto = new();

    public CreateGameSettingsDtoBuilder WithMurder(bool murder)
    {
        _dto.Murder = murder;
        return this;
    }

    public CreateGameSettingsDtoBuilder WithKidnap(bool kidnap)
    {
        _dto.Kidnap = kidnap;
        return this;
    }

    public CreateGameSettingsDtoBuilder WithSeenPoint(int seenPoint)
    {
        _dto.SeenPoint = seenPoint;
        return this;
    }

    public CreateGameSettingsDtoBuilder WithUnseenPoint(int unseenPoint)
    {
        _dto.UnseenPoint = unseenPoint;
        return this;
    }

    public CreateGameSettingsDtoBuilder WithPointRate(double pointRate)
    {
        _dto.PointRate = pointRate;
        return this;
    }

    public CreateGameSettingsDtoBuilder WithAudio(bool audio)
    {
        _dto.Audio = audio;
        return this;
    }

    public CreateGameSettingsDto Build() => _dto;
}

/// <summary>
/// Builder for MarriageGameRound test data
/// </summary>
public class MarriageGameRoundBuilder
{
    private readonly MarriageGameRound _round = new()
    {
        Id = 0, // Let Entity Framework assign ID
        Sequence = 1,
        MarriageGameSetId = 1,
        Completed = false
    };

    public MarriageGameRoundBuilder WithId(int id)
    {
        _round.Id = id;
        return this;
    }

    public MarriageGameRoundBuilder WithSequence(int sequence)
    {
        _round.Sequence = sequence;
        return this;
    }

    public MarriageGameRoundBuilder WithMarriageGameSetId(int gameSetId)
    {
        _round.MarriageGameSetId = gameSetId;
        return this;
    }

    public MarriageGameRoundBuilder WithCompleted(bool completed)
    {
        _round.Completed = completed;
        return this;
    }

    public MarriageGameRound Build() => _round;
}

/// <summary>
/// Builder for MarriageGameScore test data
/// </summary>
public class MarriageGameScoreBuilder
{
    private readonly MarriageGameScore _score = new()
    {
        Id = 0, // Let Entity Framework assign ID
        MarriageGameId = 1,
        PlayerId = Guid.NewGuid(),
        Position = 1,
        Seen = false,
        Playing = false,
        Maal = 0,
        BonusPoint = 0,
        Duply = false,
        Winner = false,
        Score = 0
    };

    public MarriageGameScoreBuilder WithId(int id)
    {
        _score.Id = id;
        return this;
    }

    public MarriageGameScoreBuilder WithMarriageGameId(int gameId)
    {
        _score.MarriageGameId = gameId;
        return this;
    }

    public MarriageGameScoreBuilder WithPlayerId(Guid playerId)
    {
        _score.PlayerId = playerId;
        return this;
    }

    public MarriageGameScoreBuilder WithPosition(int position)
    {
        _score.Position = position;
        return this;
    }

    public MarriageGameScoreBuilder WithSeen(bool seen)
    {
        _score.Seen = seen;
        return this;
    }

    public MarriageGameScoreBuilder WithPlaying(bool playing)
    {
        _score.Playing = playing;
        return this;
    }

    public MarriageGameScoreBuilder WithMaal(int maal)
    {
        _score.Maal = maal;
        return this;
    }

    public MarriageGameScoreBuilder WithBonusPoint(int bonusPoint)
    {
        _score.BonusPoint = bonusPoint;
        return this;
    }

    public MarriageGameScoreBuilder WithDuply(bool duply)
    {
        _score.Duply = duply;
        return this;
    }

    public MarriageGameScoreBuilder WithWinner(bool winner)
    {
        _score.Winner = winner;
        return this;
    }

    public MarriageGameScoreBuilder WithScore(int score)
    {
        _score.Score = score;
        return this;
    }

    public MarriageGameScore Build() => _score;
}

/// <summary>
/// Builder for MarriageGameSetPlayer test data
/// </summary>
public class MarriageGameSetPlayerBuilder
{
    private readonly MarriageGameSetPlayer _gameSetPlayer = new()
    {
        MarriageGameSetId = 1,
        PlayerId = Guid.NewGuid()
    };

    public MarriageGameSetPlayerBuilder WithMarriageGameSetId(int gameSetId)
    {
        _gameSetPlayer.MarriageGameSetId = gameSetId;
        return this;
    }

    public MarriageGameSetPlayerBuilder WithPlayerId(Guid playerId)
    {
        _gameSetPlayer.PlayerId = playerId;
        return this;
    }

    public MarriageGameSetPlayer Build() => _gameSetPlayer;
}

/// <summary>
/// Builder for UserEmailVerification test data
/// </summary>
public class UserEmailVerificationBuilder
{
    private readonly UserEmailVerification _verification = new()
    {
        Id = 0, // Let Entity Framework assign ID
        UserId = Guid.NewGuid(),
        VerificationCode = "123456",
        CreatedAt = DateTime.UtcNow,
        ExpiresAt = DateTime.UtcNow.AddMinutes(15),
        IsUsed = false,
        UsedAt = null
    };

    public UserEmailVerificationBuilder WithId(int id)
    {
        _verification.Id = id;
        return this;
    }

    public UserEmailVerificationBuilder WithUserId(Guid userId)
    {
        _verification.UserId = userId;
        return this;
    }

    public UserEmailVerificationBuilder WithVerificationCode(string code)
    {
        _verification.VerificationCode = code;
        return this;
    }

    public UserEmailVerificationBuilder WithCreatedAt(DateTime createdAt)
    {
        _verification.CreatedAt = createdAt;
        return this;
    }

    public UserEmailVerificationBuilder WithExpiresAt(DateTime expiresAt)
    {
        _verification.ExpiresAt = expiresAt;
        return this;
    }

    public UserEmailVerificationBuilder WithIsUsed(bool isUsed)
    {
        _verification.IsUsed = isUsed;
        return this;
    }

    public UserEmailVerificationBuilder WithUsedAt(DateTime? usedAt)
    {
        _verification.UsedAt = usedAt;
        return this;
    }

    public UserEmailVerification Build() => _verification;
}
