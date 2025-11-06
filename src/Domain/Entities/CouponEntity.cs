namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents a promotional discount coupon that can be applied to orders.
/// Coupons can provide percentage-based or fixed-amount discounts with various restrictions
/// including date ranges, usage limits, and product/category applicability.
/// </summary>
/// <remarks>
/// Coupons support both broad and targeted discount strategies:
/// <list type="bullet">
/// <item><description>Percentage discounts (e.g., 20% off) with optional maximum discount caps</description></item>
/// <item><description>Fixed amount discounts (e.g., $10 off)</description></item>
/// <item><description>Minimum order requirements</description></item>
/// <item><description>Product or category-specific discounts</description></item>
/// <item><description>Usage limits per customer and overall</description></item>
/// <item><description>Time-bounded validity periods</description></item>
/// </list>
/// </remarks>
/// <example>
/// Example coupon codes: "SAVE20", "FREESHIP", "WELCOME10"
/// </example>
public sealed class CouponEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the coupon.
    /// </summary>
    /// <value>A GUID that uniquely identifies this coupon in the system.</value>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user who created this coupon.
    /// </summary>
    /// <value>The GUID of the administrator or system user who created the coupon.</value>
    /// <remarks>
    /// This property is required and should reference a valid user ID in the system.
    /// Typically used for audit trails and administrative tracking.
    /// </remarks>
    /// <example>a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d</example>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user who last updated this coupon.
    /// </summary>
    /// <value>The GUID of the user who performed the most recent update, or null if never updated.</value>
    /// <remarks>
    /// This value is null when the coupon has never been modified after creation.
    /// Updated automatically when any coupon property changes.
    /// </remarks>
    /// <example>b2c3d4e5-f6a7-5b6c-9d0e-1f2a3b4c5d6e</example>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// Gets or sets the promotional code that customers enter to redeem the discount.
    /// </summary>
    /// <value>
    /// A string representing the coupon code. Must be unique within the system.
    /// Typically alphanumeric, case-insensitive, and between 4-20 characters.
    /// </value>
    /// <remarks>
    /// Code should be memorable and easy to type. Common patterns include:
    /// <list type="bullet">
    /// <item><description>Descriptive codes: "SAVE20", "FREESHIP"</description></item>
    /// <item><description>Event-based: "BLACKFRIDAY", "CYBER2024"</description></item>
    /// <item><description>Welcome codes: "WELCOME10", "NEWUSER"</description></item>
    /// </list>
    /// </remarks>
    /// <example>SAVE20</example>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a human-readable description of the coupon's purpose and terms.
    /// </summary>
    /// <value>A descriptive text explaining what the coupon offers and any special conditions.</value>
    /// <remarks>
    /// This description is typically displayed to administrators and may be shown to customers
    /// during checkout or in promotional materials. Should clearly state the offer and any restrictions.
    /// </remarks>
    /// <example>Get 20% off your entire order. Valid on orders over $50.</example>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of discount this coupon provides.
    /// </summary>
    /// <value>
    /// Must be either "percentage" for percentage-based discounts or "fixed" for fixed-amount discounts.
    /// Default value is "percentage".
    /// </value>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description><b>percentage</b>: DiscountValue represents a percentage (e.g., 20 for 20% off)</description></item>
    /// <item><description><b>fixed</b>: DiscountValue represents a monetary amount (e.g., 10.00 for $10 off)</description></item>
    /// </list>
    /// </remarks>
    /// <example>percentage</example>
    public string DiscountType { get; set; } = "percentage";

    /// <summary>
    /// Gets or sets the numeric value of the discount.
    /// </summary>
    /// <value>
    /// For percentage discounts: a value between 0 and 100 representing the percentage off.
    /// For fixed discounts: a monetary amount representing the absolute discount value.
    /// </value>
    /// <remarks>
    /// Interpretation depends on <see cref="DiscountType"/>:
    /// <list type="bullet">
    /// <item><description>If DiscountType is "percentage": 20 means 20% off</description></item>
    /// <item><description>If DiscountType is "fixed": 10.00 means $10.00 off</description></item>
    /// </list>
    /// Must be a positive value. For percentage discounts, typically should not exceed 100.
    /// </remarks>
    /// <example>20</example>
    public decimal DiscountValue { get; set; }

    /// <summary>
    /// Gets or sets the minimum order subtotal required to apply this coupon.
    /// </summary>
    /// <value>
    /// The minimum monetary amount required in the cart/order before the coupon can be applied.
    /// Null indicates no minimum requirement.
    /// </value>
    /// <remarks>
    /// Used to ensure coupons are only applied to orders meeting a certain value threshold.
    /// The subtotal is typically calculated before shipping and taxes.
    /// </remarks>
    /// <example>50.00</example>
    public decimal? MinimumOrderAmount { get; set; }

    /// <summary>
    /// Gets or sets the maximum discount amount that can be applied (applicable for percentage discounts).
    /// </summary>
    /// <value>
    /// The maximum monetary amount that can be discounted, or null for no cap.
    /// This property is primarily used with percentage-based discounts.
    /// </value>
    /// <remarks>
    /// Prevents excessive discounts on high-value orders. For example, a 20% discount coupon
    /// with MaximumDiscountAmount of $50 will discount at most $50 even if 20% would be $100.
    /// Generally ignored for fixed-amount discount types.
    /// </remarks>
    /// <example>50.00</example>
    public decimal? MaximumDiscountAmount { get; set; }

    /// <summary>
    /// Gets or sets the maximum total number of times this coupon can be used across all customers.
    /// </summary>
    /// <value>
    /// The total usage limit for the coupon, or null for unlimited uses.
    /// Once <see cref="UsageCount"/> reaches this value, the coupon becomes unavailable.
    /// </value>
    /// <remarks>
    /// Used to create limited-time or limited-quantity promotions.
    /// First-come, first-served basis. Consider combining with <see cref="MaxUsagePerCustomer"/>
    /// to prevent single customers from exhausting the entire quota.
    /// </remarks>
    /// <example>1000</example>
    public int? MaxUsageCount { get; set; }

    /// <summary>
    /// Gets or sets the current number of times this coupon has been successfully used.
    /// </summary>
    /// <value>
    /// The count of completed orders where this coupon was applied.
    /// Default value is 0.
    /// </value>
    /// <remarks>
    /// Automatically incremented when an order using this coupon is completed.
    /// Should be compared against <see cref="MaxUsageCount"/> to determine availability.
    /// This counter tracks actual usage, not just coupon code entries.
    /// </remarks>
    /// <example>237</example>
    public int UsageCount { get; set; } = 0;

    /// <summary>
    /// Gets or sets the maximum number of times a single customer can use this coupon.
    /// </summary>
    /// <value>
    /// The per-customer usage limit, or null to allow unlimited uses per customer
    /// (subject to other restrictions like <see cref="MaxUsageCount"/>).
    /// </value>
    /// <remarks>
    /// Prevents abuse by limiting how many times each individual customer can benefit from the coupon.
    /// Requires tracking coupon usage per customer in a separate table or relationship.
    /// Common values are 1 (one-time use per customer) or small numbers like 3-5.
    /// </remarks>
    /// <example>1</example>
    public int? MaxUsagePerCustomer { get; set; }

    /// <summary>
    /// Gets or sets the array of product IDs to which this coupon applies.
    /// </summary>
    /// <value>
    /// An array of product GUIDs, or null if the coupon applies to all products.
    /// Empty array is treated the same as null (applies to all products).
    /// </value>
    /// <remarks>
    /// Use this for product-specific promotions. When specified, the coupon can only be applied
    /// if the cart contains at least one of the listed products.
    /// Mutually considered with <see cref="ApplicableCategoryIds"/> - typically use one or the other.
    /// If both are specified, products matching either condition qualify.
    /// </remarks>
    /// <example>["3fa85f64-5717-4562-b3fc-2c963f66afa6", "4gb96f75-6828-5673-c4gd-3d074g77bgb7"]</example>
    public Guid[]? ApplicableProductIds { get; set; }

    /// <summary>
    /// Gets or sets the array of category IDs to which this coupon applies.
    /// </summary>
    /// <value>
    /// An array of category integer IDs, or null if the coupon applies to all categories.
    /// Empty array is treated the same as null (applies to all categories).
    /// </value>
    /// <remarks>
    /// Use this for category-wide promotions (e.g., "20% off all electronics").
    /// When specified, the coupon applies to products belonging to any of the listed categories.
    /// Mutually considered with <see cref="ApplicableProductIds"/> - typically use one or the other.
    /// If both are specified, products matching either condition qualify.
    /// </remarks>
    /// <example>[1, 5, 12]</example>
    public int[]? ApplicableCategoryIds { get; set; }

    /// <summary>
    /// Gets or sets the start date and time from which the coupon becomes valid.
    /// </summary>
    /// <value>
    /// The UTC timestamp when the coupon becomes active, or null if valid immediately.
    /// </value>
    /// <remarks>
    /// Allows scheduling future promotions. The coupon cannot be used before this date/time.
    /// Should be stored and compared in UTC to avoid timezone issues.
    /// Combine with <see cref="ValidUntil"/> to create time-bounded promotions.
    /// </remarks>
    /// <example>2024-01-01T00:00:00Z</example>
    public DateTime? ValidFrom { get; set; }

    /// <summary>
    /// Gets or sets the end date and time when the coupon expires.
    /// </summary>
    /// <value>
    /// The UTC timestamp when the coupon becomes invalid, or null if valid indefinitely.
    /// </value>
    /// <remarks>
    /// Defines the expiration date for time-limited promotions. The coupon cannot be used after this date/time.
    /// Should be stored and compared in UTC to avoid timezone issues.
    /// Combine with <see cref="ValidFrom"/> to create time-bounded promotions (e.g., holiday sales).
    /// </remarks>
    /// <example>2024-12-31T23:59:59Z</example>
    public DateTime? ValidUntil { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the coupon is currently active and can be used.
    /// </summary>
    /// <value>
    /// <c>true</c> if the coupon is active and available for use; otherwise, <c>false</c>.
    /// Default value is <c>true</c>.
    /// </value>
    /// <remarks>
    /// Provides manual control over coupon availability without deleting it.
    /// Inactive coupons cannot be used even if they meet all other validity criteria
    /// (date range, usage limits, etc.). Useful for temporarily pausing promotions.
    /// </remarks>
    /// <example>true</example>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the coupon has been soft deleted.
    /// </summary>
    /// <value>
    /// <c>true</c> if the coupon is marked as deleted; otherwise, <c>false</c>.
    /// Default value is <c>false</c>.
    /// </value>
    /// <remarks>
    /// Implements soft delete pattern - deleted coupons remain in the database for audit purposes
    /// but should be excluded from normal queries and cannot be used.
    /// Preserves historical data and allows for potential recovery.
    /// Deleted coupons should not appear in active coupon lists or be applicable to orders.
    /// </remarks>
    /// <example>false</example>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Gets or sets the date and time when the coupon was created.
    /// </summary>
    /// <value>
    /// The UTC timestamp of coupon creation. Default value is the current UTC time when the entity is instantiated.
    /// </value>
    /// <remarks>
    /// Automatically set to the current UTC time when the coupon is first created.
    /// Used for audit trails, reporting, and sorting coupons by creation date.
    /// Should not be modified after initial creation.
    /// </remarks>
    /// <example>2024-01-15T10:30:00Z</example>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the coupon was last updated.
    /// </summary>
    /// <value>
    /// The UTC timestamp of the most recent update. Default value is the current UTC time when the entity is instantiated.
    /// </value>
    /// <remarks>
    /// Automatically updated to the current UTC time whenever any property of the coupon is modified.
    /// Used for tracking change history and determining freshness of coupon data.
    /// Initially set to the same value as <see cref="CreatedAt"/>, then updated on each modification.
    /// </remarks>
    /// <example>2024-01-20T14:45:30Z</example>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
