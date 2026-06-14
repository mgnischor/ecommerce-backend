namespace ECommerce.Application.Interfaces;

/// <summary>
/// Interface for password hashing and verification
/// </summary>
public interface IPasswordService
{
    /// <summary>
    /// Hashes a plain text password
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <returns>Hashed password</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a password against a hash
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <param name="hashedPassword">Hashed password</param>
    /// <returns>True if password matches, false otherwise</returns>
    bool VerifyPassword(string password, string hashedPassword);

    /// <summary>
    /// Returns a stable dummy hash to be used for timing-attack mitigation
    /// when the requested user does not exist. The dummy hash has the same
    /// format/cost as a real hash so that VerifyPassword takes the same time.
    /// </summary>
    string GetDummyHash();
}
