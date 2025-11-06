using System.Collections.Concurrent;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ECommerce.API.Filters;

/// <summary>
/// Action filter that implements rate limiting to prevent API abuse
/// </summary>
/// <remarks>
/// <para>
/// Protects API endpoints from excessive requests by limiting the number of requests
/// a client can make within a time window. Uses IP address for identification in anonymous
/// requests and user ID for authenticated requests.
/// </para>
/// <para>
/// <strong>Features:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><strong>Request Throttling:</strong> Limits requests per time window</description></item>
/// <item><description><strong>Per-User/IP:</strong> Tracks limits separately for each client</description></item>
/// <item><description><strong>Configurable Limits:</strong> Set max requests and window duration</description></item>
/// <item><description><strong>Automatic Cleanup:</strong> Removes expired entries to prevent memory leaks</description></item>
/// <item><description><strong>Standard Headers:</strong> Returns X-RateLimit headers for client awareness</description></item>
/// </list>
/// <para>
/// <strong>Usage:</strong>
/// </para>
/// <code>
/// // Apply globally in Program.cs:
/// builder.Services.AddScoped&lt;RateLimitingFilter&gt;();
/// builder.Services.AddControllers(options =>
/// {
///     options.Filters.AddService&lt;RateLimitingFilter&gt;();
/// });
///
/// // Or apply per controller/action:
/// [ServiceFilter(typeof(RateLimitingFilter))]
/// [HttpPost("login")]
/// public async Task&lt;IActionResult&gt; Login(...) { }
/// </code>
/// <para>
/// <strong>Default Limits:</strong> 100 requests per 60 seconds. Customize via constructor parameters.
/// </para>
/// <para>
/// <strong>Response Headers:</strong>
/// </para>
/// <list type="bullet">
/// <item><description>X-RateLimit-Limit: Maximum requests allowed</description></item>
/// <item><description>X-RateLimit-Remaining: Requests remaining in window</description></item>
/// <item><description>X-RateLimit-Reset: Time when the limit resets (Unix timestamp)</description></item>
/// </list>
/// </remarks>
public sealed class RateLimitingFilter : IActionFilter
{
    /// <summary>
    /// Storage for request counts per client
    /// </summary>
    private static readonly ConcurrentDictionary<string, ClientRequestInfo> RequestCounts = new();

    /// <summary>
    /// Logger instance for tracking rate limit violations
    /// </summary>
    private readonly ILogger<RateLimitingFilter> _logger;

    /// <summary>
    /// Maximum number of requests allowed per time window
    /// </summary>
    private readonly int _maxRequests;

    /// <summary>
    /// Time window duration in seconds
    /// </summary>
    private readonly int _windowSeconds;

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitingFilter"/> class
    /// </summary>
    /// <param name="logger">Logger for recording rate limit violations</param>
    /// <param name="maxRequests">Maximum requests per time window (default: 100)</param>
    /// <param name="windowSeconds">Time window in seconds (default: 60)</param>
    /// <exception cref="ArgumentNullException">Thrown when logger is null</exception>
    /// <exception cref="ArgumentException">Thrown when maxRequests or windowSeconds is less than 1</exception>
    public RateLimitingFilter(
        ILogger<RateLimitingFilter> logger,
        int maxRequests = 100,
        int windowSeconds = 60
    )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (maxRequests < 1)
        {
            throw new ArgumentException("Max requests must be at least 1", nameof(maxRequests));
        }

        if (windowSeconds < 1)
        {
            throw new ArgumentException("Window seconds must be at least 1", nameof(windowSeconds));
        }

