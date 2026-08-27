namespace Comex.Application.Interfaces;

/// <summary>
/// A single security audit event captured by <see cref="ISecurityAuditService"/>.
/// </summary>
public sealed class SecurityAuditEvent
{
    /// <summary>Unique identifier of the event.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>UTC timestamp when the event occurred.</summary>
    public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Event category, e.g. "Authentication", "Waf", "Backup".</summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>Severity level, e.g. "Info", "Warning", "Critical".</summary>
    public string Severity { get; init; } = string.Empty;

    /// <summary>Human-readable description of the event.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Optional client IP address.</summary>
    public string? IpAddress { get; init; }

    /// <summary>Optional client user agent.</summary>
    public string? UserAgent { get; init; }

    /// <summary>Optional request path associated with the event.</summary>
    public string? Path { get; init; }
}
