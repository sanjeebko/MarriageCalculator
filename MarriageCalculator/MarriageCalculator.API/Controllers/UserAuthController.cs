using MarriageCalculator.API.Services.Interfaces;
using MarriageCalculator.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace MarriageCalculator.API.Controllers;

/// <summary>
/// User Authentication Controller - Manages user authentication, JWT tokens, and session management
/// 
/// ENDPOINTS SUMMARY:
/// ==================
/// POST   /api/userauth/login              - Authenticate user and return JWT token
/// POST   /api/userauth/logout             - Logout user and invalidate JWT token (requires auth)
/// GET    /api/userauth/me                 - Get current authenticated user information (requires auth)
/// POST   /api/userauth/validate-token     - Validate JWT token
/// POST   /api/userauth/refresh-token      - Refresh access token using refresh token
/// POST   /api/userauth/revoke-token       - Revoke a specific refresh token
/// POST   /api/userauth/revoke-all-tokens  - Revoke all refresh tokens for current user (requires auth)
/// 
/// AUTHENTICATION:
/// - Some endpoints require authentication ([Authorize]): logout, me, revoke-all-tokens
/// - JWT-based authentication with access and refresh token support
/// - Token blacklisting for secure logout
/// 
/// KEY FEATURES:
/// - Secure JWT-based authentication with refresh token rotation
/// - Email verification status handling during login
/// - Token validation and invalidation
/// - Multi-device logout support via token revocation
/// - Comprehensive error handling and logging
/// - Swagger/OpenAPI documentation with detailed response types
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("User Authentication")]
public class UserAuthController : ControllerBase
{
    private readonly IUserAuthService _userAuthService;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ILogger<UserAuthController> _logger;

