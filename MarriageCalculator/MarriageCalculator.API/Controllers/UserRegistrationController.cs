using MarriageCalculator.API.Services;
using MarriageCalculator.API.Services.Interfaces;
using MarriageCalculator.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MarriageCalculator.API.Controllers;

/// <summary>
/// User Registration Controller - Manages user registration, email verification, and account setup
/// 
/// ENDPOINTS SUMMARY:
/// ==================
/// POST   /api/userregistration/register            - Register a new user account
/// POST   /api/userregistration/verify-email        - Verify user email address with verification code
/// POST   /api/userregistration/resend-verification - Resend email verification code
/// 
/// AUTHENTICATION:
/// - All endpoints are public (no authentication required)
/// - Email verification required before full account access
/// 
/// KEY FEATURES:
/// - Secure user registration with email verification
/// - 5-digit verification codes with 2-hour expiration
/// - Password validation (minimum 8 characters, one capital letter, one number/symbol)
/// - Automatic verification email sending
/// - Code regeneration and resending capability
/// - Comprehensive validation and error handling
/// - Swagger/OpenAPI documentation with detailed response types
/// - Proper HTTP status code usage (201 Created, 404 Not Found, etc.)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("User Registration")]
public class UserRegistrationController : ControllerBase
{
    private readonly IUserAuthService _userAuthService;
    private readonly ILogger<UserRegistrationController> _logger;

    /// <summary>
    /// Initializes a new instance of the UserRegistrationController
    /// </summary>
    /// <param name="userAuthService">User authentication service</param>
    /// <param name="logger">Logger for tracking operations</param>
    public UserRegistrationController(IUserAuthService userAuthService, ILogger<UserRegistrationController> logger)
    {
        _userAuthService = userAuthService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    /// <param name="registerDto">User registration details</param>
    /// <returns>User registration result with verification requirements</returns>
    /// <response code="201">User registered successfully. Email verification required.</response>
    /// <response code="400">Invalid registration data or user already exists</response>
    /// <response code="500">Internal server error during registration</response>
    [HttpPost("register")]
    [SwaggerOperation(
        Summary = "Register new user",
        Description = "Creates a new user account and sends email verification code. Password must be at least 8 characters with one capital letter and one number or symbol."
    )]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), 400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<UserDto>>> RegisterUser([FromBody] RegisterUserDto registerDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                
                return BadRequest(new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = $"Validation failed: {errors}"
                });
            }

            var result = await _userAuthService.RegisterUserAsync(registerDto);

            if (result.Success)
            {
                return CreatedAtAction(nameof(RegisterUser), new { email = result.Data?.Email }, result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration for email: {Email}", registerDto.Email);
            return StatusCode(500, new ApiResponse<UserDto>
            {
                Success = false,
                Message = "An internal error occurred during registration. Please try again."
            });
        }
    }

    /// <summary>
    /// Verify user email address with verification code
    /// </summary>
    /// <param name="verifyDto">Email verification details</param>
    /// <returns>Email verification result</returns>
    /// <response code="200">Email verified successfully</response>
    /// <response code="400">Invalid verification code or email</response>
    /// <response code="404">User not found</response>
    /// <response code="500">Internal server error during verification</response>
    [HttpPost("verify-email")]
    [SwaggerOperation(
        Summary = "Verify email address",
        Description = "Verifies user email address using the 5-digit verification code sent via email. Code expires after 2 hours."
    )]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse>> VerifyEmail([FromBody] VerifyEmailDto verifyDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"Validation failed: {errors}"
                });
            }

            var result = await _userAuthService.VerifyEmailAsync(verifyDto);

            if (result.Success)
            {
                return Ok(result);
            }

            // Determine appropriate status code based on message
            if (result.Message.Contains("not found"))
            {
                return NotFound(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during email verification for email: {Email}", verifyDto.Email);
            return StatusCode(500, new ApiResponse
            {
                Success = false,
                Message = "An internal error occurred during email verification. Please try again."
            });
        }
    }

    /// <summary>
    /// Resend email verification code
    /// </summary>
    /// <param name="resendDto">Resend verification request details</param>
    /// <returns>Resend verification code result</returns>
    /// <response code="200">Verification code sent successfully</response>
    /// <response code="400">Invalid email or email already verified</response>
    /// <response code="404">User not found</response>
    /// <response code="500">Internal server error during resend</response>
    [HttpPost("resend-verification")]
    [SwaggerOperation(
        Summary = "Resend verification code",
        Description = "Resends email verification code to the user's email address. Previous codes are invalidated."
    )]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse>> ResendVerificationCode([FromBody] ResendVerificationDto resendDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"Validation failed: {errors}"
                });
            }

            var result = await _userAuthService.ResendVerificationCodeAsync(resendDto);

            if (result.Success)
            {
                return Ok(result);
            }

            // Determine appropriate status code based on message
            if (result.Message.Contains("not found"))
            {
                return NotFound(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during resend verification for email: {Email}", resendDto.Email);
            return StatusCode(500, new ApiResponse
            {
                Success = false,
                Message = "An internal error occurred while resending verification code. Please try again."
            });
        }
    }
} 