using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents a notification in the e-commerce system.
/// </summary>
/// <remarks>
/// This entity manages user notifications across multiple channels including in-app,
/// email, and push notifications. It tracks delivery status, read status, and includes
/// support for priority levels, expiration, and related entity references.
/// Notifications can be system-generated or manually created by administrators.
/// </remarks>
public class NotificationEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for this notification.
    /// </summary>
    /// <value>A <see cref="Guid"/> that uniquely identifies the notification.</value>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user or system that created this notification.
    /// </summary>
    /// <value>
    /// A <see cref="Guid"/> referencing the creator. This may be a system account GUID
    /// for automated notifications or an admin user ID for manually created notifications.
    /// </value>
    /// <example>7c9e6679-7425-40de-944b-e07fc1f90ae7</example>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who will receive this notification.
    /// </summary>
    /// <value>A <see cref="Guid"/> referencing the recipient user.</value>
    /// <example>8b3c8d7a-9c4e-4f5a-8d3c-9e5f8a7b6c5d</example>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the type classification of this notification.
    /// </summary>
    /// <value>
    /// A <see cref="NotificationType"/> enumeration value categorizing the notification.
    /// Defaults to <see cref="NotificationType.System"/>.
    /// Valid values: System, Order, Payment, Shipping, Marketing, etc.
    /// </value>
    public NotificationType Type { get; set; } = NotificationType.System;

    /// <summary>
    /// Gets or sets the notification title or subject line.
    /// </summary>
    /// <value>A <see cref="string"/> containing the notification headline.</value>
    /// <example>Order Shipped, New Product Available, Payment Received</example>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the detailed notification message content.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> containing the full notification message body.
    /// May include plain text or formatted content depending on the delivery channel.
    /// </value>
    /// <example>Your order #12345 has been shipped and is on its way.</example>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional URL to navigate to when the notification is clicked.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> containing a relative or absolute URL for the call-to-action,
    /// or <c>null</c> if the notification has no associated action.
    /// </value>
    /// <example>/orders/12345, /products/new-arrivals, https://example.com/promo</example>
    public string? ActionUrl { get; set; }

    /// <summary>
    /// Gets or sets the optional icon identifier or URL for visual representation.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> containing an icon name, class, or image URL,
    /// or <c>null</c> for default icon behavior.
    /// </value>
    /// <example>shopping-cart, bell, /icons/truck.svg, fa-envelope</example>
    public string? Icon { get; set; }

    /// <summary>
    /// Gets or sets the optional identifier of the entity related to this notification.
    /// </summary>
    /// <value>
    /// A <see cref="Guid"/> referencing the related entity (order, product, payment, etc.),
    /// or <c>null</c> if no specific entity is associated.
    /// </value>
    /// <example>9d4e5f8b-0d5f-4a6b-9e8d-0f1a2b3c4d5e</example>
    public Guid? RelatedEntityId { get; set; }

    /// <summary>
    /// Gets or sets the optional type name of the related entity.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> indicating the entity type,
    /// or <c>null</c> if no specific entity is associated.
    /// </value>
    /// <example>Order, Product, Payment, Shipment, Review</example>
    public string? RelatedEntityType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the notification has been read by the user.
    /// </summary>
    /// <value>
    /// <c>true</c> if the user has viewed or acknowledged the notification;
    /// otherwise, <c>false</c>. Defaults to <c>false</c>.
    /// </value>
    public bool IsRead { get; set; } = false;

    /// <summary>
    /// Gets or sets the date and time when the notification was read.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> in UTC format representing when the notification was marked as read,
    /// or <c>null</c> if it hasn't been read yet.
    /// </value>
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether an email should be sent for this notification.
    /// </summary>
    /// <value>
    /// <c>true</c> if an email notification should be sent to the user;
    /// otherwise, <c>false</c>. Defaults to <c>false</c>.
    /// </value>
    public bool SendEmail { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the email notification was successfully sent.
    /// </summary>
    /// <value>
    /// <c>true</c> if the email has been sent;
    /// otherwise, <c>false</c>. Defaults to <c>false</c>.
    /// </value>
    public bool EmailSent { get; set; } = false;

    /// <summary>
    /// Gets or sets the date and time when the email notification was sent.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> in UTC format representing when the email was sent,
    /// or <c>null</c> if the email hasn't been sent yet.
    /// </value>
    public DateTime? EmailSentAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a push notification should be sent.
    /// </summary>
    /// <value>
    /// <c>true</c> if a push notification should be sent to the user's devices;
    /// otherwise, <c>false</c>. Defaults to <c>false</c>.
    /// </value>
    public bool SendPush { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the push notification was successfully sent.
    /// </summary>
    /// <value>
    /// <c>true</c> if the push notification has been sent;
    /// otherwise, <c>false</c>. Defaults to <c>false</c>.
    /// </value>
    public bool PushSent { get; set; } = false;

    /// <summary>
    /// Gets or sets the date and time when the push notification was sent.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> in UTC format representing when the push notification was sent,
    /// or <c>null</c> if it hasn't been sent yet.
    /// </value>
    public DateTime? PushSentAt { get; set; }

    /// <summary>
    /// Gets or sets the priority level of this notification.
    /// </summary>
    /// <value>
    /// An <see cref="int"/> between 0 and 10, where higher values indicate greater importance.
    /// Defaults to 5 (medium priority).
    /// Priority affects display order and delivery urgency.
    /// </value>
    /// <example>1 (Low), 5 (Normal), 8 (High), 10 (Critical)</example>
    public int Priority { get; set; } = 5;

    /// <summary>
    /// Gets or sets the optional expiration date for this notification.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> in UTC format representing when the notification expires,
    /// or <c>null</c> if the notification doesn't expire.
    /// Expired notifications may be automatically archived or deleted.
    /// </value>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the optional metadata dictionary for additional custom data.
    /// </summary>
    /// <value>
    /// A <see cref="Dictionary{TKey, TValue}"/> of string key-value pairs containing additional information,
    /// or <c>null</c> if no additional metadata is needed.
    /// Can store custom fields for specific notification types or integrations.
    /// </value>
    /// <example>{"OrderNumber": "12345", "TrackingCode": "ABC123", "ProductCount": "3"}</example>
    public Dictionary<string, string>? Metadata { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this notification has been soft-deleted.
    /// </summary>
    /// <value>
    /// <c>true</c> if the notification is deleted but retained for audit purposes;
    /// otherwise, <c>false</c>. Defaults to <c>false</c>.
    /// </value>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Gets or sets the date and time when this notification was created.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> in UTC format representing the creation timestamp.
    /// Defaults to <see cref="DateTime.UtcNow"/> when the entity is instantiated.
    /// </value>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
