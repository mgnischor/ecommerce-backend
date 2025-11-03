namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents a shipping zone in the e-commerce system.
/// Defines geographical areas and their associated shipping rates.
/// </summary>
public class ShippingZoneEntity
{
    /// <summary>
    /// Unique identifier for the shipping zone
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User who created this shipping zone
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// User who last updated this shipping zone
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// Zone name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Zone description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// List of countries included in this zone
    /// </summary>
    public List<string> Countries { get; set; } = new List<string>();

    /// <summary>
    /// List of states/provinces included (country:state format)
    /// </summary>
    public List<string> States { get; set; } = new List<string>();

    /// <summary>
    /// List of postal/zip code patterns (supports wildcards)
    /// </summary>
    public List<string> PostalCodes { get; set; } = new List<string>();

    /// <summary>
    /// Base shipping rate for this zone
    /// </summary>
    public decimal BaseRate { get; set; }

    /// <summary>
    /// Rate per kg
    /// </summary>
    public decimal? RatePerKg { get; set; }

    /// <summary>
    /// Rate per item
    /// </summary>
    public decimal? RatePerItem { get; set; }

    /// <summary>
    /// Free shipping threshold (order amount)
    /// </summary>
    public decimal? FreeShippingThreshold { get; set; }

    /// <summary>
    /// Minimum order amount for this zone
    /// </summary>
    public decimal? MinimumOrderAmount { get; set; }

    /// <summary>
    /// Maximum order amount for this zone
    /// </summary>
    public decimal? MaximumOrderAmount { get; set; }

    /// <summary>
    /// Estimated delivery time in days (minimum)
    /// </summary>
    public int EstimatedDeliveryDaysMin { get; set; } = 1;

    /// <summary>
    /// Estimated delivery time in days (maximum)
    /// </summary>
    public int EstimatedDeliveryDaysMax { get; set; } = 7;

    /// <summary>
    /// Available shipping methods for this zone
    /// </summary>
    public List<string> AvailableShippingMethods { get; set; } = new List<string>();

    /// <summary>
    /// Tax rate for this zone (percentage)
    /// </summary>
    public decimal? TaxRate { get; set; }

    /// <summary>
    /// Priority order (lower numbers have higher priority)
    /// </summary>
    public int Priority { get; set; } = 0;

    /// <summary>
    /// Whether this zone is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether the zone is deleted (soft delete)
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Date and time when the zone was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the zone was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
