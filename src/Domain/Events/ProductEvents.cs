namespace ECommerce.Domain.Events;

/// <summary>
/// Event raised when a new product is created
/// </summary>
public sealed class ProductCreatedEvent : DomainEvent
{
    public Guid ProductId { get; }
    public string ProductName { get; }
    public string Sku { get; }
    public decimal Price { get; }

    public ProductCreatedEvent(Guid productId, string productName, string sku, decimal price)
    {
        ProductId = productId;
        ProductName = productName;
        Sku = sku;
        Price = price;
    }
}

/// <summary>
/// Event raised when product price is updated
/// </summary>
public sealed class ProductPriceChangedEvent : DomainEvent
{
    public Guid ProductId { get; }
    public string ProductName { get; }
    public decimal OldPrice { get; }
    public decimal NewPrice { get; }

    public ProductPriceChangedEvent(
        Guid productId,
        string productName,
        decimal oldPrice,
        decimal newPrice
    )
    {
        ProductId = productId;
        ProductName = productName;
        OldPrice = oldPrice;
        NewPrice = newPrice;
    }
}

/// <summary>
/// Event raised when a product goes on sale
/// </summary>
public sealed class ProductOnSaleEvent : DomainEvent
{
    public Guid ProductId { get; }
    public string ProductName { get; }
    public decimal OriginalPrice { get; }
    public decimal SalePrice { get; }
    public decimal DiscountPercentage { get; }

    public ProductOnSaleEvent(
        Guid productId,
        string productName,
        decimal originalPrice,
        decimal salePrice,
        decimal discountPercentage
    )
    {
        ProductId = productId;
        ProductName = productName;
        OriginalPrice = originalPrice;
        SalePrice = salePrice;
        DiscountPercentage = discountPercentage;
    }
}

/// <summary>
/// Event raised when a product is discontinued
/// </summary>
public sealed class ProductDiscontinuedEvent : DomainEvent
{
    public Guid ProductId { get; }
    public string ProductName { get; }
    public string Sku { get; }
    public string? Reason { get; }

    public ProductDiscontinuedEvent(
        Guid productId,
        string productName,
        string sku,
        string? reason = null
    )
    {
        ProductId = productId;
        ProductName = productName;
        Sku = sku;
        Reason = reason;
    }
}
