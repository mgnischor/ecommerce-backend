namespace ECommerce.Domain.Exceptions;

/// <summary>
/// Exception thrown when an invalid price is provided
/// </summary>
public sealed class InvalidPriceException : DomainException
{
    public decimal Price { get; }

    public InvalidPriceException(decimal price)
        : base($"Invalid price: {price}. Price must be positive and within acceptable range.")
    {
        Price = price;
    }

    public InvalidPriceException(decimal originalPrice, decimal discountPrice)
        : base(
            $"Invalid discount price: {discountPrice}. Discount price must be less than original price: {originalPrice}"
        )
    {
        Price = discountPrice;
    }
}

/// <summary>
/// Exception thrown when a coupon is invalid or cannot be applied
/// </summary>
public sealed class InvalidCouponException : DomainException
{
    public string CouponCode { get; }

    public InvalidCouponException(string couponCode, string reason)
        : base($"Coupon '{couponCode}' cannot be applied: {reason}")
    {
        CouponCode = couponCode;
    }
}

/// <summary>
/// Exception thrown when coupon has been exhausted
/// </summary>
public sealed class CouponExhaustedException : DomainException
{
    public string CouponCode { get; }

    public CouponExhaustedException(string couponCode)
        : base($"Coupon '{couponCode}' has reached its maximum usage limit")
    {
        CouponCode = couponCode;
    }
}

/// <summary>
/// Exception thrown when order amount doesn't meet minimum requirements
/// </summary>
public sealed class MinimumOrderAmountException : DomainException
{
    public decimal CurrentAmount { get; }
    public decimal MinimumAmount { get; }

    public MinimumOrderAmountException(decimal currentAmount, decimal minimumAmount)
        : base(
            $"Order amount ${currentAmount:F2} does not meet minimum requirement of ${minimumAmount:F2}"
        )
    {
        CurrentAmount = currentAmount;
        MinimumAmount = minimumAmount;
    }
}
