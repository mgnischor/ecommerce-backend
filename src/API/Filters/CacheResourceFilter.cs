using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;

namespace ECommerce.API.Filters;

/// <summary>
/// Resource filter that implements in-memory response caching for GET requests to improve API performance.
/// </summary>
/// <remarks>
/// <para>
/// Provides automatic response caching for HTTP GET requests to reduce database load and improve response times.
/// This filter operates at the resource filter level, executing before model binding and action execution.
/// It caches successful responses (HTTP 2xx status codes) for a configurable duration.
/// </para>
/// <para>
/// <strong>Features:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><strong>Automatic Caching:</strong> Automatically caches successful GET responses without code changes</description></item>
/// <item><description><strong>Smart Cache Keys:</strong> Generates unique MD5-based keys from URL path and query parameters</description></item>
/// <item><description><strong>Configurable Duration:</strong> Set custom cache expiration time per filter instance</description></item>
/// <item><description><strong>GET Only:</strong> Only caches idempotent GET operations, bypassing other HTTP methods</description></item>
/// <item><description><strong>Cache Headers:</strong> Adds X-Cache header (HIT/MISS) for cache status visibility</description></item>
/// <item><description><strong>Status-Based Caching:</strong> Only caches successful responses (2xx status codes)</description></item>
/// <item><description><strong>Memory Efficient:</strong> Uses ASP.NET Core's IMemoryCache with automatic eviction</description></item>
/// </list>
/// <para>
/// <strong>Caching Strategy:</strong>
/// </para>
/// <list type="number">
/// <item><description>Intercepts incoming GET requests before action execution</description></item>
/// <item><description>Generates unique cache key from request path and query string</description></item>
/// <item><description>Returns cached response if available (cache HIT)</description></item>
/// <item><description>Executes action and caches result if cache MISS occurs</description></item>
/// <item><description>Only caches successful responses (status codes 200-299)</description></item>
/// <item><description>Automatically evicts entries after configured duration</description></item>
/// </list>
/// <para>
/// <strong>Usage Examples:</strong>
/// </para>
/// <code>
/// // Apply to specific controller actions:
/// [ServiceFilter(typeof(CacheResourceFilter), Arguments = new object[] { 300 })] // 5 minutes
/// [HttpGet]
/// public async Task&lt;ActionResult&lt;IEnumerable&lt;Product&gt;&gt;&gt; GetProducts()
/// {
///     return Ok(await _productService.GetAllAsync());
/// }
///
/// // Apply to entire controller:
/// [ServiceFilter(typeof(CacheResourceFilter), Arguments = new object[] { 600 })] // 10 minutes
/// [ApiController]
/// [Route("api/[controller]")]
/// public class ProductsController : ControllerBase { }
///
/// // Register as a global filter in Program.cs:
/// builder.Services.AddControllers(options =>
/// {
///     options.Filters.Add(new CacheResourceFilter(cache, logger, 120)); // 2 minutes
/// });
/// </code>
/// <para>
/// <strong>Cache Invalidation:</strong>
/// Cache entries automatically expire after the specified duration. For manual cache invalidation:
/// </para>
/// <code>
/// // Inject IMemoryCache and remove specific entries:
/// _cache.Remove($"api_cache_{hash}");
///
/// // Or clear all cache entries (requires custom implementation):
/// // Implement a cache clearing service that tracks keys
/// </code>
/// <para>
/// <strong>Performance Considerations:</strong>
/// </para>
/// <list type="bullet">
/// <item><description>Best suited for frequently accessed, relatively static data (catalogs, reference data)</description></item>
/// <item><description>Not recommended for user-specific or frequently changing data</description></item>
/// <item><description>Cache duration should balance freshness vs. performance</description></item>
/// <item><description>Monitor memory usage for high-traffic applications</description></item>
/// <item><description>Consider distributed caching (Redis) for multi-server deployments</description></item>
/// </list>
/// <para>
/// <strong>HTTP Headers:</strong>
/// The filter adds the following custom header to responses:
/// </para>
/// <list type="bullet">
/// <item><term>X-Cache: HIT</term><description>Response served from cache</description></item>
/// <item><term>X-Cache: MISS</term><description>Response generated and cached</description></item>
/// </list>
/// <para>
/// <strong>Thread Safety:</strong> This filter is thread-safe as it relies on IMemoryCache which provides
/// thread-safe operations for concurrent access.
/// </para>
/// </remarks>
/// <example>
/// Complete example with service registration:
/// <code>
/// // In Program.cs or Startup.cs:
/// builder.Services.AddMemoryCache();
/// builder.Services.AddScoped&lt;CacheResourceFilter&gt;();
///
/// // In controller:
/// [ApiController]
/// [Route("api/[controller]")]
/// public class CategoriesController : ControllerBase
/// {
///     [HttpGet]
///     [ServiceFilter(typeof(CacheResourceFilter), Arguments = new object[] { 300 })]
///     public async Task&lt;ActionResult&lt;IEnumerable&lt;Category&gt;&gt;&gt; GetCategories()
///     {
///         // This response will be cached for 5 minutes
///         var categories = await _categoryService.GetAllAsync();
///         return Ok(categories);
///     }
///
///     [HttpPost]
///     public async Task&lt;ActionResult&lt;Category&gt;&gt; CreateCategory(Category category)
///     {
///         // POST requests are never cached
///         var created = await _categoryService.CreateAsync(category);
///         return CreatedAtAction(nameof(GetCategory), new { id = created.Id }, created);
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="IAsyncResourceFilter"/>
/// <seealso cref="IMemoryCache"/>
public sealed class CacheResourceFilter : IAsyncResourceFilter
{
    /// <summary>
    /// Memory cache instance for storing and retrieving cached responses.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Uses ASP.NET Core's in-memory cache implementation (<see cref="IMemoryCache"/>) to store
    /// serialized action results. The cache provides automatic memory management and eviction policies
    /// based on memory pressure and expiration settings.
    /// </para>
    /// <para>
    /// <strong>Cache Storage:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Stores <see cref="IActionResult"/> objects as cache values</description></item>
    /// <item><description>Keys are MD5 hashes of request paths and query strings</description></item>
    /// <item><description>Entries expire based on <see cref="_durationSeconds"/> configuration</description></item>
    /// <item><description>Automatic eviction occurs under memory pressure</description></item>
    /// </list>
    /// <para>
    /// <strong>Thread Safety:</strong> IMemoryCache is thread-safe for concurrent access.
    /// </para>
    /// </remarks>
    private readonly IMemoryCache _cache;

