namespace ECommerce.Domain.Enums;

/// <summary>
/// Represents the type of promotion
/// </summary>
public enum PromotionType
{
    /// <summary>
    /// Percentage discount
    /// </summary>
    PercentageDiscount = 0,

    /// <summary>
    /// Fixed amount discount
    /// </summary>
    FixedAmountDiscount = 1,

    /// <summary>
    /// Buy X get Y free
    /// </summary>
    BuyXGetYFree = 2,

    /// <summary>
    /// Free shipping
    /// </summary>
    FreeShipping = 3,

    /// <summary>
    /// Bundle deal
    /// </summary>
    Bundle = 4,

    /// <summary>
    /// Flash sale
    /// </summary>
    FlashSale = 5,

    /// <summary>
    /// Clearance sale
    /// </summary>
    Clearance = 6,
}
