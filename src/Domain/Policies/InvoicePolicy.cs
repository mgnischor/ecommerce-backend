namespace ECommerce.Domain.Policies;

/// <summary>
/// Defines business rules for invoice generation and payment terms
/// </summary>
public static class InvoicePolicy
{
    private const int MinInvoiceNumberLength = 5;
    private const int MaxInvoiceNumberLength = 50;

    /// <summary>
    /// Validates an invoice number format
    /// </summary>
    public static bool IsValidInvoiceNumber(string? invoiceNumber)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            return false;

        if (invoiceNumber.Length is < MinInvoiceNumberLength or > MaxInvoiceNumberLength)
            return false;

        // Invoice numbers typically contain digits, letters, and separators
        return invoiceNumber.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '/');
    }

    /// <summary>
    /// Checks if an invoice has been fully paid
    /// </summary>
    public static bool IsInvoicePaid(decimal paidAmount, decimal invoiceTotal)
    {
        if (invoiceTotal <= 0)
            return false;

        return paidAmount >= invoiceTotal;
    }

    /// <summary>
    /// Checks if a credit note can be issued for an invoice
    /// </summary>
    public static bool CanIssueCreditNote(
        bool isPaid,
        bool isWithinPeriod,
        decimal amount,
        decimal invoiceTotal
    )
    {
        if (!isPaid || !isWithinPeriod)
            return false;

        if (amount <= 0)
            return false;

        // Credit note cannot exceed the invoice total
        return amount <= invoiceTotal;
    }

    /// <summary>
    /// Calculates the invoice total from its components
    /// </summary>
    public static decimal CalculateInvoiceTotal(
        decimal subtotal,
        decimal taxAmount,
        decimal shippingCost
    )
    {
        if (subtotal < 0 || taxAmount < 0 || shippingCost < 0)
            return 0m;

        return Math.Round(subtotal + taxAmount + shippingCost, 2);
    }

    /// <summary>
    /// Checks if an invoice is still within its payment terms
    /// </summary>
    public static bool IsWithinPaymentTerms(DateTime dueDate)
    {
        return DateTime.UtcNow <= dueDate;
    }

    /// <summary>
    /// Checks if an order requires prepayment
    /// </summary>
    public static bool RequiresPrepayment(
        bool isNewCustomer,
        decimal orderTotal,
        decimal? prepaymentThreshold
    )
    {
        if (isNewCustomer)
            return true;

        if (!prepaymentThreshold.HasValue || prepaymentThreshold.Value <= 0)
            return false;

        return orderTotal >= prepaymentThreshold.Value;
    }
}
