namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents an item within an order
/// </summary>
public sealed class OrderItemEntity
{
    /// <summary>
    /// Unique identifier for the order item
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Order that this item belongs to
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Product being ordered
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Product name (snapshot at time of order)
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Product SKU (snapshot at time of order)
    /// </summary>
    public string ProductSku { get; set; } = string.Empty;

    /// <summary>
    /// Quantity ordered
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Unit price at time of order
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Discount applied to this item
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Tax amount for this item
    /// </summary>
    public decimal TaxAmount { get; set; }

    /// <summary>
    /// Total price for this item (quantity * unitPrice - discount + tax)
    /// </summary>
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// Product image URL (snapshot at time of order)
    /// </summary>
    public string? ProductImageUrl { get; set; }

    /// <summary>
    /// Date and time when the item was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the item was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
