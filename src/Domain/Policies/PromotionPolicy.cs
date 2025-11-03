using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Policies;

/// <summary>
/// Defines business rules for promotion validation and application
/// </summary>
public static class PromotionPolicy
{
    private const int MaxPromotionNameLength = 200;
    private const int MinPromotionNameLength = 3;
    private const decimal MaxDiscountPercentage = 100m;
    private const decimal MaxDiscountAmount = 999999.99m;
    private const int MaxCombinablePromotions = 3;
    private const int MinimumPromotionDurationHours = 1;

    /// <summary>
    /// Validates if a promotion is currently active
    /// </summary>
    public static bool IsActive(
        bool isActive,
        bool isDeleted,
        DateTime startDate,
        DateTime endDate
    )
    {
        if (!isActive || isDeleted)
            return false;

        var now = DateTime.UtcNow;
        return now >= startDate && now <= endDate;
    }

    /// <summary>
    /// Validates promotion date range
    /// </summary>
    public static bool IsValidDateRange(DateTime startDate, DateTime endDate)
    {
        if (endDate <= startDate)
            return false;

        var duration = endDate - startDate;
        return duration.TotalHours >= MinimumPromotionDurationHours;
    }

    /// <summary>
    /// Checks if promotion has usage remaining
    /// </summary>
    public static bool HasUsageRemaining(int currentUsage, int? maxUsage)
    {
        if (!maxUsage.HasValue)
            return true;

        return currentUsage < maxUsage.Value;
    }

    /// <summary>
    /// Checks if user can use the promotion
    /// </summary>
    public static bool CanUserUsePromotion(
        int userUsageCount,
        int? maxUsagePerUser,
        Guid userId,
        List<Guid> eligibleUserIds
    )
    {
        // Check usage limit per user
        if (maxUsagePerUser.HasValue && userUsageCount >= maxUsagePerUser.Value)
            return false;

        // Check if promotion is restricted to specific users
        if (eligibleUserIds != null && eligibleUserIds.Any())
            return eligibleUserIds.Contains(userId);

        return true;
    }

    /// <summary>
    /// Validates if the promotion applies to the given products
    /// </summary>
    public static bool AppliesToProducts(
        List<Guid> cartProductIds,
        List<Guid> eligibleProductIds,
        List<Guid> eligibleCategoryIds
    )
    {
        // If no restrictions, applies to all products
        if (!eligibleProductIds.Any() && !eligibleCategoryIds.Any())
            return true;

        // Check if any cart product is in eligible list
        if (eligibleProductIds.Any())
            return cartProductIds.Any(p => eligibleProductIds.Contains(p));

        return false;
    }

    /// <summary>
    /// Validates minimum order requirement
    /// </summary>
    public static bool MeetsMinimumOrderAmount(decimal orderAmount, decimal? minimumOrderAmount)
    {
        if (!minimumOrderAmount.HasValue)
            return true;

        return orderAmount >= minimumOrderAmount.Value;
    }

    /// <summary>
    /// Calculates discount amount based on promotion type
    /// </summary>
    public static decimal CalculateDiscountAmount(
        PromotionType type,
        decimal? discountPercentage,
        decimal? discountAmount,
        decimal orderSubtotal,
        decimal? maximumDiscountAmount
    )
    {
        decimal discount = type switch
        {
            PromotionType.PercentageDiscount
                => CalculatePercentageDiscount(
                    discountPercentage ?? 0,
                    orderSubtotal,
                    maximumDiscountAmount
                ),
            PromotionType.FixedAmountDiscount => Math.Min(discountAmount ?? 0, orderSubtotal),
            PromotionType.FreeShipping => 0, // Handled separately in shipping calculation
            _ => 0,
        };

        return Math.Max(discount, 0);
    }

    /// <summary>
    /// Calculates percentage-based discount
    /// </summary>
    private static decimal CalculatePercentageDiscount(
        decimal percentage,
        decimal amount,
        decimal? maxDiscount
    )
    {
        var discount = Math.Round((amount * percentage) / 100, 2);

        if (maxDiscount.HasValue)
            discount = Math.Min(discount, maxDiscount.Value);

        return discount;
    }

    /// <summary>
    /// Validates if a discount percentage is acceptable
    /// </summary>
    public static bool IsValidDiscountPercentage(decimal percentage)
    {
        return percentage > 0 && percentage <= MaxDiscountPercentage;
    }

    /// <summary>
    /// Validates if a discount amount is acceptable
    /// </summary>
    public static bool IsValidDiscountAmount(decimal amount)
    {
        return amount > 0 && amount <= MaxDiscountAmount;
    }

    /// <summary>
    /// Checks if promotions can be combined
    /// </summary>
    public static bool CanCombinePromotions(
        List<bool> promotionCombinableFlags,
        int promotionCount
    )
    {
        // Check if all promotions allow combination
        if (!promotionCombinableFlags.All(flag => flag))
            return false;

        // Check maximum combinable limit
        return promotionCount <= MaxCombinablePromotions;
    }

    /// <summary>
    /// Determines promotion priority order for application
    /// </summary>
    public static List<T> OrderByPriority<T>(
        List<T> promotions,
        Func<T, int> prioritySelector
    )
    {
        return promotions.OrderByDescending(prioritySelector).ToList();
    }

    /// <summary>
    /// Validates if promotion code format is acceptable
    /// </summary>
    public static bool IsValidPromotionCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return true; // Code is optional

        if (code.Length < 3 || code.Length > 50)
            return false;

        // Code should contain only alphanumeric characters and hyphens
        return code.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_');
    }
}
