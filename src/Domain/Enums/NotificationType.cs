namespace ECommerce.Domain.Enums;

/// <summary>
/// Represents the type of notification
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// General system notification
    /// </summary>
    System = 0,

    /// <summary>
    /// Order-related notification
    /// </summary>
    Order = 1,

    /// <summary>
    /// Payment-related notification
    /// </summary>
    Payment = 2,

    /// <summary>
    /// Shipment-related notification
    /// </summary>
    Shipment = 3,

    /// <summary>
    /// Product-related notification
    /// </summary>
    Product = 4,

    /// <summary>
    /// Promotional notification
    /// </summary>
    Promotion = 5,

    /// <summary>
    /// Account-related notification
    /// </summary>
    Account = 6,

    /// <summary>
    /// Review-related notification
    /// </summary>
    Review = 7
}
