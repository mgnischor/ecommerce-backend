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

/// <summary>
/// Event raised when stock is fulfilled (shipped) for an order
/// </summary>
public sealed class StockFulfilledEvent : DomainEvent
{
    public Guid OrderId { get; }
    public Guid ProductId { get; }
    public string ProductName { get; }
    public int QuantityFulfilled { get; }
    public int RemainingStock { get; }
    public string Location { get; }

    public StockFulfilledEvent(
        Guid orderId,
        Guid productId,
        string productName,
        int quantityFulfilled,
        int remainingStock,
        string location
    )
    {
        OrderId = orderId;
        ProductId = productId;
        ProductName = productName;
        QuantityFulfilled = quantityFulfilled;
        RemainingStock = remainingStock;
        Location = location;
    }
}

/// <summary>
/// Event raised when stock is purchased and added to inventory
/// </summary>
public sealed class StockPurchasedEvent : DomainEvent
{
    public Guid ProductId { get; }
    public string ProductName { get; }
    public int Quantity { get; }
    public decimal UnitCost { get; }
    public decimal TotalCost { get; }
    public string Location { get; }
    public string? DocumentNumber { get; }

    public StockPurchasedEvent(
        Guid productId,
        string productName,
        int quantity,
        decimal unitCost,
        decimal totalCost,
        string location,
        string? documentNumber
    )
    {
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitCost = unitCost;
        TotalCost = totalCost;
        Location = location;
        DocumentNumber = documentNumber;
    }
}

/// <summary>
/// Event raised when stock is transferred between locations
/// </summary>
public sealed class StockTransferredEvent : DomainEvent
{
    public Guid ProductId { get; }
    public string ProductName { get; }
    public int Quantity { get; }
    public string FromLocation { get; }
    public string ToLocation { get; }

    public StockTransferredEvent(
        Guid productId,
        string productName,
        int quantity,
        string fromLocation,
        string toLocation
    )
    {
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        FromLocation = fromLocation;
        ToLocation = toLocation;
    }
}

/// <summary>
/// Event raised when stock is adjusted (physical count correction)
/// </summary>
public sealed class StockAdjustedEvent : DomainEvent
{
    public Guid ProductId { get; }
    public string ProductName { get; }
    public int AdjustmentQuantity { get; }
    public int PreviousStock { get; }
    public int NewStock { get; }
    public string Location { get; }
    public string Reason { get; }

    public StockAdjustedEvent(
        Guid productId,
        string productName,
        int adjustmentQuantity,
        int previousStock,
        int newStock,
        string location,
        string reason
    )
    {
        ProductId = productId;
        ProductName = productName;
        AdjustmentQuantity = adjustmentQuantity;
        PreviousStock = previousStock;
        NewStock = newStock;
        Location = location;
        Reason = reason;
    }
}

/// <summary>
/// Event raised when stock loss is recorded (shrinkage, damage, theft)
/// </summary>
public sealed class StockLossEvent : DomainEvent
{
    public Guid ProductId { get; }
    public string ProductName { get; }
    public int QuantityLost { get; }
    public decimal EstimatedCost { get; }
    public string Location { get; }
    public string Reason { get; }

    public StockLossEvent(
        Guid productId,
        string productName,
        int quantityLost,
        decimal estimatedCost,
        string location,
        string reason
    )
    {
        ProductId = productId;
        ProductName = productName;
        QuantityLost = quantityLost;
        EstimatedCost = estimatedCost;
        Location = location;
        Reason = reason;
    }
}

/// <summary>
/// Event raised when a purchase return is processed
/// </summary>
public sealed class PurchaseReturnedEvent : DomainEvent
{
    public Guid ProductId { get; }
    public string ProductName { get; }
    public int Quantity { get; }
    public decimal UnitCost { get; }
    public string Location { get; }
    public string? Reason { get; }

    public PurchaseReturnedEvent(
        Guid productId,
        string productName,
        int quantity,
        decimal unitCost,
        string location,
        string? reason
    )
    {
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitCost = unitCost;
        Location = location;
        Reason = reason;
    }
}

/// <summary>
/// Event raised when a sale return is processed
/// </summary>
public sealed class SaleReturnedEvent : DomainEvent
{
    public Guid OrderId { get; }
    public Guid ProductId { get; }
    public string ProductName { get; }
    public int Quantity { get; }
    public decimal UnitCost { get; }
    public string Location { get; }

    public SaleReturnedEvent(
        Guid orderId,
        Guid productId,
        string productName,
        int quantity,
        decimal unitCost,
        string location
    )
    {
        OrderId = orderId;
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitCost = unitCost;
        Location = location;
    }
}

/// <summary>
/// Event raised when a transaction requires supervisor approval
/// </summary>
public sealed class TransactionApprovalRequiredEvent : DomainEvent
{
    public Guid TransactionId { get; }
    public string TransactionType { get; }
    public Guid ProductId { get; }
    public int Quantity { get; }
    public decimal TotalCost { get; }
    public string Reason { get; }

    public TransactionApprovalRequiredEvent(
        Guid transactionId,
        string transactionType,
        Guid productId,
        int quantity,
        decimal totalCost,
        string reason
    )
    {
        TransactionId = transactionId;
        TransactionType = transactionType;
        ProductId = productId;
        Quantity = quantity;
        TotalCost = totalCost;
        Reason = reason;
    }
}
