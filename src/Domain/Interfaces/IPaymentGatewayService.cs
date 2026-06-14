using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Interfaces;

/// <summary>
/// Represents the result returned by the payment gateway after processing a payment or refund.
/// </summary>
public sealed class PaymentGatewayResult
{
    public bool IsSuccess { get; init; }
    public string TransactionId { get; init; } = string.Empty;
    public PaymentStatus Status { get; init; }
    public string? ErrorMessage { get; init; }
    public string ProviderResponse { get; init; } = string.Empty;
    public DateTime ProcessedAt { get; init; }
}

/// <summary>
/// Contract for an external payment gateway integration.
/// </summary>
/// <remarks>
/// Implementations must simulate or communicate with a real payment provider.
/// Processing time is intentionally non-deterministic to reflect real-world latency.
/// </remarks>
public interface IPaymentGatewayService
{
    /// <summary>
    /// Sends a payment authorization and capture request to the gateway.
    /// </summary>
    /// <param name="orderId">The order being paid.</param>
    /// <param name="amount">The amount to charge.</param>
    /// <param name="currency">ISO 4217 currency code (e.g. "USD").</param>
    /// <param name="method">Payment method chosen by the customer.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The gateway result containing the transaction outcome.</returns>
    Task<PaymentGatewayResult> ProcessPaymentAsync(
        Guid orderId,
        decimal amount,
        string currency,
        PaymentMethod method,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Sends a refund request to the gateway for a previously captured transaction.
    /// </summary>
    /// <param name="originalTransactionId">The transaction ID returned by <see cref="ProcessPaymentAsync"/>.</param>
    /// <param name="amount">The amount to refund (may be partial).</param>
    /// <param name="currency">ISO 4217 currency code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The gateway result containing the refund outcome.</returns>
    Task<PaymentGatewayResult> RefundPaymentAsync(
        string originalTransactionId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default
    );
}
