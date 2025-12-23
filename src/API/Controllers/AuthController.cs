using ECommerce.API.DTOs;
using ECommerce.API.Filters;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Interfaces;

namespace ECommerce.API.Controllers;

/// <summary>
/// Authentication endpoints for user login and token management
/// </summary>
/// <remarks>
/// <para>
/// Provides JWT-based authentication services for secure API access. All authentication endpoints are public
/// and do not require prior authorization, allowing users to obtain access tokens.
/// </para>
/// <para>
/// <strong>Authentication Flow:</strong>
/// </para>
/// <list type="number">
/// <item><description>User submits credentials (email and password) to the login endpoint</description></item>
/// <item><description>System validates credentials against stored user data</description></item>
/// <item><description>System verifies user account status (active, not banned, not deleted)</description></item>
/// <item><description>JWT token is generated with user claims and access level</description></item>
/// <item><description>Token is returned to client with expiration information</description></item>
/// <item><description>Client includes token in Authorization header for subsequent API requests</description></item>
/// </list>
/// <para>
/// <strong>Security Features:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><strong>Password Hashing:</strong> Passwords are securely hashed and never stored in plain text</description></item>
/// <item><description><strong>JWT Tokens:</strong> Stateless authentication using industry-standard JSON Web Tokens</description></item>
/// <item><description><strong>Account Status Validation:</strong> Checks for active, banned, and deleted account states</description></item>
/// <item><description><strong>Token Expiration:</strong> Tokens have configurable expiration times for security</description></item>
/// <item><description><strong>Audit Logging:</strong> All authentication attempts are logged for security monitoring</description></item>
/// </list>
/// <para>
/// <strong>Token Usage:</strong> Include the JWT token in the Authorization header using the Bearer scheme:
/// <c>Authorization: Bearer {token}</c>
/// </para>
/// </remarks>
[ApiController]
[Route("api/v1")]
[Produces("application/json")]
[Tags("Authentication")]
public sealed class AuthController : ControllerBase
{
    /// <summary>
    /// Repository for user data access and retrieval operations
    /// </summary>
    /// <remarks>
    /// Provides methods to query user information from the data store, including retrieval by email for authentication.
    /// </remarks>
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Service for JWT token generation and management
    /// </summary>
    /// <remarks>
    /// Handles the creation of JSON Web Tokens containing user claims and access levels,
    /// as well as token expiration configuration.
    /// </remarks>
    private readonly IJwtService _jwtService;

    /// <summary>
    /// Service for password hashing and verification
    /// </summary>
    /// <remarks>
    /// Provides secure password verification by comparing plaintext passwords against stored hashes
    /// using cryptographically secure algorithms.
    /// </remarks>
    private readonly IPasswordService _passwordService;