    /// <summary>
    /// Initializes a new instance of the UserAuthController
    /// </summary>
    /// <param name="userAuthService">User authentication service</param>
    /// <param name="jwtService">JWT token service</param>
    /// <param name="refreshTokenService">Refresh token service</param>
    /// <param name="logger">Logger for tracking operations</param>
    public UserAuthController(
        IUserAuthService userAuthService, 
        IJwtService jwtService,
        IRefreshTokenService refreshTokenService,
        ILogger<UserAuthController> logger)
    {
        _userAuthService = userAuthService;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticate user and return JWT token
    /// </summary>
    /// <param name="loginDto">User login credentials</param>
    /// <returns>JWT token and user information if authentication successful</returns>
    /// <response code="200">Login successful, returns JWT token (may require email verification)</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Authentication failed</response>
    /// <response code="500">Internal server error during authentication</response>
    [HttpPost("login")]
    [SwaggerOperation(
        Summary = "User login",
        Description = "Authenticates user credentials and returns JWT token. If email is not verified, the user data will indicate verification is required."
    )]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), 400)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), 401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginUserDto loginDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                
                return BadRequest(new ApiResponse<LoginResponseDto>
                {
                    Success = false,
                    Message = $"Validation failed: {errors}"
                });
            }

            var result = await _userAuthService.LoginUserAsync(loginDto);

            if (result.Success)
            {
                // Always return 200 OK for successful login, regardless of email verification status
                return Ok(result);
            }

            // Return 401 only for invalid credentials
            if (result.Message.Contains("Invalid email or password"))
            {
                return Unauthorized(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user login for email: {Email}", loginDto.Email);
            return StatusCode(500, new ApiResponse<LoginResponseDto>
            {
                Success = false,
                Message = "An internal error occurred during login. Please try again."
            });
        }
    }

    /// <summary>
    /// Logout user and invalidate JWT token
    /// </summary>
    /// <returns>Logout result</returns>
    /// <response code="200">Logout successful</response>
    /// <response code="401">Invalid or missing token</response>
    /// <response code="500">Internal server error during logout</response>
    [HttpPost("logout")]
    [Authorize]
    [SwaggerOperation(
        Summary = "User logout",
        Description = "Logs out the authenticated user and blacklists the JWT token to prevent further use."
    )]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse>> Logout()
    {
        try
        {
            // Extract token from Authorization header
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new ApiResponse
                {
                    Success = false,
                    Message = "Authorization token is required."
                });
            }

            var token = authHeader["Bearer ".Length..].Trim();

            var result = await _userAuthService.LogoutUserAsync(token);

            if (result.Success)
            {
                return Ok(result);
            }

            return Unauthorized(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user logout");
            return StatusCode(500, new ApiResponse
            {
                Success = false,
                Message = "An internal error occurred during logout. Please try again."
            });
        }
    }

    /// <summary>
    /// Get current authenticated user information
    /// </summary>
    /// <returns>Current user information</returns>
    /// <response code="200">User information retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("me")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Get current user",
        Description = "Returns information about the currently authenticated user based on JWT token."
    )]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(500)]
    public ActionResult<ApiResponse<UserDto>> GetCurrentUser()
    {
        try
        {
            // Extract user information from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var displayName = User.FindFirst(ClaimTypes.Name)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var emailVerified = User.FindFirst("email_verified")?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Unauthorized(new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "Invalid token claims."
                });
            }

            var userDto = new UserDto
            {
                Id = userId,
                DisplayName = displayName ?? "",
                Email = email ?? "",
                IsEmailVerified = bool.TryParse(emailVerified, out bool verified) && verified,
                IsActive = true  
            };

            return Ok(new ApiResponse<UserDto>
            {
                Success = true,
                Message = "User information retrieved successfully.",
                Data = userDto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current user information");
            return StatusCode(500, new ApiResponse<UserDto>
            {
                Success = false,
                Message = "An internal error occurred while retrieving user information."
            });
        }
    }

    /// <summary>
    /// Validate JWT token
    /// </summary>
    /// <param name="token">JWT token to validate</param>
    /// <returns>Token validation result</returns>
    /// <response code="200">Token is valid</response>
    /// <response code="400">Token is invalid or expired</response>
    /// <response code="500">Internal server error during validation</response>
    [HttpPost("validate-token")]
    [SwaggerOperation(
        Summary = "Validate JWT token",
        Description = "Validates a JWT token and returns the user ID if valid."
    )]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(500)]
    public ActionResult<ApiResponse<object>> ValidateToken([FromBody] string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Token is required."
                });
            }

            var userId = _jwtService.ValidateToken(token);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid or expired token."
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Token is valid.",
                Data = new { UserId = userId }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token validation");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An internal error occurred during token validation."
            });
        }
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <returns>New access token and refresh token</returns>
    /// <response code="200">Tokens refreshed successfully</response>
    /// <response code="400">Invalid or expired refresh token</response>
    /// <response code="500">Internal server error during refresh</response>
    [HttpPost("refresh-token")]
    [SwaggerOperation(
        Summary = "Refresh access token",
        Description = "Uses a refresh token to generate a new access token and refresh token pair. The old refresh token is automatically revoked."
    )]
    [ProducesResponseType(typeof(ApiResponse<RefreshTokenResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<RefreshTokenResponseDto>>> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                
                return BadRequest(new ApiResponse<RefreshTokenResponseDto>
                {
                    Success = false,
                    Message = $"Validation failed: {errors}"
                });
            }

            var result = await _refreshTokenService.RefreshTokenAsync(request.RefreshToken);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, new ApiResponse<RefreshTokenResponseDto>
            {
                Success = false,
                Message = "An internal error occurred during token refresh."
            });
        }
    }

    /// <summary>
    /// Revoke a refresh token
    /// </summary>
    /// <param name="request">Revoke token request</param>
    /// <returns>Revocation result</returns>
    /// <response code="200">Token revoked successfully</response>
    /// <response code="400">Invalid token</response>
    /// <response code="500">Internal server error during revocation</response>
    [HttpPost("revoke-token")]
    [SwaggerOperation(
        Summary = "Revoke refresh token", 
        Description = "Revokes a specific refresh token, making it invalid for future use."
    )]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse>> RevokeToken([FromBody] RevokeTokenRequestDto request)
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

            var result = await _refreshTokenService.RevokeTokenAsync(request.RefreshToken);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token revocation");
            return StatusCode(500, new ApiResponse
            {
                Success = false,
                Message = "An internal error occurred during token revocation."
            });
        }
    }

    /// <summary>
    /// Revoke all refresh tokens for the current user
    /// </summary>
    /// <returns>Revocation result</returns>
    /// <response code="200">All tokens revoked successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="500">Internal server error during revocation</response>
    [HttpPost("revoke-all-tokens")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Revoke all refresh tokens",
        Description = "Revokes all refresh tokens for the currently authenticated user. Useful for logging out from all devices."
    )]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse>> RevokeAllTokens()
    {
        try
        {
            // Extract user ID from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Unauthorized(new ApiResponse
                {
                    Success = false,
                    Message = "Invalid token claims."
                });
            }

            var result = await _refreshTokenService.RevokeAllUserTokensAsync(userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during all tokens revocation");
            return StatusCode(500, new ApiResponse
            {
                Success = false,
                Message = "An internal error occurred during token revocation."
            });
        }
    }
}