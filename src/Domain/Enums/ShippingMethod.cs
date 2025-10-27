namespace ECommerce.Domain.Enums;

/// <summary>
/// Represents the shipping method for an order
/// </summary>
public enum ShippingMethod
{
    /// <summary>
    /// Not specified
    /// </summary>
    NotSpecified = 0,

    /// <summary>
    /// Standard shipping (5-7 business days)
    /// </summary>
    Standard = 1,

    /// <summary>
    /// Express shipping (2-3 business days)
    /// </summary>
    Express = 2,

    /// <summary>
    /// Next day delivery
    /// </summary>
    NextDay = 3,

    /// <summary>
    /// Same day delivery
    /// </summary>
    SameDay = 4,

    /// <summary>
    /// Store pickup
    /// </summary>
    StorePickup = 5,

    /// <summary>
    /// Free shipping
    /// </summary>
    FreeShipping = 6,

    /// <summary>
    /// International shipping
    /// </summary>
    International = 7
}
