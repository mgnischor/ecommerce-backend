using ECommerce.API.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

/// <summary>
/// Authentication endpoints for user login and token management
/// </summary>
/// <remarks>
/// Provides JWT-based authentication services. All authentication endpoints are public.
/// </remarks>
[ApiController]
[Route("api/v1")]
[Produces("application/json")]
[Tags("Authentication")]
public sealed class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IPasswordService _passwordService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUserRepository userRepository,
        IJwtService jwtService,
        IPasswordService passwordService,
        ILogger<AuthController> logger
    )
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        _passwordService =
            passwordService ?? throw new ArgumentNullException(nameof(passwordService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token
    /// </summary>
    /// <remarks>
    /// Validates user credentials and generates a JWT bearer token for API access.
    ///
    /// Sample request:
    ///
    ///     POST /api/v1/login
    ///     {
    ///        "email": "admin@ecommerce.com.br",
    ///        "password": "admin"
    ///     }
    ///
    /// The token expires after the configured expiration time (default: 60 minutes).
    /// Include the token in subsequent requests using the Authorization header:
    ///
    ///     Authorization: Bearer {token}
    ///
    /// </remarks>
    /// <param name="loginRequest">Login credentials containing email and password</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>JWT token with expiration details and user information</returns>
    /// <response code="200">Login successful. Returns JWT token and user details.</response>
    /// <response code="400">Invalid request. Missing or malformed credentials.</response>
    /// <response code="401">Authentication failed. Invalid email or password, or account is not active.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponseDto>> Login(
        [FromBody] LoginRequestDto loginRequest,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Login attempt for email: {Email}", loginRequest?.Email ?? "null");

        if (loginRequest == null)
        {
            _logger.LogWarning("Login request is null");
            return BadRequest(new { Message = "Login request is required" });
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for login request");
            return BadRequest(ModelState);
        }

        // Find user by email
        var user = await _userRepository.GetByEmailAsync(loginRequest.Email, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning(
                "Login failed: User not found for email: {Email}",
                loginRequest.Email
            );
            return Unauthorized(new { Message = "Invalid email or password" });
        }

        // Verify password
        if (!_passwordService.VerifyPassword(loginRequest.Password, user.PasswordHash))
        {
            _logger.LogWarning(
                "Login failed: Invalid password for email: {Email}",
                loginRequest.Email
            );
            return Unauthorized(new { Message = "Invalid email or password" });
        }

        // Check if user is active
        if (!user.IsActive || user.IsBanned || user.IsDeleted)
        {
            _logger.LogWarning(
                "Login failed: User account is not active for email: {Email}",
                loginRequest.Email
            );
            return Unauthorized(new { Message = "User account is not active" });
        }

        // Generate JWT token
        var token = _jwtService.GenerateToken(user);

        _logger.LogInformation(
            "Login successful for user: {Email}, UserId: {UserId}",
            user.Email,
            user.Id
        );

        var response = new LoginResponseDto
        {
            Token = token,
            ExpiresIn = _jwtService.GetTokenExpirationSeconds(),
            TokenType = "Bearer",
            UserId = user.Id,
            Email = user.Email,
            AccessLevel = user.AccessLevel.ToString(),
        };

        return Ok(response);
    }

    /// <summary>
    /// Lists all available authentication endpoints
    /// </summary>
    /// <remarks>
    /// Returns a list of authentication-related API endpoints with their HTTP methods.
    /// This is a utility endpoint for API discovery.
    /// </remarks>
    /// <returns>List of available endpoints</returns>
    /// <response code="200">Successfully retrieved endpoints list</response>
    [HttpGet("endpoints")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetEndpoints()
    {
        var endpoints = new[]
        {
            new { Method = "POST", Path = "/api/v1/login" },
            new { Method = "GET", Path = "/api/v1/endpoints" },
        };

        return Ok(endpoints);
    }

    /// <summary>
    /// Returns allowed HTTP methods for authentication endpoints
    /// </summary>
    /// <remarks>
    /// OPTIONS request to discover supported HTTP methods for this resource.
    /// Returns methods in the Allow header.
    /// </remarks>
    /// <response code="204">No content. Check the Allow header for supported methods.</response>
    [HttpOptions]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult GetOptions()
    {
        Response.Headers.Append("Allow", "POST, GET, OPTIONS");
        return NoContent();
    }
}
