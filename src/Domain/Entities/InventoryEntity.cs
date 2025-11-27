namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents inventory and stock management information for a product in the e-commerce system.
/// </summary>
/// <remarks>
/// This entity tracks real-time inventory levels across different warehouse locations,
/// manages stock reservations, and automates reorder point notifications.
/// Supports multi-location inventory management and stock allocation strategies.
/// The available quantity is calculated as: QuantityInStock - QuantityReserved.
/// </remarks>
public sealed class InventoryEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for this inventory record.
    /// </summary>
    /// <value>A <see cref="Guid"/> that uniquely identifies the inventory record.</value>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the product being tracked.
    /// </summary>
    /// <value>A <see cref="Guid"/> referencing the <see cref="ProductEntity"/>.</value>
    /// <remarks>
    /// A product can have multiple inventory records if stored in different locations.
    /// Each location maintains a separate inventory record for the same product.
    /// </remarks>
    /// <example>7c9e6679-7425-40de-944b-e07fc1f90ae7</example>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Gets or sets the warehouse or storage location identifier.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> identifying the physical location where stock is held.
    /// Defaults to "Main Warehouse".
    /// </value>
    /// <remarks>
    /// Location identifiers should be consistent across the system.
    /// Common patterns: warehouse names, store IDs, or location codes.
    /// Supports multi-location fulfillment and inventory distribution strategies.
    /// </remarks>
    /// <example>Main Warehouse, Store-NYC-001, DC-West-LA, Fulfillment Center A</example>
    public string Location { get; set; } = "Main Warehouse";

    /// <summary>
    /// Gets or sets the total physical quantity currently in stock.
    /// </summary>
    /// <value>
    /// An <see cref="int"/> representing the total units physically present in the location.
    /// Includes both available and reserved quantities.
    /// </value>
    /// <remarks>
    /// This is the actual physical count of items in the warehouse.
    /// Should be updated through inventory transactions (receives, adjustments, sales).
    /// Must always be greater than or equal to <see cref="QuantityReserved"/>.
    /// </remarks>
    /// <example>150</example>
    public int QuantityInStock { get; set; }

    /// <summary>
    /// Gets or sets the quantity reserved for pending orders or shopping carts.
    /// </summary>
    /// <value>
    /// An <see cref="int"/> representing units allocated but not yet shipped.
    /// These units are in stock but committed to specific orders.
    /// </value>
    /// <remarks>
    /// Reserved quantities prevent overselling by:
    /// <list type="bullet">
    /// <item><description>Holding stock for orders being processed</description></item>
    /// <item><description>Allocating inventory for shopping cart items</description></item>
    /// <item><description>Reserving products for in-progress checkouts</description></item>
    /// </list>
    /// Reservations should be released if orders are cancelled or carts expire.
    /// Cannot exceed <see cref="QuantityInStock"/>.
    /// </remarks>
    /// <example>25</example>
    public int QuantityReserved { get; set; }

    /// <summary>
    /// Gets or sets the quantity available for new orders.
    /// </summary>
    /// <value>
    /// An <see cref="int"/> representing units available for immediate sale.
    /// Calculated as: QuantityInStock - QuantityReserved.
    /// </value>
    /// <remarks>
    /// This is the quantity displayed to customers as "in stock" or "available".
    /// Should be calculated automatically based on stock and reservations:
    /// QuantityAvailable = QuantityInStock - QuantityReserved
    /// When this reaches zero, products should be marked as out of stock.
    /// </remarks>
    /// <example>125</example>
    public int QuantityAvailable { get; set; }

    /// <summary>
    /// Gets or sets the minimum stock level that triggers reorder alerts.
    /// </summary>
    /// <value>
    /// An <see cref="int"/> representing the reorder point threshold.
    /// Defaults to 10 units.
    /// </value>
    /// <remarks>
    /// When <see cref="QuantityAvailable"/> falls below this level:
    /// <list type="bullet">
    /// <item><description>Automatic reorder notifications may be triggered</description></item>
    /// <item><description>Procurement teams are alerted to replenish stock</description></item>
    /// <item><description>Low stock warnings may appear to customers</description></item>
    /// </list>
    /// Should be set based on lead time, sales velocity, and safety stock requirements.
    /// Higher values provide more buffer against stockouts.
    /// </remarks>
    /// <example>10</example>
    public int ReorderLevel { get; set; } = 10;

    /// <summary>
    /// Gets or sets the standard quantity to order when restocking.
    /// </summary>
    /// <value>
    /// An <see cref="int"/> representing the economic order quantity or standard reorder amount.
    /// Defaults to 50 units.
    /// </value>
    /// <remarks>
    /// This quantity represents:
    /// <list type="bullet">
    /// <item><description>Economic order quantity (EOQ) for cost optimization</description></item>
    /// <item><description>Standard purchase order size from suppliers</description></item>
    /// <item><description>Minimum order quantities (MOQ) requirements</description></item>
    /// </list>
    /// Should be calculated considering:
    /// supplier MOQs, volume discounts, storage capacity, and demand forecasts.
    /// </remarks>
    /// <example>50</example>
    public int ReorderQuantity { get; set; } = 50;

    /// <summary>
    /// Gets or sets the date and time when stock was last received.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> in UTC format of the most recent stock receipt,
    /// or <c>null</c> if no stock has been received yet.
    /// </value>
    /// <remarks>
    /// Updated when:
    /// <list type="bullet">
    /// <item><description>New inventory arrives from suppliers</description></item>
    /// <item><description>Stock transfers are received from other locations</description></item>
    /// <item><description>Returned items are restocked</description></item>
    /// </list>
    /// Used for tracking inventory age and identifying slow-moving stock.
    /// </remarks>
    public DateTime? LastStockReceived { get; set; }

    /// <summary>
    /// Gets or sets the date and time when physical inventory was last counted.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> in UTC format of the last physical count verification,
    /// or <c>null</c> if never counted.
    /// </value>
    /// <remarks>
    /// Physical inventory counts (stocktakes) are performed to:
    /// <list type="bullet">
    /// <item><description>Verify system quantities match physical stock</description></item>
    /// <item><description>Identify discrepancies due to theft, damage, or errors</description></item>
    /// <item><description>Comply with accounting and audit requirements</description></item>
    /// </list>
    /// Regular counting (monthly, quarterly, annually) helps maintain accuracy.
    /// Cycle counting strategies may focus on high-value or fast-moving items.
    /// </remarks>
    public DateTime? LastInventoryCount { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this inventory record was created.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> in UTC format representing the creation timestamp.
    /// Defaults to <see cref="DateTime.UtcNow"/> when the entity is instantiated.
    /// </value>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when this inventory record was last updated.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> in UTC format representing the last modification timestamp.
    /// Defaults to <see cref="DateTime.UtcNow"/> when the entity is instantiated.
    /// </value>
    /// <remarks>
    /// Updated whenever:
    /// <list type="bullet">
    /// <item><description>Stock quantities change due to sales or receipts</description></item>
    /// <item><description>Reservations are created or released</description></item>
    /// <item><description>Reorder levels or quantities are adjusted</description></item>
    /// <item><description>Physical counts result in adjustments</description></item>
    /// </list>
    /// </remarks>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
