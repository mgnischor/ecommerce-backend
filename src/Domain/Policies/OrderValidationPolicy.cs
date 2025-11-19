using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Policies;

/// <summary>
/// Defines business rules for order validation and processing
/// </summary>
public static class OrderValidationPolicy
{
    private const decimal MinimumOrderAmount = 0.01m;
    private const decimal MaximumOrderAmount = 999999.99m;
    private const int MaximumItemsPerOrder = 100;
    private const int OrderCancellationWindowHours = 24;

    /// <summary>
    /// Validates if an order amount is within acceptable limits
    /// </summary>
    public static bool IsValidOrderAmount(decimal amount)
    {
        return amount >= MinimumOrderAmount && amount <= MaximumOrderAmount;
    }

    /// <summary>
    /// Validates if the number of items in an order is acceptable
    /// </summary>
    public static bool IsValidItemCount(int itemCount)
    {
        return itemCount > 0 && itemCount <= MaximumItemsPerOrder;
    }

    /// <summary>
    /// Checks if an order can be cancelled based on its status
    /// </summary>
    public static bool CanCancelOrder(OrderStatus currentStatus)
    {
        return currentStatus switch
        {
            OrderStatus.Pending => true,
            OrderStatus.Processing => true,
            OrderStatus.Confirmed => true,
            OrderStatus.Shipped => false,
            OrderStatus.Delivered => false,
            OrderStatus.Cancelled => false,
            OrderStatus.Refunded => false,
            _ => false,
        };
    }

    /// <summary>
    /// Checks if an order can be cancelled based on time elapsed
    /// </summary>
    public static bool IsWithinCancellationWindow(DateTime orderCreatedAt)
    {
        var timeElapsed = DateTime.UtcNow - orderCreatedAt;
        return timeElapsed.TotalHours <= OrderCancellationWindowHours;
    }

    /// <summary>
    /// Checks if an order can be modified
    /// </summary>
    public static bool CanModifyOrder(OrderStatus currentStatus)
    {
        return currentStatus == OrderStatus.Pending;
    }

    /// <summary>
    /// Checks if an order can be refunded
    /// </summary>
    public static bool CanRefundOrder(OrderStatus currentStatus, DateTime? deliveredAt)
    {
        if (currentStatus != OrderStatus.Delivered)
            return false;

        if (!deliveredAt.HasValue)
            return false;

        // Allow refunds within 30 days of delivery
        var daysSinceDelivery = (DateTime.UtcNow - deliveredAt.Value).TotalDays;
        return daysSinceDelivery <= 30;
    }

    /// <summary>
    /// Validates order totals calculation
    /// </summary>
    public static bool IsValidOrderTotal(
        decimal subtotal,
        decimal taxAmount,
        decimal shippingCost,
        decimal discountAmount,
        decimal totalAmount
    )
    {
        if (subtotal < 0 || taxAmount < 0 || shippingCost < 0 || discountAmount < 0)
            return false;

        var calculatedTotal = subtotal + taxAmount + shippingCost - discountAmount;
        var tolerance = 0.01m; // Allow 1 cent tolerance for rounding

        return Math.Abs(totalAmount - calculatedTotal) <= tolerance;
    }

    /// <summary>
    /// Determines the next valid status transition
    /// </summary>
    public static bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
    {
        return (currentStatus, newStatus) switch
        {
            (OrderStatus.Pending, OrderStatus.Processing) => true,
            (OrderStatus.Pending, OrderStatus.Cancelled) => true,
            (OrderStatus.Processing, OrderStatus.Confirmed) => true,
            (OrderStatus.Processing, OrderStatus.Cancelled) => true,
            (OrderStatus.Processing, OrderStatus.Shipped) => true,
            (OrderStatus.Confirmed, OrderStatus.Shipped) => true,
            (OrderStatus.Confirmed, OrderStatus.Cancelled) => true,
            (OrderStatus.Shipped, OrderStatus.Delivered) => true,
            (OrderStatus.Delivered, OrderStatus.Refunded) => true,
            _ => false,
        };
    }

    /// <summary>
    /// Validates if shipping address is required for the order
    /// </summary>
    public static bool RequiresShippingAddress(ShippingMethod shippingMethod)
    {
        return shippingMethod != ShippingMethod.StorePickup
            && shippingMethod != ShippingMethod.NotSpecified;
    }
}
