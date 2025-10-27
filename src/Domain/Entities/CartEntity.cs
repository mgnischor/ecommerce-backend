namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents a shopping cart for a customer
/// </summary>
public sealed class CartEntity
{
    /// <summary>
    /// Unique identifier for the cart
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Customer who owns this cart
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Session ID for anonymous carts
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// Coupon code applied to cart (if any)
    /// </summary>
    public string? CouponCode { get; set; }

    /// <summary>
    /// Discount amount from coupon
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Whether the cart is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Date and time when the cart expires
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Date and time when the cart was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the cart was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
