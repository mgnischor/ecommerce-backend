using System.Net;
using ECommerce.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ECommerce.API.Filters;

/// <summary>
/// Exception filter that handles domain exceptions and converts them to appropriate HTTP responses
/// </summary>
/// <remarks>
/// <para>
/// Provides centralized exception handling for domain-specific exceptions that occur during action execution.
/// This filter works in conjunction with the global ExceptionHandlingMiddleware to provide comprehensive
/// error handling at the MVC action filter level.
/// </para>
/// <para>
/// <strong>Features:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><strong>Domain Exception Mapping:</strong> Maps domain exceptions to HTTP status codes</description></item>
/// <item><description><strong>Structured Responses:</strong> Returns RFC 7807 ProblemDetails format</description></item>
/// <item><description><strong>Detailed Logging:</strong> Logs exceptions with full context</description></item>
/// <item><description><strong>Client-Friendly:</strong> Provides clear error messages without exposing internals</description></item>
/// </list>
/// <para>
/// <strong>Exception Mapping:</strong>
/// </para>
/// <list type="bullet">
/// <item><description>ProductNotFoundException → 404 Not Found</description></item>
/// <item><description>DuplicateSkuException → 409 Conflict</description></item>
/// <item><description>ProductUnavailableException → 400 Bad Request</description></item>
/// <item><description>Other DomainException → 400 Bad Request</description></item>
/// <item><description>UnauthorizedAccessException → 403 Forbidden</description></item>
/// <item><description>All other exceptions → 500 Internal Server Error</description></item>
/// </list>
/// <para>
/// <strong>Usage:</strong> Apply globally to handle exceptions consistently across all controllers.
/// </para>
/// </remarks>
public sealed class ApiExceptionFilter : IExceptionFilter
{
    /// <summary>
    /// Logger instance for recording exception details
    /// </summary>
    private readonly ILogger<ApiExceptionFilter> _logger;

    /// <summary>
    /// Hosting environment for determining error detail level
    /// </summary>
    private readonly IWebHostEnvironment _environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiExceptionFilter"/> class
    /// </summary>
    /// <param name="logger">Logger for recording exception information</param>
    /// <param name="environment">Hosting environment to check for development mode</param>
    /// <exception cref="ArgumentNullException">Thrown when logger or environment is null</exception>
    public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger, IWebHostEnvironment environment)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    /// <summary>
    /// Handles exceptions that occur during action execution
    /// </summary>
    /// <param name="context">The exception context containing exception details</param>
    /// <remarks>
    /// Maps domain exceptions to appropriate HTTP status codes and creates standardized error responses.
    /// Logs all exceptions with contextual information for monitoring and debugging.
    /// In development mode, includes stack trace in response for debugging purposes.
    /// </remarks>
    public void OnException(ExceptionContext context)
    {
        var statusCode = GetStatusCode(context.Exception);
        var title = GetTitle(context.Exception);

        _logger.LogError(
            context.Exception,
            "Exception occurred in {ActionName}. Status: {StatusCode}, Message: {Message}",
            context.ActionDescriptor.DisplayName,
            statusCode,
            context.Exception.Message
        );

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = context.Exception.Message,
            Instance = context.HttpContext.Request.Path,
        };

        // Include stack trace in development for debugging
        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["stackTrace"] = context.Exception.StackTrace;
        }

        // Add exception type for client-side handling
        problemDetails.Extensions["exceptionType"] = context.Exception.GetType().Name;

        context.Result = new ObjectResult(problemDetails) { StatusCode = statusCode };

        context.ExceptionHandled = true;
    }

    /// <summary>
    /// Maps exception types to HTTP status codes
    /// </summary>
    /// <param name="exception">The exception to map</param>
    /// <returns>The appropriate HTTP status code</returns>
    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            ProductNotFoundException => (int)HttpStatusCode.NotFound,
            DuplicateSkuException => (int)HttpStatusCode.Conflict,
            ProductUnavailableException => (int)HttpStatusCode.BadRequest,
            DomainException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException => (int)HttpStatusCode.Forbidden,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            ArgumentNullException => (int)HttpStatusCode.BadRequest,
            ArgumentException => (int)HttpStatusCode.BadRequest,
            InvalidOperationException => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.InternalServerError,
        };
    }

    /// <summary>
    /// Gets a user-friendly title for the exception
    /// </summary>
    /// <param name="exception">The exception to get title for</param>
    /// <returns>A descriptive title for the error</returns>
    private static string GetTitle(Exception exception)
    {
        return exception switch
        {
            ProductNotFoundException => "Product Not Found",
            DuplicateSkuException => "Duplicate Product SKU",
            ProductUnavailableException => "Product Unavailable",
            DomainException => "Business Rule Violation",
            UnauthorizedAccessException => "Access Forbidden",
            KeyNotFoundException => "Resource Not Found",
            ArgumentNullException => "Invalid Request",
            ArgumentException => "Invalid Request",
            InvalidOperationException => "Invalid Operation",
            _ => "Internal Server Error",
        };
    }
}
