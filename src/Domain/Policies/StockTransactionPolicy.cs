using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Policies;

/// <summary>
/// Defines business rules for stock transactions and inventory movements
/// </summary>
public static class StockTransactionPolicy
{
    private const decimal MaxUnitCost = 1_000_000m;
    private const decimal MinUnitCost = 0.01m;
    private const int MaxTransactionQuantity = 100_000;
    private const int MaxNegativeAdjustmentPercent = 20; // Maximum 20% adjustment per transaction

    /// <summary>
    /// Validates if a purchase transaction can be recorded
    /// </summary>
    public static (bool isValid, string? errorMessage) CanRecordPurchase(
        int quantity,
        decimal unitCost,
        string location,
        string? documentNumber
    )
    {
        if (quantity <= 0)
            return (false, "Purchase quantity must be positive");

        if (quantity > MaxTransactionQuantity)
            return (false, $"Purchase quantity exceeds maximum allowed ({MaxTransactionQuantity})");

        if (unitCost < MinUnitCost || unitCost > MaxUnitCost)
            return (false, $"Unit cost must be between {MinUnitCost} and {MaxUnitCost}");

        if (string.IsNullOrWhiteSpace(location))
            return (false, "Location is required for purchase transactions");

        if (string.IsNullOrWhiteSpace(documentNumber))
            return (false, "Document number is required for purchase transactions");

        return (true, null);
    }

    /// <summary>
    /// Validates if a sale/fulfillment transaction can be recorded
    /// </summary>
    public static (bool isValid, string? errorMessage) CanRecordSale(
        int quantity,
        int availableStock,
        string location
    )
    {
        if (quantity <= 0)
            return (false, "Sale quantity must be positive");

        if (quantity > availableStock)
            return (
                false,
                $"Insufficient stock. Available: {availableStock}, Requested: {quantity}"
            );

        if (string.IsNullOrWhiteSpace(location))
            return (false, "Location is required for sale transactions");

        return (true, null);
    }

    /// <summary>
    /// Validates if a stock adjustment can be performed
    /// </summary>
    public static (bool isValid, string? errorMessage) CanRecordAdjustment(
        int currentStock,
        int adjustmentQuantity,
        string reason
    )
    {
        if (adjustmentQuantity == 0)
            return (false, "Adjustment quantity cannot be zero");

        var newStock = currentStock + adjustmentQuantity;
        if (newStock < 0)
            return (false, $"Adjustment would result in negative stock: {newStock}");

        // Validate large negative adjustments (potential theft or loss)
        if (adjustmentQuantity < 0)
        {
            var percentChange = Math.Abs(adjustmentQuantity * 100.0 / currentStock);
            if (percentChange > MaxNegativeAdjustmentPercent)
                return (
                    false,
                    $"Negative adjustment exceeds {MaxNegativeAdjustmentPercent}% of current stock. Use Loss transaction instead."
                );
        }

        if (string.IsNullOrWhiteSpace(reason))
            return (false, "Reason is required for adjustment transactions");

        return (true, null);
    }

    /// <summary>
    /// Validates if a loss transaction can be recorded
    /// </summary>
    public static (bool isValid, string? errorMessage) CanRecordLoss(
        int quantity,
        int currentStock,
        string reason
    )
    {
        if (quantity <= 0)
            return (false, "Loss quantity must be positive");

        if (quantity > currentStock)
            return (
                false,
                $"Loss quantity exceeds current stock. Current: {currentStock}, Loss: {quantity}"
            );

        if (string.IsNullOrWhiteSpace(reason))
            return (false, "Reason is required for loss transactions");

        return (true, null);
    }

    /// <summary>
    /// Validates if a transfer between locations can be performed
    /// </summary>
    public static (bool isValid, string? errorMessage) CanRecordTransfer(
        int quantity,
        int sourceAvailableStock,
        string fromLocation,
        string toLocation
    )
    {
        if (quantity <= 0)
            return (false, "Transfer quantity must be positive");

        if (quantity > sourceAvailableStock)
            return (
                false,
                $"Insufficient stock at source. Available: {sourceAvailableStock}, Requested: {quantity}"
            );

        if (string.IsNullOrWhiteSpace(fromLocation))
            return (false, "Source location is required for transfers");

        if (string.IsNullOrWhiteSpace(toLocation))
            return (false, "Destination location is required for transfers");

        if (fromLocation.Equals(toLocation, StringComparison.OrdinalIgnoreCase))
            return (false, "Source and destination locations must be different");

        return (true, null);
    }

    /// <summary>
    /// Validates if a reservation can be made
    /// </summary>
    public static (bool isValid, string? errorMessage) CanRecordReservation(
        int quantity,
        int availableStock,
        Guid? orderId
    )
    {
        if (quantity <= 0)
            return (false, "Reservation quantity must be positive");

        if (quantity > availableStock)
            return (
                false,
                $"Insufficient stock. Available: {availableStock}, Requested: {quantity}"
            );

        if (!orderId.HasValue)
            return (false, "Order ID is required for reservation transactions");

        return (true, null);
    }

    /// <summary>
    /// Validates if a reservation can be released
    /// </summary>
    public static (bool isValid, string? errorMessage) CanReleaseReservation(
        int quantity,
        int reservedStock,
        Guid? orderId
    )
    {
        if (quantity <= 0)
            return (false, "Release quantity must be positive");

        if (quantity > reservedStock)
            return (
                false,
                $"Cannot release more than reserved. Reserved: {reservedStock}, Requested: {quantity}"
            );

        if (!orderId.HasValue)
            return (false, "Order ID is required for reservation release transactions");

        return (true, null);
    }

