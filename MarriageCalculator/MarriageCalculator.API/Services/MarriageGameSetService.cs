using MarriageCalculator.Core.DTOs;
using MarriageCalculator.API.Repositories;
using MarriageCalculator.Core.Models;
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

    public async Task<bool> DeleteGameSetAsync(string id, string hostUserId)
    {
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
            var roundDto = new MarriageGameRoundDto
            {
                Id = r.Id,
                Sequence = r.Sequence,
                MarriageGameSetId = r.MarriageGameSetId,
                Completed = r.Completed,
                MarriageGames = new List<MarriageGameDto>(),
                TotalScore = new Dictionary<string, double>()
            };

            var games = await _context.MarriageGames.Find(g => g.MarriageGameRoundId == r.Id).SortBy(g => g.Sequence).ToListAsync();
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
                    CreatedTime = g.CreatedTime
                });
            }

            // Calculate TotalScore for this round
            if (games.Count > 0)
            {
                var gameIds = games.Select(g => g.Id).ToList();
                var scores = await _context.MarriageGameScores.Find(s => gameIds.Contains(s.MarriageGameId)).ToListAsync();
                
                roundDto.TotalScore = scores
                    .GroupBy(s => s.PlayerId)
                    .ToDictionary(g => g.Key, g => (double)g.Sum(s => s.Score));
            }

            dto.Rounds.Add(roundDto);
        }

        return dto;
    }
}