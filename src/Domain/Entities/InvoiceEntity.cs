namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents an invoice in the e-commerce system.
/// Tracks invoice amounts, payment status, and payment terms.
/// </summary>
public class InvoiceEntity
{
    /// <summary>
    /// Unique identifier for the invoice
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User who created this invoice
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// User who last updated this invoice
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// Invoice reference number (5-50 alphanumeric characters with optional separators)
    /// </summary>
    public string InvoiceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Order associated with this invoice
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Customer the invoice is billed to
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Subtotal before taxes and shipping
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// Tax amount applied to the invoice
    /// </summary>
    public decimal TaxAmount { get; set; }

    /// <summary>
    /// Shipping cost charged on the invoice
    /// </summary>
    public decimal ShippingCost { get; set; }

    /// <summary>
    /// Total invoice amount (subtotal + tax + shipping)
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// Amount paid towards the invoice
    /// </summary>
    public decimal PaidAmount { get; set; }

    /// <summary>
    /// Date when payment is due
    /// </summary>
    public DateTime DueDate { get; set; }

    /// <summary>
    /// Date when the invoice was issued
    /// </summary>
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the invoice has been fully paid
    /// </summary>
    public bool IsPaid { get; set; } = false;

    /// <summary>
    /// Whether the invoice is deleted (soft delete)
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Date and time when the invoice was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the invoice was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
