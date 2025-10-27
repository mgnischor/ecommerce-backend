namespace ECommerce.Domain.Policies;

/// <summary>
/// Defines business rules for coupon validation and application
/// </summary>
public static class CouponValidationPolicy
{
    private const int MaxCodeLength = 50;
    private const int MinCodeLength = 3;
    private const decimal MaxDiscountPercentage = 100m;
    private const decimal MaxDiscountAmount = 999999.99m;

    /// <summary>
    /// Validates if a coupon code format is acceptable
    /// </summary>
    public static bool IsValidCouponCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        if (code.Length < MinCodeLength || code.Length > MaxCodeLength)
            return false;

        // Code should contain only alphanumeric characters and hyphens
        return code.All(c => char.IsLetterOrDigit(c) || c == '-');
    }

    /// <summary>
    /// Validates if a coupon is currently active
    /// </summary>
    public static bool IsActive(bool isActive, bool isDeleted)
    {
        return isActive && !isDeleted;
    }

    /// <summary>
    /// Validates if a coupon is within its validity period
    /// </summary>
    public static bool IsWithinValidityPeriod(DateTime? validFrom, DateTime? validUntil)
    {
        var now = DateTime.UtcNow;

        if (validFrom.HasValue && now < validFrom.Value)
            return false;

        if (validUntil.HasValue && now > validUntil.Value)
            return false;

        return true;
    }

    /// <summary>
    /// Validates if coupon has not exceeded maximum usage count
    /// </summary>
    public static bool HasUsageRemaining(int currentUsageCount, int? maxUsageCount)
    {
        if (!maxUsageCount.HasValue)
            return true;

        return currentUsageCount < maxUsageCount.Value;
    }

    /// <summary>
    /// Validates if customer hasn't exceeded their usage limit for this coupon
    /// </summary>
    public static bool CustomerCanUseCoupon(int customerUsageCount, int? maxUsagePerCustomer)
    {
        if (!maxUsagePerCustomer.HasValue)
            return true;

        return customerUsageCount < maxUsagePerCustomer.Value;
    }

    /// <summary>
    /// Validates if order meets minimum amount requirement
    /// </summary>
    public static bool MeetsMinimumOrderAmount(decimal orderAmount, decimal? minimumOrderAmount)
    {
        if (!minimumOrderAmount.HasValue)
            return true;

        return orderAmount >= minimumOrderAmount.Value;
    }

    /// <summary>
    /// Calculates the discount amount based on coupon type
    /// </summary>
    public static decimal CalculateDiscountAmount(
        string discountType,
        decimal discountValue,
        decimal orderAmount,
        decimal? maximumDiscountAmount
    )
    {
        decimal discountAmount = discountType.ToLowerInvariant() switch
        {
            "percentage" => Math.Round((orderAmount * discountValue) / 100, 2),
            "fixed" => discountValue,
            _ => 0,
        };

        // Apply maximum discount cap for percentage-based coupons
        if (discountType.ToLowerInvariant() == "percentage" && maximumDiscountAmount.HasValue)
        {
            discountAmount = Math.Min(discountAmount, maximumDiscountAmount.Value);
        }

        // Ensure discount doesn't exceed order amount
        discountAmount = Math.Min(discountAmount, orderAmount);

        return Math.Max(discountAmount, 0);
    }

    /// <summary>
    /// Validates if the discount value is acceptable
    /// </summary>
    public static bool IsValidDiscountValue(string discountType, decimal discountValue)
    {
        if (discountValue <= 0)
            return false;

        return discountType.ToLowerInvariant() switch
        {
            "percentage" => discountValue <= MaxDiscountPercentage,
            "fixed" => discountValue <= MaxDiscountAmount,
            _ => false,
        };
    }

    /// <summary>
    /// Validates if coupon applies to specific products
    /// </summary>
    public static bool AppliesToProducts(Guid[] cartProductIds, Guid[]? applicableProductIds)
    {
        // If no specific products defined, applies to all
        if (applicableProductIds == null || applicableProductIds.Length == 0)
            return true;

        // Check if any cart product is in the applicable list
        return cartProductIds.Any(productId => applicableProductIds.Contains(productId));
    }
}
