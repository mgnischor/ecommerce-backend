using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ECommerce.API.Filters;

/// <summary>
/// Action filter that logs API request and response details for monitoring and debugging
/// </summary>
/// <remarks>
/// <para>
/// Captures and logs comprehensive information about API requests including timing, user identity,
/// parameters, and response status. Useful for performance monitoring, debugging, and audit trails.
/// </para>
/// <para>
/// <strong>Features:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><strong>Request Logging:</strong> Logs HTTP method, path, user, and parameters</description></item>
/// <item><description><strong>Response Logging:</strong> Logs status code and execution time</description></item>
/// <item><description><strong>Performance Tracking:</strong> Measures action execution duration</description></item>
/// <item><description><strong>User Tracking:</strong> Includes authenticated user information</description></item>
/// <item><description><strong>Error Detection:</strong> Highlights slow requests and errors</description></item>
/// </list>
/// <para>
/// <strong>Usage:</strong> Apply globally for comprehensive API monitoring or per-controller for specific tracking.
/// </para>
/// <code>
/// // Global registration in Program.cs:
/// builder.Services.AddControllers(options =>
/// {
///     options.Filters.Add&lt;RequestLoggingFilter&gt;();
/// });
/// </code>
/// </remarks>
public sealed class RequestLoggingFilter : IActionFilter, IResultFilter
{
    /// <summary>
    /// Logger instance for recording request/response information
    /// </summary>
    private readonly ILogger<RequestLoggingFilter> _logger;

    /// <summary>
    /// Context key for storing request start time
    /// </summary>
    private const string StopwatchKey = "RequestStopwatch";

    /// <summary>
    /// Threshold for slow request warning (in milliseconds)
    /// </summary>
    private const int SlowRequestThresholdMs = 3000;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestLoggingFilter"/> class
    /// </summary>
    /// <param name="logger">Logger for recording request/response details</param>
    /// <exception cref="ArgumentNullException">Thrown when logger is null</exception>
    public RequestLoggingFilter(ILogger<RequestLoggingFilter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Logs request details before action execution
    /// </summary>
    /// <param name="context">The action executing context containing request information and action metadata</param>
    /// <remarks>
    /// <para>
    /// This method is called before the action method executes. It performs the following operations:
    /// </para>
    /// <list type="number">
    /// <item><description>Starts a high-precision Stopwatch for performance measurement</description></item>
    /// <item><description>Stores the stopwatch in HttpContext.Items for later retrieval</description></item>
    /// <item><description>Extracts user identity (NameIdentifier claim) or defaults to "Anonymous"</description></item>
    /// <item><description>Extracts user role (Role claim) or defaults to "None"</description></item>
    /// <item><description>Filters out sensitive parameters (password, token, etc.) from logging</description></item>
    /// <item><description>Serializes safe action arguments to JSON for logging</description></item>
    /// <item><description>Logs comprehensive request information at Information level</description></item>
    /// </list>
    /// <para>
    /// <strong>Logged Information:</strong> HTTP method, request path, user ID, user role, action name, and sanitized parameters.
    /// </para>
    /// <para>
    /// <strong>Security Note:</strong> Sensitive parameters are automatically excluded from logs to prevent credential exposure.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if context is null (handled by framework)</exception>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        context.HttpContext.Items[StopwatchKey] = stopwatch;

        var userId =
            context.HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";
        var userRole = context.HttpContext.User?.FindFirst(ClaimTypes.Role)?.Value ?? "None";

        var actionArguments = context
            .ActionArguments.Where(arg => !IsSensitiveParameter(arg.Key))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString() ?? "null");

