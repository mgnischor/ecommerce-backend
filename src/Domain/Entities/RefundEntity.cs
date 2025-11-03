using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents a refund in the e-commerce system.
/// Handles refund requests and processing for orders or individual items.
/// </summary>
public class RefundEntity
{
    /// <summary>
    /// Unique identifier for the refund
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User who created this refund
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// User who last updated this refund
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// Order associated with this refund
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Customer requesting the refund
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Payment ID to refund to
    /// </summary>
    public Guid? PaymentId { get; set; }

    /// <summary>
    /// Refund reference number
    /// </summary>
    public string RefundNumber { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the refund
    /// </summary>
    public RefundStatus Status { get; set; } = RefundStatus.Requested;

    /// <summary>
    /// Amount to be refunded
    /// </summary>
    public decimal RefundAmount { get; set; }

    /// <summary>
    /// Reason for the refund
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description from customer
    /// </summary>
    public string? CustomerNotes { get; set; }

    /// <summary>
    /// Internal admin notes
    /// </summary>
    public string? AdminNotes { get; set; }

    /// <summary>
    /// List of order item IDs being refunded
    /// </summary>
    public List<Guid> OrderItemIds { get; set; } = new List<Guid>();

    /// <summary>
    /// Whether items need to be returned
    /// </summary>
    public bool RequiresReturn { get; set; } = true;

    /// <summary>
    /// Return tracking number
    /// </summary>
    public string? ReturnTrackingNumber { get; set; }

    /// <summary>
    /// Date when items were returned
    /// </summary>
    public DateTime? ReturnedAt { get; set; }

    /// <summary>
    /// Date when refund was approved
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// User who approved the refund
    /// </summary>
    public Guid? ApprovedBy { get; set; }

    /// <summary>
    /// Date when refund was processed
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Date when refund was completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Rejection reason if applicable
    /// </summary>
    public string? RejectionReason { get; set; }

    /// <summary>
    /// Transaction ID from payment gateway
    /// </summary>
    public string? TransactionId { get; set; }

    /// <summary>
    /// Restocking fee if applicable
    /// </summary>
    public decimal? RestockingFee { get; set; }

    /// <summary>
    /// Whether the refund is deleted (soft delete)
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Date and time when the refund was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the refund was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
