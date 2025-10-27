namespace ECommerce.Domain.Events;

/// <summary>
/// Event raised when a new review is submitted
/// </summary>
public sealed class ReviewSubmittedEvent : DomainEvent
{
    public Guid ReviewId { get; }
    public Guid ProductId { get; }
    public Guid CustomerId { get; }
    public int Rating { get; }
    public bool IsVerifiedPurchase { get; }

    public ReviewSubmittedEvent(
        Guid reviewId,
        Guid productId,
        Guid customerId,
        int rating,
        bool isVerifiedPurchase
    )
    {
        ReviewId = reviewId;
        ProductId = productId;
        CustomerId = customerId;
        Rating = rating;
        IsVerifiedPurchase = isVerifiedPurchase;
    }
}

/// <summary>
/// Event raised when a review is approved by admin
/// </summary>
public sealed class ReviewApprovedEvent : DomainEvent
{
    public Guid ReviewId { get; }
    public Guid ProductId { get; }
    public Guid ApprovedBy { get; }

    public ReviewApprovedEvent(Guid reviewId, Guid productId, Guid approvedBy)
    {
        ReviewId = reviewId;
        ProductId = productId;
        ApprovedBy = approvedBy;
    }
}

/// <summary>
/// Event raised when a review is flagged for moderation
/// </summary>
public sealed class ReviewFlaggedEvent : DomainEvent
{
    public Guid ReviewId { get; }
    public Guid ProductId { get; }
    public Guid FlaggedBy { get; }
    public string? Reason { get; }

    public ReviewFlaggedEvent(Guid reviewId, Guid productId, Guid flaggedBy, string? reason = null)
    {
        ReviewId = reviewId;
        ProductId = productId;
        FlaggedBy = flaggedBy;
        Reason = reason;
    }
}

/// <summary>
/// Event raised when a coupon is applied to an order
/// </summary>
public sealed class CouponAppliedEvent : DomainEvent
{
    public Guid OrderId { get; }
    public string CouponCode { get; }
    public decimal DiscountAmount { get; }
    public Guid CustomerId { get; }

    public CouponAppliedEvent(
        Guid orderId,
        string couponCode,
        decimal discountAmount,
        Guid customerId
    )
    {
        OrderId = orderId;
        CouponCode = couponCode;
        DiscountAmount = discountAmount;
        CustomerId = customerId;
    }
}

/// <summary>
/// Event raised when a coupon usage limit is reached
/// </summary>
public sealed class CouponExhaustedEvent : DomainEvent
{
    public Guid CouponId { get; }
    public string CouponCode { get; }
    public int TotalUsageCount { get; }

    public CouponExhaustedEvent(Guid couponId, string couponCode, int totalUsageCount)
    {
        CouponId = couponId;
        CouponCode = couponCode;
        TotalUsageCount = totalUsageCount;
    }
}
