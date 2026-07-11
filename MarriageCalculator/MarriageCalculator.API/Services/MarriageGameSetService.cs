using MarriageCalculator.Core.DTOs;
using MarriageCalculator.API.Repositories;
using MarriageCalculator.Core.Models;
using MarriageCalculator.Core.Services;
using MarriageCalculator.API.Data;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarriageCalculator.API.Services;

public class MarriageGameSetService : IMarriageGameSetService
{
    private readonly IMarriageGameSetRepository _gameSetRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IUserRepository _userRepository;
    private readonly IFcmService _fcmService;
    private readonly MongoDbContext _context;

    public MarriageGameSetService(
        IMarriageGameSetRepository gameSetRepository, 
        IPlayerRepository playerRepository,
        IUserRepository userRepository,
        IFcmService fcmService,
        MongoDbContext context)
    {
        _gameSetRepository = gameSetRepository;
        _playerRepository = playerRepository;
        _userRepository = userRepository;
        _fcmService = fcmService;
        _context = context;
    }

    public async Task<IEnumerable<MarriageGameSetDto>> GetAllGameSetsAsync(string hostUserId, string email)
    {
        var playerIds = new List<string>();
        if (!string.IsNullOrEmpty(email))
        {
            var players = await _playerRepository.GetPlayersByEmailAsync(email);
            playerIds.AddRange(players.Select(p => p.Id));
        }

        var gameSets = await _gameSetRepository.GetAllForUserAsync(hostUserId, playerIds);
        
        var dtoList = new List<MarriageGameSetDto>();
        foreach (var gs in gameSets)
        {
            dtoList.Add(await MapToDtoAsync(gs));
        }
        return dtoList;
    }

    public async Task<MarriageGameSetDto?> GetGameSetByIdAsync(string id, string hostUserId, string email)
    {
        var gameSet = await _gameSetRepository.GetByIdRawAsync(id);
        if (gameSet == null) return null;

        // Verify authorization: is owner/host or participant player
        bool authorized = false;
        if (gameSet.HostUserId == hostUserId)
        {
            authorized = true;
        }
        else if (!string.IsNullOrEmpty(email))
        {
            var players = await _playerRepository.GetPlayersByEmailAsync(email);
            var playerIds = players.Select(p => p.Id).ToList();
            if (gameSet.PlayerIds.Any(pId => playerIds.Contains(pId)))
            {
                authorized = true;
            }
        }

        if (authorized)
        {
            return await MapToDtoAsync(gameSet);
        }

        return null; // Not authorized or not found
    }

    public async Task<MarriageGameSetDto> CreateGameSetAsync(CreateMarriageGameSetDto createDto)
    {
        var gameSet = new MarriageGameSet
        {
            HostUserId = createDto.HostUserId,
            Name = createDto.Name,
            GameSettingsId = createDto.GameSettingsId,
            Created = DateTime.UtcNow,
            LastPlayed = DateTime.UtcNow,
            IsActive = true,
            PlayerIds = createDto.PlayerIds
        };

        var createdGameSet = await _gameSetRepository.CreateAsync(gameSet);
        return await MapToDtoAsync(createdGameSet);
    }

