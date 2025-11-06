using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;

namespace ECommerce.API.Filters;

/// <summary>
/// Resource filter that implements response caching for GET requests
/// </summary>
/// <remarks>
/// <para>
/// Caches GET request responses in memory to improve performance and reduce database load.
/// Automatically generates cache keys based on request path and query parameters.
/// Useful for frequently accessed, relatively static data like product catalogs.
/// </para>
/// <para>
/// <strong>Features:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><strong>Automatic Caching:</strong> Caches successful GET responses</description></item>
/// <item><description><strong>Smart Cache Keys:</strong> Generates unique keys from URL and parameters</description></item>
/// <item><description><strong>Configurable Duration:</strong> Set cache expiration per instance</description></item>
/// <item><description><strong>GET Only:</strong> Only caches idempotent GET operations</description></item>
/// <item><description><strong>Cache Headers:</strong> Adds X-Cache header to indicate cache status</description></item>
/// </list>
/// <para>
/// <strong>Usage:</strong>
/// </para>
/// <code>
/// // Apply to specific actions:
/// [ServiceFilter(typeof(CacheResourceFilter), Arguments = new object[] { 300 })] // 5 minutes
/// [HttpGet]
/// public async Task&lt;ActionResult&lt;IEnumerable&lt;Product&gt;&gt;&gt; GetProducts() { }
/// </code>
/// <para>
/// <strong>Cache Invalidation:</strong> Cache entries expire after the specified duration.
/// For manual invalidation, use IMemoryCache to remove specific keys.
/// </para>
/// </remarks>
public sealed class CacheResourceFilter : IAsyncResourceFilter
{
    /// <summary>
    /// Memory cache for storing responses
    /// </summary>
    private readonly IMemoryCache _cache;

    /// <summary>
    /// Logger instance for tracking cache operations
    /// </summary>
    private readonly ILogger<CacheResourceFilter> _logger;

    /// <summary>
    /// Cache duration in seconds
    /// </summary>
    private readonly int _durationSeconds;

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheResourceFilter"/> class
    /// </summary>
    /// <param name="cache">Memory cache for storing responses</param>
    /// <param name="logger">Logger for recording cache operations</param>
    /// <param name="durationSeconds">Cache duration in seconds (default: 60)</param>
    /// <exception cref="ArgumentNullException">Thrown when cache or logger is null</exception>
    /// <exception cref="ArgumentException">Thrown when duration is less than 1</exception>
    public CacheResourceFilter(
        IMemoryCache cache,
        ILogger<CacheResourceFilter> logger,
        int durationSeconds = 60
    )
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (durationSeconds < 1)
        {
            throw new ArgumentException(
                "Duration must be at least 1 second",
                nameof(durationSeconds)
            );
        }

        _durationSeconds = durationSeconds;
    }

    /// <summary>
    /// Executes caching logic for the resource
    /// </summary>
    /// <param name="context">The resource executing context</param>
    /// <param name="next">Delegate to execute the rest of the pipeline</param>
    /// <remarks>
    /// For GET requests, checks cache for existing response. If found, returns cached response.
    /// If not found, executes action and caches the successful response.
    /// Non-GET requests bypass caching entirely.
    /// </remarks>
    public async Task OnResourceExecutionAsync(
        ResourceExecutingContext context,
        ResourceExecutionDelegate next
    )
    {
        // Only cache GET requests
        if (!HttpMethods.IsGet(context.HttpContext.Request.Method))
        {
            await next();
            return;
        }

        var cacheKey = GenerateCacheKey(context.HttpContext.Request);

        // Try to get cached response
        if (_cache.TryGetValue(cacheKey, out object? cachedResult))
        {
            _logger.LogDebug("Cache HIT for key: {CacheKey}", cacheKey);

            context.HttpContext.Response.Headers["X-Cache"] = "HIT";
            context.Result = cachedResult as IActionResult;
            return;
        }

        _logger.LogDebug("Cache MISS for key: {CacheKey}", cacheKey);

        // Execute the action
        var executedContext = await next();

        // Cache successful results only
        if (
            executedContext.Result is ObjectResult objectResult
            && objectResult.StatusCode >= 200
            && objectResult.StatusCode < 300
        )
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_durationSeconds),
            };

            _cache.Set(cacheKey, executedContext.Result, cacheOptions);

            _logger.LogDebug(
                "Cached response for key: {CacheKey}, Duration: {Duration}s",
                cacheKey,
                _durationSeconds
            );

            executedContext.HttpContext.Response.Headers["X-Cache"] = "MISS";
        }
    }

    /// <summary>
    /// Generates a unique cache key from the HTTP request
    /// </summary>
    /// <param name="request">The HTTP request to generate key for</param>
    /// <returns>A unique cache key string</returns>
    /// <remarks>
    /// Combines request path and query string, then creates an MD5 hash for a compact key.
    /// Ensures different parameter combinations generate different cache entries.
    /// </remarks>
    private static string GenerateCacheKey(HttpRequest request)
    {
        var keyBuilder = new StringBuilder();
        keyBuilder.Append(request.Path);

        if (request.QueryString.HasValue)
        {
            keyBuilder.Append(request.QueryString.Value);
        }

        var key = keyBuilder.ToString();

        // Hash the key to keep it short and consistent
        using var md5 = MD5.Create();
        var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(key));
        var hash = Convert.ToBase64String(hashBytes);

        return $"api_cache_{hash}";
    }
}
