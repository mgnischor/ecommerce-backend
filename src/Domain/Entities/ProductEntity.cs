using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents a product in the e-commerce system.
/// </summary>
/// <remarks>
/// This entity stores comprehensive product information including identification,
/// categorization, pricing, inventory levels, and presentation details.
/// Every product is identified by a unique GUID and can have multiple variants,
/// images, and tags for enhanced discoverability and organization.
/// The entity supports soft deletion, featured products, and sale promotions.
/// </remarks>
public class ProductEntity
{
    /// <summary>
    /// Gets or sets the identifier of the user who created this product.
    /// </summary>
    /// <value>A <see cref="Guid"/> referencing the user who created the product.</value>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for this product.
    /// </summary>
    /// <value>A <see cref="Guid"/> that uniquely identifies the product.</value>
    /// <example>7c9e6679-7425-40de-944b-e07fc1f90ae7</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last updated this product.
    /// </summary>
    /// <value>A <see cref="Guid"/> referencing the user who performed the last modification.</value>
    /// <example>8b3c8d7a-9c4e-4f5a-8d3c-9e5f8a7b6c5d</example>
    public Guid UpdatedBy { get; set; }

    /// <summary>
    /// Gets or sets the category classification for this product.
    /// </summary>
    /// <value>
    /// A <see cref="ProductCategory"/> enumeration value defining the product type.
    /// Defaults to <see cref="ProductCategory.General"/>.
    /// Used for categorization, filtering, and navigation in the storefront.
    /// </value>
    public ProductCategory Category { get; set; } = ProductCategory.General;

    /// <summary>
    /// Gets or sets the current status of this product.
    /// </summary>
    /// <value>
    /// A <see cref="ProductStatus"/> enumeration value indicating the product's lifecycle state.
    /// Defaults to <see cref="ProductStatus.Active"/>.
    /// Valid values: Active, Inactive, OutOfStock, Discontinued, etc.
    /// </value>
    public ProductStatus Status { get; set; } = ProductStatus.Active;

    /// <summary>
    /// Gets or sets the product name.
    /// </summary>
    /// <value>A <see cref="string"/> containing the product's display name.</value>
    /// <example>Wireless Bluetooth Headphones, Organic Cotton T-Shirt</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the detailed product description.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> containing the full product description.
    /// May include features, specifications, materials, care instructions, etc.
    /// Supports rich text or HTML formatting depending on implementation.
    /// </value>
    /// <example>Premium wireless headphones with active noise cancellation and 30-hour battery life.</example>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Stock Keeping Unit (SKU) identifier.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> containing the unique SKU code for inventory management.
    /// Must be unique across all products for proper inventory tracking.
    /// </value>
    /// <example>WH-1000XM4-BLK, TS-ORG-COT-M-RED</example>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the brand or manufacturer name.
    /// </summary>
    /// <value>A <see cref="string"/> containing the product's brand name.</value>
    /// <example>Sony, Nike, Apple, Samsung</example>
    public string Brand { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the primary product image URL.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> containing the URL or path to the main product image.
    /// This is the featured image displayed in product listings.
    /// </value>
    /// <example>/images/products/headphones-main.jpg, https://cdn.example.com/products/12345.png</example>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the regular price of the product.
    /// </summary>
    /// <value>
    /// A <see cref="decimal"/> representing the base price in the system's currency.
    /// This is the standard retail price before any discounts.
    /// </value>
    /// <example>299.99</example>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the discounted sale price, if applicable.
    /// </summary>
    /// <value>
    /// A <see cref="decimal"/> representing the sale price,
    /// or <c>null</c> if no discount is currently applied.
    /// When set, this price should be displayed instead of the regular price.
    /// </value>
    /// <example>249.99</example>
    public decimal? DiscountPrice { get; set; }

    /// <summary>
    /// Gets or sets the product weight.
    /// </summary>
    /// <value>
    /// A <see cref="decimal"/> representing the weight in kilograms (kg).
    /// Used for shipping cost calculations and logistics planning.
    /// </value>
    /// <example>0.5, 2.75, 15.0</example>
    public decimal Weight { get; set; }

    /// <summary>
    /// Gets or sets the current quantity available in stock.
    /// </summary>
    /// <value>
    /// An <see cref="int"/> representing the number of units available for sale.
    /// Should be updated when orders are placed or inventory is received.
    /// </value>
    /// <example>150</example>
    public int StockQuantity { get; set; }

    /// <summary>
    /// Gets or sets the minimum stock level threshold.
    /// </summary>
    /// <value>
    /// An <see cref="int"/> representing the minimum quantity before reordering is needed.
    /// Defaults to 0. When stock falls below this level, alerts may be triggered.
    /// </value>
    /// <example>10, 25, 50</example>
    public int MinStockLevel { get; set; } = 0;

    /// <summary>
    /// Gets or sets the maximum quantity allowed per order.
    /// </summary>
    /// <value>
    /// An <see cref="int"/> representing the maximum units a customer can purchase in a single order.
    /// Defaults to 100. Used to prevent bulk buying or ensure fair distribution.
    /// </value>
    /// <example>5, 10, 100</example>
    public int MaxOrderQuantity { get; set; } = 100;

    /// <summary>
    /// Gets or sets a value indicating whether the product is active and available for purchase.
    /// </summary>
    /// <value>
    /// <c>true</c> if the product is active and visible in the storefront;
    /// otherwise, <c>false</c>. Defaults to <c>true</c>.
    /// </value>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the product has been soft-deleted.
    /// </summary>
    /// <value>
    /// <c>true</c> if the product is deleted but retained for historical purposes;
    /// otherwise, <c>false</c>. Defaults to <c>false</c>.
    /// </value>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the product is featured.
    /// </summary>
    /// <value>
    /// <c>true</c> if the product should be highlighted in featured sections or promotions;
    /// otherwise, <c>false</c>. Defaults to <c>false</c>.
    /// Featured products may appear on homepage or special landing pages.
    /// </value>
    public bool IsFeatured { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the product is currently on sale.
    /// </summary>
    /// <value>
    /// <c>true</c> if the product has a sale price or promotional discount;
    /// otherwise, <c>false</c>. Defaults to <c>false</c>.
    /// Typically used in conjunction with <see cref="DiscountPrice"/>.
    /// </value>
    public bool IsOnSale { get; set; } = false;

    /// <summary>
    /// Gets or sets the collection of tags associated with this product.
    /// </summary>
    /// <value>
    /// A <see cref="List{T}"/> of <see cref="string"/> values representing product tags or keywords.
    /// Used for search optimization, filtering, and product discovery.
    /// </value>
    /// <example>["wireless", "bluetooth", "noise-cancelling", "premium"]</example>
    public List<string> Tags { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the collection of additional product image URLs.
    /// </summary>
    /// <value>
    /// A <see cref="List{T}"/> of <see cref="string"/> values containing URLs or paths to product images.
    /// Includes all product photos beyond the primary <see cref="ImageUrl"/>.
    /// </value>
    /// <example>["/images/products/headphones-side.jpg", "/images/products/headphones-detail.jpg"]</example>
    public List<string> Images { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the date and time when this product was created.
    /// </summary>
    /// <value>A <see cref="DateTime"/> in UTC format representing the creation timestamp.</value>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this product was last updated.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> in UTC format representing the last modification timestamp.
    /// Updated whenever any product information is changed.
    /// </value>
    public DateTime UpdatedAt { get; set; }
}
