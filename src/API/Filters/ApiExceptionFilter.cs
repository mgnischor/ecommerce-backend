using System.Net;
using ECommerce.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ECommerce.API.Filters;

/// <summary>
/// Exception filter that handles domain exceptions and converts them to appropriate HTTP responses.
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
/// <item><description>KeyNotFoundException → 404 Not Found</description></item>
/// <item><description>ArgumentNullException → 400 Bad Request</description></item>
/// <item><description>ArgumentException → 400 Bad Request</description></item>
/// <item><description>InvalidOperationException → 400 Bad Request</description></item>
/// <item><description>All other exceptions → 500 Internal Server Error</description></item>
/// </list>
/// <para>
/// <strong>Response Format:</strong>
/// All error responses follow the RFC 7807 Problem Details specification with the following structure:
/// </para>
/// <code>
/// {
///   "type": "about:blank",
///   "title": "Error Title",
///   "status": 400,
///   "detail": "Detailed error message",
///   "instance": "/api/endpoint",
///   "exceptionType": "ExceptionName"
/// }
/// </code>
/// <para>
/// <strong>Usage:</strong>
/// Apply globally in <c>Program.cs</c> or <c>Startup.cs</c>:
/// </para>
/// <code>
/// services.AddControllers(options =>
/// {
///     options.Filters.Add&lt;ApiExceptionFilter&gt;();
/// });
/// </code>
/// </remarks>
/// <example>
/// Example of a handled exception response for a product not found:
/// <code>
/// HTTP/1.1 404 Not Found
/// Content-Type: application/problem+json
///
/// {
///   "type": "about:blank",
///   "title": "Product Not Found",
///   "status": 404,
///   "detail": "Product with ID '12345' was not found.",
///   "instance": "/api/products/12345",
///   "exceptionType": "ProductNotFoundException"
/// }
/// </code>
/// </example>
public sealed class ApiExceptionFilter : IExceptionFilter
{
    /// <summary>
    /// Logger instance for recording exception details and diagnostic information.
    /// </summary>
    /// <remarks>
    /// Used to log all exceptions with contextual information including:
    /// <list type="bullet">
    /// <item><description>Action name where the exception occurred</description></item>
    /// <item><description>HTTP status code assigned to the error</description></item>
    /// <item><description>Exception message and stack trace</description></item>
    /// <item><description>Request path and other context data</description></item>
    /// </list>
    /// </remarks>
    private readonly ILogger<ApiExceptionFilter> _logger;