        _maxRequests = maxRequests;
        _windowSeconds = windowSeconds;
    }

    /// <summary>
    /// Checks rate limit before action execution
    /// </summary>
    /// <param name="context">The action executing context</param>
    /// <remarks>
    /// Tracks request count for the client (by IP or user ID) and enforces rate limits.
    /// Returns 429 Too Many Requests if limit is exceeded.
    /// Adds rate limit headers to response for client awareness.
    /// Periodically cleans up expired entries to prevent memory growth.
    /// </remarks>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var clientId = GetClientIdentifier(context.HttpContext);
        var now = DateTimeOffset.UtcNow;

        var requestInfo = RequestCounts.AddOrUpdate(
            clientId,
            new ClientRequestInfo { Count = 1, WindowStart = now },
            (_, existing) =>
            {
                // Reset window if expired
                if (now - existing.WindowStart > TimeSpan.FromSeconds(_windowSeconds))
                {
                    return new ClientRequestInfo { Count = 1, WindowStart = now };
                }

                existing.Count++;
                return existing;
            }
        );

        var remaining = Math.Max(0, _maxRequests - requestInfo.Count);
        var resetTime = requestInfo.WindowStart.AddSeconds(_windowSeconds);

        // Add rate limit headers
        context.HttpContext.Response.Headers["X-RateLimit-Limit"] = _maxRequests.ToString();
        context.HttpContext.Response.Headers["X-RateLimit-Remaining"] = remaining.ToString();
        context.HttpContext.Response.Headers["X-RateLimit-Reset"] = resetTime
            .ToUnixTimeSeconds()
            .ToString();

        // Check if limit exceeded
        if (requestInfo.Count > _maxRequests)
        {
            _logger.LogWarning(
                "Rate limit exceeded for client {ClientId}. Count: {Count}, Limit: {Limit}",
                clientId,
                requestInfo.Count,
                _maxRequests
            );

            context.Result = new ObjectResult(
                new
                {
                    Message = "Too many requests. Please try again later.",
                    RetryAfter = (int)(resetTime - now).TotalSeconds,
                }
            )
            {
                StatusCode = (int)HttpStatusCode.TooManyRequests,
            };

            context.HttpContext.Response.Headers["Retry-After"] = (
                (int)(resetTime - now).TotalSeconds
            ).ToString();
        }

        // Cleanup old entries periodically (every 1000 requests)
        if (RequestCounts.Count > 10000)
        {
            CleanupExpiredEntries(now);
        }
    }

    /// <summary>
    /// Called after action execution (no-op for this filter)
    /// </summary>
    /// <param name="context">The action executed context</param>
    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No action needed after execution
    }

    /// <summary>
    /// Gets a unique identifier for the client
    /// </summary>
    /// <param name="httpContext">The HTTP context</param>
    /// <returns>A unique client identifier string</returns>
    /// <remarks>
    /// Uses authenticated user ID if available, otherwise falls back to IP address.
    /// Ensures each client is tracked separately for fair rate limiting.
    /// </remarks>
    private static string GetClientIdentifier(HttpContext httpContext)
    {
        var userId = httpContext
            .User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
            ?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            return $"user_{userId}";
        }

        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip_{ipAddress}";
    }

    /// <summary>
    /// Removes expired request tracking entries
    /// </summary>
    /// <param name="now">Current timestamp</param>
    /// <remarks>
    /// Cleans up entries older than the time window to prevent memory leaks.
    /// Called periodically when the dictionary grows large.
    /// </remarks>
    private void CleanupExpiredEntries(DateTimeOffset now)
    {
        var expiredKeys = RequestCounts
            .Where(kvp => now - kvp.Value.WindowStart > TimeSpan.FromSeconds(_windowSeconds * 2))
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            RequestCounts.TryRemove(key, out _);
        }

        if (expiredKeys.Count > 0)
        {
            _logger.LogDebug("Cleaned up {Count} expired rate limit entries", expiredKeys.Count);
        }
    }

    /// <summary>
    /// Stores request count and window start time for a client
    /// </summary>
    private sealed class ClientRequestInfo
    {
        public int Count { get; set; }
        public DateTimeOffset WindowStart { get; set; }
    }
}
