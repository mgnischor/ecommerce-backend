namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents a customer in the e-commerce system.
/// Tracks spending, order history, and activity for segmentation and lifecycle analysis.
/// </summary>
public class CustomerEntity
{
    /// <summary>
    /// Unique identifier for the customer
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User account associated with this customer
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Total lifetime spending across all orders
    /// </summary>
    public decimal TotalSpending { get; set; }

    /// <summary>
    /// Total number of orders placed
    /// </summary>
    public int TotalOrders { get; set; }

    /// <summary>
    /// Date of the most recent order
    /// </summary>
    public DateTime? LastOrderDate { get; set; }

    /// <summary>
    /// Date of the most recent login
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Historical average number of days between orders
    /// </summary>
    public int? HistoricalAverageOrderDays { get; set; }

    /// <summary>
    /// Whether the customer account is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether the customer is deleted (soft delete)
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Date and time when the customer was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the customer was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
