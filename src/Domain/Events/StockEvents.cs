namespace ECommerce.Domain.Events;

/// <summary>
/// Event raised when product stock falls below minimum level
/// </summary>
public sealed class LowStockAlertEvent : DomainEvent
{
    public Guid ProductId { get; }
    public string ProductName { get; }
    public int CurrentStock { get; }
    public int MinimumStockLevel { get; }

    public LowStockAlertEvent(
        Guid productId,
        string productName,
        int currentStock,
        int minimumStockLevel
    )
    {
        ProductId = productId;
        ProductName = productName;
        CurrentStock = currentStock;
        MinimumStockLevel = minimumStockLevel;
    }
}

/// <summary>
/// Event raised when a product goes out of stock
/// </summary>
public sealed class ProductOutOfStockEvent : DomainEvent
{
    public Guid ProductId { get; }
    public string ProductName { get; }
    public string Sku { get; }

    public ProductOutOfStockEvent(Guid productId, string productName, string sku)
    {
        ProductId = productId;
        ProductName = productName;
        Sku = sku;
    }
}

/// <summary>
/// Event raised when product stock is replenished
/// </summary>
public sealed class StockReplenishedEvent : DomainEvent
{
    public Guid ProductId { get; }
    public string ProductName { get; }
    public int QuantityAdded { get; }
    public int NewStockLevel { get; }

    public StockReplenishedEvent(
        Guid productId,
        string productName,
        int quantityAdded,
        int newStockLevel
    )
    {
        ProductId = productId;
        ProductName = productName;
        QuantityAdded = quantityAdded;
        NewStockLevel = newStockLevel;
    }
}

/// <summary>
/// Event raised when stock needs to be reserved for an order
/// </summary>
public sealed class StockReservedEvent : DomainEvent
{
    public Guid OrderId { get; }
    public Guid ProductId { get; }
    public int QuantityReserved { get; }
    public int RemainingStock { get; }

    public StockReservedEvent(
        Guid orderId,
        Guid productId,
        int quantityReserved,
        int remainingStock
    )
    {
        OrderId = orderId;
        ProductId = productId;
        QuantityReserved = quantityReserved;
        RemainingStock = remainingStock;
    }
}

/// <summary>
/// Event raised when reserved stock is released (e.g., order cancelled)
/// </summary>
public sealed class StockReleasedEvent : DomainEvent
{
    public Guid OrderId { get; }
    public Guid ProductId { get; }
    public int QuantityReleased { get; }
    public int NewStockLevel { get; }

    public StockReleasedEvent(Guid orderId, Guid productId, int quantityReleased, int newStockLevel)
    {
        OrderId = orderId;
        ProductId = productId;
        QuantityReleased = quantityReleased;
        NewStockLevel = newStockLevel;
    }
}
