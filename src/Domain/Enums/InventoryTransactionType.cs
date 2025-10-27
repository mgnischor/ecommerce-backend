namespace ECommerce.Domain.Enums;

/// <summary>
/// Inventory transaction types
/// </summary>
public enum InventoryTransactionType
{
    /// <summary>
    /// Purchase of goods/raw materials
    /// </summary>
    Purchase = 1,
    
    /// <summary>
    /// Sale of goods
    /// </summary>
    Sale = 2,
    
    /// <summary>
    /// Sales return
    /// </summary>
    SaleReturn = 3,
    
    /// <summary>
    /// Purchase return
    /// </summary>
    PurchaseReturn = 4,
    
    /// <summary>
    /// Inventory adjustment (physical count)
    /// </summary>
    Adjustment = 5,
    
    /// <summary>
    /// Transfer between locations
    /// </summary>
    Transfer = 6,
    
    /// <summary>
    /// Loss/shrinkage/breakage
    /// </summary>
    Loss = 7,
    
    /// <summary>
    /// Stock reservation for order
    /// </summary>
    Reservation = 8,
    
    /// <summary>
    /// Release of reservation
    /// </summary>
    ReservationRelease = 9,
    
    /// <summary>
    /// Fulfillment (sale shipment)
    /// </summary>
    Fulfillment = 10
}
