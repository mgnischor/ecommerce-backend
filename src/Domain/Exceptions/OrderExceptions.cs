using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Exceptions;

/// <summary>
/// Exception thrown when an invalid order status transition is attempted
/// </summary>
public sealed class InvalidOrderStatusTransitionException : DomainException
{
    public OrderStatus CurrentStatus { get; }
    public OrderStatus TargetStatus { get; }

    public InvalidOrderStatusTransitionException(
        OrderStatus currentStatus,
        OrderStatus targetStatus
    )
        : base($"Cannot transition order from {currentStatus} to {targetStatus}")
    {
        CurrentStatus = currentStatus;
        TargetStatus = targetStatus;
    }
}

/// <summary>
/// Exception thrown when an order cannot be cancelled
/// </summary>
public sealed class OrderCannotBeCancelledException : DomainException
{
    public Guid OrderId { get; }
    public OrderStatus CurrentStatus { get; }

    public OrderCannotBeCancelledException(Guid orderId, OrderStatus currentStatus)
        : base($"Order {orderId} cannot be cancelled. Current status: {currentStatus}")
    {
        OrderId = orderId;
        CurrentStatus = currentStatus;
    }
}

/// <summary>
/// Exception thrown when an order cannot be refunded
/// </summary>
public sealed class OrderCannotBeRefundedException : DomainException
{
    public Guid OrderId { get; }
    public string Reason { get; }

    public OrderCannotBeRefundedException(Guid orderId, string reason)
        : base($"Order {orderId} cannot be refunded: {reason}")
    {
        OrderId = orderId;
        Reason = reason;
    }
}

/// <summary>
/// Exception thrown when order validation fails
/// </summary>
public sealed class OrderValidationException : DomainException
{
    public List<string> ValidationErrors { get; }

    public OrderValidationException(string message)
        : base(message)
    {
        ValidationErrors = new List<string> { message };
    }

    public OrderValidationException(List<string> validationErrors)
        : base($"Order validation failed: {string.Join(", ", validationErrors)}")
    {
        ValidationErrors = validationErrors;
    }
}

/// <summary>
/// Exception thrown when an order is detected as potentially fraudulent
/// </summary>
public sealed class FraudulentOrderException : DomainException
{
    public Guid OrderId { get; }
    public int RiskScore { get; }

    public FraudulentOrderException(Guid orderId, int riskScore)
        : base($"Order {orderId} has been flagged as high risk (score: {riskScore})")
    {
        OrderId = orderId;
        RiskScore = riskScore;
    }
}
