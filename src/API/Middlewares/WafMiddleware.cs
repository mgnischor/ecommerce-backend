using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.RegularExpressions;
using Comex.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Comex.API.Middlewares;

/// <summary>
/// Web Application Firewall middleware providing OWASP-style request filtering.
/// </summary>
/// <remarks>
/// <para>
/// Inspects every incoming request and blocks suspicious traffic before it reaches the
/// application pipeline. Rules currently enforced:
/// </para>
/// <list type="bullet">
/// <item><description><strong>IP blocklist:</strong> Requests from configured IPs are rejected with 403</description></item>
/// <item><description><strong>User-agent blocklist:</strong> Known scanner/bot user agents are rejected with 403</description></item>
/// <item><description><strong>SQL injection:</strong> Classic injection payloads in path/query are rejected with 403</description></item>
/// <item><description><strong>Cross-site scripting (XSS):</strong> Script payloads in path/query are rejected with 403</description></item>
/// <item><description><strong>Path traversal:</strong> <c>../</c>, <c>%2e%2e</c> sequences are rejected with 403</description></item>
/// <item><description><strong>Command injection:</strong> Shell metacharacter payloads are rejected with 403</description></item>
/// <item><description><strong>Oversized bodies:</strong> Requests exceeding <c>Waf:MaxRequestBodySizeBytes</c> are rejected with 413</description></item>
/// </list>
/// <para>
/// All blocked requests are logged and recorded through <see cref="ISecurityAuditService"/>.
/// Block counters are exposed for the security dashboard.
/// </para>
/// </remarks>
public sealed class WafMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private static readonly ConcurrentDictionary<string, long> BlockedCounts = new();

    // Ordered rule name -> regex. Anchored and bounded to avoid expensive backtracking.
    private static readonly (string Name, Regex Pattern)[] AttackRules =
    {
        // SQL injection
        (
            "sql_union",
            new Regex(
                @"\bunion\s+(all\s+)?select\b",
                RegexOptions.IgnoreCase | RegexOptions.Compiled
            )
        ),
        (
            "sql_comment",
            new Regex(
                @"\b(select|insert|update|delete|drop|truncate|alter)\b[\s\S]*?--",
                RegexOptions.IgnoreCase | RegexOptions.Compiled
            )
        ),
        (
            "sql_boolean",
            new Regex(
                @"'\s*(\b(or|and)\b\s*)?'\s*=\s*'",
                RegexOptions.IgnoreCase | RegexOptions.Compiled
            )
        ),
        (
            "sql_or_equals",
            new Regex(@"'\s*\b(or|and)\b\s+'?\d", RegexOptions.IgnoreCase | RegexOptions.Compiled)
        ),
        (
            "sql_sleep",
            new Regex(
                @"\b(pg_sleep|sleep|benchmark)\s*\(",
                RegexOptions.IgnoreCase | RegexOptions.Compiled
            )
        ),
        ("sql_stacked", new Regex(@";\s*--\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled)),
        // XSS
        (
            "xss_script_tag",
            new Regex(@"<\s*/?\s*script[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled)
        ),
        (
            "xss_javascript",
            new Regex(@"\bjavascript\s*:", RegexOptions.IgnoreCase | RegexOptions.Compiled)
        ),
        (
            "xss_event_handler",
            new Regex(
                @"\bon(load|error|click|mouseover|focus|blur|change)\s*=\s*['""]?[^>]*",
                RegexOptions.IgnoreCase | RegexOptions.Compiled
            )
        ),
        // Path traversal
        (
            "path_traversal",
            new Regex(
                @"(\.\.[/\\]|\.\.%2f|\.\.%5c|%2e%2e[/\\])",
                RegexOptions.IgnoreCase | RegexOptions.Compiled
            )
        ),
        // Command injection
        (
            "cmd_chain",
            new Regex(
                @"(;|&&|\|\|)\s*(cmd\.exe|powershell|sh\b|bash\b|whoami|wget\b|curl\b|rm\s+-|cat\s+/etc/passwd)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled
            )
        ),
        (
            "cmd_shell_var",
            new Regex(@"\$\{IFS\}|\$\([^)]*\)", RegexOptions.IgnoreCase | RegexOptions.Compiled)
        ),
    };

    private static readonly string[] DefaultBlockedUserAgents =
    {
        "sqlmap",
        "nikto",
        "nmap",
        "masscan",
        "nessus",
        "acunetix",
        "metasploit",
        "wpscan",
        "dirbuster",
        "gobuster",
        "joomscan",
        "zgrab",
        "nuclei",
        "hydra",
        "whatweb",
        "fimap",
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<WafMiddleware> _logger;
    private readonly bool _enabled;
    private readonly long _maxRequestBodySizeBytes;
    private readonly HashSet<string> _blockedIpAddresses;
    private readonly HashSet<string> _blockedUserAgents;

    public WafMiddleware(
        RequestDelegate next,
        ILogger<WafMiddleware> logger,
        IConfiguration configuration
    )
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _enabled = configuration.GetValue("Waf:Enabled", true);
        _maxRequestBodySizeBytes = configuration.GetValue(
            "Waf:MaxRequestBodySizeBytes",
            10L * 1024 * 1024
        );
        _blockedIpAddresses = ParseList(configuration["Waf:BlockedIpAddresses"]);
        _blockedUserAgents = ParseList(configuration["Waf:BlockedUserAgents"]);
        foreach (var agent in DefaultBlockedUserAgents)
        {
            _blockedUserAgents.Add(agent);
        }
    }

    public async Task InvokeAsync(HttpContext context, ISecurityAuditService auditService)
    {
        if (!_enabled)
        {
            await _next(context);
            return;
        }

        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        var userAgent = context.Request.Headers.UserAgent.ToString();
        var path = context.Request.Path.Value ?? string.Empty;
        var query = context.Request.QueryString.Value ?? string.Empty;

        // Match against both the raw and the URL-decoded forms so encoded payloads
        // (e.g. %3Cscript%3E) are also caught.
        var targets = new[] { path + query, path + Uri.UnescapeDataString(query) };

        // 1. IP blocklist
        if (ipAddress is not null && _blockedIpAddresses.Contains(ipAddress))
        {
            await BlockAsync(
                context,
                auditService,
                rule: "blocked_ip",
                statusCode: StatusCodes.Status403Forbidden,
                message: "Forbidden",
                ipAddress,
                userAgent,
                context.Request.Path
            );
            return;
        }

        // 2. User-agent blocklist (known scanners / bots)
        if (!string.IsNullOrWhiteSpace(userAgent) && IsBlockedUserAgent(userAgent))
        {
            await BlockAsync(
                context,
                auditService,
                rule: "blocked_user_agent",
                statusCode: StatusCodes.Status403Forbidden,
                message: "Forbidden",
                ipAddress,
                userAgent,
                context.Request.Path
            );
            return;
        }

        // 3. Oversized request body
        if (
            _maxRequestBodySizeBytes > 0
            && context.Request.ContentLength.HasValue
            && context.Request.ContentLength.Value > _maxRequestBodySizeBytes
        )
        {
            await BlockAsync(
                context,
                auditService,
                rule: "oversized_body",
                statusCode: StatusCodes.Status413PayloadTooLarge,
                message: "Request body too large",
                ipAddress,
                userAgent,
                context.Request.Path
            );
            return;
        }

        // 4. Attack payload patterns in path and query string
        foreach (var rule in AttackRules)
        {
            foreach (var target in targets)
            {
                if (rule.Pattern.IsMatch(target))
                {
                    await BlockAsync(
                        context,
                        auditService,
                        rule: rule.Name,
                        statusCode: StatusCodes.Status403Forbidden,
                        message: "Forbidden",
                        ipAddress,
                        userAgent,
                        context.Request.Path
                    );
                    return;
                }
            }
        }

        await _next(context);
    }

    /// <summary>
    /// Returns the current WAF block counters (rule name -> number of blocked requests).
    /// </summary>
    public static IReadOnlyDictionary<string, long> GetBlockedStats() =>
        BlockedCounts
            .OrderByDescending(kvp => kvp.Value)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    private async Task BlockAsync(
        HttpContext context,
        ISecurityAuditService auditService,
        string rule,
        int statusCode,
        string message,
        string? ipAddress,
        string? userAgent,
        PathString path
    )
    {
        BlockedCounts.AddOrUpdate(rule, 1, (_, count) => count + 1);

        _logger.LogWarning(
            "WAF blocked request. Rule: {Rule}, IP: {Ip}, Method: {Method}, Path: {Path}, Status: {Status}",
            rule,
            ipAddress ?? "unknown",
            context.Request.Method,
            path,
            statusCode
        );

        auditService.Record(
            category: "Waf",
            severity: "Warning",
            message: $"Request blocked by WAF rule '{rule}'",
            ipAddress,
            userAgent,
            path
        );

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = message,
            Detail = $"Request blocked by security policy ({rule}).",
            Instance = path,
        };

        await context.Response.WriteAsJsonAsync(problemDetails, JsonOptions);
    }

    private bool IsBlockedUserAgent(string userAgent)
    {
        return _blockedUserAgents.Any(blocked =>
            userAgent.Contains(blocked, StringComparison.OrdinalIgnoreCase)
        );
    }

    private static HashSet<string> ParseList(string? value)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(value))
        {
            return set;
        }

        foreach (var item in value.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = item.Trim();
            if (trimmed.Length > 0)
            {
                set.Add(trimmed);
            }
        }

        return set;
    }
}