    /// <summary>
    /// Hosting environment for determining the current environment (Development, Staging, Production).
    /// </summary>
    /// <remarks>
    /// Used to conditionally include sensitive debugging information such as stack traces
    /// in error responses. Stack traces are only included in Development environments
    /// to prevent exposing internal implementation details in production.
    /// </remarks>
    private readonly IWebHostEnvironment _environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiExceptionFilter"/> class.
    /// </summary>
    /// <param name="logger">
    /// Logger for recording exception information and diagnostic data.
    /// Must not be <see langword="null"/>.
    /// </param>
    /// <param name="environment">
    /// Hosting environment to determine error detail level based on environment (Development/Production).
    /// Must not be <see langword="null"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="logger"/> is <see langword="null"/>.
    /// -or-
    /// Thrown when <paramref name="environment"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// This constructor uses dependency injection to receive required services.
    /// The filter is typically registered in the service collection and applied globally
    /// to all MVC controllers.
    /// </remarks>
    /// <example>
    /// Manual instantiation (not recommended, use DI instead):
    /// <code>
    /// var logger = loggerFactory.CreateLogger&lt;ApiExceptionFilter&gt;();
    /// var environment = app.Environment;
    /// var filter = new ApiExceptionFilter(logger, environment);
    /// </code>
    /// </example>
    public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger, IWebHostEnvironment environment)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    /// <summary>
    /// Handles exceptions that occur during action execution and converts them to standardized HTTP responses.
    /// </summary>
    /// <param name="context">
    /// The exception context containing exception details, HTTP context, and action descriptor.
    /// Provides access to the exception that was thrown and allows setting the result.
    /// </param>
    /// <remarks>
    /// <para>
    /// This method is called automatically by the ASP.NET Core framework when an unhandled exception
    /// occurs during action execution. It performs the following operations:
    /// </para>
    /// <list type="number">
    /// <item><description>Determines the appropriate HTTP status code based on exception type</description></item>
    /// <item><description>Generates a user-friendly error title</description></item>
    /// <item><description>Logs the exception with full contextual information</description></item>
    /// <item><description>Creates an RFC 7807 compliant ProblemDetails response</description></item>
    /// <item><description>Optionally includes stack trace in Development environment</description></item>
    /// <item><description>Adds exception type metadata for client-side handling</description></item>
    /// <item><description>Sets the response and marks the exception as handled</description></item>
    /// </list>
    /// <para>
    /// <strong>Response Structure:</strong>
    /// The method creates a <see cref="ProblemDetails"/> object with the following properties:
    /// </para>
    /// <list type="bullet">
    /// <item><term>Status</term><description>HTTP status code (e.g., 404, 400, 500)</description></item>
    /// <item><term>Title</term><description>Brief, human-readable summary of the error</description></item>
    /// <item><term>Detail</term><description>Detailed error message from the exception</description></item>
    /// <item><term>Instance</term><description>Request path where the error occurred</description></item>
    /// <item><term>Extensions["exceptionType"]</term><description>Type name of the exception</description></item>
    /// <item><term>Extensions["stackTrace"]</term><description>Stack trace (Development only)</description></item>
    /// </list>
    /// <para>
    /// <strong>Thread Safety:</strong> This method is thread-safe and can handle concurrent requests.
    /// </para>
    /// </remarks>
    /// <example>
    /// When a <c>ProductNotFoundException</c> is thrown:
    /// <code>
    /// // In controller action:
    /// throw new ProductNotFoundException("Product with ID '123' was not found");
    ///
    /// // Results in HTTP response:
    /// // Status: 404 Not Found
    /// // Body:
    /// // {
    /// //   "status": 404,
    /// //   "title": "Product Not Found",
    /// //   "detail": "Product with ID '123' was not found",
    /// //   "instance": "/api/products/123",
    /// //   "exceptionType": "ProductNotFoundException"
    /// // }
    /// </code>
    /// </example>
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
    /// Maps exception types to appropriate HTTP status codes following REST conventions.
    /// </summary>
    /// <param name="exception">
    /// The exception to map to an HTTP status code. Must not be <see langword="null"/>.
    /// </param>
    /// <returns>
    /// An HTTP status code as an integer value. Returns one of the following:
    /// <list type="bullet">
    /// <item><description><c>404</c> - Not Found (for ProductNotFoundException, KeyNotFoundException)</description></item>
    /// <item><description><c>409</c> - Conflict (for DuplicateSkuException)</description></item>
    /// <item><description><c>400</c> - Bad Request (for validation and argument exceptions)</description></item>
    /// <item><description><c>403</c> - Forbidden (for UnauthorizedAccessException)</description></item>
    /// <item><description><c>500</c> - Internal Server Error (for unhandled exceptions)</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method uses pattern matching to determine the most appropriate HTTP status code
    /// for each exception type. The mapping follows REST API best practices and HTTP semantics:
    /// </para>
    /// <list type="table">
    /// <listheader>
    /// <term>Exception Type</term>
    /// <description>HTTP Status</description>
    /// </listheader>
    /// <item>
    /// <term>ProductNotFoundException</term>
    /// <description>404 Not Found - Resource does not exist</description>
    /// </item>
    /// <item>
    /// <term>DuplicateSkuException</term>
    /// <description>409 Conflict - Resource already exists</description>
    /// </item>
    /// <item>
    /// <term>ProductUnavailableException</term>
    /// <description>400 Bad Request - Business rule violation</description>
    /// </item>
    /// <item>
    /// <term>DomainException (base)</term>
    /// <description>400 Bad Request - Generic domain error</description>
    /// </item>
    /// <item>
    /// <term>UnauthorizedAccessException</term>
    /// <description>403 Forbidden - Access denied</description>
    /// </item>
    /// <item>
    /// <term>KeyNotFoundException</term>
    /// <description>404 Not Found - Key not found in collection</description>
    /// </item>
    /// <item>
    /// <term>ArgumentNullException</term>
    /// <description>400 Bad Request - Required parameter missing</description>
    /// </item>
    /// <item>
    /// <term>ArgumentException</term>
    /// <description>400 Bad Request - Invalid parameter value</description>
    /// </item>
    /// <item>
    /// <term>InvalidOperationException</term>
    /// <description>400 Bad Request - Operation not allowed in current state</description>
    /// </item>
    /// <item>
    /// <term>All others</term>
    /// <description>500 Internal Server Error - Unexpected error</description>
    /// </item>
    /// </list>
    /// <para>
    /// <strong>Performance:</strong> Uses switch expression for efficient pattern matching.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var exception = new ProductNotFoundException("Product not found");
    /// var statusCode = GetStatusCode(exception); // Returns 404
    ///
    /// var domainEx = new DomainException("Business rule violated");
    /// var domainStatus = GetStatusCode(domainEx); // Returns 400
    /// </code>
    /// </example>
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
    /// Gets a user-friendly, human-readable title for the exception suitable for API responses.
    /// </summary>
    /// <param name="exception">
    /// The exception to generate a title for. Must not be <see langword="null"/>.
    /// </param>
    /// <returns>
    /// A concise, descriptive title string that summarizes the error type without exposing
    /// sensitive implementation details. The title is suitable for display in user interfaces
    /// and API responses. Returns one of the following:
    /// <list type="bullet">
    /// <item><description>"Product Not Found" - for ProductNotFoundException</description></item>
    /// <item><description>"Duplicate Product SKU" - for DuplicateSkuException</description></item>
    /// <item><description>"Product Unavailable" - for ProductUnavailableException</description></item>
    /// <item><description>"Business Rule Violation" - for generic DomainException</description></item>
    /// <item><description>"Access Forbidden" - for UnauthorizedAccessException</description></item>
    /// <item><description>"Resource Not Found" - for KeyNotFoundException</description></item>
    /// <item><description>"Invalid Request" - for argument validation exceptions</description></item>
    /// <item><description>"Invalid Operation" - for InvalidOperationException</description></item>
    /// <item><description>"Internal Server Error" - for unhandled exceptions</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method provides consistent, user-friendly error titles that are suitable for
    /// both end users and API consumers. The titles are designed to:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Be concise and easy to understand</description></item>
    /// <item><description>Avoid exposing internal implementation details</description></item>
    /// <item><description>Provide enough context to understand the error category</description></item>
    /// <item><description>Follow RFC 7807 Problem Details specification</description></item>
    /// <item><description>Maintain consistency across the API</description></item>
    /// </list>
    /// <para>
    /// The titles are mapped using pattern matching for efficient lookup and type safety.
    /// Each title corresponds to the HTTP status code returned by <see cref="GetStatusCode"/>.
    /// </para>
    /// <para>
    /// <strong>Localization:</strong> Consider extending this method to support localization
    /// for international applications by using resource files or localization services.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var exception = new ProductNotFoundException("Product 123 not found");
    /// var title = GetTitle(exception); // Returns "Product Not Found"
    ///
    /// var argumentEx = new ArgumentNullException("productId");
    /// var argTitle = GetTitle(argumentEx); // Returns "Invalid Request"
    ///
    /// var unknownEx = new CustomException("Something went wrong");
    /// var defaultTitle = GetTitle(unknownEx); // Returns "Internal Server Error"
    /// </code>
    /// </example>
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