    public async Task<MarriageGameSetDto?> UpdateGameSetAsync(string id, CreateMarriageGameSetDto updateDto, string hostUserId)
    {
        // Freeze history before a reshuffle: legacy rounds created before per-round seat
        // snapshots existed have no PlayerIds and would otherwise re-render in the new order.
        // Stamp them with the outgoing order - the seating they were actually played with.
        var existing = await _gameSetRepository.GetByIdRawAsync(id);
        if (existing != null && existing.PlayerIds.Count > 0 &&
            !existing.PlayerIds.SequenceEqual(updateDto.PlayerIds))
        {
            // Legacy round docs predate the PlayerIds field entirely, so the filter must match
            // both a missing field and an empty array ($size: 0 alone misses absent fields).
            var unsnapshotted = Builders<MarriageGameRound>.Filter.And(
                Builders<MarriageGameRound>.Filter.Eq(r => r.MarriageGameSetId, id),
                Builders<MarriageGameRound>.Filter.Or(
                    Builders<MarriageGameRound>.Filter.Exists(r => r.PlayerIds, false),
                    Builders<MarriageGameRound>.Filter.Size(r => r.PlayerIds, 0)));
            await _context.MarriageGameRounds.UpdateManyAsync(
                unsnapshotted,
                Builders<MarriageGameRound>.Update.Set(r => r.PlayerIds, existing.PlayerIds));
        }

        var gameSetToUpdate = new MarriageGameSet
        {
            HostUserId = updateDto.HostUserId,
            Name = updateDto.Name,
            GameSettingsId = updateDto.GameSettingsId,
            PlayerIds = updateDto.PlayerIds,
            LastPlayed = DateTime.UtcNow
        };

        var updatedGameSet = await _gameSetRepository.UpdateAsync(id, gameSetToUpdate, hostUserId);
        return updatedGameSet != null ? await MapToDtoAsync(updatedGameSet) : null;
    }

    /// <summary>
    /// Deletes an entire game set: every round, every game, and every score belonging to it,
    /// then the game set itself. Irreversible - the caller must confirm with the user first.
    /// </summary>
    public async Task<bool> DeleteGameSetAsync(string id, string hostUserId)
    {
        var gameSet = await _gameSetRepository.GetByIdRawAsync(id);
        if (gameSet == null || gameSet.HostUserId != hostUserId) return false;

        var roundIds = await _context.MarriageGameRounds
            .Find(r => r.MarriageGameSetId == id)
            .Project(r => r.Id)
            .ToListAsync();

        if (roundIds.Count > 0)
        {
            var gameIds = await _context.MarriageGames
                .Find(g => roundIds.Contains(g.MarriageGameRoundId))
                .Project(g => g.Id)
                .ToListAsync();

            if (gameIds.Count > 0)
            {
                await _context.MarriageGameScores.DeleteManyAsync(s => gameIds.Contains(s.MarriageGameId));
                await _context.MarriageGames.DeleteManyAsync(g => roundIds.Contains(g.MarriageGameRoundId));
            }

            await _context.MarriageGameRounds.DeleteManyAsync(r => r.MarriageGameSetId == id);
        }

        return await _gameSetRepository.DeleteAsync(id, hostUserId);
    }

    public async Task<bool> GameSetExistsAsync(string id, string hostUserId)
    {
        return await _gameSetRepository.ExistsAsync(id, hostUserId);
    }

    public async Task<MarriageGameSetDto?> GetLatestActiveGameSetAsync(string hostUserId)
    {
        var gameSet = await _gameSetRepository.GetLatestActiveAsync(hostUserId);
        return gameSet != null ? await MapToDtoAsync(gameSet) : null;
    }

    public async Task<MarriageGameSetDto?> TransferHostAsync(string id, string currentHostUserId, string newHostUserId)
    {
        var gameSet = await _gameSetRepository.GetByIdRawAsync(id);
        if (gameSet == null) return null;

        if (gameSet.HostUserId != currentHostUserId)
        {
            throw new UnauthorizedAccessException("Only the current host can transfer game set ownership.");
        }

        gameSet.HostUserId = newHostUserId;
        gameSet.LastPlayed = DateTime.UtcNow;

        var updated = await _gameSetRepository.UpdateAsync(id, gameSet, currentHostUserId);
        return updated != null ? await MapToDtoAsync(updated) : null;
    }

