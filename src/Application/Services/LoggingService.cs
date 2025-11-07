using ECommerce.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Services;

/// <summary>
/// Service for centralized logging across the application
/// </summary>
/// <typeparam name="T">The type that is requesting the logger</typeparam>
public sealed class LoggingService<T> : ILoggingService
{
    private readonly ILogger<T> _logger;

    public LoggingService(ILogger<T> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

    public void LogWarning(Exception exception, string message, params object[] args)
    {
        if (exception != null)
        {
            _logger.LogWarning(exception, message, args);
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
        if (exception != null)
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
        if (exception != null)
        {
            _logger.LogCritical(exception, message, args);
        }
    }
}