    /// <summary>
    /// Logger instance for tracking authentication operations and security events
    /// </summary>
    /// <remarks>
    /// Used to log authentication attempts, successes, failures, and security-related events
    /// for monitoring and audit purposes.
    /// </remarks>
    private readonly ILoggingService _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class
    /// </summary>
    /// <param name="userRepository">
    /// Repository for user data access. Used to retrieve user information during authentication.
    /// Cannot be null.
    /// </param>
    /// <param name="jwtService">
    /// Service for JWT token generation and management. Handles token creation with user claims.
    /// Cannot be null.
    /// </param>
    /// <param name="passwordService">
    /// Service for password verification. Validates plaintext passwords against stored hashes.
    /// Cannot be null.
    /// </param>
    /// <param name="logger">
    /// Logger instance for recording authentication events and errors. Used for security monitoring and debugging.
    /// Cannot be null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any of the required dependencies (userRepository, jwtService, passwordService, or logger) are null.
    /// </exception>
    /// <remarks>
    /// This constructor uses dependency injection to provide all required services.
    /// All parameters are validated for null values to ensure the controller operates correctly and securely.
    /// The constructor is called by the ASP.NET Core dependency injection container during request processing.
    /// </remarks>
    public AuthController(
        IUserRepository userRepository,
        IJwtService jwtService,
        IPasswordService passwordService,
        ILoggingService logger
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
    /// <para>
    /// Validates user credentials and generates a JWT bearer token for API access.
    /// This is the primary authentication endpoint for the e-commerce platform.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// POST /api/v1/login
    /// Content-Type: application/json
    ///
    /// {
    ///    "email": "admin@ecommerce.com.br",
    ///    "password": "admin"
    /// }
    /// </code>
    /// <para>
    /// <strong>Authentication Process:</strong>
    /// </para>
    /// <list type="number">
    /// <item><description><strong>Credential Validation:</strong> Verifies that email and password are provided</description></item>
    /// <item><description><strong>User Lookup:</strong> Searches for user account by email address</description></item>
    /// <item><description><strong>Password Verification:</strong> Compares provided password against stored hash</description></item>
    /// <item><description><strong>Account Status Check:</strong> Ensures account is active, not banned, and not deleted</description></item>
    /// <item><description><strong>Token Generation:</strong> Creates JWT token with user claims and access level</description></item>
    /// <item><description><strong>Response:</strong> Returns token with expiration details and user information</description></item>
    /// </list>
    /// <para>
    /// <strong>Token Expiration:</strong> The token expires after the configured expiration time (default: 60 minutes).
    /// Clients should handle token refresh or re-authentication when tokens expire.
    /// </para>
    /// <para>
    /// <strong>Using the Token:</strong> Include the token in subsequent requests using the Authorization header:
    /// </para>
    /// <code>
    /// Authorization: Bearer {token}
    /// </code>
    /// <para>
    /// <strong>Security Considerations:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Always use HTTPS in production to protect credentials in transit</description></item>
    /// <item><description>Failed login attempts are logged for security monitoring</description></item>
    /// <item><description>Passwords are never stored in plain text or returned in responses</description></item>
    /// <item><description>Consider implementing rate limiting to prevent brute force attacks</description></item>
    /// <item><description>Tokens should be stored securely on the client side (not in localStorage for sensitive apps)</description></item>
    /// </list>
    /// <para>
    /// <strong>Account Status Requirements:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>IsActive:</strong> Must be true - account must be activated</description></item>
    /// <item><description><strong>IsBanned:</strong> Must be false - account must not be banned</description></item>
    /// <item><description><strong>IsDeleted:</strong> Must be false - account must not be soft-deleted</description></item>
    /// </list>
    /// </remarks>
    /// <param name="loginRequest">
    /// Login credentials containing the user's email address and password.
    /// Must be a valid <see cref="LoginRequestDto"/> object with non-empty email and password fields.
    /// The request body must be valid JSON matching the DTO schema.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to cancel the asynchronous operation.
    /// Allows the client to cancel the authentication request if it's no longer needed.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing a <see cref="LoginResponseDto"/> object on success.
    /// The response includes the JWT token, token type (Bearer), expiration time in seconds,
    /// user ID, email, and access level for the authenticated user.
    /// </returns>
    /// <response code="200">
    /// Login successful. Returns a JSON object with the JWT token and user details.
    /// The token should be included in the Authorization header for subsequent API requests.
    /// </response>
    /// <response code="400">
    /// Bad request. The request body is invalid, missing required fields, or fails model validation.
    /// Common causes include missing email/password, invalid JSON format, or validation errors.
    /// </response>
    /// <response code="401">
    /// Unauthorized. Authentication failed for one of the following reasons:
    /// <list type="bullet">
    /// <item><description>User not found with the provided email address</description></item>
    /// <item><description>Password does not match the stored hash</description></item>
    /// <item><description>User account is not active (inactive, banned, or deleted)</description></item>
    /// </list>
    /// For security reasons, the exact failure reason is not disclosed to prevent user enumeration attacks.
    /// </response>
    /// <response code="500">
    /// Internal server error. An unexpected error occurred during authentication.
    /// Check server logs for detailed error information.
    /// </response>
    [HttpPost("login")]
    [TypeFilter(typeof(RateLimitingFilter), Arguments = new object[] { 5, 300 })]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LoginResponseDto>> Login(
        [FromBody] LoginRequestDto loginRequest,
        CancellationToken cancellationToken = default
    )
    {
        // Add security headers
        Response.Headers.Append("X-Content-Type-Options", "nosniff");
        Response.Headers.Append("X-Frame-Options", "DENY");
        Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        Response.Headers.Append("X-XSS-Protection", "1; mode=block");

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

        if (loginRequest == null)
        {
            _logger.LogWarning("Login request is null from IP: {IpAddress}", ipAddress);
            return BadRequest(new { Message = "Login request is required" });
        }

        // Hash email for secure logging (LGPD/GDPR compliance)
        var emailHash = ComputeSha256Hash(loginRequest.Email ?? "unknown");

        _logger.LogInformation(
            "Login attempt - EmailHash: {EmailHash}, IP: {IpAddress}",
            emailHash,
            ipAddress
        );

        if (!ModelState.IsValid)
        {
            _logger.LogWarning(
                "Invalid model state - EmailHash: {EmailHash}, IP: {IpAddress}",
                emailHash,
                ipAddress
            );
            return BadRequest(ModelState);
        }

        // Find user by email
        var user = await _userRepository.GetByEmailAsync(loginRequest.Email, cancellationToken);

        // Check account lockout (before password verification)
        if (user != null && user.LockedUntil.HasValue && user.LockedUntil.Value > DateTime.UtcNow)
        {
            var lockTimeRemaining = (user.LockedUntil.Value - DateTime.UtcNow).TotalMinutes;
            _logger.LogWarning(
                "Login blocked - Account locked. UserId: {UserId}, IP: {IpAddress}, TimeRemaining: {Minutes}min",
                user.Id,
                ipAddress,
                Math.Ceiling(lockTimeRemaining)
            );
            return Unauthorized(
                new
                {
                    Message = $"Account temporarily locked. Try again in {Math.Ceiling(lockTimeRemaining)} minutes.",
                }
            );
        }

        // Use dummy hash for timing attack protection when user doesn't exist
        var passwordHash =
            user?.PasswordHash ?? "$2a$11$dummyhashfortimingattackprotection1234567890123456789012";
        var isPasswordValid = _passwordService.VerifyPassword(loginRequest.Password, passwordHash);

        // Authentication failed
        if (user == null || !isPasswordValid)
        {
            // Record failed attempt if user exists
            if (user != null)
            {
                user.FailedLoginAttempts++;
                user.LastFailedLoginAt = DateTime.UtcNow;
                user.LastLoginIpAddress = ipAddress;

                // Lock account after 5 failed attempts
                if (user.FailedLoginAttempts >= 5)
                {
                    user.LockedUntil = DateTime.UtcNow.AddMinutes(15);
                    _logger.LogWarning(
                        "Account locked - Too many failed attempts. UserId: {UserId}, IP: {IpAddress}, Attempts: {Attempts}",
                        user.Id,
                        ipAddress,
                        user.FailedLoginAttempts
                    );
                }

                _userRepository.Update(user);
                await _userRepository.SaveChangesAsync(cancellationToken);
            }

            // Add random delay to prevent timing attacks
            await Task.Delay(Random.Shared.Next(100, 300), cancellationToken);

            _logger.LogWarning(
                "Login failed - Invalid credentials. EmailHash: {EmailHash}, IP: {IpAddress}, UserAgent: {UserAgent}",
                emailHash,
                ipAddress,
                userAgent
            );

            return Unauthorized(new { Message = "Invalid email or password" });
        }

        // Check if user is active
        if (!user.IsActive || user.IsBanned || user.IsDeleted)
        {
            _logger.LogWarning(
                "Login failed - Account not active. UserId: {UserId}, IP: {IpAddress}, IsActive: {IsActive}, IsBanned: {IsBanned}, IsDeleted: {IsDeleted}",
                user.Id,
                ipAddress,
                user.IsActive,
                user.IsBanned,
                user.IsDeleted
            );
            return Unauthorized(new { Message = "User account is not active" });
        }

        // Successful login - Reset failed attempts and update login info
        user.FailedLoginAttempts = 0;
        user.LastSuccessfulLoginAt = DateTime.UtcNow;
        user.LastLoginIpAddress = ipAddress;
        user.LockedUntil = null;

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        // Generate JWT token
        var token = _jwtService.GenerateToken(user);

        _logger.LogInformation(
            "Login successful - UserId: {UserId}, IP: {IpAddress}",
            user.Id,
            ipAddress
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
    /// <para>
    /// Returns a list of authentication-related API endpoints with their HTTP methods and paths.
    /// This is a utility endpoint for API discovery and documentation purposes.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// GET /api/v1/endpoints
    /// </code>
    /// <para>
    /// <strong>Sample response:</strong>
    /// </para>
    /// <code>
    /// [
    ///   {
    ///     "method": "POST",
    ///     "path": "/api/v1/login"
    ///   },
    ///   {
    ///     "method": "GET",
    ///     "path": "/api/v1/endpoints"
    ///   }
    /// ]
    /// </code>
    /// <para>
    /// <strong>Use cases:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>API discovery for client applications</description></item>
    /// <item><description>Dynamic documentation generation</description></item>
    /// <item><description>Testing and development tools</description></item>
    /// <item><description>Client SDK generation</description></item>
    /// </list>
    /// <para>
    /// <strong>Note:</strong> This endpoint is public and does not require authentication.
    /// It provides metadata about available authentication operations.
    /// </para>
    /// </remarks>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a JSON array of endpoint objects.
    /// Each object contains the HTTP method and path for an available authentication endpoint.
    /// </returns>
    /// <response code="200">
    /// Successfully retrieved the list of available authentication endpoints.
    /// Returns a JSON array with method and path information for each endpoint.
    /// </response>
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
    /// <para>
    /// Handles OPTIONS requests to discover supported HTTP methods for the authentication resource.
    /// This endpoint is part of the HTTP protocol specification and is commonly used for CORS preflight requests.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// OPTIONS /api/v1
    /// </code>
    /// <para>
    /// <strong>Response:</strong> The allowed methods are returned in the <c>Allow</c> response header:
    /// <c>Allow: POST, GET, OPTIONS</c>
    /// </para>
    /// <para>
    /// <strong>Supported Methods:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>POST:</strong> For authentication (login)</description></item>
    /// <item><description><strong>GET:</strong> For endpoint discovery</description></item>
    /// <item><description><strong>OPTIONS:</strong> For capability discovery (this method)</description></item>
    /// </list>
    /// <para>
    /// <strong>CORS:</strong> This endpoint is essential for Cross-Origin Resource Sharing (CORS) preflight requests.
    /// Browsers automatically send OPTIONS requests before actual requests when performing cross-origin API calls.
    /// </para>
    /// <para>
    /// <strong>HTTP Specification:</strong> OPTIONS is defined in RFC 7231 Section 4.3.7 as a method to
    /// describe the communication options for the target resource.
    /// </para>
    /// </remarks>
    /// <returns>
    /// An <see cref="IActionResult"/> with HTTP status code 204 (No Content).
    /// The response body is empty, but the <c>Allow</c> header contains the list of supported HTTP methods.
    /// </returns>
    /// <response code="204">
    /// No content. Successfully processed the OPTIONS request.
    /// Check the <c>Allow</c> response header for the list of supported HTTP methods (POST, GET, OPTIONS).
    /// The response body is intentionally empty as per HTTP specification for OPTIONS requests.
    /// </response>
    [HttpOptions]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult GetOptions()
    {
        Response.Headers.Append("Allow", "POST, GET, OPTIONS");
        return NoContent();
    }

    #region Private Methods

    /// <summary>
    /// Computes SHA256 hash of a string for secure logging
    /// </summary>
    /// <param name="input">Input string to hash</param>
    /// <returns>Hexadecimal representation of the hash</returns>
    /// <remarks>
    /// Used to hash emails and other PII before logging to comply with LGPD/GDPR.
    /// Allows correlation of events without exposing sensitive data in logs.
    /// </remarks>
    private static string ComputeSha256Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    #endregion
}
