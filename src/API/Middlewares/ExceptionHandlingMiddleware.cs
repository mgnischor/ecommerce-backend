using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Middlewares;

/// <summary>
/// Middleware for handling exceptions globally and logging them
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment
    )
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(
            exception,
            "An unhandled exception occurred. Path: {Path}, Method: {Method}",
            context.Request.Path,
            context.Request.Method
        );

        var statusCode = GetStatusCode(exception);
        var isServerError = statusCode >= 500;
        var includeExceptionDetails = _environment.IsDevelopment();

        // For server errors in non-development environments, do not leak the
        // exception message - it may contain sensitive information (SQL, paths, etc.).
        var detail =
            isServerError && !includeExceptionDetails
                ? "An unexpected error occurred while processing your request."
                : exception.Message;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = GetTitle(exception),
            Detail = detail,
            Instance = context.Request.Path,
        };

        if (includeExceptionDetails && isServerError)
        {
            problemDetails.Extensions["exceptionType"] = exception.GetType().Name;
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, options));
    }

    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            ArgumentNullException => (int)HttpStatusCode.BadRequest,
            ArgumentException => (int)HttpStatusCode.BadRequest,
            InvalidOperationException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            _ => (int)HttpStatusCode.InternalServerError,
        };
    }

    private static string GetTitle(Exception exception)
    {
        return exception switch
        {
            ArgumentNullException => "Bad Request",
            ArgumentException => "Bad Request",
            InvalidOperationException => "Bad Request",
            UnauthorizedAccessException => "Unauthorized",
            KeyNotFoundException => "Not Found",
            _ => "Internal Server Error",
        };
    }
}