    public async Task<bool> NudgePlayerAsync(string gameSetId, string hostUserId, string playerId)
    {
        var gameSet = await _gameSetRepository.GetByIdRawAsync(gameSetId);
        if (gameSet == null) return false;

        // Verify that the caller is indeed the host of the game set
        if (gameSet.HostUserId != hostUserId)
        {
            throw new UnauthorizedAccessException("Only the game host can nudge other players.");
        }

        var player = await _playerRepository.GetByIdAsync(playerId);
        if (player == null || string.IsNullOrEmpty(player.Email)) return false;

        var targetUser = await _userRepository.GetByEmailAsync(player.Email);
        if (targetUser == null || string.IsNullOrEmpty(targetUser.FcmToken)) return false;

        var hostUser = await _userRepository.GetByUserIdAsync(hostUserId);
        var hostName = hostUser?.DisplayName ?? "The host";

        await _fcmService.SendNotificationAsync(
            targetUser.FcmToken,
            "Game Nudge!",
            $"{hostName} is waiting for you in the game '{gameSet.Name}'. Join now!",
            new Dictionary<string, string> { { "gameSetId", gameSetId } }
        );

        return true;
    }

    /// <summary>
    /// Records one game's result within the game set's current round. A round holds one game per
    /// player (dealt in turn); this appends to the latest open round, or starts a new one if the
    /// previous round is already complete (or was closed early). Scores are always computed
    /// server-side via ScoringEngine, never trusted from the client.
    /// </summary>
    public async Task<MarriageGameRoundDto> SubmitRoundAsync(string gameSetId, string hostUserId, SubmitRoundDto dto)
    {
        var gameSet = await _gameSetRepository.GetByIdRawAsync(gameSetId);
        if (gameSet == null)
        {
            throw new KeyNotFoundException($"Marriage game set with ID {gameSetId} not found");
        }

        if (gameSet.HostUserId != hostUserId)
        {
            throw new UnauthorizedAccessException("Only the game host can add rounds.");
        }

        if (dto.Players.Count < 2)
        {
            throw new ArgumentException("At least 2 players are required.");
        }

        if (!dto.Players.Any(p => p.PlayerId == dto.WinnerId))
        {
            throw new ArgumentException("Winner must be one of the players in this round.");
        }

        var settingsDoc = !string.IsNullOrEmpty(gameSet.GameSettingsId)
            ? await _context.GameSettings.Find(s => s.Id == gameSet.GameSettingsId).FirstOrDefaultAsync()
            : null;
        var settings = settingsDoc ?? new GameSettings();

        // Reuse the latest still-open round, or start a new one if none is open.
        var round = await _context.MarriageGameRounds
            .Find(r => r.MarriageGameSetId == gameSetId && !r.Completed)
            .SortByDescending(r => r.Sequence)
            .FirstOrDefaultAsync();

        if (round == null)
        {
            var existingRoundsCount = await _context.MarriageGameRounds.CountDocumentsAsync(r => r.MarriageGameSetId == gameSetId);
            round = new MarriageGameRound
            {
                Sequence = (int)existingRoundsCount + 1,
                MarriageGameSetId = gameSetId,
                Completed = false,
                // Snapshot the seating so a later reshuffle doesn't rewrite this round's history.
                PlayerIds = [.. gameSet.PlayerIds]
            };
            await _context.MarriageGameRounds.InsertOneAsync(round);
        }

        var gamesInRoundCount = await _context.MarriageGames.CountDocumentsAsync(g => g.MarriageGameRoundId == round.Id);
        var gameSequence = (int)gamesInRoundCount + 1;
        var playerCount = gameSet.PlayerIds.Count;
        var completesRound = gameSequence >= playerCount;

        var game = new MarriageGame
        {
            Sequence = gameSequence,
            MarriageGameRoundId = round.Id,
            WinnerId = dto.WinnerId,
            DealerId = dto.DealerId,
            ClosedRound = completesRound,
            CreatedTime = DateTime.UtcNow
        };

        // Compute scores server-side using the same engine ScoringController exposes for previews.
        foreach (var p in dto.Players)
        {
            game.MarriageGameScores[p.PlayerId] = new MarriageGameScore
            {
                PlayerId = p.PlayerId,
                Seen = p.Seen || p.PlayerId == dto.WinnerId,
                Playing = true,
                Maal = p.Maal,
                Duply = p.Duply,
                Winner = p.PlayerId == dto.WinnerId,
                Deal = p.PlayerId == dto.DealerId
            };
        }
        ScoringEngine.CalculateScores(game, settings);

        await _context.MarriageGames.InsertOneAsync(game);

        var scoreDocs = game.MarriageGameScores.Values.Select(s =>
        {
            s.MarriageGameId = game.Id;
            return s;
        }).ToList();
        await _context.MarriageGameScores.InsertManyAsync(scoreDocs);

        if (completesRound)
        {
            await _context.MarriageGameRounds.UpdateOneAsync(
                r => r.Id == round.Id,
                Builders<MarriageGameRound>.Update.Set(r => r.Completed, true));
            round.Completed = true;
        }

        gameSet.LastPlayed = DateTime.UtcNow;
        await _gameSetRepository.UpdateAsync(gameSetId, gameSet, hostUserId);

        return await BuildRoundDtoAsync(round);
    }

