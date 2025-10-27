using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents a product in the e-commerce system.
/// Every product is identified by a unique GUID.
/// Includes product details, pricing, inventory, status flags, and timestamps.
/// </summary>
public class ProductEntity
{
    public Guid CreatedBy { get; set; }
    public Guid Id { get; set; }
    public Guid UpdatedBy { get; set; }

    public ProductCategory Category { get; set; } = ProductCategory.General;
    public ProductStatus Status { get; set; } = ProductStatus.Active;

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;

    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public decimal Weight { get; set; }

    public int StockQuantity { get; set; }
    public int MinStockLevel { get; set; } = 0;
    public int MaxOrderQuantity { get; set; } = 100;

    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public bool IsFeatured { get; set; } = false;
    public bool IsOnSale { get; set; } = false;

    public List<string> Tags { get; set; } = new List<string>();
    public List<string> Images { get; set; } = new List<string>();

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
