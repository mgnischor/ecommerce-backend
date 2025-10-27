using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Policies;

namespace ECommerce.Domain.Services;

/// <summary>
/// Domain service for managing order lifecycle and state transitions
/// </summary>
public static class OrderLifecycleService
{
    /// <summary>
    /// Validates if an order can be placed
    /// </summary>
    static public (bool isValid, string? errorMessage) ValidateOrderPlacement(
        List<(ProductEntity product, int quantity)> orderItems,
        decimal totalAmount,
        Guid? shippingAddressId,
        ShippingMethod shippingMethod
    )
    {
        // Validate items count
        if (!OrderValidationPolicy.IsValidItemCount(orderItems.Count))
            return (false, "Order exceeds maximum number of items");

        // Validate total amount
        if (!OrderValidationPolicy.IsValidOrderAmount(totalAmount))
            return (false, "Order amount is invalid");

        // Validate stock availability
        foreach (var (product, quantity) in orderItems)
        {
            if (!StockManagementPolicy.CanReserveStock(product.StockQuantity, quantity))
                return (false, $"Product {product.Name} is out of stock");

            if (
                !StockManagementPolicy.IsQuantityWithinOrderLimits(
                    quantity,
                    product.MaxOrderQuantity
                )
            )
                return (false, $"Quantity exceeds maximum order limit for {product.Name}");
        }

        // Validate shipping address if required
        if (
            OrderValidationPolicy.RequiresShippingAddress(shippingMethod)
            && !shippingAddressId.HasValue
        )
            return (false, "Shipping address is required");

        return (true, null);
    }

    /// <summary>
    /// Attempts to transition order to a new status
    /// </summary>
    static public (bool success, string? errorMessage) TransitionOrderStatus(
        OrderEntity order,
        OrderStatus newStatus,
        string? reason = null
    )
    {
        if (!OrderValidationPolicy.IsValidStatusTransition(order.Status, newStatus))
            return (false, $"Cannot transition from {order.Status} to {newStatus}");

        // Additional validations based on new status
        switch (newStatus)
        {
            case OrderStatus.Cancelled:
                if (!OrderValidationPolicy.CanCancelOrder(order.Status))
                    return (false, "Order cannot be cancelled in current status");
                break;

            case OrderStatus.Shipped:
                if (string.IsNullOrWhiteSpace(order.TrackingNumber))
                    return (false, "Tracking number is required before shipping");
                break;

            case OrderStatus.Refunded:
                if (!OrderValidationPolicy.CanRefundOrder(order.Status, order.DeliveredAt))
                    return (false, "Order is not eligible for refund");
                break;
        }

        return (true, null);
    }

    /// <summary>
    /// Generates a unique order number
    /// </summary>
    static public string GenerateOrderNumber()
    {
        var timestamp = DateTime.UtcNow;
        var datePart = timestamp.ToString("yyyyMMdd");
        var timePart = timestamp.ToString("HHmmss");
        var random = new Random().Next(1000, 9999);

        return $"ORD-{datePart}-{timePart}-{random}";
    }

    /// <summary>
    /// Calculates expected delivery date based on shipping method
    /// </summary>
    static public DateTime CalculateExpectedDeliveryDate(
        ShippingMethod shippingMethod,
        DateTime orderDate
    )
    {
        var businessDays = shippingMethod switch
        {
            ShippingMethod.SameDay => 0,
            ShippingMethod.NextDay => 1,
            ShippingMethod.Express => 3,
            ShippingMethod.Standard => 7,
            ShippingMethod.International => 14,
            ShippingMethod.FreeShipping => 10,
            _ => 7,
        };

        return AddBusinessDays(orderDate, businessDays);
    }

    /// <summary>
    /// Calculates shipping cost based on weight and method
    /// </summary>
    static public decimal CalculateShippingCost(decimal totalWeight, ShippingMethod shippingMethod)
    {
        if (shippingMethod == ShippingMethod.FreeShipping)
            return 0;

        var baseCost = shippingMethod switch
        {
            ShippingMethod.SameDay => 25.00m,
            ShippingMethod.NextDay => 15.00m,
            ShippingMethod.Express => 10.00m,
            ShippingMethod.Standard => 5.00m,
            ShippingMethod.International => 30.00m,
            ShippingMethod.StorePickup => 0m,
            _ => 5.00m,
        };

        // Add weight-based cost ($0.50 per kg over 1kg)
        if (totalWeight > 1)
        {
            var additionalCost = (totalWeight - 1) * 0.50m;
            baseCost += additionalCost;
        }

        return Math.Round(baseCost, 2);
    }

    private static DateTime AddBusinessDays(DateTime date, int days)
    {
        var result = date;
        var addedDays = 0;

        while (addedDays < days)
        {
            result = result.AddDays(1);
            if (result.DayOfWeek != DayOfWeek.Saturday && result.DayOfWeek != DayOfWeek.Sunday)
            {
                addedDays++;
            }
        }

        return result;
    }
}
