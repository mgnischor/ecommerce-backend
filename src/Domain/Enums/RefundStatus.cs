namespace ECommerce.Domain.Enums;

/// <summary>
/// Represents the status of a refund
/// </summary>
public enum RefundStatus
{
    /// <summary>
    /// Refund request has been submitted
    /// </summary>
    Requested = 0,

    /// <summary>
    /// Refund is being reviewed
    /// </summary>
    UnderReview = 1,

    /// <summary>
    /// Refund has been approved
    /// </summary>
    Approved = 2,

    /// <summary>
    /// Refund has been rejected
    /// </summary>
    Rejected = 3,

    /// <summary>
    /// Refund is being processed
    /// </summary>
    Processing = 4,

    /// <summary>
    /// Refund has been completed
    /// </summary>
    Completed = 5,

    /// <summary>
    /// Refund was cancelled
    /// </summary>
    Cancelled = 6,
}
