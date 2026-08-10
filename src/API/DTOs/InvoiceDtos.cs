using System.ComponentModel.DataAnnotations;

namespace ECommerce.API.DTOs;

/// <summary>
/// Data Transfer Object for recording a payment against an invoice
/// </summary>
public sealed class InvoicePaymentRequestDto
{
    /// <summary>
    /// Gets or sets the amount paid towards the invoice
    /// </summary>
    /// <value>The amount being paid. Must be greater than zero.</value>
    [Required(ErrorMessage = "Payment amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Payment amount must be greater than zero")]
    public decimal PaidAmount { get; set; }
}

/// <summary>
/// Data Transfer Object for issuing a credit note against an invoice
/// </summary>
public sealed class CreditNoteRequestDto
{
    /// <summary>
    /// Gets or sets the amount of the credit note
    /// </summary>
    /// <value>The credit note amount. Must be greater than zero and cannot exceed the invoice total.</value>
    [Required(ErrorMessage = "Credit note amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Credit note amount must be greater than zero")]
    public decimal Amount { get; set; }
}
