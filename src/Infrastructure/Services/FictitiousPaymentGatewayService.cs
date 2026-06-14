using System.Text.Json;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Interfaces;

namespace ECommerce.Infrastructure.Services;

/// <summary>
/// A fictitious payment gateway that simulates real-world payment processing latency and outcomes.
/// </summary>
/// <remarks>
/// Each operation introduces a random delay between 1 and 10 seconds to mimic network round-trips
/// and bank authorization times. The outcome (success or failure) is also randomized, with
/// approximately 80% of payments succeeding and 20% failing — representative of a real gateway.
/// This implementation is intended for development, testing, and demonstration purposes only.
/// </remarks>
public sealed class FictitiousPaymentGatewayService : IPaymentGatewayService
{
    private const string ProviderName = "FictitiousGateway";
    private const double SuccessRate = 0.8;

    private static readonly string[] FailureReasons =
    [
        "Insufficient funds",
        "Card declined by issuer",
        "Transaction limit exceeded",
        "Suspected fraud – transaction blocked",
        "Invalid card number",
        "Card expired",
    ];

    private readonly ILoggingService _logger;
    private readonly Random _random = Random.Shared;

    public FictitiousPaymentGatewayService(ILoggingService logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<PaymentGatewayResult> ProcessPaymentAsync(
        Guid orderId,
        decimal amount,
        string currency,
        PaymentMethod method,
        CancellationToken cancellationToken = default
    )
    {
        var delaySeconds = _random.Next(1, 11); // [1, 10]
        _logger.LogInformation(
            "FictitiousGateway: Processing payment for Order={OrderId}, Amount={Amount} {Currency}, Method={Method}. Simulated latency: {Delay}s",
            orderId,
            amount,
            currency,
            method,
            delaySeconds
        );

        await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);

        var isSuccess = _random.NextDouble() < SuccessRate;
        var transactionId = GenerateTransactionId("TXN");
        var processedAt = DateTime.UtcNow;

        if (isSuccess)
        {
            var response = JsonSerializer.Serialize(
                new
                {
                    provider = ProviderName,
                    transaction_id = transactionId,
                    order_id = orderId,
                    amount,
                    currency,
                    payment_method = method.ToString(),
                    status = "captured",
                    error = (string?)null,
                    processed_at = processedAt,
                }
            );

            _logger.LogInformation(
                "FictitiousGateway: Payment APPROVED. TransactionId={TransactionId}, OrderId={OrderId}, Amount={Amount}",
                transactionId,
                orderId,
                amount
            );

            return new PaymentGatewayResult
            {
                IsSuccess = true,
                TransactionId = transactionId,
                Status = PaymentStatus.Completed,
                ProviderResponse = response,
                ProcessedAt = processedAt,
            };
        }
        else
        {
            var reason = FailureReasons[_random.Next(FailureReasons.Length)];
            var response = JsonSerializer.Serialize(
                new
                {
                    provider = ProviderName,
                    transaction_id = transactionId,
                    order_id = orderId,
                    amount,
                    currency,
                    payment_method = method.ToString(),
                    status = "failed",
                    error = reason,
                    processed_at = processedAt,
                }
            );

            _logger.LogWarning(
                "FictitiousGateway: Payment DECLINED. TransactionId={TransactionId}, OrderId={OrderId}, Reason={Reason}",
                transactionId,
                orderId,
                reason
            );

            return new PaymentGatewayResult
            {
                IsSuccess = false,
                TransactionId = transactionId,
                Status = PaymentStatus.Failed,
                ErrorMessage = reason,
                ProviderResponse = response,
                ProcessedAt = processedAt,
            };
        }
    }

    /// <inheritdoc />
    public async Task<PaymentGatewayResult> RefundPaymentAsync(
        string originalTransactionId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default
    )
    {
        var delaySeconds = _random.Next(1, 11);
        _logger.LogInformation(
            "FictitiousGateway: Processing refund for OriginalTxn={OriginalTxnId}, Amount={Amount} {Currency}. Simulated latency: {Delay}s",
            originalTransactionId,
            amount,
            currency,
            delaySeconds
        );

        await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);

        var isSuccess = _random.NextDouble() < SuccessRate;
        var refundTransactionId = GenerateTransactionId("REF");
        var processedAt = DateTime.UtcNow;

        if (isSuccess)
        {
            var response = JsonSerializer.Serialize(
                new
                {
                    provider = ProviderName,
                    refund_id = refundTransactionId,
                    original_transaction_id = originalTransactionId,
                    amount,
                    currency,
                    status = "refunded",
                    refunded_at = processedAt,
                }
            );

            _logger.LogInformation(
                "FictitiousGateway: Refund APPROVED. RefundId={RefundId}, OriginalTxn={OriginalTxnId}",
                refundTransactionId,
                originalTransactionId
            );

            return new PaymentGatewayResult
            {
                IsSuccess = true,
                TransactionId = refundTransactionId,
                Status = PaymentStatus.Refunded,
                ProviderResponse = response,
                ProcessedAt = processedAt,
            };
        }
        else
        {
            var reason = "Refund rejected by issuer – please contact support";
            var response = JsonSerializer.Serialize(
                new
                {
                    provider = ProviderName,
                    refund_id = refundTransactionId,
                    original_transaction_id = originalTransactionId,
                    amount,
                    currency,
                    status = "failed",
                    error = reason,
                    attempted_at = processedAt,
                }
            );

            _logger.LogWarning(
                "FictitiousGateway: Refund DECLINED. OriginalTxn={OriginalTxnId}, Reason={Reason}",
                originalTransactionId,
                reason
            );

            return new PaymentGatewayResult
            {
                IsSuccess = false,
                TransactionId = refundTransactionId,
                Status = PaymentStatus.Failed,
                ErrorMessage = reason,
                ProviderResponse = response,
                ProcessedAt = processedAt,
            };
        }
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static string GenerateTransactionId(string prefix)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        return $"{prefix}-{timestamp}-{suffix}";
    }
}
