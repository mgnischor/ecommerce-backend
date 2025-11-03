using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Policies;

/// <summary>
/// Defines business rules for refund validation and processing
/// </summary>
public static class RefundPolicy
{
    private const int RefundWindowDays = 30;
    private const int ReturnShippingWindowDays = 14;
    private const decimal MaxRestockingFeePercentage = 20m;
    private const decimal MinRefundAmount = 0.01m;

    /// <summary>
    /// Checks if an order is eligible for refund based on delivery date
    /// </summary>
    public static bool IsEligibleForRefund(DateTime? deliveredAt)
    {
        if (!deliveredAt.HasValue)
            return false;

        var daysSinceDelivery = (DateTime.UtcNow - deliveredAt.Value).TotalDays;
        return daysSinceDelivery <= RefundWindowDays;
    }

    /// <summary>
    /// Checks if an order status allows refund request
    /// </summary>
    public static bool CanRequestRefund(OrderStatus orderStatus)
    {
        return orderStatus switch
        {
            OrderStatus.Delivered => true,
            OrderStatus.Shipped => true, // Allow refund requests even before delivery
            _ => false,
        };
    }

    /// <summary>
    /// Validates if a refund amount is acceptable
    /// </summary>
    public static bool IsValidRefundAmount(decimal refundAmount, decimal orderTotal)
    {
        if (refundAmount < MinRefundAmount)
            return false;

        // Refund amount should not exceed the original order total
        return refundAmount <= orderTotal;
    }

    /// <summary>
    /// Checks if a refund status transition is valid
    /// </summary>
    public static bool IsValidStatusTransition(RefundStatus currentStatus, RefundStatus newStatus)
    {
        return (currentStatus, newStatus) switch
        {
            (RefundStatus.Requested, RefundStatus.UnderReview) => true,
            (RefundStatus.Requested, RefundStatus.Rejected) => true,
            (RefundStatus.Requested, RefundStatus.Cancelled) => true,
            (RefundStatus.UnderReview, RefundStatus.Approved) => true,
            (RefundStatus.UnderReview, RefundStatus.Rejected) => true,
            (RefundStatus.Approved, RefundStatus.Processing) => true,
            (RefundStatus.Approved, RefundStatus.Cancelled) => true,
            (RefundStatus.Processing, RefundStatus.Completed) => true,
            _ => false,
        };
    }

    /// <summary>
    /// Validates if items have been returned within the acceptable window
    /// </summary>
    public static bool IsWithinReturnWindow(DateTime refundRequestedAt)
    {
        var daysSinceRequest = (DateTime.UtcNow - refundRequestedAt).TotalDays;
        return daysSinceRequest <= ReturnShippingWindowDays;
    }

    /// <summary>
    /// Checks if a restocking fee is valid
    /// </summary>
    public static bool IsValidRestockingFee(decimal restockingFee, decimal refundAmount)
    {
        if (restockingFee < 0)
            return false;

        var maxFee = refundAmount * (MaxRestockingFeePercentage / 100);
        return restockingFee <= maxFee;
    }

    /// <summary>
    /// Calculates the final refund amount after restocking fee
    /// </summary>
    public static decimal CalculateFinalRefundAmount(decimal refundAmount, decimal? restockingFee)
    {
        var finalAmount = refundAmount - (restockingFee ?? 0);
        return Math.Max(finalAmount, 0);
    }

    /// <summary>
    /// Determines if a return is required for this refund
    /// </summary>
    public static bool RequiresReturn(OrderStatus orderStatus, bool isDigitalProduct)
    {
        // Digital products don't require physical return
        if (isDigitalProduct)
            return false;

        // If order was delivered, return is typically required
        return orderStatus == OrderStatus.Delivered;
    }

    /// <summary>
    /// Checks if refund can be auto-approved based on amount
    /// </summary>
    public static bool CanAutoApprove(decimal refundAmount, decimal autoApprovalThreshold)
    {
        return refundAmount <= autoApprovalThreshold;
    }
}
