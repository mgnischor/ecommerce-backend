namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents an inventory planning record in the e-commerce system.
/// Tracks stock levels, demand, and performance metrics for forecasting and reordering.
/// </summary>
public class InventoryPlanEntity
{
    /// <summary>
    /// Unique identifier for the inventory plan
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Product associated with this inventory plan
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Current stock level on hand
    /// </summary>
    public int CurrentStock { get; set; }

    /// <summary>
    /// Stock level that triggers a reorder
    /// </summary>
    public int ReorderPoint { get; set; }

    /// <summary>
    /// Average daily sales used for forecasting
    /// </summary>
    public int AverageDailySales { get; set; }

    /// <summary>
    /// Supplier lead time in days
    /// </summary>
    public int? LeadTimeDays { get; set; }

    /// <summary>
    /// Target service level percentage for safety stock calculation
    /// </summary>
    public int? ServiceLevelPercent { get; set; }

    /// <summary>
    /// Date when the current stock was received
    /// </summary>
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Cost of goods sold used for inventory turnover calculation
    /// </summary>
    public decimal CostOfGoodsSold { get; set; }

    /// <summary>
    /// Average inventory value used for inventory turnover calculation
    /// </summary>
    public decimal AverageInventoryValue { get; set; }

    /// <summary>
    /// Units sold in the last 90 days used for slow-moving detection
    /// </summary>
    public int SalesInLast90Days { get; set; }

    /// <summary>
    /// Whether the inventory plan is deleted (soft delete)
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Date and time when the inventory plan was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the inventory plan was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
