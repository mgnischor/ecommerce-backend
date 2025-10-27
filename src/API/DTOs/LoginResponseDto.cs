namespace ECommerce.API.DTOs;

/// <summary>
/// Represents the response returned after a successful login
/// </summary>
public sealed class LoginResponseDto
{
    /// <summary>
    /// JWT access token
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration time in seconds
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Token type (usually "Bearer")
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// User's unique identifier
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's access level
    /// </summary>
    public string AccessLevel { get; set; } = string.Empty;
}
