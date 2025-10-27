namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents inventory/stock information for a product
/// </summary>
public sealed class InventoryEntity
{
    /// <summary>
    /// Unique identifier for the inventory record
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Product this inventory is for
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Warehouse or location identifier
    /// </summary>
    public string Location { get; set; } = "Main Warehouse";

    /// <summary>
    /// Current quantity in stock
    /// </summary>
    public int QuantityInStock { get; set; }

    /// <summary>
    /// Quantity reserved (in carts or pending orders)
    /// </summary>
    public int QuantityReserved { get; set; }

    /// <summary>
    /// Quantity available (in stock - reserved)
    /// </summary>
    public int QuantityAvailable { get; set; }

    /// <summary>
    /// Reorder level (minimum stock before reordering)
    /// </summary>
    public int ReorderLevel { get; set; } = 10;

    /// <summary>
    /// Reorder quantity (how much to order when restocking)
    /// </summary>
    public int ReorderQuantity { get; set; } = 50;

    /// <summary>
    /// Last time stock was received
    /// </summary>
    public DateTime? LastStockReceived { get; set; }

    /// <summary>
    /// Last time inventory was counted
    /// </summary>
    public DateTime? LastInventoryCount { get; set; }

    /// <summary>
    /// Date and time when the inventory was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the inventory was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
