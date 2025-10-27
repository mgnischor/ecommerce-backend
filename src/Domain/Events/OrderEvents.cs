namespace ECommerce.Domain.Events;

/// <summary>
/// Event raised when a new order is placed
/// </summary>
public sealed class OrderPlacedEvent : DomainEvent
{
    public Guid OrderId { get; }
    public Guid CustomerId { get; }
    public decimal TotalAmount { get; }
    public string OrderNumber { get; }

    public OrderPlacedEvent(Guid orderId, Guid customerId, decimal totalAmount, string orderNumber)
    {
        OrderId = orderId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
        OrderNumber = orderNumber;
    }
}

/// <summary>
/// Event raised when an order status changes
/// </summary>
public sealed class OrderStatusChangedEvent : DomainEvent
{
    public Guid OrderId { get; }
    public string OldStatus { get; }
    public string NewStatus { get; }
    public string? Reason { get; }

    public OrderStatusChangedEvent(
        Guid orderId,
        string oldStatus,
        string newStatus,
        string? reason = null
    )
    {
        OrderId = orderId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
        Reason = reason;
    }
}

/// <summary>
/// Event raised when an order is cancelled
/// </summary>
public sealed class OrderCancelledEvent : DomainEvent
{
    public Guid OrderId { get; }
    public Guid CustomerId { get; }
    public string? CancellationReason { get; }
    public decimal RefundAmount { get; }

    public OrderCancelledEvent(
        Guid orderId,
        Guid customerId,
        string? cancellationReason,
        decimal refundAmount
    )
    {
        OrderId = orderId;
        CustomerId = customerId;
        CancellationReason = cancellationReason;
        RefundAmount = refundAmount;
    }
}

/// <summary>
/// Event raised when an order is shipped
/// </summary>
public sealed class OrderShippedEvent : DomainEvent
{
    public Guid OrderId { get; }
    public Guid CustomerId { get; }
    public string TrackingNumber { get; }
    public DateTime ExpectedDeliveryDate { get; }

    public OrderShippedEvent(
        Guid orderId,
        Guid customerId,
        string trackingNumber,
        DateTime expectedDeliveryDate
    )
    {
        OrderId = orderId;
        CustomerId = customerId;
        TrackingNumber = trackingNumber;
        ExpectedDeliveryDate = expectedDeliveryDate;
    }
}

/// <summary>
/// Event raised when an order is delivered
/// </summary>
public sealed class OrderDeliveredEvent : DomainEvent
{
    public Guid OrderId { get; }
    public Guid CustomerId { get; }
    public DateTime DeliveredAt { get; }

    public OrderDeliveredEvent(Guid orderId, Guid customerId, DateTime deliveredAt)
    {
        OrderId = orderId;
        CustomerId = customerId;
        DeliveredAt = deliveredAt;
    }
}

/// <summary>
/// Event raised when a high-risk order is detected
/// </summary>
public sealed class HighRiskOrderDetectedEvent : DomainEvent
{
    public Guid OrderId { get; }
    public Guid CustomerId { get; }
    public int RiskScore { get; }
    public string RiskFactors { get; }

    public HighRiskOrderDetectedEvent(
        Guid orderId,
        Guid customerId,
        int riskScore,
        string riskFactors
    )
    {
        OrderId = orderId;
        CustomerId = customerId;
        RiskScore = riskScore;
        RiskFactors = riskFactors;
    }
}
