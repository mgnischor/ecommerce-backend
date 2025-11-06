namespace ECommerce.API.DTOs;

/// <summary>
/// Data Transfer Object (DTO) for successful authentication login responses
/// </summary>
/// <remarks>
/// <para>
/// This DTO contains the JWT token and user information returned after successful authentication.
/// The token should be included in subsequent API requests via the Authorization header.
/// </para>
/// <para>
/// <strong>Response Example:</strong>
/// </para>
/// <code>
/// {
///   "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
///   "expiresIn": 3600,
///   "tokenType": "Bearer",
///   "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
///   "email": "user@example.com",
///   "accessLevel": "Admin"
/// }
/// </code>
/// <para>
/// <strong>Usage:</strong> Include the token in the Authorization header as "Bearer {token}"
/// for all authenticated API requests.
/// </para>
/// </remarks>
public sealed class LoginResponseDto
{
    /// <summary>
    /// Gets or sets the JWT (JSON Web Token) access token for authentication
    /// </summary>
    /// <value>
    /// A Base64-encoded JWT token string containing encoded user claims and signature.
    /// This token must be included in the Authorization header of subsequent requests.
    /// </value>
    /// <remarks>
    /// The token is cryptographically signed and contains claims about the user's identity
    /// and permissions. It expires after the time specified in <see cref="ExpiresIn"/>.
    /// </remarks>
    /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c</example>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the token expiration time in seconds
    /// </summary>
    /// <value>
    /// The number of seconds from token issuance until the token expires.
    /// After expiration, the token becomes invalid and a new login is required.
    /// </value>
    /// <remarks>
    /// Clients should track token expiration and request a new token before expiry
    /// to maintain uninterrupted access. Typical values range from 3600 (1 hour) to 86400 (24 hours).
    /// </remarks>
    /// <example>3600</example>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Gets or sets the token type for HTTP Authorization header format
    /// </summary>
    /// <value>
    /// The authentication scheme to use with the token. Always returns "Bearer" for JWT tokens.
    /// </value>
    /// <remarks>
    /// This value should be prepended to the token when constructing the Authorization header:
    /// <c>Authorization: Bearer {token}</c>
    /// </remarks>
    /// <example>Bearer</example>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Gets or sets the authenticated user's unique identifier
    /// </summary>
    /// <value>
    /// A globally unique identifier (GUID) representing the user in the system.
    /// This ID can be used to retrieve user-specific data or perform user-related operations.
    /// </value>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the authenticated user's email address
    /// </summary>
    /// <value>
    /// The email address used for authentication. This is the user's primary contact
    /// and login identifier in the system.
    /// </value>
    /// <example>user@example.com</example>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the authenticated user's access level or role
    /// </summary>
    /// <value>
    /// The user's role/access level in the system (e.g., "Admin", "Manager", "Customer").
    /// This determines which API endpoints and operations the user can access.
    /// </value>
    /// <remarks>
    /// The access level is also encoded in the JWT token claims and used for authorization
    /// checks on protected endpoints.
    /// </remarks>
    /// <example>Admin</example>
    public string AccessLevel { get; set; } = string.Empty;
}