    /// <summary>
    /// Ends the current round early (fewer than N games played), e.g. when players want to add
    /// someone new or restart. The next submitted game starts a fresh round.
    /// </summary>
    public async Task<MarriageGameRoundDto?> CloseRoundAsync(string gameSetId, string roundId, string hostUserId)
    {
        var gameSet = await _gameSetRepository.GetByIdRawAsync(gameSetId);
        if (gameSet == null)
        {
            throw new KeyNotFoundException($"Marriage game set with ID {gameSetId} not found");
        }

        if (gameSet.HostUserId != hostUserId)
        {
            throw new UnauthorizedAccessException("Only the game host can close a round.");
        }

        var round = await _context.MarriageGameRounds
            .Find(r => r.Id == roundId && r.MarriageGameSetId == gameSetId)
            .FirstOrDefaultAsync();
        if (round == null) return null;

        if (!round.Completed)
        {
            await _context.MarriageGameRounds.UpdateOneAsync(
                r => r.Id == roundId,
                Builders<MarriageGameRound>.Update.Set(r => r.Completed, true));
            round.Completed = true;
        }

        return await BuildRoundDtoAsync(round);
    }

    /// <summary>
    /// Removes only the most recently played game (the last game of the last round), to undo a
    /// mistake. If that was the round's only game, the now-empty round is removed too. Blocked
    /// once the game set is settled (IsActive = false).
    /// </summary>
    public async Task<MarriageGameRoundDto?> DeleteLastGameAsync(string gameSetId, string hostUserId)
    {
        var gameSet = await _gameSetRepository.GetByIdRawAsync(gameSetId);
        if (gameSet == null)
        {
            throw new KeyNotFoundException($"Marriage game set with ID {gameSetId} not found");
        }

        if (gameSet.HostUserId != hostUserId)
        {
            throw new UnauthorizedAccessException("Only the game host can delete a game.");
        }

        if (!gameSet.IsActive)
        {
            throw new InvalidOperationException("Cannot modify a settled game set.");
        }

        var round = await _context.MarriageGameRounds
            .Find(r => r.MarriageGameSetId == gameSetId)
            .SortByDescending(r => r.Sequence)
            .FirstOrDefaultAsync();
        if (round == null)
        {
            throw new InvalidOperationException("There are no games to delete.");
        }

        var lastGame = await _context.MarriageGames
            .Find(g => g.MarriageGameRoundId == round.Id)
            .SortByDescending(g => g.Sequence)
            .FirstOrDefaultAsync();
        if (lastGame == null)
        {
            throw new InvalidOperationException("There are no games to delete.");
        }

        await _context.MarriageGameScores.DeleteManyAsync(s => s.MarriageGameId == lastGame.Id);
        await _context.MarriageGames.DeleteOneAsync(g => g.Id == lastGame.Id);

        var remainingCount = await _context.MarriageGames.CountDocumentsAsync(g => g.MarriageGameRoundId == round.Id);
        if (remainingCount == 0)
        {
            await _context.MarriageGameRounds.DeleteOneAsync(r => r.Id == round.Id);
            return null;
        }

        if (round.Completed)
        {
            await _context.MarriageGameRounds.UpdateOneAsync(
                r => r.Id == round.Id,
                Builders<MarriageGameRound>.Update.Set(r => r.Completed, false));
            round.Completed = false;
        }

        return await BuildRoundDtoAsync(round);
    }

