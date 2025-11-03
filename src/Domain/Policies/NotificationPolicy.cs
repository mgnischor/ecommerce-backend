namespace ECommerce.Domain.Policies;

/// <summary>
/// Defines business rules for notification management
/// </summary>
public static class NotificationPolicy
{
    private const int MaxTitleLength = 200;
    private const int MinTitleLength = 1;
    private const int MaxMessageLength = 2000;
    private const int MinMessageLength = 1;
    private const int MaxPriority = 10;
    private const int MinPriority = 0;
    private const int DefaultPriority = 5;
    private const int HighPriorityThreshold = 7;
    private const int NotificationRetentionDays = 90;
    private const int UnreadNotificationLimit = 100;

    /// <summary>
    /// Validates notification title
    /// </summary>
    public static bool IsValidTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return false;

        return title.Length >= MinTitleLength && title.Length <= MaxTitleLength;
    }

    /// <summary>
    /// Validates notification message
    /// </summary>
    public static bool IsValidMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return false;

        return message.Length >= MinMessageLength && message.Length <= MaxMessageLength;
    }

    /// <summary>
    /// Validates notification priority
    /// </summary>
    public static bool IsValidPriority(int priority)
    {
        return priority >= MinPriority && priority <= MaxPriority;
    }

    /// <summary>
    /// Determines if notification is high priority
    /// </summary>
    public static bool IsHighPriority(int priority)
    {
        return priority >= HighPriorityThreshold;
    }

    /// <summary>
    /// Checks if notification should send email
    /// </summary>
    public static bool ShouldSendEmail(int priority, bool userEmailPreference)
    {
        // Send email for high priority notifications or when user prefers it
        return IsHighPriority(priority) || userEmailPreference;
    }

    /// <summary>
    /// Checks if notification should send push
    /// </summary>
    public static bool ShouldSendPush(int priority, bool userPushPreference)
    {
        // Send push for high priority or when user prefers it
        return IsHighPriority(priority) || userPushPreference;
    }

    /// <summary>
    /// Checks if notification has expired
    /// </summary>
    public static bool IsExpired(DateTime? expiresAt)
    {
        if (!expiresAt.HasValue)
            return false;

        return DateTime.UtcNow > expiresAt.Value;
    }

    /// <summary>
    /// Determines if notification should be auto-deleted
    /// </summary>
    public static bool ShouldAutoDelete(DateTime createdAt, bool isRead)
    {
        var age = (DateTime.UtcNow - createdAt).TotalDays;

        // Delete read notifications older than retention period
        if (isRead && age > NotificationRetentionDays)
            return true;

        // Keep unread notifications longer
        return age > (NotificationRetentionDays * 2);
    }

    /// <summary>
    /// Checks if user has too many unread notifications
    /// </summary>
    public static bool HasTooManyUnreadNotifications(int unreadCount)
    {
        return unreadCount >= UnreadNotificationLimit;
    }

    /// <summary>
    /// Determines notification grouping key
    /// </summary>
    public static string GetGroupingKey(string relatedEntityType, Guid? relatedEntityId)
    {
        if (string.IsNullOrWhiteSpace(relatedEntityType) || !relatedEntityId.HasValue)
            return string.Empty;

        return $"{relatedEntityType}:{relatedEntityId}";
    }

    /// <summary>
    /// Validates notification action URL
    /// </summary>
    public static bool IsValidActionUrl(string? actionUrl)
    {
        if (string.IsNullOrWhiteSpace(actionUrl))
            return true; // Action URL is optional

        // Basic URL validation
        return Uri.TryCreate(actionUrl, UriKind.RelativeOrAbsolute, out _);
    }

    /// <summary>
    /// Determines if notification should batch with similar notifications
    /// </summary>
    public static bool ShouldBatch(string relatedEntityType, DateTime createdAt)
    {
        // Batch certain notification types if created within a short time window
        var batchableTypes = new[] { "Order", "Product", "Review" };

        if (!batchableTypes.Contains(relatedEntityType))
            return false;

        var minutesSinceCreation = (DateTime.UtcNow - createdAt).TotalMinutes;
        return minutesSinceCreation <= 5; // Batch within 5 minutes
    }

    /// <summary>
    /// Calculates notification priority based on type and context
    /// </summary>
    public static int CalculatePriority(string notificationType, Dictionary<string, string>? metadata)
    {
        var priority = notificationType switch
        {
            "System" => 8,
            "Order" => 7,
            "Payment" => 9,
            "Shipment" => 6,
            "Product" => 3,
            "Promotion" => 2,
            "Account" => 7,
            "Review" => 4,
            _ => DefaultPriority,
        };

        // Adjust priority based on metadata
        if (metadata != null && metadata.ContainsKey("urgent") && metadata["urgent"] == "true")
        {
            priority = Math.Min(priority + 2, MaxPriority);
        }

        return priority;
    }
}
