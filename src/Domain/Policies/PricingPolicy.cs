namespace ECommerce.Domain.Policies;

/// <summary>
/// Defines business rules for product pricing
/// </summary>
public static class PricingPolicy
{
    private const decimal MinimumPrice = 0.01m;
    private const decimal MaximumDiscountPercentage = 99m;
    private const decimal MaximumPrice = 999999.99m;

    /// <summary>
    /// Validates if a price is within acceptable range
    /// </summary>
    public static bool IsValidPrice(decimal price)
    {
        return price >= MinimumPrice && price <= MaximumPrice;
    }

    /// <summary>
    /// Validates if a discount price is valid compared to the original price
    /// </summary>
    public static bool IsValidDiscountPrice(decimal originalPrice, decimal discountPrice)
    {
        if (discountPrice >= originalPrice)
            return false;

        if (discountPrice < MinimumPrice)
            return false;

        var discountPercentage = CalculateDiscountPercentage(originalPrice, discountPrice);
        return discountPercentage <= MaximumDiscountPercentage;
    }

    /// <summary>
    /// Calculates the discount percentage between original and discount price
    /// </summary>
    public static decimal CalculateDiscountPercentage(decimal originalPrice, decimal discountPrice)
    {
        if (originalPrice <= 0)
            return 0;

        return Math.Round(((originalPrice - discountPrice) / originalPrice) * 100, 2);
    }

    /// <summary>
    /// Calculates the final price after applying a discount percentage
    /// </summary>
    public static decimal ApplyDiscountPercentage(decimal originalPrice, decimal discountPercentage)
    {
        if (discountPercentage < 0 || discountPercentage > MaximumDiscountPercentage)
            throw new ArgumentException(
                $"Discount percentage must be between 0 and {MaximumDiscountPercentage}"
            );

        var discountAmount = originalPrice * (discountPercentage / 100);
        return Math.Round(originalPrice - discountAmount, 2);
    }

    /// <summary>
    /// Determines the effective price (considering discount)
    /// </summary>
    public static decimal GetEffectivePrice(decimal price, decimal? discountPrice)
    {
        if (
            discountPrice.HasValue
            && discountPrice.Value < price
            && discountPrice.Value >= MinimumPrice
        )
            return discountPrice.Value;

        return price;
    }

    /// <summary>
    /// Validates bulk pricing rules
    /// </summary>
    public static bool IsValidBulkPrice(decimal unitPrice, int quantity, decimal totalPrice)
    {
        var expectedTotal = unitPrice * quantity;
        var tolerance = 0.01m; // Allow 1 cent tolerance for rounding

        return Math.Abs(totalPrice - expectedTotal) <= tolerance;
    }
}
