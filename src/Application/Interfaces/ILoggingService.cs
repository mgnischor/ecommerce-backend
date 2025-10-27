namespace ECommerce.Application.Interfaces;

/// <summary>
/// Interface for logging service to track errors, warnings, and information across the application
/// </summary>
public interface ILoggingService
{
    /// <summary>
    /// Logs an informational message
    /// </summary>
    /// <param name="message">The message to log</param>
    /// <param name="args">Optional message arguments</param>
    void LogInformation(string message, params object[] args);

    /// <summary>
    /// Logs a warning message
    /// </summary>
    /// <param name="message">The warning message to log</param>
    /// <param name="args">Optional message arguments</param>
    void LogWarning(string message, params object[] args);

    /// <summary>
    /// Logs an error message
    /// </summary>
    /// <param name="message">The error message to log</param>
    /// <param name="args">Optional message arguments</param>
    void LogError(string message, params object[] args);

    /// <summary>
    /// Logs an error with exception details
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <param name="message">Additional error message</param>
    /// <param name="args">Optional message arguments</param>
    void LogError(Exception exception, string message, params object[] args);

    /// <summary>
    /// Logs a debug message
    /// </summary>
    /// <param name="message">The debug message to log</param>
    /// <param name="args">Optional message arguments</param>
    void LogDebug(string message, params object[] args);

    /// <summary>
    /// Logs a critical error
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <param name="message">The critical error message</param>
    /// <param name="args">Optional message arguments</param>
    void LogCritical(Exception exception, string message, params object[] args);
}
