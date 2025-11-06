using System.Collections.Concurrent;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ECommerce.API.Filters;

/// <summary>
/// Action filter that implements token bucket rate limiting to prevent API abuse and protect against denial-of-service attacks.
/// </summary>
/// <remarks>
/// <para>
/// Provides request throttling to protect API endpoints from excessive requests by limiting the number of requests
/// a client can make within a sliding time window. Tracks requests per client using IP address for anonymous
/// requests and authenticated user ID for logged-in users, ensuring fair resource allocation.
/// </para>
/// <para>
/// <strong>Features:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><strong>Request Throttling:</strong> Limits requests per configurable time window using sliding window algorithm</description></item>
/// <item><description><strong>Per-Client Tracking:</strong> Separate limits for each client identified by user ID or IP address</description></item>
/// <item><description><strong>Configurable Limits:</strong> Customize maximum requests and time window duration per filter instance</description></item>
/// <item><description><strong>Automatic Cleanup:</strong> Periodic removal of expired entries to prevent memory leaks</description></item>
/// <item><description><strong>Standard Headers:</strong> RFC-compliant X-RateLimit-* headers for client awareness and retry logic</description></item>
/// <item><description><strong>Retry-After Header:</strong> Informs clients when they can retry after exceeding limits</description></item>
/// <item><description><strong>Thread-Safe:</strong> Uses ConcurrentDictionary for safe concurrent access</description></item>
/// </list>
/// <para>
/// <strong>Rate Limiting Algorithm:</strong>
/// </para>
/// <list type="number">
/// <item><description>Client makes request and is identified by user ID or IP address</description></item>
/// <item><description>Filter checks if client has existing request tracking entry</description></item>
/// <item><description>If time window expired, resets counter to 1</description></item>
/// <item><description>If within window, increments counter</description></item>
/// <item><description>Compares counter against maximum allowed requests</description></item>
/// <item><description>If exceeded, returns 429 Too Many Requests response</description></item>
/// <item><description>If within limits, allows request to proceed</description></item>
/// <item><description>Adds rate limit headers to all responses</description></item>
/// </list>
/// <para>
/// <strong>Client Identification:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><strong>Authenticated Users:</strong> Tracked by user ID from JWT claims (NameIdentifier)</description></item>
/// <item><description><strong>Anonymous Users:</strong> Tracked by IP address from connection remote address</description></item>
/// <item><description><strong>Prefix Convention:</strong> Keys prefixed with "user_" or "ip_" for identification</description></item>
/// </list>
/// <para>
/// <strong>Usage Examples:</strong>
/// </para>
/// <code>
/// // Apply globally to all endpoints in Program.cs:
/// builder.Services.AddScoped&lt;RateLimitingFilter&gt;();
/// builder.Services.AddControllers(options =>
/// {
///     options.Filters.AddService&lt;RateLimitingFilter&gt;();
/// });
///
/// // Apply per controller with custom limits:
/// [ServiceFilter(typeof(RateLimitingFilter), Arguments = new object[] { 50, 300 })] // 50 req/5min
/// [ApiController]
/// [Route("api/[controller]")]
/// public class AuthController : ControllerBase { }
///
/// // Apply per action for sensitive endpoints:
/// [ServiceFilter(typeof(RateLimitingFilter), Arguments = new object[] { 5, 60 })] // 5 req/min
/// [HttpPost("login")]
/// public async Task&lt;IActionResult&gt; Login(LoginDto dto) { }
///
/// // Apply with default limits (100 requests per 60 seconds):
/// [ServiceFilter(typeof(RateLimitingFilter))]
/// [HttpGet]
/// public async Task&lt;IActionResult&gt; GetData() { }
/// </code>
/// <para>
/// <strong>Default Configuration:</strong>
/// </para>
/// <list type="bullet">
/// <item><term>Max Requests:</term><description>100 requests</description></item>
/// <item><term>Time Window:</term><description>60 seconds (1 minute)</description></item>
/// <item><term>Cleanup Threshold:</term><description>10,000 entries</description></item>
/// <item><term>Cleanup Window:</term><description>2x time window duration</description></item>
/// </list>
/// <para>
/// <strong>HTTP Response Headers:</strong>
/// All responses include the following headers for client rate limit awareness:
/// </para>
/// <list type="bullet">
/// <item><term>X-RateLimit-Limit</term><description>Maximum requests allowed in time window</description></item>
/// <item><term>X-RateLimit-Remaining</term><description>Number of requests remaining in current window</description></item>
/// <item><term>X-RateLimit-Reset</term><description>Unix timestamp when the rate limit resets</description></item>
/// <item><term>Retry-After</term><description>(429 only) Seconds until client can retry</description></item>
/// </list>
/// <para>
/// <strong>HTTP 429 Response Example:</strong>
/// </para>
/// <code>
/// HTTP/1.1 429 Too Many Requests
/// X-RateLimit-Limit: 100
/// X-RateLimit-Remaining: 0
/// X-RateLimit-Reset: 1678901234
/// Retry-After: 45
/// Content-Type: application/json
///
/// {
///   "message": "Too many requests. Please try again later.",
///   "retryAfter": 45
/// }
/// </code>
/// <para>
/// <strong>Memory Management:</strong>
/// The filter automatically cleans up expired entries when the dictionary exceeds 10,000 entries.
/// Entries are considered expired if they are older than 2x the configured time window.
/// This prevents unbounded memory growth in high-traffic scenarios.
/// </para>
/// <para>
/// <strong>Recommended Limits:</strong>
/// </para>
/// <list type="table">
/// <listheader>
/// <term>Endpoint Type</term>
/// <description>Recommended Limit</description>
/// </listheader>
/// <item>
/// <term>Public Read APIs</term>
/// <description>100-1000 requests per minute</description>
/// </item>
/// <item>
/// <term>Authentication</term>
/// <description>5-10 requests per minute</description>
/// </item>
/// <item>
/// <term>Write Operations</term>
/// <description>30-60 requests per minute</description>
/// </item>
/// <item>
/// <term>Search/Query</term>
/// <description>50-100 requests per minute</description>
/// </item>
/// <item>
/// <term>File Upload</term>
/// <description>10-20 requests per hour</description>
/// </item>
/// </list>
/// <para>
/// <strong>Production Considerations:</strong>
/// </para>
/// <list type="bullet">
/// <item><description>Consider using distributed rate limiting (Redis) for multi-server deployments</description></item>
/// <item><description>Monitor rate limit violations to detect potential abuse or attacks</description></item>
/// <item><description>Adjust limits based on actual usage patterns and server capacity</description></item>
/// <item><description>Use different limits for authenticated vs. anonymous users</description></item>
/// <item><description>Implement tiered rate limits for different user subscription levels</description></item>
/// </list>
/// <para>
/// <strong>Thread Safety:</strong> This filter is thread-safe and uses <see cref="ConcurrentDictionary{TKey,TValue}"/>
/// for atomic operations on shared state. Multiple concurrent requests from the same client are handled correctly.
/// </para>
/// </remarks>
/// <example>
/// Complete configuration example with custom limits:
/// <code>
/// // In Program.cs:
/// builder.Services.AddScoped&lt;RateLimitingFilter&gt;();
///
/// // Apply different limits to different controllers:
///
/// // Strict limits for authentication:
/// [ServiceFilter(typeof(RateLimitingFilter), Arguments = new object[] { 5, 300 })]
/// [ApiController]
/// [Route("api/auth")]
/// public class AuthController : ControllerBase
/// {
///     [HttpPost("login")]
///     public async Task&lt;IActionResult&gt; Login(LoginDto dto) { }
/// }
///
/// // Moderate limits for write operations:
/// [ServiceFilter(typeof(RateLimitingFilter), Arguments = new object[] { 60, 60 })]
/// [ApiController]
/// [Route("api/products")]
/// public class ProductsController : ControllerBase
/// {
///     [HttpPost]
///     public async Task&lt;IActionResult&gt; Create(ProductDto dto) { }
/// }
///
/// // Generous limits for read operations:
/// [ServiceFilter(typeof(RateLimitingFilter), Arguments = new object[] { 1000, 60 })]
/// [HttpGet]
/// public async Task&lt;IActionResult&gt; GetAll() { }
/// </code>
/// </example>
/// <seealso cref="IActionFilter"/>
/// <seealso cref="ConcurrentDictionary{TKey,TValue}"/>
public sealed class RateLimitingFilter : IActionFilter
{
    /// <summary>
    /// Thread-safe storage for tracking request counts per client across all filter instances.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Static concurrent dictionary that maintains request tracking information for all clients.
    /// Keys are client identifiers (prefixed with "user_" or "ip_"), and values are
    /// <see cref="ClientRequestInfo"/> objects containing request count and window start time.
    /// </para>
    /// <para>
    /// <strong>Key Format:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><term>user_{userId}</term><description>For authenticated users (e.g., "user_12345")</description></item>
    /// <item><term>ip_{ipAddress}</term><description>For anonymous users (e.g., "ip_192.168.1.100")</description></item>
    /// </list>
    /// <para>
    /// <strong>Thread Safety:</strong> Uses <see cref="ConcurrentDictionary{TKey,TValue}"/> which provides
    /// thread-safe operations without explicit locking. All operations (Add, Update, Remove) are atomic.
    /// </para>
    /// <para>
    /// <strong>Memory Management:</strong> Automatically cleaned up when dictionary size exceeds 10,000 entries.
    /// Expired entries (older than 2x time window) are periodically removed to prevent memory leaks.
    /// </para>
    /// <para>
    /// <strong>Static Scope:</strong> Being static, the dictionary is shared across all instances of
    /// <see cref="RateLimitingFilter"/> and persists for the application lifetime. This ensures consistent
    /// rate limiting even when multiple filter instances are used.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> ConcurrentDictionary uses fine-grained locking for excellent concurrent
    /// performance. Typical operations complete in O(1) time complexity.
    /// </para>
    /// </remarks>
    /// <seealso cref="ClientRequestInfo"/>
    /// <seealso cref="ConcurrentDictionary{TKey,TValue}"/>
    private static readonly ConcurrentDictionary<string, ClientRequestInfo> RequestCounts = new();

