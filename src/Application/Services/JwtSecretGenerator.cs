using System.Security.Cryptography;
using System.Text;

namespace ECommerce.Application.Services;

/// <summary>
/// Generates and validates cryptographically strong JWT signing secrets.
/// </summary>
/// <remarks>
/// <para>
/// A JWT signing secret should be a high-entropy, random value with at least 256 bits (32 bytes)
/// of entropy. This helper centralizes generation and validation so the enforcement is consistent
/// across startup validation, CLI tooling, and configuration.
/// </para>
/// <para>
/// <strong>Generation:</strong> Uses <see cref="RandomNumberGenerator.GetBytes(int)"/> which is a
/// cryptographically secure random source. The generated secret is a Base64-encoded string, so it
/// always contains a mix of character classes and is URL-safe enough for configuration files and
/// environment variables.
/// </para>
/// <para>
/// <strong>Validation:</strong> Enforces the following rules:
/// </para>
/// <list type="bullet">
/// <item><description>The secret must not be null, empty, or whitespace</description></item>
/// <item><description>The UTF-8 byte length must be at least 32 bytes (256 bits)</description></item>
/// <item><description>The secret must contain at least three character classes (upper, lower, digit, symbol) to reject predictable values</description></item>
/// </list>
/// </remarks>
public static class JwtSecretGenerator
{
    /// <summary>
    /// Minimum secret entropy in bytes (256 bits).
    /// </summary>
    public const int MinimumByteLength = 32;

    /// <summary>
    /// Default number of random bytes used when generating a new secret (384 bits).
    /// </summary>
    public const int DefaultByteCount = 48;

    /// <summary>
    /// Generates a cryptographically strong random JWT signing secret.
    /// </summary>
    /// <param name="byteCount">
    /// Number of random bytes to generate. Must be at least <see cref="MinimumByteLength"/>.
    /// Defaults to <see cref="DefaultByteCount"/> (48 bytes / 384 bits).
    /// </param>
    /// <returns>A Base64-encoded random secret suitable for use as a JWT signing key.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="byteCount"/> is less than <see cref="MinimumByteLength"/>.
    /// </exception>
    public static string GenerateSecret(int byteCount = DefaultByteCount)
    {
        if (byteCount < MinimumByteLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(byteCount),
                $"Secret byte count must be at least {MinimumByteLength}."
            );
        }

        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(byteCount));
    }

    /// <summary>
    /// Validates the strength of a configured JWT signing secret.
    /// </summary>
    /// <param name="secret">The secret to validate. May be <see langword="null"/>.</param>
    /// <param name="isDevelopment">
    /// When <see langword="true"/>, weak-but-configured secrets are reported via <see cref="JwtSecretValidationResult.Warning"/>
    /// instead of being rejected. Production always requires a strong secret.
    /// </param>
    /// <returns>A <see cref="JwtSecretValidationResult"/> describing the validation outcome.</returns>
    public static JwtSecretValidationResult ValidateSecret(string? secret, bool isDevelopment)
    {
        if (string.IsNullOrWhiteSpace(secret))
        {
            return new JwtSecretValidationResult(
                IsValid: false,
                Error: "JWT SecretKey is not configured.",
                Warning: null
            );
        }

        var byteLength = Encoding.UTF8.GetByteCount(secret);
        if (byteLength < MinimumByteLength)
        {
            return new JwtSecretValidationResult(
                IsValid: false,
                Error: $"JWT SecretKey must be at least {MinimumByteLength * 8} bits ({MinimumByteLength} bytes) long. Current length: {byteLength} bytes.",
                Warning: null
            );
        }

        var characterClasses = CountCharacterClasses(secret);
        if (characterClasses < 3)
        {
            var message =
                $"JWT SecretKey is too predictable: it must contain at least 3 character classes (upper case, lower case, digits, symbols). Detected: {characterClasses}.";
            if (isDevelopment)
            {
                return new JwtSecretValidationResult(IsValid: true, Error: null, Warning: message);
            }

            return new JwtSecretValidationResult(IsValid: false, Error: message, Warning: null);
        }

        return new JwtSecretValidationResult(IsValid: true, Error: null, Warning: null);
    }

    private static int CountCharacterClasses(string value)
    {
        var hasUpper = false;
        var hasLower = false;
        var hasDigit = false;
        var hasSymbol = false;

        foreach (var character in value)
        {
            if (char.IsUpper(character))
            {
                hasUpper = true;
            }
            else if (char.IsLower(character))
            {
                hasLower = true;
            }
            else if (char.IsDigit(character))
            {
                hasDigit = true;
            }
            else
            {
                hasSymbol = true;
            }
        }

        return (hasUpper ? 1 : 0) + (hasLower ? 1 : 0) + (hasDigit ? 1 : 0) + (hasSymbol ? 1 : 0);
    }
}

/// <summary>
/// Describes the outcome of JWT secret validation.
/// </summary>
public sealed record JwtSecretValidationResult(bool IsValid, string? Error, string? Warning);