    /// <summary>
    /// Logger instance for tracking cache operations and diagnostic information.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used to log cache-related events at various levels:
    /// </para>
    /// <list type="bullet">
    /// <item><term>Debug Level:</term><description>Cache HIT/MISS events with cache keys</description></item>
    /// <item><term>Debug Level:</term><description>Cache entry creation with expiration duration</description></item>
    /// <item><term>Warning Level:</term><description>Cache operation failures (if implemented)</description></item>
    /// </list>
    /// <para>
    /// Log messages include:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Cache key for correlation and debugging</description></item>
    /// <item><description>Cache status (HIT/MISS)</description></item>
    /// <item><description>Cache duration configuration</description></item>
    /// </list>
    /// <para>
    /// <strong>Example Log Output:</strong>
    /// </para>
    /// <code>
    /// DEBUG: Cache HIT for key: api_cache_abc123xyz...
    /// DEBUG: Cache MISS for key: api_cache_def456uvw...
    /// DEBUG: Cached response for key: api_cache_def456uvw..., Duration: 300s
    /// </code>
    /// </remarks>
    private readonly ILogger<CacheResourceFilter> _logger;

    /// <summary>
    /// Cache entry duration in seconds before automatic expiration.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Defines the absolute expiration time for cached responses. After this duration,
    /// cache entries are automatically removed and subsequent requests will be cache MISSes.
    /// </para>
    /// <para>
    /// <strong>Value Range:</strong> Must be at least 1 second (validated in constructor).
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
    /// <term>30-60 seconds</term>
    /// <description>Frequently changing data (stock levels, prices)</description>
    /// </item>
    /// <item>
    /// <term>5-10 minutes</term>
    /// <description>Semi-static data (product catalogs, categories)</description>
    /// </item>
    /// <item>
    /// <term>30-60 minutes</term>
    /// <description>Static reference data (countries, currencies)</description>
    /// </item>
    /// <item>
    /// <term>24+ hours</term>
    /// <description>Rarely changing configuration data</description>
    /// </item>
    /// </list>
    /// <para>
    /// <strong>Expiration Type:</strong> Uses absolute expiration (not sliding), meaning
    /// the entry expires exactly after the specified duration regardless of access patterns.
    /// </para>
    /// </remarks>
    private readonly int _durationSeconds;

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheResourceFilter"/> class with the specified cache duration.
    /// </summary>
    /// <param name="cache">
    /// Memory cache instance for storing and retrieving cached responses.
    /// Must not be <see langword="null"/>.
    /// </param>
    /// <param name="logger">
    /// Logger for recording cache operations and diagnostic information.
    /// Must not be <see langword="null"/>.
    /// </param>
    /// <param name="durationSeconds">
    /// Cache entry duration in seconds before automatic expiration.
    /// Must be at least 1 second. Default is 60 seconds (1 minute).
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="cache"/> is <see langword="null"/>.
    /// -or-
    /// Thrown when <paramref name="logger"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="durationSeconds"/> is less than 1.
    /// Cache duration must be at least 1 second to prevent invalid cache configurations.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This constructor is typically called by the ASP.NET Core dependency injection container
    /// when the filter is applied using <c>[ServiceFilter]</c> attribute. The cache and logger
    /// are injected automatically, while the duration can be specified as an argument.
    /// </para>
    /// <para>
    /// <strong>Dependency Injection Setup:</strong>
    /// </para>
    /// <code>
    /// // In Program.cs:
    /// builder.Services.AddMemoryCache();
    /// builder.Services.AddScoped&lt;CacheResourceFilter&gt;();
    /// </code>
    /// <para>
    /// <strong>Parameter Validation:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Validates that cache is not null (required dependency)</description></item>
    /// <item><description>Validates that logger is not null (required dependency)</description></item>
    /// <item><description>Validates that duration is at least 1 second (minimum sensible value)</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// Apply filter with custom duration:
    /// <code>
    /// // 5 minutes cache duration:
    /// [ServiceFilter(typeof(CacheResourceFilter), Arguments = new object[] { 300 })]
    /// [HttpGet]
    /// public async Task&lt;ActionResult&gt; GetData() { }
    ///
    /// // Use default 60 seconds (omit Arguments parameter):
    /// [ServiceFilter(typeof(CacheResourceFilter))]
    /// [HttpGet]
    /// public async Task&lt;ActionResult&gt; GetOtherData() { }
    ///
    /// // Manual instantiation (not recommended, use DI):
    /// var cache = serviceProvider.GetRequiredService&lt;IMemoryCache&gt;();
    /// var logger = serviceProvider.GetRequiredService&lt;ILogger&lt;CacheResourceFilter&gt;&gt;();
    /// var filter = new CacheResourceFilter(cache, logger, 120); // 2 minutes
    /// </code>
    /// </example>
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
    /// Executes the caching logic for the current resource request.
    /// </summary>
    /// <param name="context">
    /// The resource executing context containing request information and allowing result manipulation.
    /// Provides access to HTTP context, action descriptor, and the ability to short-circuit the pipeline.
    /// </param>
    /// <param name="next">
    /// Delegate to execute the next filter in the pipeline or the action itself.
    /// Returns a <see cref="ResourceExecutedContext"/> containing the action execution result.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method implements the core caching logic and is called automatically by the ASP.NET Core
    /// framework during request processing. It operates at the resource filter level, which executes
    /// before model binding and authorization.
    /// </para>
    /// <para>
    /// <strong>Execution Flow:</strong>
    /// </para>
    /// <list type="number">
    /// <item><description><strong>HTTP Method Check:</strong> Verifies if request is a GET method</description></item>
    /// <item><description><strong>Non-GET Bypass:</strong> If not GET, immediately calls <paramref name="next"/> delegate</description></item>
    /// <item><description><strong>Cache Key Generation:</strong> Creates unique MD5-based key from request path and query</description></item>
    /// <item><description><strong>Cache Lookup:</strong> Attempts to retrieve cached response</description></item>
    /// <item><description><strong>Cache HIT:</strong> If found, returns cached result and adds X-Cache: HIT header</description></item>
    /// <item><description><strong>Cache MISS:</strong> If not found, executes action via <paramref name="next"/> delegate</description></item>
    /// <item><description><strong>Status Code Check:</strong> Validates response has 2xx status code</description></item>
    /// <item><description><strong>Cache Storage:</strong> Stores successful responses with configured expiration</description></item>
    /// <item><description><strong>Header Addition:</strong> Adds X-Cache: MISS header to response</description></item>
    /// </list>
    /// <para>
    /// <strong>Caching Criteria:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><term>HTTP Method:</term><description>Must be GET (idempotent operation)</description></item>
    /// <item><term>Response Type:</term><description>Must be <see cref="ObjectResult"/></description></item>
    /// <item><term>Status Code:</term><description>Must be between 200-299 (successful responses)</description></item>
    /// </list>
    /// <para>
    /// <strong>Non-Cached Scenarios:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>POST, PUT, DELETE, PATCH requests (non-idempotent)</description></item>
    /// <item><description>Responses with error status codes (4xx, 5xx)</description></item>
    /// <item><description>Responses that are not ObjectResult (e.g., FileResult, RedirectResult)</description></item>
    /// <item><description>Explicitly uncacheable responses (custom logic can be added)</description></item>
    /// </list>
    /// <para>
    /// <strong>Cache Headers:</strong>
    /// The method adds custom HTTP headers to indicate cache status:
    /// </para>
    /// <list type="bullet">
    /// <item><term>X-Cache: HIT</term><description>Response was served from cache</description></item>
    /// <item><term>X-Cache: MISS</term><description>Response was generated and cached</description></item>
    /// </list>
    /// <para>
    /// <strong>Performance Impact:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Cache HIT: Bypasses action execution, model binding, and database access</description></item>
    /// <item><description>Cache MISS: Normal execution overhead plus cache storage operation</description></item>
    /// <item><description>MD5 hashing: Negligible performance impact (~microseconds)</description></item>
    /// </list>
    /// <para>
    /// <strong>Thread Safety:</strong> This method is thread-safe and can handle concurrent requests
    /// for the same resource. IMemoryCache handles synchronization internally.
    /// </para>
    /// </remarks>
    /// <example>
    /// Request flow with cache HIT:
    /// <code>
    /// // First request (cache MISS):
    /// GET /api/products?category=electronics
    /// → No cache entry exists
    /// → Executes action
    /// → Stores result in cache with key "api_cache_[hash]"
    /// → Returns response with "X-Cache: MISS"
    ///
    /// // Second request within cache duration (cache HIT):
    /// GET /api/products?category=electronics
    /// → Cache entry found
    /// → Returns cached result immediately
    /// → Returns response with "X-Cache: HIT"
    /// → Action is never executed
    ///
    /// // Different parameters (cache MISS):
    /// GET /api/products?category=books
    /// → Different cache key generated
    /// → No cache entry for this key
    /// → Executes action and caches result
    /// </code>
    /// </example>
    /// <seealso cref="GenerateCacheKey"/>
    /// <seealso cref="HttpMethods.IsGet"/>
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
    /// Generates a unique cache key from the HTTP request path and query string.
    /// </summary>
    /// <param name="request">
    /// The HTTP request to generate a cache key for. Contains path and query string information.
    /// </param>
    /// <returns>
    /// A unique cache key string in the format "api_cache_{hash}" where {hash} is a Base64-encoded
    /// MD5 hash of the request path and query string. The key is URL-safe and consistent for
    /// identical requests.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Creates a deterministic cache key by combining the request path and query string, then
    /// hashing the result using MD5 for compactness and consistency. This ensures that:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Identical requests generate identical keys</description></item>
    /// <item><description>Different parameter values generate different keys</description></item>
    /// <item><description>Parameter order matters (different order = different key)</description></item>
    /// <item><description>Keys are short and memory-efficient</description></item>
    /// </list>
    /// <para>
    /// <strong>Key Generation Process:</strong>
    /// </para>
    /// <list type="number">
    /// <item><description>Concatenates request path (e.g., "/api/products")</description></item>
    /// <item><description>Appends query string if present (e.g., "?category=electronics&amp;page=1")</description></item>
    /// <item><description>Computes MD5 hash of the combined string</description></item>
    /// <item><description>Converts hash to Base64 string for readability</description></item>
    /// <item><description>Prefixes with "api_cache_" for namespace separation</description></item>
    /// </list>
    /// <para>
    /// <strong>Key Format Examples:</strong>
    /// </para>
    /// <code>
    /// // Request: GET /api/products
    /// // Key: api_cache_YWJjMTIz... (hash of "/api/products")
    ///
    /// // Request: GET /api/products?category=electronics
    /// // Key: api_cache_ZGVmNDU2... (hash of "/api/products?category=electronics")
    ///
    /// // Request: GET /api/products?category=electronics&amp;page=2
    /// // Key: api_cache_Z2hpNzg5... (hash of "/api/products?category=electronics&amp;page=2")
    /// </code>
    /// <para>
    /// <strong>Security Considerations:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>MD5 is used for hashing, not cryptographic security</description></item>
    /// <item><description>Hash collisions are extremely rare for typical URLs</description></item>
    /// <item><description>Keys do not expose sensitive query parameters</description></item>
    /// <item><description>Cache key space is effectively unlimited</description></item>
    /// </list>
    /// <para>
    /// <strong>Performance:</strong>
    /// MD5 hashing is fast (microseconds) and has minimal performance impact.
    /// The operation is synchronous and deterministic.
    /// </para>
    /// <para>
    /// <strong>Important Notes:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Query parameter order affects the key (consider sorting for consistency)</description></item>
    /// <item><description>Case-sensitive: "/api/Products" and "/api/products" generate different keys</description></item>
    /// <item><description>Fragments (#hash) are not included in the key (not sent to server)</description></item>
    /// <item><description>Does not consider HTTP headers (Accept, Authorization, etc.)</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// Cache key generation examples:
    /// <code>
    /// var request1 = new HttpRequest { Path = "/api/products" };
    /// var key1 = GenerateCacheKey(request1);
    /// // Result: "api_cache_[base64_hash1]"
    ///
    /// var request2 = new HttpRequest
    /// {
    ///     Path = "/api/products",
    ///     QueryString = "?category=electronics&amp;page=1"
    /// };
    /// var key2 = GenerateCacheKey(request2);
    /// // Result: "api_cache_[base64_hash2]" (different from key1)
    ///
    /// var request3 = new HttpRequest
    /// {
    ///     Path = "/api/products",
    ///     QueryString = "?page=1&amp;category=electronics"  // Different order
    /// };
    /// var key3 = GenerateCacheKey(request3);
    /// // Result: "api_cache_[base64_hash3]" (different from key2 due to order)
    /// </code>
    /// </example>
    /// <seealso cref="MD5"/>
    /// <seealso cref="HttpRequest"/>
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
