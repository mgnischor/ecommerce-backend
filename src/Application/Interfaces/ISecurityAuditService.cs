namespace ECommerce.Application.Interfaces;

/// <summary>
/// Records and queries security-relevant events (login failures, account lockouts,
/// WAF blocks, admin actions) for monitoring and auditing.
/// </summary>
/// <remarks>
/// Events are written to the application logger and retained in a bounded in-memory
/// buffer so they can be queried through the security API. The buffer is not a
/// substitute for a persistent audit store.
/// </remarks>
public interface ISecurityAuditService
{
    /// <summary>
    /// Records a single security audit event.
    /// </summary>
    /// <param name="category">Event category, e.g. "Authentication", "Waf", "Backup".</param>
    /// <param name="severity">Severity level, e.g. "Info", "Warning", "Critical".</param>
    /// <param name="message">Human-readable description of the event.</param>
    /// <param name="ipAddress">Optional client IP address.</param>
    /// <param name="userAgent">Optional client user agent.</param>
    /// <param name="path">Optional request path associated with the event.</param>
    void Record(
        string category,
        string severity,
        string message,
        string? ipAddress = null,
        string? userAgent = null,
        string? path = null
    );

    /// <summary>
    /// Returns the most recent security audit events.
    /// </summary>
    /// <param name="count">Maximum number of events to return.</param>
    /// <returns>A read-only list of events, newest first.</returns>
    IReadOnlyList<SecurityAuditEvent> GetRecent(int count);
}
