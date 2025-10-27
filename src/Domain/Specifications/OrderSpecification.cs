using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Specifications;

/// <summary>
/// Specification for filtering orders based on various criteria
/// </summary>
public sealed class OrderSpecification
{
    public Guid? CustomerId { get; set; }
    public OrderStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public ShippingMethod? ShippingMethod { get; set; }
    public string? OrderNumber { get; set; }
    public string? CouponCode { get; set; }
    public bool? HasTrackingNumber { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Validates if the specification parameters are valid
    /// </summary>
    public bool IsValid()
    {
        if (FromDate.HasValue && ToDate.HasValue && FromDate.Value > ToDate.Value)
            return false;

        if (MinAmount.HasValue && MinAmount.Value < 0)
            return false;

        if (MaxAmount.HasValue && MaxAmount.Value < 0)
            return false;

        if (MinAmount.HasValue && MaxAmount.HasValue && MinAmount.Value > MaxAmount.Value)
            return false;

        if (PageNumber < 1 || PageSize < 1 || PageSize > 100)
            return false;

        return true;
    }

    /// <summary>
    /// Creates a specification for pending orders
    /// </summary>
    public static OrderSpecification PendingOrders()
    {
        return new OrderSpecification
        {
            Status = OrderStatus.Pending,
            SortBy = "CreatedAt",
            SortDescending = false, // Oldest first for processing
        };
    }

    /// <summary>
    /// Creates a specification for recent customer orders
    /// </summary>
    public static OrderSpecification CustomerRecentOrders(Guid customerId, int count = 10)
    {
        return new OrderSpecification
        {
            CustomerId = customerId,
            SortBy = "CreatedAt",
            SortDescending = true,
            PageSize = count,
        };
    }

    /// <summary>
    /// Creates a specification for orders requiring shipment
    /// </summary>
    public static OrderSpecification OrdersToShip()
    {
        return new OrderSpecification
        {
            Status = OrderStatus.Confirmed,
            HasTrackingNumber = false,
            SortBy = "CreatedAt",
            SortDescending = false,
        };
    }
}
