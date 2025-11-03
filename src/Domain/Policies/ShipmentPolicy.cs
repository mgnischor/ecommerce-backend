using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Policies;

/// <summary>
/// Defines business rules for shipment validation and tracking
/// </summary>
public static class ShipmentPolicy
{
    private const int MaxTrackingNumberLength = 100;
    private const int MinTrackingNumberLength = 5;
    private const decimal MaxShippingCost = 999999.99m;
    private const decimal MaxPackageWeight = 500m; // kg
    private const int StandardDeliveryDays = 7;
    private const int ExpressDeliveryDays = 3;

    /// <summary>
    /// Validates if a tracking number format is acceptable
    /// </summary>
    public static bool IsValidTrackingNumber(string trackingNumber)
    {
        if (string.IsNullOrWhiteSpace(trackingNumber))
            return false;

        return trackingNumber.Length >= MinTrackingNumberLength
            && trackingNumber.Length <= MaxTrackingNumberLength;
    }

    /// <summary>
    /// Validates if a shipment status transition is valid
    /// </summary>
    public static bool IsValidStatusTransition(
        ShipmentStatus currentStatus,
        ShipmentStatus newStatus
    )
    {
        return (currentStatus, newStatus) switch
        {
            (ShipmentStatus.Preparing, ShipmentStatus.ReadyForPickup) => true,
            (ShipmentStatus.Preparing, ShipmentStatus.Cancelled) => true,
            (ShipmentStatus.ReadyForPickup, ShipmentStatus.PickedUp) => true,
            (ShipmentStatus.ReadyForPickup, ShipmentStatus.Cancelled) => true,
            (ShipmentStatus.PickedUp, ShipmentStatus.InTransit) => true,
            (ShipmentStatus.InTransit, ShipmentStatus.OutForDelivery) => true,
            (ShipmentStatus.InTransit, ShipmentStatus.Returning) => true,
            (ShipmentStatus.OutForDelivery, ShipmentStatus.Delivered) => true,
            (ShipmentStatus.OutForDelivery, ShipmentStatus.FailedDelivery) => true,
            (ShipmentStatus.FailedDelivery, ShipmentStatus.OutForDelivery) => true,
            (ShipmentStatus.FailedDelivery, ShipmentStatus.Returning) => true,
            (ShipmentStatus.Returning, ShipmentStatus.Returned) => true,
            _ => false,
        };
    }

    /// <summary>
    /// Validates if shipping cost is within acceptable range
    /// </summary>
    public static bool IsValidShippingCost(decimal cost)
    {
        return cost >= 0 && cost <= MaxShippingCost;
    }

    /// <summary>
    /// Validates if package weight is acceptable
    /// </summary>
    public static bool IsValidWeight(decimal weight)
    {
        return weight > 0 && weight <= MaxPackageWeight;
    }

    /// <summary>
    /// Validates if package dimensions are provided when required
    /// </summary>
    public static bool HasValidDimensions(decimal? length, decimal? width, decimal? height)
    {
        // If dimensions are provided, all three must be positive
        if (length.HasValue || width.HasValue || height.HasValue)
        {
            return length.GetValueOrDefault() > 0
                && width.GetValueOrDefault() > 0
                && height.GetValueOrDefault() > 0;
        }

        return true; // Dimensions are optional
    }

    /// <summary>
    /// Calculates volumetric weight for shipping
    /// </summary>
    public static decimal CalculateVolumetricWeight(
        decimal length,
        decimal width,
        decimal height,
        decimal divisor = 5000m
    )
    {
        // Standard formula: (L × W × H) / divisor
        return Math.Round((length * width * height) / divisor, 2);
    }

    /// <summary>
    /// Determines the billable weight (higher of actual or volumetric)
    /// </summary>
    public static decimal GetBillableWeight(
        decimal actualWeight,
        decimal? length,
        decimal? width,
        decimal? height
    )
    {
        if (!length.HasValue || !width.HasValue || !height.HasValue)
            return actualWeight;

        var volumetricWeight = CalculateVolumetricWeight(length.Value, width.Value, height.Value);
        return Math.Max(actualWeight, volumetricWeight);
    }

    /// <summary>
    /// Estimates delivery date based on shipping method
    /// </summary>
    public static DateTime EstimateDeliveryDate(DateTime shipDate, ShippingMethod shippingMethod)
    {
        var deliveryDays = shippingMethod switch
        {
            ShippingMethod.Express => ExpressDeliveryDays,
            ShippingMethod.NextDay => 1,
            ShippingMethod.SameDay => 0,
            ShippingMethod.Standard => StandardDeliveryDays,
            ShippingMethod.International => 10,
            _ => StandardDeliveryDays,
        };

        return shipDate.AddDays(deliveryDays);
    }

    /// <summary>
    /// Checks if a shipment can be cancelled
    /// </summary>
    public static bool CanCancelShipment(ShipmentStatus status)
    {
        return status switch
        {
            ShipmentStatus.Preparing => true,
            ShipmentStatus.ReadyForPickup => true,
            _ => false,
        };
    }

    /// <summary>
    /// Checks if a shipment requires insurance based on value
    /// </summary>
    public static bool ShouldRequireInsurance(
        decimal shipmentValue,
        decimal insuranceThreshold = 500m
    )
    {
        return shipmentValue >= insuranceThreshold;
    }

    /// <summary>
    /// Validates insurance amount
    /// </summary>
    public static bool IsValidInsuranceAmount(decimal insuranceAmount, decimal shipmentValue)
    {
        return insuranceAmount >= 0 && insuranceAmount <= shipmentValue;
    }

    /// <summary>
    /// Checks if shipment is overdue for delivery
    /// </summary>
    public static bool IsOverdue(DateTime? expectedDeliveryDate, ShipmentStatus status)
    {
        if (!expectedDeliveryDate.HasValue)
            return false;

        // Only consider overdue if not yet delivered
        if (status == ShipmentStatus.Delivered || status == ShipmentStatus.Returned)
            return false;

        return DateTime.UtcNow > expectedDeliveryDate.Value;
    }
}
