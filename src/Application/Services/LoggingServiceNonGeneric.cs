using ECommerce.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Services;

/// <summary>
/// Non-generic logging service implementation
/// This wraps ILogger for general purpose logging without requiring generic type parameter
/// </summary>
public sealed class LoggingService : ILoggingService
{
    private readonly ILogger _logger;

    public LoggingService(ILoggerFactory loggerFactory)
    {
        _logger =
            loggerFactory?.CreateLogger("ECommerce.Application")
            ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    public void LogInformation(string message, params object[] args)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            _logger.LogInformation(message, args);
        }
    }

    public void LogWarning(string message, params object[] args)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            _logger.LogWarning(message, args);
        }
    }

    public void LogError(string message, params object[] args)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            _logger.LogError(message, args);
        }
    }

    public void LogError(Exception exception, string message, params object[] args)
    {
        if (exception != null && !string.IsNullOrWhiteSpace(message))
        {
            _logger.LogError(exception, message, args);
        }
    }

    public void LogDebug(string message, params object[] args)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            _logger.LogDebug(message, args);
        }
    }

    public void LogCritical(Exception exception, string message, params object[] args)
    {
        if (exception != null && !string.IsNullOrWhiteSpace(message))
        {
            _logger.LogCritical(exception, message, args);
        }
    }
}
