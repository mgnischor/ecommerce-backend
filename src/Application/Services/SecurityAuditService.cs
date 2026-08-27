using System.Collections.Concurrent;
using Comex.Application.Interfaces;

namespace Comex.Application.Services;

/// <summary>
/// In-memory security audit service that logs events and retains them in a bounded buffer.
/// </summary>
/// <remarks>
/// <para>
/// Each event is forwarded to <see cref="ILoggingService"/> using a severity-appropriate log
/// level so it lands in the application's structured log pipeline. Events are also stored in a
/// thread-safe bounded queue (maximum <see cref="MaxEntries"/>) for querying via the security API.
/// </para>
/// </remarks>
public sealed class SecurityAuditService : ISecurityAuditService
{
    /// <summary>
    /// Maximum number of events retained in the in-memory buffer.
    /// </summary>
    private const int MaxEntries = 1000;

    private readonly ConcurrentQueue<SecurityAuditEvent> _events = new();
    private readonly ILoggingService _logger;

    public SecurityAuditService(ILoggingService logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public void Record(
        string category,
        string severity,
        string message,
        string? ipAddress = null,
        string? userAgent = null,
        string? path = null
    )
    {
        var entry = new SecurityAuditEvent
        {
            Category = category,
            Severity = severity,
            Message = message,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Path = path,
        };

        Log(entry);

        _events.Enqueue(entry);
        while (_events.Count > MaxEntries)
        {
            _events.TryDequeue(out _);
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<SecurityAuditEvent> GetRecent(int count)
    {
        if (count <= 0)
        {
            return Array.Empty<SecurityAuditEvent>();
        }

        return _events.Reverse().Take(count).ToList();
    }

    private void Log(SecurityAuditEvent entry)
    {
        var message =
            $"[AUDIT {entry.Category}/{entry.Severity}] {entry.Message} (IP: {entry.IpAddress ?? "unknown"}, UA: {entry.UserAgent ?? "unknown"}, Path: {entry.Path ?? "-"})";

        switch (entry.Severity.ToLowerInvariant())
        {
            case "critical":
                _logger.LogCritical(new InvalidOperationException(message), message);
                break;
            case "warning":
                _logger.LogWarning(message);
                break;
            case "error":
                _logger.LogError(message);
                break;
            default:
                _logger.LogInformation(message);
                break;
        }
    }
}
