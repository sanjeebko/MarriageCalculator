using MarriageCalculator.API.Repositories;
using MarriageCalculator.API.Services;
using MarriageCalculator.Core.DTOs;
using MarriageCalculator.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarriageCalculator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILoginAuditRepository? _loginAuditRepository;
    private readonly IUserRepository? _userRepository;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService, 
        ILogger<AuthController> logger,
        ILoginAuditRepository? loginAuditRepository = null,
        IUserRepository? userRepository = null)
    {
        _authService = authService;
        _logger = logger;
        _loginAuditRepository = loginAuditRepository;
        _userRepository = userRepository;
    }

    private string GetClientIp()
    {
        try
        {
            if (Request?.Headers != null)
            {
                if (Request.Headers.TryGetValue("CF-Connecting-IP", out var cfIp) && !string.IsNullOrWhiteSpace(cfIp))
                {
                    return cfIp.ToString();
                }
                if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor) && !string.IsNullOrWhiteSpace(forwardedFor))
                {
                    return forwardedFor.ToString().Split(',')[0].Trim();
                }
            }
            return HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    private string GetClientUserAgent()
    {
        try
        {
            return Request?.Headers?.UserAgent.ToString() ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    [HttpPost("send-verification-code")]
    [AllowAnonymous]
    public async Task<ActionResult<SendVerificationCodeResultDto>> SendVerificationCode([FromBody] SendVerificationCodeRequestDto dto)
    {
        try
        {
            var result = await _authService.SendVerificationCodeAsync(dto.Email);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send verification code to {Email}", dto?.Email);
            return BadRequest(new SendVerificationCodeResultDto { Success = false, Message = ex.Message });
        }
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthTokenResultDto>> Register([FromBody] RegisterUserDto dto)
    {
        try
        {
            var result = await _authService.RegisterAsync(dto);
            var ip = GetClientIp();
            var userAgent = GetClientUserAgent();
            var now = DateTime.UtcNow;

            _logger.LogInformation(
                "LOGIN AUDIT [Register]: User {Email} (UID: {UserId}, Name: {DisplayName}) registered and logged in from IP {Ip} at {TimeUtc:u}. UA: {UserAgent}",
                result.Email, result.UserId, result.DisplayName, ip, now, userAgent);

            if (_loginAuditRepository != null)
            {
                await _loginAuditRepository.RecordLoginAsync(new LoginAudit
                {
                    UserId = result.UserId,
                    Email = result.Email,
                    DisplayName = result.DisplayName,
                    AuthMethod = "Register",
                    IpAddress = ip,
                    UserAgent = userAgent,
                    TimestampUtc = now
                });
            }

            if (_userRepository != null)
            {
                await _userRepository.UpdateLastLoginAsync(result.UserId, now, ip);
            }

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed for {Email}", dto?.Email);
            return StatusCode(500, new { message = "Registration failed." });
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthTokenResultDto>> Login([FromBody] LoginDto dto)
    {
        try
        {
            var result = await _authService.LoginAsync(dto);
            var ip = GetClientIp();
            var userAgent = GetClientUserAgent();
            var now = DateTime.UtcNow;

            _logger.LogInformation(
                "LOGIN AUDIT [Email/Password]: User {Email} (UID: {UserId}, Name: {DisplayName}) logged in from IP {Ip} at {TimeUtc:u}. UA: {UserAgent}",
                result.Email, result.UserId, result.DisplayName, ip, now, userAgent);

            if (_loginAuditRepository != null)
            {
                await _loginAuditRepository.RecordLoginAsync(new LoginAudit
                {
                    UserId = result.UserId,
                    Email = result.Email,
                    DisplayName = result.DisplayName,
                    AuthMethod = "EmailPassword",
                    IpAddress = ip,
                    UserAgent = userAgent,
                    TimestampUtc = now
                });
            }

            if (_userRepository != null)
            {
                await _userRepository.UpdateLastLoginAsync(result.UserId, now, ip);
            }

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for {Input}", dto?.UsernameOrEmail);
            return StatusCode(500, new { message = "Login failed." });
        }
    }
}
