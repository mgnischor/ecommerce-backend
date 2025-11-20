using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Policies;

/// <summary>
/// Defines business rules for order processing and lifecycle management
/// </summary>
public static class OrderProcessingPolicy
{
    private const decimal MinimumOrderValue = 0.01m;
    private const decimal MaximumOrderValue = 1_000_000m;
    private const int MaximumItemsPerOrder = 100;
    private const int MinimumQuantityPerItem = 1;
    private const int MaximumQuantityPerItem = 1000;

    /// <summary>
    /// Validates if an order can be created with the given parameters
    /// </summary>
    public static (bool isValid, string? errorMessage) CanCreateOrder(
        decimal totalAmount,
        int itemCount,
        Guid customerId
    )
    {
        if (customerId == Guid.Empty)
            return (false, "Customer ID is required");

        if (totalAmount < MinimumOrderValue)
            return (false, $"Order total must be at least {MinimumOrderValue}");

        if (totalAmount > MaximumOrderValue)
            return (false, $"Order total exceeds maximum allowed amount ({MaximumOrderValue})");

        if (itemCount <= 0)
            return (false, "Order must contain at least one item");

        if (itemCount > MaximumItemsPerOrder)
            return (false, $"Order cannot exceed {MaximumItemsPerOrder} items");

        return (true, null);
    }

    /// <summary>
    /// Validates if an item can be added to an order
    /// </summary>
    public static (bool isValid, string? errorMessage) CanAddOrderItem(
        Guid productId,
        int quantity,
        decimal unitPrice,
        int currentItemCount
    )
    {
        if (productId == Guid.Empty)
            return (false, "Product ID is required");

        if (quantity < MinimumQuantityPerItem)
            return (false, $"Quantity must be at least {MinimumQuantityPerItem}");

        if (quantity > MaximumQuantityPerItem)
            return (false, $"Quantity cannot exceed {MaximumQuantityPerItem} per item");

        if (unitPrice <= 0)
            return (false, "Unit price must be positive");

        if (currentItemCount >= MaximumItemsPerOrder)
            return (false, $"Order already has maximum allowed items ({MaximumItemsPerOrder})");

        return (true, null);
    }

    /// <summary>
    /// Validates if an order status can transition to a new status
    /// </summary>
    public static (bool isValid, string? errorMessage) CanTransitionOrderStatus(
        OrderStatus currentStatus,
        OrderStatus newStatus
    )
    {
        // Define valid status transitions
        var validTransitions = new Dictionary<OrderStatus, HashSet<OrderStatus>>
        {
            [OrderStatus.Pending] = new HashSet<OrderStatus>
            {
                OrderStatus.Confirmed,
                OrderStatus.Cancelled,
            },
            [OrderStatus.Confirmed] = new HashSet<OrderStatus>
            {
                OrderStatus.Processing,
                OrderStatus.Cancelled,
            },
            [OrderStatus.Processing] = new HashSet<OrderStatus>
            {
                OrderStatus.Shipped,
                OrderStatus.Cancelled,
            },
            [OrderStatus.Shipped] = new HashSet<OrderStatus>
            {
                OrderStatus.Delivered,
                OrderStatus.Returned,
            },
            [OrderStatus.Delivered] = new HashSet<OrderStatus>
            {
                OrderStatus.Completed,
                OrderStatus.Returned,
            },
            [OrderStatus.Completed] = new HashSet<OrderStatus>
            {
                OrderStatus.Returned, // Allow returns even after completion
            },
            [OrderStatus.Cancelled] = new HashSet<OrderStatus>(), // Cannot transition from cancelled
            [OrderStatus.Returned] = new HashSet<OrderStatus> { OrderStatus.Refunded },
            [OrderStatus.Refunded] = new HashSet<OrderStatus>(), // Final state
        };

        if (currentStatus == newStatus)
            return (false, "New status must be different from current status");

        if (!validTransitions.ContainsKey(currentStatus))
            return (false, $"Invalid current status: {currentStatus}");

        if (!validTransitions[currentStatus].Contains(newStatus))
            return (
                false,
                $"Cannot transition from {currentStatus} to {newStatus}. Valid transitions: {string.Join(", ", validTransitions[currentStatus])}"
            );

        return (true, null);
    }

    /// <summary>
    /// Validates if an order can be cancelled
    /// </summary>
    public static (bool isValid, string? errorMessage) CanCancelOrder(OrderStatus currentStatus)
    {
        var cancellableStatuses = new[]
        {
            OrderStatus.Pending,
            OrderStatus.Confirmed,
            OrderStatus.Processing,
        };

        if (!cancellableStatuses.Contains(currentStatus))
            return (
                false,
                $"Orders with status {currentStatus} cannot be cancelled. Only orders in Pending, Confirmed, or Processing status can be cancelled."
            );

        return (true, null);
    }

