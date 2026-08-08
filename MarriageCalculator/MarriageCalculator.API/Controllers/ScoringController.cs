using MarriageCalculator.Core.DTOs;
using MarriageCalculator.Core.Models;
using MarriageCalculator.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarriageCalculator.API.Controllers;

/// <summary>
/// Handles score calculation for Marriage card games.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ScoringController : ControllerBase
{
    private readonly ILogger<ScoringController> _logger;

    public ScoringController(ILogger<ScoringController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Calculates scores for a game round using the Central Collection algorithm.
    /// Supports Normal, Kidnap, and Murder game modes.
    /// </summary>
    [HttpPost("calculate")]
    public ActionResult<CalculateScoreResponseDto> CalculateScore([FromBody] CalculateScoreRequestDto request)
    {
        try
        {
            if (request.PlayerScores.Count < 2)
                return BadRequest(new CalculateScoreResponseDto 
                { 
                    IsValid = false, 
                    Message = "At least 2 players are required." 
                });

            if (!request.PlayerScores.Any(p => p.PlayerId == request.WinnerId))
                return BadRequest(new CalculateScoreResponseDto 
                { 
                    IsValid = false, 
                    Message = "Winner must be one of the playing players." 
                });

            // Build GameSettings from DTO
            var settings = new GameSettings
            {
                Murder = request.Settings.Murder,
                Kidnap = request.Settings.Kidnap,
                SeenPoint = request.Settings.SeenPoint,
                UnseenPoint = request.Settings.UnseenPoint,
                PointRate = request.Settings.PointRate,
                Dublee = request.Settings.Dublee,
                DubleePointBonus = request.Settings.DubleePointBonus
            };

            // Build MarriageGame with scores
            var game = new MarriageGame { WinnerId = request.WinnerId };
            foreach (var ps in request.PlayerScores)
            {
                game.MarriageGameScores[ps.PlayerId] = new MarriageGameScore
                {
                    PlayerId = ps.PlayerId,
                    Seen = ps.Seen || ps.PlayerId == request.WinnerId,
                    Playing = ps.Playing,
                    Maal = ps.Maal,
                    Duply = ps.Duply,
                    Winner = ps.PlayerId == request.WinnerId,
                    Score = 0,
                    MoneyWon = 0
                };
            }

            // Calculate
            ScoringEngine.CalculateScores(game, settings);
            bool isZeroSum = ScoringEngine.ValidateZeroSum(game);

            // Map results
            var results = game.MarriageGameScores.Values
                .Where(s => s.Playing)
                .Select(s => new PlayerScoreResultDto
                {
                    PlayerId = s.PlayerId,
                    PlayerName = request.PlayerScores.FirstOrDefault(p => p.PlayerId == s.PlayerId)?.PlayerName ?? "",
                    Seen = s.Seen,
                    Winner = s.Winner,
                    Duply = s.Duply,
                    Maal = s.Maal,
                    Score = s.Score,
                    MoneyWon = s.MoneyWon
                })
                .ToList();

            return Ok(new CalculateScoreResponseDto
            {
                IsValid = isZeroSum,
                Message = isZeroSum ? "Scores calculated successfully." : "Warning: Scores are not zero-sum.",
                Results = results,
                TotalMaal = game.TotalMaal
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating scores");
            return StatusCode(500, new CalculateScoreResponseDto 
            { 
                IsValid = false, 
                Message = $"Error: {ex.Message}" 
            });
        }
    }
}
