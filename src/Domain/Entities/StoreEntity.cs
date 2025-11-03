namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents a store/branch in the e-commerce system.
/// Supports multi-store/multi-location operations.
/// </summary>
public class StoreEntity
{
    /// <summary>
    /// Unique identifier for the store
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User who created this store
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// User who last updated this store
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// Store name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Store code (unique identifier for internal use)
    /// </summary>
    public string StoreCode { get; set; } = string.Empty;

    /// <summary>
    /// Store description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Store email
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Store phone number
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Store address
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// City
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// State/Province
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Postal code
    /// </summary>
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>
    /// Country
    /// </summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Latitude for map location
    /// </summary>
    public decimal? Latitude { get; set; }

    /// <summary>
    /// Longitude for map location
    /// </summary>
    public decimal? Longitude { get; set; }

    /// <summary>
    /// Store manager user ID
    /// </summary>
    public Guid? ManagerId { get; set; }

    /// <summary>
    /// Store opening hours (JSON format)
    /// </summary>
    public string? OpeningHours { get; set; }

    /// <summary>
    /// Store timezone
    /// </summary>
    public string Timezone { get; set; } = "UTC";

    /// <summary>
    /// Currency used in this store
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Store logo URL
    /// </summary>
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Store image URL
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Whether this is the default/main store
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Whether the store supports pickup orders
    /// </summary>
    public bool SupportsPickup { get; set; } = true;

    /// <summary>
    /// Whether the store supports delivery
    /// </summary>
    public bool SupportsDelivery { get; set; } = true;

    /// <summary>
    /// Whether the store is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Display order for sorting
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// Whether the store is deleted (soft delete)
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Date and time when the store was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the store was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
