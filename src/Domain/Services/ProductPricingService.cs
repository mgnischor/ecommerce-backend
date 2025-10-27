using ECommerce.Domain.Entities;
using ECommerce.Domain.Policies;

namespace ECommerce.Domain.Services;

/// <summary>
/// Domain service for product pricing strategies and calculations
/// </summary>
public static class ProductPricingService
{
    /// <summary>
    /// Validates product pricing
    /// </summary>
    static public (bool isValid, string? errorMessage) ValidateProductPricing(ProductEntity product)
    {
        if (!PricingPolicy.IsValidPrice(product.Price))
            return (false, $"Invalid product price: {product.Price}");

        if (
            product.DiscountPrice.HasValue
            && !PricingPolicy.IsValidDiscountPrice(product.Price, product.DiscountPrice.Value)
        )
        {
            return (false, "Invalid discount price");
        }

        return (true, null);
    }

    /// <summary>
    /// Calculates recommended retail price based on cost and margin
    /// </summary>
    static public decimal CalculateRetailPrice(decimal cost, decimal targetMarginPercentage)
    {
        if (cost <= 0)
            throw new ArgumentException("Cost must be greater than zero", nameof(cost));

        if (targetMarginPercentage <= 0 || targetMarginPercentage >= 100)
            throw new ArgumentException(
                "Margin percentage must be between 0 and 100",
                nameof(targetMarginPercentage)
            );

        // Retail Price = Cost / (1 - Margin%)
        var retailPrice = cost / (1 - (targetMarginPercentage / 100));
        return Math.Round(retailPrice, 2);
    }

    /// <summary>
    /// Calculates profit margin percentage
    /// </summary>
    static public decimal CalculateProfitMargin(decimal cost, decimal sellingPrice)
    {
        if (sellingPrice <= 0)
            return 0;

        return Math.Round(((sellingPrice - cost) / sellingPrice) * 100, 2);
    }

    /// <summary>
    /// Suggests optimal discount price to boost sales
    /// </summary>
    static public decimal SuggestDiscountPrice(
        decimal currentPrice,
        int stockQuantity,
        int minStockLevel,
        int averageDailySales
    )
    {
        // If overstocked, suggest higher discount
        if (StockManagementPolicy.IsLowStock(stockQuantity, minStockLevel))
        {
            // Low stock - minimal discount
            return PricingPolicy.ApplyDiscountPercentage(currentPrice, 5);
        }

        var daysOfSupply = averageDailySales > 0 ? stockQuantity / (decimal)averageDailySales : 0;

        // Aggressive discount for overstock
        if (daysOfSupply > 90)
            return PricingPolicy.ApplyDiscountPercentage(currentPrice, 30);

        if (daysOfSupply > 60)
            return PricingPolicy.ApplyDiscountPercentage(currentPrice, 20);

        if (daysOfSupply > 30)
            return PricingPolicy.ApplyDiscountPercentage(currentPrice, 10);

        return currentPrice; // No discount needed
    }

    /// <summary>
    /// Calculates dynamic pricing based on demand and stock
    /// </summary>
    static public decimal CalculateDynamicPrice(
        decimal basePrice,
        int stockQuantity,
        int recentSales,
        bool isFeatured
    )
    {
        var price = basePrice;

        // High demand adjustment (recent sales high relative to stock)
        if (stockQuantity > 0 && recentSales > 0)
        {
            var demandRatio = (decimal)recentSales / stockQuantity;

            if (demandRatio > 0.5m) // High demand
                price *= 1.1m; // 10% increase
            else if (demandRatio < 0.1m) // Low demand
                price *= 0.9m; // 10% decrease
        }

        // Featured product premium
        if (isFeatured)
            price *= 1.05m; // 5% increase

        // Ensure price stays within valid range
        return Math.Round(Math.Max(basePrice * 0.5m, Math.Min(price, basePrice * 1.5m)), 2);
    }

    /// <summary>
    /// Compares product price with competitor prices
    /// </summary>
    static public string GetPriceCompetitiveness(decimal ourPrice, List<decimal> competitorPrices)
    {
        if (competitorPrices == null || !competitorPrices.Any())
            return "No competition data";

        var avgCompetitorPrice = competitorPrices.Average();
        var minCompetitorPrice = competitorPrices.Min();
        var maxCompetitorPrice = competitorPrices.Max();

        if (ourPrice < minCompetitorPrice)
            return "Highly competitive";

        if (ourPrice <= avgCompetitorPrice)
            return "Competitive";

        if (ourPrice <= maxCompetitorPrice)
            return "Average";

        return "Above market";
    }
}
