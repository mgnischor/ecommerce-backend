namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents a discount coupon or promo code
/// </summary>
public sealed class CouponEntity
{
    /// <summary>
    /// Unique identifier for the coupon
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User who created this coupon
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// User who last updated this coupon
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// Coupon code (e.g., "SAVE20", "FREESHIP")
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Coupon description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Discount type: "percentage" or "fixed"
    /// </summary>
    public string DiscountType { get; set; } = "percentage";

    /// <summary>
    /// Discount value (percentage or fixed amount)
    /// </summary>
    public decimal DiscountValue { get; set; }

    /// <summary>
    /// Minimum order amount to use this coupon
    /// </summary>
    public decimal? MinimumOrderAmount { get; set; }

    /// <summary>
    /// Maximum discount amount (for percentage discounts)
    /// </summary>
    public decimal? MaximumDiscountAmount { get; set; }

    /// <summary>
    /// Maximum number of times this coupon can be used
    /// </summary>
    public int? MaxUsageCount { get; set; }

    /// <summary>
    /// Current usage count
    /// </summary>
    public int UsageCount { get; set; } = 0;

    /// <summary>
    /// Maximum uses per customer
    /// </summary>
    public int? MaxUsagePerCustomer { get; set; }

    /// <summary>
    /// Specific product IDs this coupon applies to (null = all products)
    /// </summary>
    public Guid[]? ApplicableProductIds { get; set; }

    /// <summary>
    /// Specific category IDs this coupon applies to (null = all categories)
    /// </summary>
    public int[]? ApplicableCategoryIds { get; set; }

    /// <summary>
    /// Start date of coupon validity
    /// </summary>
    public DateTime? ValidFrom { get; set; }

    /// <summary>
    /// End date of coupon validity
    /// </summary>
    public DateTime? ValidUntil { get; set; }

    /// <summary>
    /// Whether the coupon is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether the coupon is deleted (soft delete)
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Date and time when the coupon was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the coupon was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
