using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents a promotion/campaign in the e-commerce system.
/// Used for marketing campaigns, sales events, and special offers.
/// </summary>
public class PromotionEntity
{
    /// <summary>
    /// Unique identifier for the promotion
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User who created this promotion
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// User who last updated this promotion
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// Promotion name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Promotion description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Type of promotion
    /// </summary>
    public PromotionType Type { get; set; } = PromotionType.PercentageDiscount;

    /// <summary>
    /// Promotion code (optional, for code-based promotions)
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Discount percentage (for percentage-based promotions)
    /// </summary>
    public decimal? DiscountPercentage { get; set; }

    /// <summary>
    /// Discount amount (for fixed amount promotions)
    /// </summary>
    public decimal? DiscountAmount { get; set; }

    /// <summary>
    /// Minimum order amount required
    /// </summary>
    public decimal? MinimumOrderAmount { get; set; }

    /// <summary>
    /// Maximum discount cap
    /// </summary>
    public decimal? MaximumDiscountAmount { get; set; }

    /// <summary>
    /// Start date of the promotion
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// End date of the promotion
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Maximum number of times this promotion can be used in total
    /// </summary>
    public int? MaxUsageCount { get; set; }

    /// <summary>
    /// Current usage count
    /// </summary>
    public int UsageCount { get; set; } = 0;

    /// <summary>
    /// Maximum times a single user can use this promotion
    /// </summary>
    public int? MaxUsagePerUser { get; set; }

    /// <summary>
    /// List of product IDs eligible for this promotion
    /// </summary>
    public List<Guid> EligibleProductIds { get; set; } = new List<Guid>();

    /// <summary>
    /// List of category IDs eligible for this promotion
    /// </summary>
    public List<Guid> EligibleCategoryIds { get; set; } = new List<Guid>();

    /// <summary>
    /// List of user IDs eligible for this promotion (empty = all users)
    /// </summary>
    public List<Guid> EligibleUserIds { get; set; } = new List<Guid>();

    /// <summary>
    /// Priority level (higher priority promotions are applied first)
    /// </summary>
    public int Priority { get; set; } = 0;

    /// <summary>
    /// Whether this promotion can be combined with other promotions
    /// </summary>
    public bool IsCombinable { get; set; } = true;

    /// <summary>
    /// Whether this promotion is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this promotion is featured on homepage
    /// </summary>
    public bool IsFeatured { get; set; } = false;

    /// <summary>
    /// Banner image URL
    /// </summary>
    public string? BannerUrl { get; set; }

    /// <summary>
    /// Terms and conditions
    /// </summary>
    public string? TermsAndConditions { get; set; }

    /// <summary>
    /// Whether the promotion is deleted (soft delete)
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Date and time when the promotion was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the promotion was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
