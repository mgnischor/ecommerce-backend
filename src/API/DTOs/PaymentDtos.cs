using System.ComponentModel.DataAnnotations;
using ECommerce.Domain.Enums;

namespace ECommerce.API.DTOs;

/// <summary>
/// Request payload to initiate a payment for an existing order.
/// </summary>
public sealed class ProcessPaymentRequestDto
{
    /// <summary>The order to be paid.</summary>
    [Required]
    public Guid OrderId { get; set; }

    /// <summary>Payment method chosen by the customer.</summary>
    [Required]
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>
    /// ISO 4217 currency code (e.g. "USD", "BRL").
    /// Defaults to "USD" when not supplied.
    /// </summary>
    [StringLength(3, MinimumLength = 3)]
    public string Currency { get; set; } = "USD";
}

/// <summary>
/// Request payload to refund a previously captured payment.
/// </summary>
public sealed class RefundPaymentRequestDto
{
    /// <summary>
    /// Amount to refund. Must be greater than zero and at most equal to the original payment amount.
    /// Leave null to refund the full amount.
    /// </summary>
    [Range(0.01, double.MaxValue, ErrorMessage = "Refund amount must be greater than zero.")]
    public decimal? Amount { get; set; }
}

/// <summary>
/// Response returned after a payment or refund operation.
/// </summary>
public sealed class PaymentResponseDto
{
    public Guid PaymentId { get; init; }
    public Guid OrderId { get; init; }
    public string TransactionId { get; init; } = string.Empty;
    public PaymentStatus Status { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public PaymentMethod PaymentMethod { get; init; }
    public string? ErrorMessage { get; init; }
    public string ProviderResponse { get; init; } = string.Empty;
    public DateTime ProcessedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}
