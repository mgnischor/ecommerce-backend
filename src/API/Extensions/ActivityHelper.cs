using System.Diagnostics;

namespace ECommerce.API.Extensions;

/// <summary>
/// Helper class for creating custom OpenTelemetry spans and enriching them with tags.
/// Use this class to add distributed tracing to your application logic.
/// </summary>
public static class ActivityHelper
{
    /// <summary>
    /// Starts a new activity (span) with the specified name and kind.
    /// </summary>
    /// <param name="name">The name of the activity/span.</param>
    /// <param name="kind">The kind of activity (default: Internal).</param>
    /// <returns>The created activity or null if not recording.</returns>
    /// <example>
    /// <code>
    /// using var activity = ActivityHelper.StartActivity("ProcessOrder");
    /// activity?.SetTag("order.id", orderId);
    /// activity?.SetTag("order.total", total);
    /// </code>
    /// </example>
    public static Activity? StartActivity(string name, ActivityKind kind = ActivityKind.Internal)
    {
        return OpenTelemetryExtensions.ActivitySource.StartActivity(name, kind);
    }

    /// <summary>
    /// Adds a tag to the current activity if one is active.
    /// </summary>
    /// <param name="key">The tag key.</param>
    /// <param name="value">The tag value.</param>
    public static void AddTag(string key, object? value)
    {
        Activity.Current?.SetTag(key, value);
    }

    /// <summary>
    /// Adds multiple tags to the current activity if one is active.
    /// </summary>
    /// <param name="tags">Dictionary of tags to add.</param>
    public static void AddTags(Dictionary<string, object?> tags)
    {
        var activity = Activity.Current;
        if (activity != null)
        {
            foreach (var tag in tags)
            {
                activity.SetTag(tag.Key, tag.Value);
            }
        }
    }

    /// <summary>
    /// Records an exception in the current activity.
    /// </summary>
    /// <param name="exception">The exception to record.</param>
    public static void RecordException(Exception exception)
    {
        var activity = Activity.Current;
        if (activity != null)
        {
            activity.SetStatus(ActivityStatusCode.Error, exception.Message);

            // Add exception details as tags
            activity.SetTag("exception.type", exception.GetType().FullName);
            activity.SetTag("exception.message", exception.Message);
            activity.SetTag("exception.stacktrace", exception.StackTrace);

            // Add event for exception
            var tags = new ActivityTagsCollection
            {
                { "exception.type", exception.GetType().FullName },
                { "exception.message", exception.Message },
            };
            activity.AddEvent(new ActivityEvent("exception", tags: tags));
        }
    }

    /// <summary>
    /// Adds an event to the current activity.
    /// </summary>
    /// <param name="name">The event name.</param>
    /// <param name="tags">Optional tags for the event.</param>
    public static void AddEvent(string name, ActivityTagsCollection? tags = null)
    {
        Activity.Current?.AddEvent(new ActivityEvent(name, tags: tags));
    }

    /// <summary>
    /// Sets the status of the current activity.
    /// </summary>
    /// <param name="status">The status code.</param>
    /// <param name="description">Optional description.</param>
    public static void SetStatus(ActivityStatusCode status, string? description = null)
    {
        Activity.Current?.SetStatus(status, description);
    }
}
