namespace ECommerce.Domain.Enums;

/// <summary>
/// Represents the status of a shipment
/// </summary>
public enum ShipmentStatus
{
    /// <summary>
    /// Shipment is being prepared
    /// </summary>
    Preparing = 0,

    /// <summary>
    /// Shipment is ready to be picked up
    /// </summary>
    ReadyForPickup = 1,

    /// <summary>
    /// Shipment has been picked up by carrier
    /// </summary>
    PickedUp = 2,

    /// <summary>
    /// Shipment is in transit
    /// </summary>
    InTransit = 3,

    /// <summary>
    /// Shipment is out for delivery
    /// </summary>
    OutForDelivery = 4,

    /// <summary>
    /// Shipment has been delivered
    /// </summary>
    Delivered = 5,

    /// <summary>
    /// Delivery attempt failed
    /// </summary>
    FailedDelivery = 6,

    /// <summary>
    /// Shipment is being returned
    /// </summary>
    Returning = 7,

    /// <summary>
    /// Shipment has been returned
    /// </summary>
    Returned = 8,

    /// <summary>
    /// Shipment was cancelled
    /// </summary>
    Cancelled = 9
}
