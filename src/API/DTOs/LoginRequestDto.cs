using System.ComponentModel.DataAnnotations;

namespace ECommerce.API.DTOs;

/// <summary>
/// Data Transfer Object (DTO) for user authentication login requests
/// </summary>
/// <remarks>
/// <para>
/// This DTO is used to capture and validate user credentials for authentication.
/// It includes DataAnnotations validation attributes to ensure data integrity before processing.
/// </para>
/// <para>
/// <strong>Validation Rules:</strong>
/// </para>
/// <list type="bullet">
/// <item><description>Email: Required, must be valid email format</description></item>
/// <item><description>Password: Required, minimum 4 characters</description></item>
/// </list>
/// <para>
/// <strong>Usage Example:</strong>
/// </para>
/// <code>
/// POST /api/v1/auth/login
/// {
///   "email": "user@example.com",
///   "password": "SecurePassword123"
/// }
/// </code>
/// </remarks>
public sealed class LoginRequestDto
{
    /// <summary>
    /// Gets or sets the user's email address used for authentication
    /// </summary>
    /// <value>
    /// A valid email address in standard format (e.g., user@example.com).
    /// This field is required and must match email format validation rules.
    /// </value>
    /// <example>user@example.com</example>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's password for authentication
    /// </summary>
    /// <value>
    /// A string containing the user's password. Must be at least 4 characters long.
    /// This field is required for authentication.
    /// </value>
    /// <remarks>
    /// <strong>Security Note:</strong> Passwords are transmitted in the request body and should
    /// only be sent over HTTPS to ensure encryption in transit. Passwords are never logged or
    /// stored in plain text on the server.
    /// </remarks>
    /// <example>SecurePassword123</example>
    [Required(ErrorMessage = "Password is required")]
    [MinLength(4, ErrorMessage = "Password must be at least 4 characters")]
    public string Password { get; set; } = string.Empty;
}
