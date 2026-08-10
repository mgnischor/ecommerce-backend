using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Policies;

/// <summary>
/// Defines business rules for shipping rate calculation and carrier selection
/// </summary>
public static class ShippingRatePolicy
{
    private const decimal MaximumHandlingFee = 100m;
    private const int MaxCarrierCodeLength = 10;

    /// <summary>
    /// Calculates the weight-based shipping rate
    /// </summary>
    public static decimal CalculateWeightBasedRate(
        decimal baseRate,
        decimal ratePerKg,
        decimal totalWeight
    )
    {
        if (baseRate < 0 || ratePerKg < 0 || totalWeight < 0)
            return 0m;

        return Math.Round(baseRate + (ratePerKg * totalWeight), 2);
    }

    /// <summary>
    /// Checks if an order qualifies for free shipping
    /// </summary>
    public static bool IsFreeShippingEligible(
        decimal orderAmount,
        decimal? freeShippingThreshold,
        string? destinationCountry,
        string[]? freeShippingCountries
    )
    {
        // Country-level free shipping takes precedence
        if (freeShippingCountries != null && freeShippingCountries.Length > 0)
        {
            if (string.IsNullOrWhiteSpace(destinationCountry))
                return false;

            if (freeShippingCountries.Contains(destinationCountry))
                return true;
        }

        // Otherwise check against the order amount threshold
        if (!freeShippingThreshold.HasValue || freeShippingThreshold.Value <= 0)
            return false;

        return orderAmount >= freeShippingThreshold.Value;
    }

    /// <summary>
    /// Calculates the per-item shipping rate
    /// </summary>
    public static decimal CalculatePerItemRate(decimal ratePerItem, int itemCount)
    {
        if (ratePerItem < 0 || itemCount <= 0)
            return 0m;

        return Math.Round(ratePerItem * itemCount, 2);
    }

    /// <summary>
    /// Applies a surcharge percentage to a shipping cost
    /// </summary>
    public static decimal ApplySurcharge(decimal baseCost, decimal surchargePercent)
    {
        if (baseCost < 0 || surchargePercent < 0)
            return baseCost;

        return Math.Round(baseCost * (1 + (surchargePercent / 100)), 2);
    }

    /// <summary>
    /// Determines the priority of a shipping method (lower value = higher priority)
    /// </summary>
    public static int GetShippingRatePriority(ShippingMethod method)
    {
        return method switch
        {
            ShippingMethod.FreeShipping => 1,
            ShippingMethod.Standard => 2,
            ShippingMethod.Express => 3,
            ShippingMethod.NextDay => 4,
            ShippingMethod.SameDay => 5,
            ShippingMethod.International => 6,
            _ => 99,
        };
    }

    /// <summary>
    /// Validates a carrier code
    /// </summary>
    public static bool IsValidCarrierCode(string? carrierCode)
    {
        if (string.IsNullOrWhiteSpace(carrierCode))
            return false;

        return carrierCode.Trim().Length <= MaxCarrierCodeLength
            && carrierCode.All(c => char.IsLetterOrDigit(c) || c == '-');
    }

    /// <summary>
    /// Calculates the handling fee for a shipment
    /// </summary>
    public static decimal CalculateHandlingFee(int itemCount, decimal ratePerItem)
    {
        if (itemCount <= 0)
            return 0m;

        var fee = ratePerItem * itemCount;
        return Math.Min(Math.Round(fee, 2), MaximumHandlingFee);
    }
}
