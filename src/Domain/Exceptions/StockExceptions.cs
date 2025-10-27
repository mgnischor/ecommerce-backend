namespace ECommerce.Domain.Exceptions;

/// <summary>
/// Exception thrown when product stock is insufficient
/// </summary>
public sealed class InsufficientStockException : DomainException
{
    public Guid ProductId { get; }
    public int RequestedQuantity { get; }
    public int AvailableQuantity { get; }

    public InsufficientStockException(Guid productId, int requestedQuantity, int availableQuantity)
        : base(
            $"Insufficient stock for product {productId}. Requested: {requestedQuantity}, Available: {availableQuantity}"
        )
    {
        ProductId = productId;
        RequestedQuantity = requestedQuantity;
        AvailableQuantity = availableQuantity;
    }
}

/// <summary>
/// Exception thrown when order exceeds maximum allowed quantity
/// </summary>
public sealed class OrderQuantityExceededException : DomainException
{
    public Guid ProductId { get; }
    public int RequestedQuantity { get; }
    public int MaximumQuantity { get; }

    public OrderQuantityExceededException(
        Guid productId,
        int requestedQuantity,
        int maximumQuantity
    )
        : base(
            $"Order quantity exceeds maximum for product {productId}. Requested: {requestedQuantity}, Maximum: {maximumQuantity}"
        )
    {
        ProductId = productId;
        RequestedQuantity = requestedQuantity;
        MaximumQuantity = maximumQuantity;
    }
}

/// <summary>
/// Exception thrown when a product is out of stock
/// </summary>
public sealed class ProductOutOfStockException : DomainException
{
    public Guid ProductId { get; }
    public string ProductName { get; }

    public ProductOutOfStockException(Guid productId, string productName)
        : base($"Product '{productName}' (ID: {productId}) is out of stock")
    {
        ProductId = productId;
        ProductName = productName;
    }
}