    /// <summary>
    /// Validates if an order can be refunded
    /// </summary>
    public static (bool isValid, string? errorMessage) CanRefundOrder(
        OrderStatus currentStatus,
        decimal refundAmount,
        decimal orderTotal
    )
    {
        var refundableStatuses = new[]
        {
            OrderStatus.Delivered,
            OrderStatus.Completed,
            OrderStatus.Returned,
        };

        if (!refundableStatuses.Contains(currentStatus))
            return (
                false,
                $"Orders with status {currentStatus} cannot be refunded. Order must be Delivered, Completed, or Returned."
            );

        if (refundAmount <= 0)
            return (false, "Refund amount must be positive");

        if (refundAmount > orderTotal)
            return (
                false,
                $"Refund amount ({refundAmount}) cannot exceed order total ({orderTotal})"
            );

        return (true, null);
    }

    /// <summary>
    /// Validates if an order can be marked as paid
    /// </summary>
    public static (bool isValid, string? errorMessage) CanMarkOrderAsPaid(
        OrderStatus currentStatus,
        decimal paidAmount,
        decimal orderTotal
    )
    {
        var payableStatuses = new[]
        {
            OrderStatus.Pending,
            OrderStatus.Confirmed,
            OrderStatus.Processing,
        };

        if (!payableStatuses.Contains(currentStatus))
            return (false, $"Orders with status {currentStatus} cannot accept payment");

        if (paidAmount <= 0)
            return (false, "Payment amount must be positive");

        if (paidAmount < orderTotal)
            return (
                false,
                $"Payment amount ({paidAmount}) is less than order total ({orderTotal}). Partial payments not currently supported."
            );

        return (true, null);
    }

    /// <summary>
    /// Determines if an order requires shipping
    /// </summary>
    public static bool RequiresShipping(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Confirmed => true,
            OrderStatus.Processing => true,
            _ => false,
        };
    }

    /// <summary>
    /// Determines if an order requires inventory reservation
    /// </summary>
    public static bool RequiresInventoryReservation(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => true,
            OrderStatus.Confirmed => true,
            OrderStatus.Processing => true,
            _ => false,
        };
    }

    /// <summary>
    /// Determines if an order allows modifications
    /// </summary>
    public static bool AllowsModifications(OrderStatus status)
    {
        return status == OrderStatus.Pending;
    }

    /// <summary>
    /// Calculates order priority based on status and age
    /// </summary>
    public static int GetOrderPriority(OrderStatus status, DateTime createdAt)
    {
        var ageInDays = (DateTime.UtcNow - createdAt).Days;

        // Base priority by status
        var basePriority = status switch
        {
            OrderStatus.Processing => 1, // Highest priority
            OrderStatus.Confirmed => 2,
            OrderStatus.Pending => 3,
            OrderStatus.Shipped => 4,
            OrderStatus.Delivered => 5,
            OrderStatus.Completed => 6,
            OrderStatus.Cancelled => 7,
            OrderStatus.Returned => 8,
            OrderStatus.Refunded => 9,
            _ => 10,
        };

        // Increase priority for older orders
        if (ageInDays > 7)
            return Math.Max(1, basePriority - 2);
        if (ageInDays > 3)
            return Math.Max(1, basePriority - 1);

        return basePriority;
    }

    /// <summary>
    /// Validates order total calculation
    /// </summary>
    public static (bool isValid, string? errorMessage) ValidateOrderTotal(
        decimal itemsTotal,
        decimal shippingCost,
        decimal taxAmount,
        decimal discountAmount,
        decimal expectedTotal
    )
    {
        if (itemsTotal < 0)
            return (false, "Items total cannot be negative");

        if (shippingCost < 0)
            return (false, "Shipping cost cannot be negative");

        if (taxAmount < 0)
            return (false, "Tax amount cannot be negative");

        if (discountAmount < 0)
            return (false, "Discount amount cannot be negative");

        var calculatedTotal = itemsTotal + shippingCost + taxAmount - discountAmount;

        if (Math.Abs(calculatedTotal - expectedTotal) > 0.01m) // Allow for rounding differences
            return (
                false,
                $"Order total mismatch. Expected: {expectedTotal}, Calculated: {calculatedTotal}"
            );

        if (calculatedTotal < MinimumOrderValue)
            return (false, $"Order total must be at least {MinimumOrderValue}");

        return (true, null);
    }

    /// <summary>
    /// Determines if an order is overdue for processing
    /// </summary>
    public static bool IsOverdueForProcessing(OrderStatus status, DateTime createdAt)
    {
        var ageInHours = (DateTime.UtcNow - createdAt).TotalHours;

        return status switch
        {
            OrderStatus.Pending when ageInHours > 24 => true,
            OrderStatus.Confirmed when ageInHours > 48 => true,
            OrderStatus.Processing when ageInHours > 72 => true,
            _ => false,
        };
    }
}