    /// <summary>
    /// Deletes an entire round - every game and score in it - and renumbers later rounds down so
    /// round sequence numbers stay contiguous. Blocked once the game set is settled.
    /// </summary>
    public async Task<bool> DeleteRoundAsync(string gameSetId, string roundId, string hostUserId)
    {
        var gameSet = await _gameSetRepository.GetByIdRawAsync(gameSetId);
        if (gameSet == null)
        {
            throw new KeyNotFoundException($"Marriage game set with ID {gameSetId} not found");
        }

        if (gameSet.HostUserId != hostUserId)
        {
            throw new UnauthorizedAccessException("Only the game host can delete a round.");
        }

        if (!gameSet.IsActive)
        {
            throw new InvalidOperationException("Cannot modify a settled game set.");
        }

        var round = await _context.MarriageGameRounds
            .Find(r => r.Id == roundId && r.MarriageGameSetId == gameSetId)
            .FirstOrDefaultAsync();
        if (round == null) return false;

        var gameIds = await _context.MarriageGames
            .Find(g => g.MarriageGameRoundId == round.Id)
            .Project(g => g.Id)
            .ToListAsync();

        if (gameIds.Count > 0)
        {
            await _context.MarriageGameScores.DeleteManyAsync(s => gameIds.Contains(s.MarriageGameId));
            await _context.MarriageGames.DeleteManyAsync(g => g.MarriageGameRoundId == round.Id);
        }

        await _context.MarriageGameRounds.DeleteOneAsync(r => r.Id == round.Id);

        var laterRounds = await _context.MarriageGameRounds
            .Find(r => r.MarriageGameSetId == gameSetId && r.Sequence > round.Sequence)
            .ToListAsync();
        foreach (var later in laterRounds)
        {
            await _context.MarriageGameRounds.UpdateOneAsync(
                r => r.Id == later.Id,
                Builders<MarriageGameRound>.Update.Set(r => r.Sequence, later.Sequence - 1));
        }

        return true;
    }

