namespace ECommerce.Domain.Exceptions;

/// <summary>
/// Exception thrown when a product cannot be found
/// </summary>
public sealed class ProductNotFoundException : DomainException
{
    public Guid ProductId { get; }

    public ProductNotFoundException(Guid productId)
        : base($"Product with ID {productId} was not found")
    {
        ProductId = productId;
    }

    public ProductNotFoundException(string sku)
        : base($"Product with SKU '{sku}' was not found") { }
}

/// <summary>
/// Exception thrown when a product is inactive or unavailable
/// </summary>
public sealed class ProductUnavailableException : DomainException
{
    public Guid ProductId { get; }
    public string ProductName { get; }

    public ProductUnavailableException(Guid productId, string productName)
        : base($"Product '{productName}' (ID: {productId}) is currently unavailable")
    {
        ProductId = productId;
        ProductName = productName;
    }
}

/// <summary>
/// Exception thrown when a duplicate SKU is detected
/// </summary>
public sealed class DuplicateSkuException : DomainException
{
    public string Sku { get; }

    public DuplicateSkuException(string sku)
        : base($"A product with SKU '{sku}' already exists")
    {
        Sku = sku;
    }
}
