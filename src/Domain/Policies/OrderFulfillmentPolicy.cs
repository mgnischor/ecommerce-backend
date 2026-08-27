using Comex.Domain.Enums;

namespace Comex.Domain.Policies;

/// <summary>
/// Defines business rules for order fulfillment and shipment planning
/// </summary>
public static class OrderFulfillmentPolicy
{
    private const int DefaultMaxItemsPerShipment = 20;
    private const decimal PalletWeightLimitKg = 500m;
    private const int DefaultFulfillmentWindowHours = 24;

    /// <summary>
    /// Checks if an order can begin the fulfillment process
    /// </summary>
    public static bool CanStartFulfillment(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Confirmed => true,
            OrderStatus.Processing => true,
            _ => false,
        };
    }

    /// <summary>
    /// Checks if a shipment can be split into multiple packages
    /// </summary>
    public static bool CanSplitShipment(int itemCount, int maxItemsPerShipment)
    {
        var effectiveMax = maxItemsPerShipment > 0 ? maxItemsPerShipment : DefaultMaxItemsPerShipment;
        return itemCount > effectiveMax;
    }

    /// <summary>
    /// Checks if an order is ready to be shipped
    /// </summary>
    public static bool IsReadyForShipment(bool isPaid, bool isInventoryAvailable, ShipmentStatus status)
    {
        if (!isPaid || !isInventoryAvailable)
            return false;

        return status switch
        {
            ShipmentStatus.Preparing => true,
            ShipmentStatus.ReadyForPickup => true,
            _ => false,
        };
    }

    /// <summary>
    /// Checks if a shipment requires palletization based on weight
    /// </summary>
    public static bool RequiresPalletization(decimal totalWeight, decimal? palletWeightLimit)
    {
        var effectiveLimit = palletWeightLimit.GetValueOrDefault(PalletWeightLimitKg);
        return totalWeight > effectiveLimit;
    }

    /// <summary>
    /// Determines the fulfillment priority for an order
    /// </summary>
    public static int GetFulfillmentPriority(OrderStatus status, DateTime createdAt)
    {
        var ageInHours = (DateTime.UtcNow - createdAt).TotalHours;

        // Base priority by status (lower number = higher priority)
        var basePriority = status switch
        {
            OrderStatus.Processing => 1,
            OrderStatus.Confirmed => 2,
            _ => 3,
        };

        // Orders approaching the fulfillment window get a boost
        if (ageInHours >= DefaultFulfillmentWindowHours)
            return Math.Max(1, basePriority - 1);

        return basePriority;
    }

    /// <summary>
    /// Checks if two orders can be combined into a single shipment
    /// </summary>
    public static bool CanCombineShipments(
        Guid customerId1,
        Guid customerId2,
        string shippingAddressKey1,
        string shippingAddressKey2
    )
    {
        // Orders can only be combined for the same customer and address
        return customerId1 == customerId2
            && string.Equals(
                shippingAddressKey1,
                shippingAddressKey2,
                StringComparison.OrdinalIgnoreCase
            );
    }

    /// <summary>
    /// Checks if an order is overdue for fulfillment
    /// </summary>
    public static bool IsOverdueForFulfillment(DateTime orderCreatedAt, int? allowedProcessingHours)
    {
        var effectiveHours = allowedProcessingHours ?? DefaultFulfillmentWindowHours;
        var ageInHours = (DateTime.UtcNow - orderCreatedAt).TotalHours;
        return ageInHours > effectiveHours;
    }

    /// <summary>
    /// Checks if an ongoing fulfillment can be cancelled
    /// </summary>
    public static bool CanCancelFulfillment(ShipmentStatus status)
    {
        return status switch
        {
            ShipmentStatus.Preparing => true,
            ShipmentStatus.ReadyForPickup => true,
            _ => false,
        };
    }
}
