using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Services;

/// <summary>
/// Domain service for fraud detection and risk assessment
/// </summary>
public static class FraudDetectionService
{
    private const int MaxOrdersPerDayNormal = 10;
    private const decimal HighValueOrderThreshold = 5000m;
    private const int MaxAddressChangesPerDay = 3;

    /// <summary>
    /// Calculates fraud risk score for an order (0-100, higher is riskier)
    /// </summary>
    public static int CalculateFraudRiskScore(
        OrderEntity order,
        UserEntity customer,
        int ordersInLast24Hours,
        int addressChangesInLast24Hours,
        bool isNewCustomer
    )
    {
        var riskScore = 0;

        // High value order
        if (order.TotalAmount > HighValueOrderThreshold)
            riskScore += 20;

        // Too many orders in short time
        if (ordersInLast24Hours > MaxOrdersPerDayNormal)
            riskScore += 30;

        // New customer with high value order
        if (isNewCustomer && order.TotalAmount > 1000m)
            riskScore += 25;

        // Frequent address changes
        if (addressChangesInLast24Hours > MaxAddressChangesPerDay)
            riskScore += 15;

        // Different billing and shipping addresses
        if (
            order.BillingAddressId.HasValue
            && order.ShippingAddressId.HasValue
            && order.BillingAddressId != order.ShippingAddressId
        )
            riskScore += 10;

        // Email not verified
        if (!customer.IsEmailVerified)
            riskScore += 15;

        // Expedited shipping on high-value order
        if (
            order.TotalAmount > 2000m
            && (
                order.ShippingMethod == Domain.Enums.ShippingMethod.NextDay
                || order.ShippingMethod == Domain.Enums.ShippingMethod.SameDay
            )
        )
            riskScore += 10;

        return Math.Min(riskScore, 100);
    }

    /// <summary>
    /// Determines if an order should be flagged for review
    /// </summary>
    public static bool ShouldFlagForReview(int riskScore)
    {
        return riskScore >= 50;
    }

    /// <summary>
    /// Determines if an order should be automatically rejected
    /// </summary>
    public static bool ShouldAutoReject(int riskScore)
    {
        return riskScore >= 80;
    }

    /// <summary>
    /// Gets risk level description
    /// </summary>
    public static string GetRiskLevelDescription(int riskScore)
    {
        return riskScore switch
        {
            >= 80 => "Very High Risk",
            >= 50 => "High Risk",
            >= 30 => "Medium Risk",
            >= 10 => "Low Risk",
            _ => "Minimal Risk",
        };
    }

    /// <summary>
    /// Validates if shipping and billing information match customer profile
    /// </summary>
    public static bool ValidateAddressConsistency(
        UserEntity customer,
        string orderShippingCity,
        string orderShippingCountry
    )
    {
        // Check if shipping location is drastically different from user profile
        if (string.IsNullOrWhiteSpace(customer.City) || string.IsNullOrWhiteSpace(customer.Country))
            return true; // No profile data to compare

        var countryMatch = customer.Country.Equals(
            orderShippingCountry,
            StringComparison.OrdinalIgnoreCase
        );

        // At least country should match
        return countryMatch;
    }
}
