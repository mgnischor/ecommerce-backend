namespace ECommerce.Domain.Enums;

/// <summary>
/// Represents the type of discount for a coupon
/// </summary>
public enum DiscountType
{
    /// <summary>
    /// Percentage discount (e.g., 10% off)
    /// </summary>
    Percentage = 0,

    /// <summary>
    /// Fixed amount discount (e.g., $10 off)
    /// </summary>
    Fixed = 1,

    /// <summary>
    /// Free shipping
    /// </summary>
    FreeShipping = 2,

    /// <summary>
    /// Buy X get Y free
    /// </summary>
    BuyXGetYFree = 3
}
