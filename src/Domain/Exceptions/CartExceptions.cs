namespace ECommerce.Domain.Exceptions;

/// <summary>
/// Exception thrown when cart validation fails
/// </summary>
public sealed class CartValidationException : DomainException
{
    public CartValidationException(string message)
        : base($"Cart validation failed: {message}") { }
}

/// <summary>
/// Exception thrown when a cart has expired
/// </summary>
public sealed class CartExpiredException : DomainException
{
    public Guid CartId { get; }
    public DateTime ExpiredAt { get; }

    public CartExpiredException(Guid cartId, DateTime expiredAt)
        : base($"Cart {cartId} expired at {expiredAt:yyyy-MM-dd HH:mm:ss} UTC")
    {
        CartId = cartId;
        ExpiredAt = expiredAt;
    }
}

/// <summary>
/// Exception thrown when attempting to add an invalid item to cart
/// </summary>
public sealed class InvalidCartItemException : DomainException
{
    public Guid ProductId { get; }

    public InvalidCartItemException(Guid productId, string reason)
        : base($"Cannot add product {productId} to cart: {reason}")
    {
        ProductId = productId;
    }
}

/// <summary>
/// Exception thrown when cart exceeds maximum allowed items
/// </summary>
public sealed class CartLimitExceededException : DomainException
{
    public int CurrentItemCount { get; }
    public int MaximumItems { get; }

    public CartLimitExceededException(int currentItemCount, int maximumItems)
        : base($"Cart contains {currentItemCount} items, exceeding the maximum of {maximumItems}")
    {
        CurrentItemCount = currentItemCount;
        MaximumItems = maximumItems;
    }
}
