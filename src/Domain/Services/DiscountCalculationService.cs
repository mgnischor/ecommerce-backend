using ECommerce.Domain.Entities;
using ECommerce.Domain.Policies;

namespace ECommerce.Domain.Services;

/// <summary>
/// Domain service for handling discount calculations and validations
/// </summary>
static public class DiscountCalculationService
{
    /// <summary>
    /// Calculates the final price after applying product discount
    /// </summary>
    static public decimal CalculateProductFinalPrice(ProductEntity product)
    {
        if (
            product.DiscountPrice.HasValue
            && PricingPolicy.IsValidDiscountPrice(product.Price, product.DiscountPrice.Value)
        )
        {
            return product.DiscountPrice.Value;
        }

        return product.Price;
    }

    /// <summary>
    /// Calculates the total savings for a product
    /// </summary>
    static public decimal CalculateSavings(decimal originalPrice, decimal finalPrice)
    {
        return Math.Max(0, originalPrice - finalPrice);
    }

    /// <summary>
    /// Calculates cart subtotal with applied discounts
    /// </summary>
    static public (decimal subtotal, decimal discount) CalculateCartTotal(
        List<(ProductEntity product, int quantity)> cartItems
    )
    {
        decimal subtotal = 0;
        decimal totalDiscount = 0;

        foreach (var (product, quantity) in cartItems)
        {
            var originalPrice = product.Price * quantity;
            var finalPrice = CalculateProductFinalPrice(product) * quantity;

            subtotal += finalPrice;
            totalDiscount += originalPrice - finalPrice;
        }

        return (subtotal, totalDiscount);
    }

    /// <summary>
    /// Applies coupon discount to order amount
    /// </summary>
    static public decimal ApplyCouponDiscount(
        CouponEntity coupon,
        decimal orderAmount,
        Guid[] cartProductIds
    )
    {
        // Validate coupon
        if (!CouponValidationPolicy.IsActive(coupon.IsActive, coupon.IsDeleted))
            return 0;

        if (!CouponValidationPolicy.IsWithinValidityPeriod(coupon.ValidFrom, coupon.ValidUntil))
            return 0;

        if (!CouponValidationPolicy.HasUsageRemaining(coupon.UsageCount, coupon.MaxUsageCount))
            return 0;

        if (!CouponValidationPolicy.MeetsMinimumOrderAmount(orderAmount, coupon.MinimumOrderAmount))
            return 0;

        if (!CouponValidationPolicy.AppliesToProducts(cartProductIds, coupon.ApplicableProductIds))
            return 0;

        // Calculate discount
        return CouponValidationPolicy.CalculateDiscountAmount(
            coupon.DiscountType,
            coupon.DiscountValue,
            orderAmount,
            coupon.MaximumDiscountAmount
        );
    }

    /// <summary>
    /// Calculates bulk discount based on quantity
    /// </summary>
    static public decimal CalculateBulkDiscount(decimal unitPrice, int quantity)
    {
        // Example bulk discount tiers
        // 10-50 items: 5% off
        // 51-100 items: 10% off
        // 100+ items: 15% off

        var discountPercentage = quantity switch
        {
            >= 100 => 15m,
            >= 51 => 10m,
            >= 10 => 5m,
            _ => 0m,
        };

        if (discountPercentage == 0)
            return 0;

        return PricingPolicy.ApplyDiscountPercentage(unitPrice * quantity, discountPercentage);
    }
}
