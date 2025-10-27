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

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);

        return string.Join(Delimiter, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        if (string.IsNullOrWhiteSpace(hashedPassword))
            throw new ArgumentException(
                "Hashed password cannot be null or empty",
                nameof(hashedPassword)
            );

        try
        {
            var elements = hashedPassword.Split(Delimiter);
            if (elements.Length != 2)
                return false;

            var salt = Convert.FromBase64String(elements[0]);
            var hash = Convert.FromBase64String(elements[1]);

            var hashInput = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                Algorithm,
                KeySize
            );

            return CryptographicOperations.FixedTimeEquals(hash, hashInput);
        }
        catch
        {
            return false;
        }
    }
}