        _logger.LogInformation(
            "API Request: {Method} {Path} by User {UserId} (Role: {UserRole}) - Action: {ActionName}, Parameters: {Parameters}",
            context.HttpContext.Request.Method,
            context.HttpContext.Request.Path,
            userId,
            userRole,
            context.ActionDescriptor.DisplayName,
            System.Text.Json.JsonSerializer.Serialize(actionArguments)
        );
    }

    /// <summary>
    /// Called after action execution (no-op for this filter)
    /// </summary>
    /// <param name="context">The action executed context containing action result and exception information</param>
    /// <remarks>
    /// <para>
    /// This method is intentionally left empty as timing information is logged in the
    /// <see cref="OnResultExecuted"/> method after the complete response has been generated.
    /// This ensures accurate timing that includes result serialization and execution.
    /// </para>
    /// <para>
    /// Part of the <see cref="IActionFilter"/> interface implementation.
    /// </para>
    /// </remarks>
    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Timing will be logged in OnResultExecuted
    }

    /// <summary>
    /// Called before result execution (no-op for this filter)
    /// </summary>
    /// <param name="context">The result executing context containing the action result about to be executed</param>
    /// <remarks>
    /// <para>
    /// This method is intentionally left empty as no pre-result processing is required.
    /// Response logging occurs in <see cref="OnResultExecuted"/> after the result has been fully executed.
    /// </para>
    /// <para>
    /// Part of the <see cref="IResultFilter"/> interface implementation.
    /// </para>
    /// </remarks>
    public void OnResultExecuting(ResultExecutingContext context)
    {
        // No action needed
    }

    /// <summary>
    /// Logs response details after result execution
    /// </summary>
    /// <param name="context">The result executed context containing the HTTP response and timing information</param>
    /// <remarks>
    /// <para>
    /// This method is called after the action result has been executed and the response has been generated.
    /// It performs comprehensive response logging and performance analysis.
    /// </para>
    /// <para>
    /// <strong>Processing Steps:</strong>
    /// </para>
    /// <list type="number">
    /// <item><description>Retrieves the Stopwatch from HttpContext.Items (stored in <see cref="OnActionExecuting"/>)</description></item>
    /// <item><description>Stops the timer to capture total execution time</description></item>
    /// <item><description>Extracts HTTP status code from the response</description></item>
    /// <item><description>Retrieves authenticated user identity or defaults to "Anonymous"</description></item>
    /// <item><description>Determines appropriate log level based on status code and duration</description></item>
    /// <item><description>Logs response details: method, path, user, status code, and duration</description></item>
    /// <item><description>Issues warning if request exceeds <see cref="SlowRequestThresholdMs"/> threshold</description></item>
    /// </list>
    /// <para>
    /// <strong>Log Levels:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Error:</strong> Status code 500+ (server errors)</description></item>
    /// <item><description><strong>Warning:</strong> Status code 400-499 (client errors) or slow requests (>3000ms)</description></item>
    /// <item><description><strong>Information:</strong> Successful requests (200-399)</description></item>
    /// </list>
    /// </remarks>
    public void OnResultExecuted(ResultExecutedContext context)
    {
        if (context.HttpContext.Items[StopwatchKey] is Stopwatch stopwatch)
        {
            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            var statusCode = context.HttpContext.Response.StatusCode;
            var userId =
                context.HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? "Anonymous";

            var logLevel = GetLogLevel(statusCode, elapsedMs);

            _logger.Log(
                logLevel,
                "API Response: {Method} {Path} by User {UserId} - Status: {StatusCode}, Duration: {DurationMs}ms",
                context.HttpContext.Request.Method,
                context.HttpContext.Request.Path,
                userId,
                statusCode,
                elapsedMs
            );

            // Warn about slow requests
            if (elapsedMs > SlowRequestThresholdMs)
            {
                _logger.LogWarning(
                    "Slow API request detected: {Method} {Path} took {DurationMs}ms (threshold: {ThresholdMs}ms)",
                    context.HttpContext.Request.Method,
                    context.HttpContext.Request.Path,
                    elapsedMs,
                    SlowRequestThresholdMs
                );
            }
        }
    }

    /// <summary>
    /// Determines if a parameter name contains sensitive information that should not be logged
    /// </summary>
    /// <param name="parameterName">The parameter name to check for sensitive keywords</param>
    /// <returns>
    /// <see langword="true"/> if the parameter name contains sensitive keywords (password, token, secret, key, credential, authorization);
    /// <see langword="false"/> otherwise
    /// </returns>
    /// <remarks>
    /// <para>
    /// Performs case-insensitive substring matching against a predefined list of sensitive keywords
    /// to prevent accidental logging of credentials, tokens, or other sensitive data.
    /// </para>
    /// <para>
    /// <strong>Filtered Keywords:</strong> password, token, secret, key, credential, authorization
    /// </para>
    /// <para>
    /// <strong>Security Best Practice:</strong> This helps comply with security standards by preventing
    /// sensitive data from appearing in application logs, which may be stored or transmitted insecurely.
    /// </para>
    /// </remarks>
    /// <example>
    /// Examples of parameter names that will be filtered:
    /// <code>
    /// IsSensitiveParameter("password") // returns true
    /// IsSensitiveParameter("userPassword") // returns true
    /// IsSensitiveParameter("accessToken") // returns true
    /// IsSensitiveParameter("apiKey") // returns true
    /// IsSensitiveParameter("username") // returns false
    /// </code>
    /// </example>
    private static bool IsSensitiveParameter(string parameterName)
    {
        var sensitiveNames = new[]
        {
            "password",
            "token",
            "secret",
            "key",
            "credential",
            "authorization",
        };

        return sensitiveNames.Any(name =>
            parameterName.Contains(name, StringComparison.OrdinalIgnoreCase)
        );
    }

    /// <summary>
    /// Determines the appropriate log level based on HTTP response status code and request duration
    /// </summary>
    /// <param name="statusCode">The HTTP status code returned by the response (e.g., 200, 404, 500)</param>
    /// <param name="elapsedMs">The request execution duration in milliseconds</param>
    /// <returns>
    /// The appropriate <see cref="LogLevel"/> for the request:
    /// <see cref="LogLevel.Error"/> for 5xx status codes,
    /// <see cref="LogLevel.Warning"/> for 4xx status codes or slow requests,
    /// <see cref="LogLevel.Information"/> for successful requests
    /// </returns>
    /// <remarks>
    /// <para>
    /// Uses a priority-based approach to determine the most appropriate log level:
    /// </para>
    /// <list type="number">
    /// <item><description><strong>Priority 1:</strong> Server errors (500+) → <see cref="LogLevel.Error"/></description></item>
    /// <item><description><strong>Priority 2:</strong> Client errors (400-499) → <see cref="LogLevel.Warning"/></description></item>
    /// <item><description><strong>Priority 3:</strong> Slow requests (>{SlowRequestThresholdMs}ms) → <see cref="LogLevel.Warning"/></description></item>
    /// <item><description><strong>Default:</strong> Successful requests → <see cref="LogLevel.Information"/></description></item>
    /// </list>
    /// <para>
    /// This helps operations teams quickly identify critical issues (errors) vs. warnings (client errors, performance issues)
    /// when reviewing logs or setting up alerts.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// GetLogLevel(200, 150) // returns LogLevel.Information
    /// GetLogLevel(404, 100) // returns LogLevel.Warning
    /// GetLogLevel(500, 50) // returns LogLevel.Error
    /// GetLogLevel(200, 3500) // returns LogLevel.Warning (slow request)
    /// </code>
    /// </example>
    private static LogLevel GetLogLevel(int statusCode, long elapsedMs)
    {
        if (statusCode >= 500)
            return LogLevel.Error;
        if (statusCode >= 400)
            return LogLevel.Warning;
        if (elapsedMs > SlowRequestThresholdMs)
            return LogLevel.Warning;

        return LogLevel.Information;
    }
}
