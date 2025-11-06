using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Policies;

/// <summary>
/// Defines business rules for payment validation and processing
/// </summary>
public static class PaymentPolicy
{
    private const decimal MinimumPaymentAmount = 0.01m;
    private const decimal MaximumPaymentAmount = 999999.99m;
    private const int PaymentTimeoutMinutes = 30;
    private const int MaxPaymentRetries = 3;
    private const int RefundProcessingDays = 7;

    /// <summary>
    /// Validates if a payment amount is within acceptable limits
    /// </summary>
    public static bool IsValidPaymentAmount(decimal amount)
    {
        return amount >= MinimumPaymentAmount && amount <= MaximumPaymentAmount;
    }

    /// <summary>
    /// Checks if a payment method is supported for the given order
    /// </summary>
    public static bool IsPaymentMethodSupported(PaymentMethod method)
    {
        return method switch
        {
            PaymentMethod.CreditCard => true,
            PaymentMethod.DebitCard => true,
            PaymentMethod.PayPal => true,
            PaymentMethod.BankTransfer => true,
            PaymentMethod.CashOnDelivery => true,
            PaymentMethod.NotSpecified => false,
            _ => false,
        };
    }

    /// <summary>
    /// Checks if a payment status transition is valid
    /// </summary>
    public static bool IsValidStatusTransition(PaymentStatus currentStatus, PaymentStatus newStatus)
    {
        return (currentStatus, newStatus) switch
        {
            (PaymentStatus.Pending, PaymentStatus.Processing) => true,
            (PaymentStatus.Pending, PaymentStatus.Cancelled) => true,
            (PaymentStatus.Pending, PaymentStatus.OnHold) => true,
            (PaymentStatus.Processing, PaymentStatus.Completed) => true,
            (PaymentStatus.Processing, PaymentStatus.Failed) => true,
            (PaymentStatus.Processing, PaymentStatus.OnHold) => true,
            (PaymentStatus.OnHold, PaymentStatus.Processing) => true,
            (PaymentStatus.OnHold, PaymentStatus.Cancelled) => true,
            (PaymentStatus.Completed, PaymentStatus.Refunded) => true,
            (PaymentStatus.Completed, PaymentStatus.PartiallyRefunded) => true,
            (PaymentStatus.PartiallyRefunded, PaymentStatus.Refunded) => true,
            _ => false,
        };
    }

    /// <summary>
    /// Checks if a payment has timed out
    /// </summary>
    public static bool HasPaymentTimedOut(DateTime paymentInitiatedAt)
    {
        var timeElapsed = DateTime.UtcNow - paymentInitiatedAt;
        return timeElapsed.TotalMinutes > PaymentTimeoutMinutes;
    }

    /// <summary>
    /// Checks if payment retry is allowed
    /// </summary>
    public static bool CanRetryPayment(int retryCount)
    {
        return retryCount < MaxPaymentRetries;
    }

    /// <summary>
    /// Validates if a payment can be refunded
    /// </summary>
    public static bool CanRefundPayment(PaymentStatus status)
    {
        return status switch
        {
            PaymentStatus.Completed => true,
            PaymentStatus.PartiallyRefunded => true,
            _ => false,
        };
    }

    /// <summary>
    /// Validates partial refund amount
    /// </summary>
    public static bool IsValidPartialRefund(
        decimal refundAmount,
        decimal originalAmount,
        decimal alreadyRefundedAmount
    )
    {
        if (refundAmount <= 0)
            return false;

        var remainingAmount = originalAmount - alreadyRefundedAmount;
        return refundAmount <= remainingAmount && refundAmount < originalAmount;
    }

    /// <summary>
    /// Calculates expected refund processing date
    /// </summary>
    public static DateTime CalculateExpectedRefundDate()
    {
        return DateTime.UtcNow.AddDays(RefundProcessingDays);
    }

    /// <summary>
    /// Validates if payment amount matches order total
    /// </summary>
    public static bool DoesPaymentMatchOrderTotal(decimal paymentAmount, decimal orderTotal)
    {
        var tolerance = 0.01m; // Allow 1 cent tolerance for rounding
        return Math.Abs(paymentAmount - orderTotal) <= tolerance;
    }

    /// <summary>
    /// Checks if payment requires additional verification
    /// </summary>
    public static bool RequiresAdditionalVerification(
        decimal amount,
        decimal verificationThreshold,
        PaymentMethod method
    )
    {
        // High-value transactions or certain payment methods require verification
        if (amount >= verificationThreshold)
            return true;

        return method switch
        {
            PaymentMethod.BankTransfer => true,
            PaymentMethod.CashOnDelivery => amount >= verificationThreshold / 2,
            _ => false,
        };
    }

    /// <summary>
    /// Determines if automatic capture is allowed
    /// </summary>
    public static bool AllowsAutomaticCapture(PaymentMethod method)
    {
        return method switch
        {
            PaymentMethod.CreditCard => true,
            PaymentMethod.DebitCard => true,
            PaymentMethod.PayPal => true,
            PaymentMethod.BankTransfer => false, // Requires manual confirmation
            PaymentMethod.CashOnDelivery => false, // Payment occurs on delivery
            _ => false,
        };
    }

    /// <summary>
    /// Checks if payment requires address verification
    /// </summary>
    public static bool RequiresAddressVerification(PaymentMethod method)
    {
        return method switch
        {
            PaymentMethod.CreditCard => true,
            PaymentMethod.DebitCard => true,
            _ => false,
        };
    }
}
