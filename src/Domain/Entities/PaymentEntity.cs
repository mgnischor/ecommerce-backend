using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents a payment transaction for an order
/// </summary>
public sealed class PaymentEntity
{
    /// <summary>
    /// Unique identifier for the payment
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Order that this payment is for
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Payment transaction ID from payment provider
    /// </summary>
    public string? TransactionId { get; set; }

    /// <summary>
    /// Payment method used
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.NotSpecified;

    /// <summary>
    /// Current status of the payment
    /// </summary>
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    /// <summary>
    /// Amount paid
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Currency code (e.g., "USD", "BRL", "EUR")
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Payment provider name (e.g., "Stripe", "PayPal", "Mercado Pago")
    /// </summary>
    public string? PaymentProvider { get; set; }

    /// <summary>
    /// Payment provider response (JSON or text)
    /// </summary>
    public string? ProviderResponse { get; set; }

    /// <summary>
    /// Error message if payment failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Date and time when payment was authorized
    /// </summary>
    public DateTime? AuthorizedAt { get; set; }

    /// <summary>
    /// Date and time when payment was captured/completed
    /// </summary>
    public DateTime? CapturedAt { get; set; }

    /// <summary>
    /// Date and time when payment was refunded
    /// </summary>
    public DateTime? RefundedAt { get; set; }

    /// <summary>
    /// Refund amount (if applicable)
    /// </summary>
    public decimal? RefundAmount { get; set; }

    /// <summary>
    /// Refund reason
    /// </summary>
    public string? RefundReason { get; set; }

    /// <summary>
    /// Date and time when the payment was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the payment was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
