using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents a notification in the e-commerce system.
/// Used to inform users about various events and updates.
/// </summary>
public class NotificationEntity
{
    /// <summary>
    /// Unique identifier for the notification
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User who created this notification (system or admin)
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// User who will receive this notification
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Type of notification
    /// </summary>
    public NotificationType Type { get; set; } = NotificationType.System;

    /// <summary>
    /// Notification title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Notification message/content
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// URL to navigate to when notification is clicked
    /// </summary>
    public string? ActionUrl { get; set; }

    /// <summary>
    /// Icon URL or icon name
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Related entity ID (e.g., Order ID, Product ID)
    /// </summary>
    public Guid? RelatedEntityId { get; set; }

    /// <summary>
    /// Related entity type (e.g., "Order", "Product")
    /// </summary>
    public string? RelatedEntityType { get; set; }

    /// <summary>
    /// Whether the notification has been read
    /// </summary>
    public bool IsRead { get; set; } = false;

    /// <summary>
    /// Date when notification was read
    /// </summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// Whether to send email notification
    /// </summary>
    public bool SendEmail { get; set; } = false;

    /// <summary>
    /// Whether email was sent
    /// </summary>
    public bool EmailSent { get; set; } = false;

    /// <summary>
    /// Date when email was sent
    /// </summary>
    public DateTime? EmailSentAt { get; set; }

    /// <summary>
    /// Whether to send push notification
    /// </summary>
    public bool SendPush { get; set; } = false;

    /// <summary>
    /// Whether push notification was sent
    /// </summary>
    public bool PushSent { get; set; } = false;

    /// <summary>
    /// Date when push notification was sent
    /// </summary>
    public DateTime? PushSentAt { get; set; }

    /// <summary>
    /// Priority level (0-10, higher is more important)
    /// </summary>
    public int Priority { get; set; } = 5;

    /// <summary>
    /// Expiration date for the notification
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Additional metadata in JSON format
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }

    /// <summary>
    /// Whether the notification is deleted (soft delete)
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Date and time when the notification was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
