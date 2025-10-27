using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents an order in the e-commerce system
/// </summary>
public sealed class OrderEntity
{
    /// <summary>
    /// Unique identifier for the order
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User who created this order
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// User who last updated this order
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// Customer who placed the order
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Order number (human-readable)
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the order
    /// </summary>
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    /// <summary>
    /// Subtotal amount (sum of all items before tax and shipping)
    /// </summary>
    public decimal SubTotal { get; set; }

    /// <summary>
    /// Tax amount
    /// </summary>
    public decimal TaxAmount { get; set; }

    /// <summary>
    /// Shipping cost
    /// </summary>
    public decimal ShippingCost { get; set; }

    /// <summary>
    /// Discount amount applied
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Total amount (subtotal + tax + shipping - discount)
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Payment method used
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.NotSpecified;

    /// <summary>
    /// Shipping method selected
    /// </summary>
    public ShippingMethod ShippingMethod { get; set; } = ShippingMethod.NotSpecified;

    /// <summary>
    /// Shipping address ID
    /// </summary>
    public Guid? ShippingAddressId { get; set; }

    /// <summary>
    /// Billing address ID
    /// </summary>
    public Guid? BillingAddressId { get; set; }

    /// <summary>
    /// Coupon code used (if any)
    /// </summary>
    public string? CouponCode { get; set; }

    /// <summary>
    /// Customer notes or comments
    /// </summary>
    public string? CustomerNotes { get; set; }

    /// <summary>
    /// Admin notes (internal use only)
    /// </summary>
    public string? AdminNotes { get; set; }

    /// <summary>
    /// Tracking number for shipment
    /// </summary>
    public string? TrackingNumber { get; set; }

    /// <summary>
    /// Expected delivery date
    /// </summary>
    public DateTime? ExpectedDeliveryDate { get; set; }

    /// <summary>
    /// Actual delivery date
    /// </summary>
    public DateTime? DeliveredAt { get; set; }

    /// <summary>
    /// Date when order was cancelled (if applicable)
    /// </summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>
    /// Reason for cancellation
    /// </summary>
    public string? CancellationReason { get; set; }

    /// <summary>
    /// Whether the order is deleted (soft delete)
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Date and time when the order was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the order was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
