namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents a product variant in the e-commerce system.
/// Used for products with multiple options like size, color, material, etc.
/// </summary>
public class ProductVariantEntity
{
    /// <summary>
    /// Unique identifier for the variant
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User who created this variant
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// User who last updated this variant
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// Parent product ID
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Variant name (e.g., "Large - Red", "XL - Blue")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// SKU for this specific variant
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// Barcode/UPC/EAN
    /// </summary>
    public string? Barcode { get; set; }

    /// <summary>
    /// Price for this variant (can differ from base product)
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Discount price if applicable
    /// </summary>
    public decimal? DiscountPrice { get; set; }

    /// <summary>
    /// Stock quantity for this variant
    /// </summary>
    public int StockQuantity { get; set; }

    /// <summary>
    /// Weight of this variant in kg
    /// </summary>
    public decimal? Weight { get; set; }

    /// <summary>
    /// Primary image URL for this variant
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Additional images for this variant
    /// </summary>
    public List<string> Images { get; set; } = new List<string>();

    /// <summary>
    /// Variant attributes (e.g., { "Size": "Large", "Color": "Red" })
    /// </summary>
    public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Whether this variant is the default/primary variant
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Whether this variant is available for purchase
    /// </summary>
    public bool IsAvailable { get; set; } = true;

    /// <summary>
    /// Display order for sorting
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// Whether the variant is deleted (soft delete)
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Date and time when the variant was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the variant was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
