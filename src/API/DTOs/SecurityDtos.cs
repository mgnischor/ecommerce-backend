namespace Comex.API.DTOs;

/// <summary>
/// A security audit event as returned by the security API.
/// </summary>
public sealed class SecurityAuditEventDto
{
    public Guid Id { get; init; }
    public DateTime TimestampUtc { get; init; }
    public string Category { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public string? Path { get; init; }
}
