using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Policies;

/// <summary>
/// Defines business rules for product returns
/// </summary>
public static class ReturnsPolicy
{
    private const int DefaultReturnWindowDays = 30;
    private const int MaxConditionScore = 10;
    private const decimal MaximumAutoApproveThreshold = 50m;

    /// <summary>
    /// Checks if an order is eligible for a return
    /// </summary>
    public static bool IsEligibleForReturn(bool isPhysicalProduct, OrderStatus orderStatus)
    {
        if (!isPhysicalProduct)
            return false;

        return orderStatus switch
        {
            OrderStatus.Delivered => true,
            OrderStatus.Completed => true,
            _ => false,
        };
    }

    /// <summary>
    /// Checks if a return request is within the accepted return window
    /// </summary>
    public static bool IsWithinReturnWindow(DateTime deliveredAt, int? returnWindowDays)
    {
        var effectiveWindow = returnWindowDays ?? DefaultReturnWindowDays;
        var daysSinceDelivery = (DateTime.UtcNow - deliveredAt).TotalDays;
        return daysSinceDelivery <= effectiveWindow;
    }

    /// <summary>
    /// Checks if a digital product can be returned
    /// </summary>
    public static bool CanReturnDigitalProduct(bool isDownloaded, bool isOpened)
    {
        // Digital products are non-returnable once downloaded or opened
        return !isDownloaded && !isOpened;
    }

    /// <summary>
    /// Validates if a return reason is provided and acceptable
    /// </summary>
    public static bool IsValidReturnReason(string? reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return false;

        return reason.Trim().Length >= 5 && reason.Trim().Length <= 500;
    }

    /// <summary>
    /// Checks if a returned item requires inspection
    /// </summary>
    public static bool RequiresInspection(bool isOpened, bool isDamaged, decimal itemValue)
    {
        if (isDamaged)
            return true;

        if (isOpened && itemValue >= 100m)
            return true;

        return itemValue >= 500m;
    }

    /// <summary>
    /// Checks if a used item can be returned
    /// </summary>
    public static bool CanReturnUsedItem(bool hasBeenUsed, bool restockingAllowed)
    {
        if (!hasBeenUsed)
            return true;

        // Used items can only be returned if the vendor allows restocking
        return restockingAllowed;
    }

    /// <summary>
    /// Calculates the refund percentage based on item condition
    /// </summary>
    public static decimal CalculateRefundPercentage(int conditionScore)
    {
        if (conditionScore < 0 || conditionScore > MaxConditionScore)
            return 0m;

        return conditionScore switch
        {
            >= 9 => 100m,
            >= 7 => 80m,
            >= 5 => 60m,
            >= 3 => 40m,
            _ => 0m,
        };
    }

    /// <summary>
    /// Checks if a return can be auto-approved
    /// </summary>
    public static bool CanAutoApproveReturn(
        decimal refundAmount,
        decimal? autoApprovalThreshold,
        bool hasPhotos
    )
    {
        var effectiveThreshold = autoApprovalThreshold ?? MaximumAutoApproveThreshold;

        if (refundAmount > effectiveThreshold)
            return false;

        // Returns below the threshold still require proof photos
        return hasPhotos;
    }
}
