namespace ECommerce.Domain.Policies;

/// <summary>
/// Defines business rules for shipping zone validation and rate calculation
/// </summary>
public static class ShippingZonePolicy
{
    private const decimal MaxBaseRate = 9999.99m;
    private const decimal MaxRatePerKg = 999.99m;
    private const decimal MaxRatePerItem = 999.99m;
    private const int MaxDeliveryDays = 60;
    private const int MinDeliveryDays = 1;
    private const decimal MaxTaxRate = 100m;

    /// <summary>
    /// Validates if base shipping rate is acceptable
    /// </summary>
    public static bool IsValidBaseRate(decimal baseRate)
    {
        return baseRate >= 0 && baseRate <= MaxBaseRate;
    }

    /// <summary>
    /// Validates if rate per kg is acceptable
    /// </summary>
    public static bool IsValidRatePerKg(decimal? ratePerKg)
    {
        if (!ratePerKg.HasValue)
            return true;

        return ratePerKg.Value >= 0 && ratePerKg.Value <= MaxRatePerKg;
    }

    /// <summary>
    /// Validates if rate per item is acceptable
    /// </summary>
    public static bool IsValidRatePerItem(decimal? ratePerItem)
    {
        if (!ratePerItem.HasValue)
            return true;

        return ratePerItem.Value >= 0 && ratePerItem.Value <= MaxRatePerItem;
    }

    /// <summary>
    /// Validates delivery time range
    /// </summary>
    public static bool IsValidDeliveryTimeRange(int minDays, int maxDays)
    {
        if (minDays < MinDeliveryDays || minDays > MaxDeliveryDays)
            return false;

        if (maxDays < MinDeliveryDays || maxDays > MaxDeliveryDays)
            return false;

        return maxDays >= minDays;
    }

    /// <summary>
    /// Validates tax rate
    /// </summary>
    public static bool IsValidTaxRate(decimal? taxRate)
    {
        if (!taxRate.HasValue)
            return true;

        return taxRate.Value >= 0 && taxRate.Value <= MaxTaxRate;
    }

    /// <summary>
    /// Calculates total shipping cost for a zone
    /// </summary>
    public static decimal CalculateShippingCost(
        decimal baseRate,
        decimal? ratePerKg,
        decimal? ratePerItem,
        decimal totalWeight,
        int itemCount,
        decimal? freeShippingThreshold,
        decimal orderAmount
    )
    {
        // Check for free shipping eligibility
        if (
            freeShippingThreshold.HasValue
            && orderAmount >= freeShippingThreshold.Value
        )
        {
            return 0m;
        }

        var cost = baseRate;

        // Add weight-based cost
        if (ratePerKg.HasValue && totalWeight > 0)
        {
            cost += ratePerKg.Value * totalWeight;
        }

        // Add item-based cost
        if (ratePerItem.HasValue && itemCount > 0)
        {
            cost += ratePerItem.Value * itemCount;
        }

        return Math.Round(cost, 2);
    }

    /// <summary>
    /// Checks if order amount meets minimum requirement for zone
    /// </summary>
    public static bool MeetsMinimumOrderAmount(
        decimal orderAmount,
        decimal? minimumOrderAmount
    )
    {
        if (!minimumOrderAmount.HasValue)
            return true;

        return orderAmount >= minimumOrderAmount.Value;
    }

    /// <summary>
    /// Checks if order amount is within maximum limit for zone
    /// </summary>
    public static bool WithinMaximumOrderAmount(
        decimal orderAmount,
        decimal? maximumOrderAmount
    )
    {
        if (!maximumOrderAmount.HasValue)
            return true;

        return orderAmount <= maximumOrderAmount.Value;
    }

    /// <summary>
    /// Checks if destination matches zone criteria
    /// </summary>
    public static bool MatchesDestination(
        string destinationCountry,
        string? destinationState,
        string? destinationPostalCode,
        List<string> zoneCountries,
        List<string> zoneStates,
        List<string> zonePostalCodes
    )
    {
        // Check country match
        if (zoneCountries.Any() && !zoneCountries.Contains(destinationCountry))
            return false;

        // Check state match (format: "Country:State")
        if (zoneStates.Any() && !string.IsNullOrWhiteSpace(destinationState))
        {
            var stateKey = $"{destinationCountry}:{destinationState}";
            if (!zoneStates.Contains(stateKey))
                return false;
        }

        // Check postal code match (supports wildcards)
        if (zonePostalCodes.Any() && !string.IsNullOrWhiteSpace(destinationPostalCode))
        {
            var matches = zonePostalCodes.Any(pattern =>
                MatchesPostalCodePattern(destinationPostalCode, pattern)
            );
            if (!matches)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Matches postal code against pattern (supports wildcards)
    /// </summary>
    private static bool MatchesPostalCodePattern(string postalCode, string pattern)
    {
        // Simple wildcard matching (* matches any characters)
        if (pattern.Contains('*'))
        {
            var regexPattern = "^" + pattern.Replace("*", ".*") + "$";
            return System.Text.RegularExpressions.Regex.IsMatch(
                postalCode,
                regexPattern,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );
        }

        return string.Equals(postalCode, pattern, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Selects the best matching zone based on priority
    /// </summary>
    public static T? SelectBestZone<T>(
        List<T> matchingZones,
        Func<T, int> prioritySelector
    )
        where T : class
    {
        if (!matchingZones.Any())
            return null;

        // Return zone with lowest priority number (highest priority)
        return matchingZones.OrderBy(prioritySelector).FirstOrDefault();
    }

    /// <summary>
    /// Calculates tax amount for zone
    /// </summary>
    public static decimal CalculateTax(decimal amount, decimal? taxRate)
    {
        if (!taxRate.HasValue || taxRate.Value <= 0)
            return 0m;

        return Math.Round(amount * (taxRate.Value / 100), 2);
    }
}
