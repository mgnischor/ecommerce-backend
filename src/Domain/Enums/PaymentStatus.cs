namespace ECommerce.Domain.Enums;

/// <summary>
/// Represents the status of a payment
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Payment is pending
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Payment is being processed
    /// </summary>
    Processing = 1,

    /// <summary>
    /// Payment was successful
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Payment failed
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Payment was cancelled
    /// </summary>
    Cancelled = 4,

    /// <summary>
    /// Payment was refunded
    /// </summary>
    Refunded = 5,

    /// <summary>
    /// Partial refund was issued
    /// </summary>
    PartiallyRefunded = 6,

    /// <summary>
    /// Payment is on hold for review
    /// </summary>
    OnHold = 7
}
