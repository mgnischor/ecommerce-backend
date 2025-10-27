namespace ECommerce.Domain.ValueObjects;

/// <summary>
/// Value object representing an email address
/// </summary>
public sealed class EmailAddress : IEquatable<EmailAddress>
{
    public string Value { get; }

    private EmailAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email address cannot be empty", nameof(value));

        if (!IsValidEmail(value))
            throw new ArgumentException($"Invalid email address: {value}", nameof(value));

        Value = value.ToLowerInvariant().Trim();
    }

    /// <summary>
    /// Creates a new EmailAddress instance
    /// </summary>
    public static EmailAddress Create(string value)
    {
        return new EmailAddress(value);
    }

    /// <summary>
    /// Validates email format
    /// </summary>
    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email.Trim();
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the domain part of the email
    /// </summary>
    public string GetDomain()
    {
        var parts = Value.Split('@');
        return parts.Length == 2 ? parts[1] : string.Empty;
    }

    /// <summary>
    /// Gets the local part of the email (before @)
    /// </summary>
    public string GetLocalPart()
    {
        var parts = Value.Split('@');
        return parts.Length > 0 ? parts[0] : string.Empty;
    }

    /// <summary>
    /// Masks the email for privacy (e.g., j***@example.com)
    /// </summary>
    public string Mask()
    {
        var parts = Value.Split('@');
        if (parts.Length != 2)
            return Value;

        var localPart = parts[0];
        var domain = parts[1];

        if (localPart.Length <= 1)
            return $"{localPart}***@{domain}";

        return $"{localPart[0]}***@{domain}";
    }

    public bool Equals(EmailAddress? other)
    {
        if (other is null)
            return false;
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is EmailAddress other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }

    public static bool operator ==(EmailAddress? left, EmailAddress? right)
    {
        if (left is null)
            return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(EmailAddress? left, EmailAddress? right)
    {
        return !(left == right);
    }

    public static implicit operator string(EmailAddress email)
    {
        return email.Value;
    }
}
