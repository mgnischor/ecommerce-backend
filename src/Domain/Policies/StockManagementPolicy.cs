namespace ECommerce.Domain.Policies;

/// <summary>
/// Defines business rules for inventory and stock management
/// </summary>
public static class StockManagementPolicy
{
    private const int AbsoluteMinimumStock = 0;
    private const int DefaultLowStockThreshold = 10;
    private const int DefaultReorderQuantity = 50;

    /// <summary>
    /// Checks if a product is out of stock
    /// </summary>
    public static bool IsOutOfStock(int currentStock)
    {
        return currentStock <= 0;
    }

    /// <summary>
    /// Checks if a product is running low on stock
    /// </summary>
    public static bool IsLowStock(int currentStock, int minStockLevel)
    {
        var threshold = minStockLevel > 0 ? minStockLevel : DefaultLowStockThreshold;
        return currentStock > 0 && currentStock <= threshold;
    }

    /// <summary>
    /// Validates if a quantity can be reserved from available stock
    /// </summary>
    public static bool CanReserveStock(int currentStock, int requestedQuantity)
    {
        if (requestedQuantity <= 0)
            return false;

        return currentStock >= requestedQuantity;
    }

    /// <summary>
    /// Validates if a quantity is within order limits
    /// </summary>
    public static bool IsQuantityWithinOrderLimits(int quantity, int maxOrderQuantity)
    {
        if (quantity <= 0)
            return false;

        if (maxOrderQuantity <= 0)
            return true; // No limit set

        return quantity <= maxOrderQuantity;
    }

    /// <summary>
    /// Calculates the recommended reorder quantity
    /// </summary>
    public static int CalculateReorderQuantity(
        int currentStock,
        int minStockLevel,
        int averageDailySales
    )
    {
        if (averageDailySales <= 0)
            return DefaultReorderQuantity;

        // Reorder to cover 30 days of sales plus minimum stock level
        var thirtyDaysSupply = averageDailySales * 30;
        var targetStock = thirtyDaysSupply + minStockLevel;
        var reorderQuantity = targetStock - currentStock;

        return Math.Max(reorderQuantity, DefaultReorderQuantity);
    }

    /// <summary>
    /// Validates stock adjustment amount
    /// </summary>
    public static bool IsValidStockAdjustment(int currentStock, int adjustmentAmount)
    {
        var newStock = currentStock + adjustmentAmount;
        return newStock >= AbsoluteMinimumStock;
    }

    /// <summary>
    /// Checks if automatic reorder is needed
    /// </summary>
    public static bool ShouldTriggerReorder(int currentStock, int minStockLevel, int reorderPoint)
    {
        if (reorderPoint > 0)
            return currentStock <= reorderPoint;

        return IsLowStock(currentStock, minStockLevel);
    }
}