    /// <summary>
    /// Validates if a return (purchase or sale) can be processed
    /// </summary>
    public static (bool isValid, string? errorMessage) CanRecordReturn(
        int quantity,
        decimal unitCost,
        string location,
        Guid? orderId,
        bool isPurchaseReturn
    )
    {
        if (quantity <= 0)
            return (false, "Return quantity must be positive");

        if (quantity > MaxTransactionQuantity)
            return (false, $"Return quantity exceeds maximum allowed ({MaxTransactionQuantity})");

        if (unitCost < MinUnitCost || unitCost > MaxUnitCost)
            return (false, $"Unit cost must be between {MinUnitCost} and {MaxUnitCost}");

        if (string.IsNullOrWhiteSpace(location))
            return (false, "Location is required for return transactions");

        if (!isPurchaseReturn && !orderId.HasValue)
            return (false, "Order ID is required for sale return transactions");

        return (true, null);
    }

    /// <summary>
    /// Validates the transaction type is appropriate for the operation
    /// </summary>
    public static bool IsValidTransactionType(InventoryTransactionType transactionType)
    {
        return Enum.IsDefined(typeof(InventoryTransactionType), transactionType);
    }

    /// <summary>
    /// Determines if a transaction requires accounting entry
    /// </summary>
    public static bool RequiresAccountingEntry(InventoryTransactionType transactionType)
    {
        return transactionType switch
        {
            InventoryTransactionType.Purchase => true,
            InventoryTransactionType.Sale => true,
            InventoryTransactionType.SaleReturn => true,
            InventoryTransactionType.PurchaseReturn => true,
            InventoryTransactionType.Adjustment => true,
            InventoryTransactionType.Loss => true,
            InventoryTransactionType.Fulfillment => true,
            InventoryTransactionType.Reservation => false,
            InventoryTransactionType.ReservationRelease => false,
            InventoryTransactionType.Transfer => false,
            _ => false,
        };
    }

    /// <summary>
    /// Determines if a transaction affects physical inventory
    /// </summary>
    public static bool AffectsPhysicalInventory(InventoryTransactionType transactionType)
    {
        return transactionType switch
        {
            InventoryTransactionType.Purchase => true,
            InventoryTransactionType.Sale => true,
            InventoryTransactionType.SaleReturn => true,
            InventoryTransactionType.PurchaseReturn => true,
            InventoryTransactionType.Adjustment => true,
            InventoryTransactionType.Loss => true,
            InventoryTransactionType.Fulfillment => true,
            InventoryTransactionType.Transfer => true,
            InventoryTransactionType.Reservation => false, // Only affects reserved quantity
            InventoryTransactionType.ReservationRelease => false, // Only affects reserved quantity
            _ => false,
        };
    }

    /// <summary>
    /// Validates if stock count is valid after transaction
    /// </summary>
    public static bool IsValidStockLevel(int stockLevel)
    {
        return stockLevel >= 0;
    }

    /// <summary>
    /// Calculates the net quantity change for a transaction
    /// </summary>
    public static int GetNetQuantityChange(InventoryTransactionType transactionType, int quantity)
    {
        return transactionType switch
        {
            InventoryTransactionType.Purchase => quantity,
            InventoryTransactionType.Sale => -quantity,
            InventoryTransactionType.SaleReturn => quantity,
            InventoryTransactionType.PurchaseReturn => -quantity,
            InventoryTransactionType.Adjustment => quantity, // Can be positive or negative
            InventoryTransactionType.Loss => -quantity,
            InventoryTransactionType.Fulfillment => -quantity,
            InventoryTransactionType.Transfer => 0, // Neutral for overall inventory
            InventoryTransactionType.Reservation => 0, // Doesn't change stock
            InventoryTransactionType.ReservationRelease => 0, // Doesn't change stock
            _ => 0,
        };
    }

    /// <summary>
    /// Determines priority level for transaction processing
    /// </summary>
    public static int GetTransactionPriority(InventoryTransactionType transactionType)
    {
        return transactionType switch
        {
            InventoryTransactionType.Loss => 1, // Highest priority
            InventoryTransactionType.Adjustment => 2,
            InventoryTransactionType.Sale => 3,
            InventoryTransactionType.Fulfillment => 3,
            InventoryTransactionType.Purchase => 4,
            InventoryTransactionType.Transfer => 5,
            InventoryTransactionType.SaleReturn => 6,
            InventoryTransactionType.PurchaseReturn => 6,
            InventoryTransactionType.Reservation => 7,
            InventoryTransactionType.ReservationRelease => 8,
            _ => 99,
        };
    }

    /// <summary>
    /// Validates transaction notes/reason length
    /// </summary>
    public static bool IsValidNotes(string? notes, int maxLength = 500)
    {
        if (string.IsNullOrWhiteSpace(notes))
            return true; // Notes are optional

        return notes.Length <= maxLength;
    }

    /// <summary>
    /// Checks if a transaction requires supervisor approval
    /// </summary>
    public static bool RequiresSupervisorApproval(
        InventoryTransactionType transactionType,
        int quantity,
        decimal totalCost
    )
    {
        const decimal HighValueThreshold = 50_000m;
        const int HighQuantityThreshold = 1_000;

        // High-value transactions require approval
        if (totalCost > HighValueThreshold)
            return true;

        // High-quantity transactions require approval
        if (quantity > HighQuantityThreshold)
            return true;

        // All loss transactions require approval
        if (transactionType == InventoryTransactionType.Loss)
            return true;

        // Large adjustments require approval
        if (transactionType == InventoryTransactionType.Adjustment && Math.Abs(quantity) > 100)
            return true;

        return false;
    }
}