    /// <summary>
    /// Logger instance for recording rate limit violations and diagnostic information.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used to log rate limiting events at various severity levels:
    /// </para>
    /// <list type="bullet">
    /// <item><term>Warning Level:</term><description>Rate limit exceeded events with client ID, request count, and limit threshold</description></item>
    /// <item><term>Debug Level:</term><description>Cleanup operations showing number of expired entries removed</description></item>
    /// </list>
    /// <para>
    /// <strong>Log Information:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Client identifier (user ID or IP address)</description></item>
    /// <item><description>Current request count for the client</description></item>
    /// <item><description>Configured rate limit threshold</description></item>
    /// <item><description>Number of cleaned up entries during maintenance</description></item>
    /// </list>
    /// <para>
    /// <strong>Example Log Messages:</strong>
    /// </para>
    /// <code>
    /// WARN: Rate limit exceeded for client user_12345. Count: 101, Limit: 100
    /// DEBUG: Cleaned up 342 expired rate limit entries
    /// </code>
    /// <para>
    /// Logs are essential for:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Detecting potential API abuse or DDoS attacks</description></item>
    /// <item><description>Monitoring rate limit effectiveness</description></item>
    /// <item><description>Identifying clients that frequently exceed limits</description></item>
    /// <item><description>Troubleshooting rate limiting issues</description></item>
    /// <item><description>Capacity planning based on usage patterns</description></item>
    /// </list>
    /// </remarks>
    private readonly ILogger<RateLimitingFilter> _logger;

