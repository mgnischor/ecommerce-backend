using System.Security.Cryptography;
using ECommerce.Application.Interfaces;

namespace ECommerce.Application.Services;

/// <summary>
/// Service for password hashing and verification using PBKDF2
/// </summary>
public sealed class PasswordService : IPasswordService
{
    private const int SaltSize = 16; // 128 bits
    private const int KeySize = 32; // 256 bits
    private const int Iterations = 100000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    private const char Delimiter = ';';

    private readonly ILogger<PasswordService> _logger;

    public PasswordService(ILogger<PasswordService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("Attempt to hash null or empty password");
            throw new ArgumentException("Password cannot be null or empty", nameof(password));
        }

        try
        {
            _logger.LogDebug("Hashing password");

            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);

            var hashedPassword = string.Join(Delimiter, Convert.ToBase64String(salt), Convert.ToBase64String(hash));

            _logger.LogDebug("Password hashed successfully");

            return hashedPassword;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to hash password");
            throw;
        }
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("Attempt to verify null or empty password");
            throw new ArgumentException("Password cannot be null or empty", nameof(password));
        }

        if (string.IsNullOrWhiteSpace(hashedPassword))
        {
            _logger.LogWarning("Attempt to verify against null or empty hashed password");
            throw new ArgumentException(
                "Hashed password cannot be null or empty",
                nameof(hashedPassword)
            );
        }

        try
        {
            _logger.LogDebug("Verifying password");

            var elements = hashedPassword.Split(Delimiter);
            if (elements.Length != 2)
            {
                _logger.LogWarning("Invalid hashed password format");
                return false;
            }

            var salt = Convert.FromBase64String(elements[0]);
            var hash = Convert.FromBase64String(elements[1]);

            var hashInput = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                Algorithm,
                KeySize
            );

            var isValid = CryptographicOperations.FixedTimeEquals(hash, hashInput);

            _logger.LogDebug("Password verification completed. Result: {IsValid}", isValid);

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during password verification");
            return false;
        }
    }
}
