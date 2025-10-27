using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces;

/// <summary>
/// Interface for JWT token generation and validation
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generates a JWT token for a given user
    /// </summary>
    /// <param name="user">User entity</param>
    /// <returns>JWT token string</returns>
    string GenerateToken(UserEntity user);

    /// <summary>
    /// Gets the token expiration time in seconds
    /// </summary>
    /// <returns>Expiration time in seconds</returns>
    int GetTokenExpirationSeconds();
}