    private async Task<MarriageGameSetDto> MapToDtoAsync(MarriageGameSet gameSet)
    {
        var dto = new MarriageGameSetDto
        {
            Id = gameSet.Id,
            HostUserId = gameSet.HostUserId,
            Name = gameSet.Name,
            LastPlayed = gameSet.LastPlayed,
            Created = gameSet.Created,
            IsActive = gameSet.IsActive,
            GameSettingsId = gameSet.GameSettingsId,
            PlayerIds = gameSet.PlayerIds
        };

        // 1. Fetch GameSettings
        if (!string.IsNullOrEmpty(gameSet.GameSettingsId))
        {
            var settings = await _context.GameSettings.Find(s => s.Id == gameSet.GameSettingsId).FirstOrDefaultAsync();
            if (settings != null)
            {
                dto.GameSettings = new GameSettingsDto
                {
                    Id = settings.Id,
                    UserId = settings.UserId,
                    Murder = settings.Murder,
                    Kidnap = settings.Kidnap,
                    SeenPoint = settings.SeenPoint,
                    UnseenPoint = settings.UnseenPoint,
                    PointRate = settings.PointRate,
                    Currency = settings.Currency.ToString(),
                    Dublee = settings.Dublee,
                    DubleePointLess = settings.DubleePointLess,
                    DubleePointBonus = settings.DubleePointBonus,
                    FoulPoint = settings.FoulPoint,
                    FoulPointBonus = settings.FoulPointBonus.ToString(),
                    Audio = settings.Audio
                };
            }
        }

        // 2. Fetch Players and construct GameSetPlayers
        dto.GameSetPlayers = new Dictionary<string, MarriageGameSetPlayerDto>();
        for (int i = 0; i < gameSet.PlayerIds.Count; i++)
        {
            var playerId = gameSet.PlayerIds[i];
            var player = await _context.Players.Find(p => p.Id == playerId).FirstOrDefaultAsync();
            
            var playerDto = player != null ? new PlayerDto
            {
                Id = player.Id,
                Name = player.Name,
                Email = player.Email,
                Deleted = player.Deleted
            } : null;

            dto.GameSetPlayers[playerId] = new MarriageGameSetPlayerDto
            {
                Id = $"{gameSet.Id}_{playerId}",
                MarriageGameSetId = gameSet.Id,
                PlayerId = playerId,
                Position = i,
                IsActive = true,
                Player = playerDto
            };
        }

        // 3. Fetch Rounds & Games
        var rounds = await _context.MarriageGameRounds.Find(r => r.MarriageGameSetId == gameSet.Id).SortBy(r => r.Sequence).ToListAsync();
        dto.Rounds = new List<MarriageGameRoundDto>();

        foreach (var r in rounds)
        {
            dto.Rounds.Add(await BuildRoundDtoAsync(r));
        }

        return dto;
    }

    /// <summary>
    /// Builds the DTO for a single round: all its games, each game's per-player scores, and the
    /// round's total score per player (summed across all games in the round).
    /// </summary>
    private async Task<MarriageGameRoundDto> BuildRoundDtoAsync(MarriageGameRound r)
    {
        var roundDto = new MarriageGameRoundDto
        {
            Id = r.Id,
            Sequence = r.Sequence,
            MarriageGameSetId = r.MarriageGameSetId,
            Completed = r.Completed,
            PlayerIds = r.PlayerIds,
            MarriageGames = new List<MarriageGameDto>(),
            TotalScore = new Dictionary<string, double>()
        };

        var games = await _context.MarriageGames.Find(g => g.MarriageGameRoundId == r.Id).SortBy(g => g.Sequence).ToListAsync();
        var gameIds = games.Select(g => g.Id).ToList();
        var scores = gameIds.Count > 0
            ? await _context.MarriageGameScores.Find(s => gameIds.Contains(s.MarriageGameId)).ToListAsync()
            : new List<MarriageGameScore>();

        roundDto.TotalScore = scores
            .GroupBy(s => s.PlayerId)
            .ToDictionary(g => g.Key, g => (double)g.Sum(s => s.Score));

        var scoresByGame = scores.ToLookup(s => s.MarriageGameId);
        foreach (var g in games)
        {
            roundDto.MarriageGames.Add(new MarriageGameDto
            {
                Id = g.Id,
                Sequence = g.Sequence,
                MarriageGameRoundId = g.MarriageGameRoundId,
                WinnerId = g.WinnerId,
                DealerId = g.DealerId,
                TotalMaal = g.TotalMaal,
                ClosedRound = g.ClosedRound,
                CreatedTime = g.CreatedTime,
                MarriageGameScores = scoresByGame[g.Id].ToDictionary(s => s.PlayerId, s => new MarriageGameScoreDto
                {
                    Id = s.Id,
                    MarriageGameId = s.MarriageGameId,
                    PlayerId = s.PlayerId,
                    Seen = s.Seen,
                    Playing = s.Playing,
                    Maal = s.Maal,
                    BonusPoint = s.BonusPoint,
                    Duply = s.Duply,
                    Winner = s.Winner,
                    Score = s.Score,
                    MoneyWon = s.MoneyWon,
                    Deal = s.Deal,
                    Position = s.Position
                })
            });
        }

        return roundDto;
    }
}