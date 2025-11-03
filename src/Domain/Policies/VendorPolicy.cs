using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Policies;

/// <summary>
/// Defines business rules for vendor validation and management
/// </summary>
public static class VendorPolicy
{
    private const decimal MinCommissionRate = 0m;
    private const decimal MaxCommissionRate = 50m;
    private const decimal DefaultCommissionRate = 10m;
    private const int MinVendorRating = 0;
    private const int MaxVendorRating = 5;
    private const int MinimumRatingsForVerification = 10;
    private const decimal MinimumRatingForVerification = 4.0m;

    /// <summary>
    /// Validates if a commission rate is acceptable
    /// </summary>
    public static bool IsValidCommissionRate(decimal commissionRate)
    {
        return commissionRate >= MinCommissionRate && commissionRate <= MaxCommissionRate;
    }

    /// <summary>
    /// Validates if a vendor status transition is valid
    /// </summary>
    public static bool IsValidStatusTransition(VendorStatus currentStatus, VendorStatus newStatus)
    {
        return (currentStatus, newStatus) switch
        {
            (VendorStatus.Pending, VendorStatus.Active) => true,
            (VendorStatus.Pending, VendorStatus.Rejected) => true,
            (VendorStatus.Active, VendorStatus.Suspended) => true,
            (VendorStatus.Active, VendorStatus.Inactive) => true,
            (VendorStatus.Suspended, VendorStatus.Active) => true,
            (VendorStatus.Suspended, VendorStatus.Inactive) => true,
            (VendorStatus.Inactive, VendorStatus.Active) => true,
            _ => false,
        };
    }

    /// <summary>
    /// Checks if a vendor can sell products
    /// </summary>
    public static bool CanSellProducts(VendorStatus status, bool isVerified)
    {
        return status == VendorStatus.Active;
    }

    /// <summary>
    /// Validates if vendor rating is within acceptable range
    /// </summary>
    public static bool IsValidRating(decimal rating)
    {
        return rating >= MinVendorRating && rating <= MaxVendorRating;
    }

    /// <summary>
    /// Calculates updated vendor rating
    /// </summary>
    public static decimal CalculateNewRating(
        decimal currentRating,
        int currentRatingCount,
        decimal newRating
    )
    {
        if (!IsValidRating(newRating))
            throw new ArgumentException("New rating must be between 0 and 5");

        var totalRating = (currentRating * currentRatingCount) + newRating;
        var newRatingCount = currentRatingCount + 1;

        return Math.Round(totalRating / newRatingCount, 2);
    }

    /// <summary>
    /// Checks if vendor qualifies for verification
    /// </summary>
    public static bool QualifiesForVerification(int totalRatings, decimal averageRating)
    {
        return totalRatings >= MinimumRatingsForVerification
            && averageRating >= MinimumRatingForVerification;
    }

    /// <summary>
    /// Determines if vendor qualifies for featured status
    /// </summary>
    public static bool QualifiesForFeaturedStatus(
        decimal rating,
        int totalOrders,
        decimal totalSales,
        bool isVerified
    )
    {
        return isVerified && rating >= 4.5m && totalOrders >= 100 && totalSales >= 10000m;
    }

    /// <summary>
    /// Validates vendor payout eligibility
    /// </summary>
    public static bool IsEligibleForPayout(
        VendorStatus status,
        bool hasPaymentMethod,
        decimal pendingAmount,
        decimal minimumPayoutAmount
    )
    {
        return status == VendorStatus.Active
            && hasPaymentMethod
            && pendingAmount >= minimumPayoutAmount;
    }

    /// <summary>
    /// Calculates vendor commission amount
    /// </summary>
    public static decimal CalculateCommission(decimal saleAmount, decimal commissionRate)
    {
        if (!IsValidCommissionRate(commissionRate))
            throw new ArgumentException("Invalid commission rate");

        return Math.Round(saleAmount * (commissionRate / 100), 2);
    }

    /// <summary>
    /// Calculates vendor payout amount after commission
    /// </summary>
    public static decimal CalculatePayoutAmount(decimal saleAmount, decimal commissionRate)
    {
        var commission = CalculateCommission(saleAmount, commissionRate);
        return saleAmount - commission;
    }
}