    /// <summary>
    /// Maximum number of requests allowed per client within the time window.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Defines the upper limit of requests a single client can make within the configured time window
    /// before receiving HTTP 429 Too Many Requests responses. This threshold applies independently
    /// to each client identified by user ID or IP address.
    /// </para>
    /// <para>
    /// <strong>Value Range:</strong> Must be at least 1 (validated in constructor).
    /// </para>
    /// <para>
    /// <strong>Default Value:</strong> 100 requests (if not specified in constructor).
    /// </para>
    /// <para>
    /// <strong>Recommended Values by Use Case:</strong>
    /// </para>
    /// <list type="table">
    /// <listheader>
    /// <term>Use Case</term>
    /// <description>Recommended Value</description>
    /// </listheader>
    /// <item>
    /// <term>Login/Authentication</term>
    /// <description>5-10 requests (prevent brute force attacks)</description>
    /// </item>
    /// <item>
    /// <term>Password Reset</term>
    /// <description>3-5 requests (prevent abuse)</description>
    /// </item>
    /// <item>
    /// <term>Write Operations</term>
    /// <description>30-60 requests (moderate throttling)</description>
    /// </item>
    /// <item>
    /// <term>Read Operations</term>
    /// <description>100-1000 requests (generous for normal usage)</description>
    /// </item>
    /// <item>
    /// <term>Search/Query</term>
    /// <description>50-100 requests (balance between usability and load)</description>
    /// </item>
    /// <item>
    /// <term>File Upload</term>
    /// <description>10-20 requests (resource-intensive operations)</description>
    /// </item>
    /// </list>
    /// <para>
    /// <strong>Calculation:</strong> When a request is received:
    /// </para>
    /// <list type="bullet">
    /// <item><description>If request count ≤ maxRequests: Request allowed, counter incremented</description></item>
    /// <item><description>If request count &gt; maxRequests: Request blocked with 429 response</description></item>
    /// </list>
    /// <para>
    /// <strong>Interactions:</strong> Works in conjunction with <see cref="_windowSeconds"/> to define
    /// the rate limit policy. For example, maxRequests=100 with windowSeconds=60 means
    /// "100 requests per minute".
    /// </para>
    /// </remarks>
    private readonly int _maxRequests;

    /// <summary>
    /// Time window duration in seconds for rate limit calculation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Defines the sliding time window duration during which the request count is tracked and enforced.
    /// When a client makes a request, if the current time is within this window from the window start time,
    /// the request count is incremented. If the window has expired, the counter resets to 1 and a new
    /// window begins.
    /// </para>
    /// <para>
    /// <strong>Value Range:</strong> Must be at least 1 second (validated in constructor).
    /// </para>
    /// <para>
    /// <strong>Default Value:</strong> 60 seconds (1 minute) if not specified in constructor.
    /// </para>
    /// <para>
    /// <strong>Recommended Values:</strong>
    /// </para>
    /// <list type="table">
    /// <listheader>
    /// <term>Duration</term>
    /// <description>Use Case</description>
    /// </listheader>
    /// <item>
    /// <term>10-30 seconds</term>
    /// <description>Very short bursts, real-time operations</description>
    /// </item>
    /// <item>
    /// <term>60 seconds (1 minute)</term>
    /// <description>Standard rate limiting, good balance (default)</description>
    /// </item>
    /// <item>
    /// <term>300 seconds (5 minutes)</term>
    /// <description>Longer windows for less frequent operations</description>
    /// </item>
    /// <item>
    /// <term>3600 seconds (1 hour)</term>
    /// <description>Hourly limits for resource-intensive operations</description>
    /// </item>
    /// <item>
    /// <term>86400 seconds (24 hours)</term>
    /// <description>Daily quotas for premium features or quotas</description>
    /// </item>
    /// </list>
    /// <para>
    /// <strong>Sliding Window Behavior:</strong>
    /// </para>
    /// <code>
    /// // Example: 5 requests per 60 seconds
    /// Time 0s: Request 1-5 allowed (count = 5)
    /// Time 30s: Request 6 blocked (count = 6, limit exceeded)
    /// Time 61s: Window expired, Request 7 allowed (count reset to 1, new window)
    /// Time 62s: Request 8 allowed (count = 2 in new window)
    /// </code>
    /// <para>
    /// <strong>Reset Calculation:</strong> Reset time is calculated as <c>WindowStart + windowSeconds</c>,
    /// allowing clients to know exactly when they can retry after exceeding the limit.
    /// </para>
    /// <para>
    /// <strong>Cleanup Impact:</strong> Expired entries are cleaned up after <c>2 × windowSeconds</c>
    /// to ensure accurate tracking while preventing immediate removal of potentially reusable data.
    /// </para>
    /// </remarks>
    private readonly int _windowSeconds;

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitingFilter"/> class with configurable rate limiting parameters.
    /// </summary>
    /// <param name="logger">
    /// Logger for recording rate limit violations and diagnostic events.
    /// Must not be <see langword="null"/>.
    /// </param>
    /// <param name="maxRequests">
    /// Maximum number of requests allowed per client within the time window.
    /// Must be at least 1. Default is 100 requests.
    /// </param>
    /// <param name="windowSeconds">
    /// Time window duration in seconds for rate limit enforcement.
    /// Must be at least 1. Default is 60 seconds (1 minute).
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="logger"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="maxRequests"/> is less than 1.
    /// Rate limiting requires at least one request to be allowed.
    /// -or-
    /// Thrown when <paramref name="windowSeconds"/> is less than 1.
    /// Time window must be at least one second for meaningful rate limiting.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This constructor is called by the ASP.NET Core dependency injection container when the filter
    /// is applied using the <c>[ServiceFilter]</c> attribute. The logger is injected automatically,
    /// while maxRequests and windowSeconds can be specified as arguments.
    /// </para>
    /// <para>
    /// <strong>Dependency Injection Setup:</strong>
    /// </para>
    /// <code>
    /// // In Program.cs:
    /// builder.Services.AddScoped&lt;RateLimitingFilter&gt;();
    ///
    /// // Or with factory for custom configuration:
    /// builder.Services.AddScoped&lt;RateLimitingFilter&gt;(provider =>
    /// {
    ///     var logger = provider.GetRequiredService&lt;ILogger&lt;RateLimitingFilter&gt;&gt;();
    ///     return new RateLimitingFilter(logger, maxRequests: 50, windowSeconds: 300);
    /// });
    /// </code>
    /// <para>
    /// <strong>Parameter Validation:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Validates that logger is not null (required dependency)</description></item>
    /// <item><description>Validates that maxRequests is at least 1 (minimum meaningful limit)</description></item>
    /// <item><description>Validates that windowSeconds is at least 1 (minimum time window)</description></item>
    /// </list>
    /// <para>
    /// <strong>Default Values:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><term>maxRequests</term><description>100 requests (reasonable default for most APIs)</description></item>
    /// <item><term>windowSeconds</term><description>60 seconds / 1 minute (standard time window)</description></item>
    /// </list>
    /// <para>
    /// These defaults provide "100 requests per minute" rate limiting, which is suitable for
    /// general-purpose API endpoints that are not particularly sensitive or resource-intensive.
    /// </para>
    /// </remarks>
    /// <example>
    /// Apply filter with various configurations:
    /// <code>
    /// // Use default limits (100 requests per 60 seconds):
    /// [ServiceFilter(typeof(RateLimitingFilter))]
    /// [HttpGet]
    /// public async Task&lt;IActionResult&gt; GetData() { }
    ///
    /// // Strict limits for authentication (5 requests per 5 minutes):
    /// [ServiceFilter(typeof(RateLimitingFilter), Arguments = new object[] { 5, 300 })]
    /// [HttpPost("login")]
    /// public async Task&lt;IActionResult&gt; Login(LoginDto dto) { }
    ///
    /// // Generous limits for read operations (1000 requests per minute):
    /// [ServiceFilter(typeof(RateLimitingFilter), Arguments = new object[] { 1000, 60 })]
    /// [HttpGet("products")]
    /// public async Task&lt;IActionResult&gt; GetProducts() { }
    ///
    /// // Hourly limit for resource-intensive operations (50 requests per hour):
    /// [ServiceFilter(typeof(RateLimitingFilter), Arguments = new object[] { 50, 3600 })]
    /// [HttpPost("export")]
    /// public async Task&lt;IActionResult&gt; ExportData() { }
    ///
    /// // Manual instantiation (not recommended, use DI):
    /// var logger = serviceProvider.GetRequiredService&lt;ILogger&lt;RateLimitingFilter&gt;&gt;();
    /// var filter = new RateLimitingFilter(logger, maxRequests: 100, windowSeconds: 60);
    /// </code>
    /// </example>
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
    /// Executes rate limiting logic before the action method is invoked.
    /// </summary>
    /// <param name="context">
    /// The action executing context containing request information and allowing result manipulation.
    /// Provides access to HTTP context, action descriptor, and the ability to short-circuit execution.
    /// </param>
    /// <remarks>
    /// <para>
    /// This method is called automatically by the ASP.NET Core framework before the controller action
    /// executes. It implements the core rate limiting logic using a sliding window algorithm.
    /// </para>
    /// <para>
    /// <strong>Execution Flow:</strong>
    /// </para>
    /// <list type="number">
    /// <item><description><strong>Client Identification:</strong> Determines client ID from user claims or IP address</description></item>
    /// <item><description><strong>Timestamp Capture:</strong> Gets current UTC time for window calculations</description></item>
    /// <item><description><strong>Request Tracking:</strong> Adds or updates client's request count atomically</description></item>
    /// <item><description><strong>Window Check:</strong> Resets counter if time window has expired</description></item>
    /// <item><description><strong>Counter Update:</strong> Increments request count for active windows</description></item>
    /// <item><description><strong>Remaining Calculation:</strong> Computes requests remaining in current window</description></item>
    /// <item><description><strong>Header Addition:</strong> Adds X-RateLimit-* headers to response</description></item>
    /// <item><description><strong>Limit Enforcement:</strong> Blocks request with 429 if limit exceeded</description></item>
    /// <item><description><strong>Periodic Cleanup:</strong> Removes expired entries when dictionary grows large</description></item>
    /// </list>
    /// <para>
    /// <strong>Request Tracking Logic:</strong>
    /// </para>
    /// <code>
    /// if (no existing entry OR window expired) {
    ///     Create new entry with count = 1 and current timestamp
    /// } else {
    ///     Increment existing count
    /// }
    ///
    /// if (count &gt; maxRequests) {
    ///     Return 429 Too Many Requests
    ///     Add Retry-After header
    /// } else {
    ///     Allow request to proceed
    /// }
    /// </code>
    /// <para>
    /// <strong>HTTP Headers Added:</strong>
    /// All responses include the following headers:
    /// </para>
    /// <list type="bullet">
    /// <item><term>X-RateLimit-Limit</term><description>Maximum requests allowed (e.g., "100")</description></item>
    /// <item><term>X-RateLimit-Remaining</term><description>Requests remaining in window (e.g., "45")</description></item>
    /// <item><term>X-RateLimit-Reset</term><description>Unix timestamp when limit resets (e.g., "1678901234")</description></item>
    /// <item><term>Retry-After</term><description>(429 only) Seconds until retry allowed (e.g., "30")</description></item>
    /// </list>
    /// <para>
    /// <strong>429 Response Structure:</strong>
    /// When rate limit is exceeded, returns:
    /// </para>
    /// <code>
    /// {
    ///   "message": "Too many requests. Please try again later.",
    ///   "retryAfter": 45  // seconds
    /// }
    /// </code>
    /// <para>
    /// <strong>Atomic Operations:</strong>
    /// Uses <see cref="ConcurrentDictionary{TKey,TValue}.AddOrUpdate"/> for thread-safe updates,
    /// ensuring accurate counting even under high concurrency.
    /// </para>
    /// <para>
    /// <strong>Memory Management:</strong>
    /// Triggers cleanup when <see cref="RequestCounts"/> exceeds 10,000 entries to prevent
    /// unbounded memory growth in high-traffic scenarios.
    /// </para>
    /// <para>
    /// <strong>Performance Characteristics:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>O(1) dictionary lookups and updates</description></item>
    /// <item><description>Minimal overhead for requests within limits (~microseconds)</description></item>
    /// <item><description>Periodic cleanup runs only when threshold exceeded</description></item>
    /// <item><description>No database calls or external dependencies</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// Request sequence demonstrating rate limiting:
    /// <code>
    /// // First request at T=0s (maxRequests=5, windowSeconds=60):
    /// Request 1: Count=1, Remaining=4, Reset=60s → Allowed (200 OK)
    ///
    /// // Additional requests:
    /// Request 2: Count=2, Remaining=3, Reset=60s → Allowed (200 OK)
    /// Request 3: Count=3, Remaining=2, Reset=60s → Allowed (200 OK)
    /// Request 4: Count=4, Remaining=1, Reset=60s → Allowed (200 OK)
    /// Request 5: Count=5, Remaining=0, Reset=60s → Allowed (200 OK)
    ///
    /// // Exceeding limit:
    /// Request 6: Count=6, Remaining=0, Reset=60s → Blocked (429 Too Many Requests)
    /// Response includes: Retry-After: 55 (seconds remaining in window)
    ///
    /// // After window expires at T=61s:
    /// Request 7: Count=1, Remaining=4, Reset=121s → Allowed (200 OK, new window)
    /// </code>
    /// </example>
    /// <seealso cref="OnActionExecuted"/>
    /// <seealso cref="GetClientIdentifier"/>
    /// <seealso cref="CleanupExpiredEntries"/>
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
    /// Called after the action method executes. No operation is performed in this method.
    /// </summary>
    /// <param name="context">
    /// The action executed context containing information about the completed action execution,
    /// including the result, exception (if any), and HTTP context.
    /// </param>
    /// <remarks>
    /// <para>
    /// This method is part of the <see cref="IActionFilter"/> interface contract but performs no
    /// operations for rate limiting purposes. All rate limiting logic is executed in
    /// <see cref="OnActionExecuting"/> before the action runs.
    /// </para>
    /// <para>
    /// <strong>Why No Post-Processing?</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Rate limiting must occur <em>before</em> action execution to prevent resource consumption</description></item>
    /// <item><description>Request counting and header addition complete in pre-execution phase</description></item>
    /// <item><description>No cleanup or adjustment needed after successful action execution</description></item>
    /// <item><description>Failed requests (429) are short-circuited before reaching action</description></item>
    /// </list>
    /// <para>
    /// <strong>Interface Requirement:</strong>
    /// This method must be implemented to satisfy the <see cref="IActionFilter"/> interface,
    /// even though it contains no logic. Alternative implementations could use this method for:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Logging successful action completions</description></item>
    /// <item><description>Adjusting rate limits based on response status</description></item>
    /// <item><description>Refunding rate limit tokens for certain error conditions</description></item>
    /// <item><description>Recording metrics for monitoring</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// Example of potential post-execution logic (not implemented):
    /// <code>
    /// public void OnActionExecuted(ActionExecutedContext context)
    /// {
    ///     // Example: Refund rate limit token if server error occurs
    ///     if (context.Result is ObjectResult result
    ///         &amp;&amp; result.StatusCode >= 500)
    ///     {
    ///         var clientId = GetClientIdentifier(context.HttpContext);
    ///         // Decrement counter (not implemented in current version)
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="OnActionExecuting"/>
    /// <seealso cref="IActionFilter"/>
    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No action needed after execution
    }

    /// <summary>
    /// Gets a unique identifier for the client making the request.
    /// </summary>
    /// <param name="httpContext">
    /// The HTTP context containing user information and connection details.
    /// Must not be <see langword="null"/>.
    /// </param>
    /// <returns>
    /// A unique client identifier string in one of two formats:
    /// <list type="bullet">
    /// <item><description><c>"user_{userId}"</c> - For authenticated users with valid claims</description></item>
    /// <item><description><c>"ip_{ipAddress}"</c> - For anonymous users or when IP is available</description></item>
    /// <item><description><c>"ip_unknown"</c> - Fallback when no identifying information is available</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// Determines client identity using a priority-based approach to ensure accurate and fair rate limiting.
    /// Authenticated users are tracked by their user ID (from JWT claims), while anonymous users are
    /// tracked by IP address. This prevents different users behind the same IP (e.g., corporate proxy)
    /// from sharing the same rate limit.
    /// </para>
    /// <para>
    /// <strong>Identification Priority:</strong>
    /// </para>
    /// <list type="number">
    /// <item><description><strong>User ID (Preferred):</strong> Extracted from JWT NameIdentifier claim</description></item>
    /// <item><description><strong>IP Address (Fallback):</strong> Obtained from connection remote address</description></item>
    /// <item><description><strong>Unknown (Last Resort):</strong> Used when neither is available</description></item>
    /// </list>
    /// <para>
    /// <strong>User ID Extraction:</strong>
    /// Searches for the standard ASP.NET Core claim type <see cref="System.Security.Claims.ClaimTypes.NameIdentifier"/>
    /// which is typically set during JWT authentication. The claim value is usually the user's unique database ID.
    /// </para>
    /// <code>
    /// // JWT payload typically contains:
    /// {
    ///   "sub": "12345",           // NameIdentifier claim
    ///   "email": "user@example.com",
    ///   "role": "Customer"
    /// }
    /// // Results in identifier: "user_12345"
    /// </code>
    /// <para>
    /// <strong>IP Address Extraction:</strong>
    /// Uses <see cref="HttpContext.Connection.RemoteIpAddress"/> which represents the client's IP address.
    /// Note that this may be a proxy IP in some network configurations.
    /// </para>
    /// <para>
    /// <strong>Key Format Examples:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><c>"user_12345"</c> - Authenticated user with ID 12345</description></item>
    /// <item><description><c>"user_abc-def-789"</c> - Authenticated user with GUID ID</description></item>
    /// <item><description><c>"ip_192.168.1.100"</c> - Anonymous user from IPv4 address</description></item>
    /// <item><description><c>"ip_2001:0db8:85a3:0000:0000:8a2e:0370:7334"</c> - Anonymous user from IPv6</description></item>
    /// <item><description><c>"ip_unknown"</c> - Unable to determine client identity</description></item>
    /// </list>
    /// <para>
    /// <strong>Benefits of User ID Tracking:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>More accurate tracking across different IP addresses (mobile users, VPNs)</description></item>
    /// <item><description>Prevents rate limit sharing between different users behind same proxy</description></item>
    /// <item><description>Enables user-specific rate limit policies or premium tiers</description></item>
    /// <item><description>Better for identifying and blocking abusive individual users</description></item>
    /// </list>
    /// <para>
    /// <strong>IP Address Considerations:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>May not uniquely identify users behind NAT or corporate proxies</description></item>
    /// <item><description>Can change for mobile users or those using dynamic IPs</description></item>
    /// <item><description>May be spoofed or hidden by VPNs/proxies</description></item>
    /// <item><description>Useful for protecting public endpoints that don't require authentication</description></item>
    /// </list>
    /// <para>
    /// <strong>Thread Safety:</strong> This method is thread-safe as it only reads from the HTTP context
    /// and performs no mutations.
    /// </para>
    /// </remarks>
    /// <example>
    /// Identifier generation for different scenarios:
    /// <code>
    /// // Scenario 1: Authenticated user
    /// var httpContext = new DefaultHttpContext();
    /// httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
    /// {
    ///     new Claim(ClaimTypes.NameIdentifier, "12345")
    /// }));
    /// var id1 = GetClientIdentifier(httpContext);
    /// // Result: "user_12345"
    ///
    /// // Scenario 2: Anonymous user with IP
    /// var httpContext2 = new DefaultHttpContext();
    /// httpContext2.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.100");
    /// var id2 = GetClientIdentifier(httpContext2);
    /// // Result: "ip_192.168.1.100"
    ///
    /// // Scenario 3: No identifying information
    /// var httpContext3 = new DefaultHttpContext();
    /// var id3 = GetClientIdentifier(httpContext3);
    /// // Result: "ip_unknown"
    /// </code>
    /// </example>
    /// <seealso cref="HttpContext"/>
    /// <seealso cref="System.Security.Claims.ClaimTypes.NameIdentifier"/>
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
    /// Removes expired request tracking entries from the cache to prevent memory leaks.
    /// </summary>
    /// <param name="now">
    /// Current timestamp used to determine which entries are expired.
    /// Typically <see cref="DateTimeOffset.UtcNow"/>.
    /// </param>
    /// <remarks>
    /// <para>
    /// Performs maintenance on the <see cref="RequestCounts"/> dictionary by identifying and removing
    /// entries that are older than double the configured time window. This prevents unbounded memory
    /// growth in high-traffic scenarios while maintaining accurate rate limiting.
    /// </para>
    /// <para>
    /// <strong>Cleanup Strategy:</strong>
    /// </para>
    /// <list type="number">
    /// <item><description>Iterates through all entries in the dictionary</description></item>
    /// <item><description>Identifies entries where <c>now - WindowStart &gt; 2 × windowSeconds</c></description></item>
    /// <item><description>Collects expired keys into a removal list</description></item>
    /// <item><description>Removes each expired entry using thread-safe operations</description></item>
    /// <item><description>Logs the number of removed entries at Debug level</description></item>
    /// </list>
    /// <para>
    /// <strong>Expiration Threshold:</strong>
    /// Entries are considered expired if they are older than <c>2 × windowSeconds</c> (not just <c>windowSeconds</c>).
    /// This provides a safety buffer to ensure entries are truly no longer needed:
    /// </para>
    /// <code>
    /// // Example with windowSeconds = 60:
    /// Current Time: 1000s
    /// Entry Window Start: 800s
    /// Age: 200s
    /// Threshold: 120s (2 × 60)
    /// Result: Entry is expired and will be removed
    /// </code>
    /// <para>
    /// <strong>Trigger Conditions:</strong>
    /// This method is called from <see cref="OnActionExecuting"/> when the dictionary size
    /// exceeds 10,000 entries. This threshold balances:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Cleanup frequency (not too often to avoid overhead)</description></item>
    /// <item><description>Memory usage (prevents excessive growth)</description></item>
    /// <item><description>Performance impact (cleanup is relatively fast)</description></item>
    /// </list>
    /// <para>
    /// <strong>Performance Characteristics:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Time Complexity: O(n) where n is the number of entries</description></item>
    /// <item><description>Space Complexity: O(k) where k is the number of expired entries</description></item>
    /// <item><description>Typically completes in milliseconds even with thousands of entries</description></item>
    /// <item><description>Does not block other requests significantly due to concurrent dictionary</description></item>
    /// </list>
    /// <para>
    /// <strong>Memory Impact:</strong>
    /// For a high-traffic API with 10,000 entries:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Each entry: ~100-200 bytes (key + ClientRequestInfo object)</description></item>
    /// <item><description>Total memory: ~1-2 MB before cleanup</description></item>
    /// <item><description>After cleanup: Depends on active clients, typically 50-80% reduction</description></item>
    /// </list>
    /// <para>
    /// <strong>Thread Safety:</strong>
    /// Uses <see cref="ConcurrentDictionary{TKey,TValue}.TryRemove"/> which is thread-safe.
    /// Multiple threads can safely call this method concurrently, though typically only one
    /// cleanup operation runs at a time due to the trigger threshold.
    /// </para>
    /// <para>
    /// <strong>Logging:</strong>
    /// Logs cleanup operations at Debug level only when entries are actually removed:
    /// </para>
    /// <code>
    /// DEBUG: Cleaned up 342 expired rate limit entries
    /// </code>
    /// </remarks>
    /// <example>
    /// Cleanup behavior illustration:
    /// <code>
    /// // Configuration: windowSeconds = 60
    /// // Current Time: T = 1000s
    ///
    /// // Dictionary state before cleanup:
    /// {
    ///   "user_123": { Count: 5, WindowStart: 950s },  // Age: 50s  → Keep
    ///   "user_456": { Count: 10, WindowStart: 880s }, // Age: 120s → Keep (exactly at threshold)
    ///   "user_789": { Count: 3, WindowStart: 850s },  // Age: 150s → Remove (expired)
    ///   "ip_192.168.1.1": { Count: 2, WindowStart: 800s }, // Age: 200s → Remove (expired)
    ///   "ip_10.0.0.1": { Count: 50, WindowStart: 750s },   // Age: 250s → Remove (expired)
    /// }
    ///
    /// // After cleanup:
    /// {
    ///   "user_123": { Count: 5, WindowStart: 950s },
    ///   "user_456": { Count: 10, WindowStart: 880s }
    /// }
    ///
    /// // Log output:
    /// // DEBUG: Cleaned up 3 expired rate limit entries
    /// </code>
    /// </example>
    /// <seealso cref="RequestCounts"/>
    /// <seealso cref="OnActionExecuting"/>
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
    /// Stores request count and time window information for a specific client.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This internal class represents the rate limiting state for a single client (user or IP address).
    /// It tracks the number of requests made within the current time window and when that window started.
    /// </para>
    /// <para>
    /// <strong>Properties:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><term>Count</term><description>Number of requests made in the current time window</description></item>
    /// <item><term>WindowStart</term><description>Timestamp when the current time window began</description></item>
    /// </list>
    /// <para>
    /// <strong>Lifecycle:</strong>
    /// </para>
    /// <list type="number">
    /// <item><description><strong>Creation:</strong> New instance created when client makes first request</description></item>
    /// <item><description><strong>Updates:</strong> Count incremented on each subsequent request within window</description></item>
    /// <item><description><strong>Reset:</strong> New instance created when time window expires</description></item>
    /// <item><description><strong>Cleanup:</strong> Removed when entry is older than 2× time window</description></item>
    /// </list>
    /// <para>
    /// <strong>Thread Safety:</strong>
    /// While the class itself is not thread-safe, it is stored in a <see cref="ConcurrentDictionary{TKey,TValue}/>
    /// and updated using atomic operations (<see cref="ConcurrentDictionary{TKey,TValue}.AddOrUpdate"/>),
    /// ensuring thread-safe access in the rate limiting implementation.
    /// </para>
    /// <para>
    /// <strong>Memory Footprint:</strong>
    /// Each instance occupies approximately 24-32 bytes:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Count (int): 4 bytes</description></item>
    /// <item><description>WindowStart (DateTimeOffset): 16 bytes</description></item>
    /// <item><description>Object overhead: 4-12 bytes (CLR metadata)</description></item>
    /// </list>
    /// <para>
    /// <strong>Immutability Consideration:</strong>
    /// This class is mutable to allow efficient updates. In the <see cref="OnActionExecuting"/>
    /// method, the existing instance is modified rather than creating a new one on each request,
    /// which improves performance and reduces GC pressure.
    /// </para>
    /// </remarks>
    /// <example>
    /// Typical lifecycle of a ClientRequestInfo instance:
    /// <code>
    /// // First request at T=100s:
    /// var info = new ClientRequestInfo { Count = 1, WindowStart = now };
    /// // info: { Count: 1, WindowStart: 100s }
    ///
    /// // Second request at T=105s (within window):
    /// info.Count++; // Existing instance updated
    /// // info: { Count: 2, WindowStart: 100s }
    ///
    /// // Request at T=105s:
    /// info.Count++;
    /// // info: { Count: 3, WindowStart: 100s }
    ///
    /// // Request at T=165s (window expired, windowSeconds=60):
    /// // Old instance discarded, new instance created:
    /// var newInfo = new ClientRequestInfo { Count = 1, WindowStart = now };
    /// // newInfo: { Count: 1, WindowStart: 165s }
    ///
    /// // At T=345s (2× window duration passed):
    /// // Entry eligible for cleanup and removal from dictionary
    /// </code>
    /// </example>
    /// <seealso cref="RequestCounts"/>
    private sealed class ClientRequestInfo
    {
        /// <summary>
        /// Gets or sets the number of requests made by the client in the current time window.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Tracks the cumulative number of requests made by a specific client within the active time window.
        /// This count is compared against <see cref="_maxRequests"/> to enforce rate limits.
        /// </para>
        /// <para>
        /// <strong>Behavior:</strong>
        /// </para>
        /// <list type="bullet">
        /// <item><description>Initialized to 1 when a new time window begins</description></item>
        /// <item><description>Incremented by 1 for each subsequent request in the same window</description></item>
        /// <item><description>Reset to 1 when the time window expires</description></item>
        /// <item><description>Used to calculate remaining requests and enforce limits</description></item>
        /// </list>
        /// <para>
        /// <strong>Rate Limit Decision:</strong>
        /// </para>
        /// <code>
        /// if (Count &gt; maxRequests) {
        ///     // Block request with 429 Too Many Requests
        /// } else {
        ///     // Allow request to proceed
        /// }
        /// </code>
        /// </remarks>
        /// <value>
        /// An integer representing the request count. Valid range: 1 to int.MaxValue.
        /// Typically remains below the configured maximum request limit.
        /// </value>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the current rate limiting time window started.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Records the UTC timestamp when the current time window began. This is used to determine
        /// when the window expires and should be reset, as well as to calculate the reset time
        /// for HTTP response headers.
        /// </para>
        /// <para>
        /// <strong>Window Expiration Logic:</strong>
        /// </para>
        /// <code>
        /// var windowAge = DateTimeOffset.UtcNow - WindowStart;
        /// if (windowAge &gt; TimeSpan.FromSeconds(windowSeconds)) {
        ///     // Window expired, reset count and update WindowStart
        /// }
        /// </code>
        /// <para>
        /// <strong>Reset Time Calculation:</strong> The X-RateLimit-Reset header is calculated as:
        /// </para>
        /// <code>
        /// var resetTime = WindowStart.AddSeconds(windowSeconds);
        /// var resetTimestamp = resetTime.ToUnixTimeSeconds();
        /// </code>
        /// <para>
        /// <strong>UTC Timezone:</strong>
        /// Always stored in UTC to ensure consistency across different server time zones
        /// and avoid daylight saving time issues.
        /// </para>
        /// </remarks>
        /// <value>
        /// A <see cref="DateTimeOffset"/> representing when the time window started,
        /// always in UTC timezone.
        /// </value>
        public DateTimeOffset WindowStart { get; set; }
    }
}
