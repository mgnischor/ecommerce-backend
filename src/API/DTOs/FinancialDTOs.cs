namespace ECommerce.API.DTOs;

/// <summary>
/// Data Transfer Object (DTO) for financial transaction responses
/// </summary>
/// <remarks>
/// <para>
/// Represents a complete financial transaction record with all details including
/// amounts, dates, relationships to other entities, and reconciliation status.
/// </para>
/// <para>
/// <strong>Use Cases:</strong>
/// </para>
/// <list type="bullet">
/// <item><description>Cash flow analysis and reporting</description></item>
/// <item><description>Payment reconciliation with bank statements</description></item>
/// <item><description>Accounts receivable and payable tracking</description></item>
/// <item><description>Financial dashboard data</description></item>
/// <item><description>Audit trails and compliance reporting</description></item>
/// </list>
/// </remarks>
public sealed class FinancialTransactionResponseDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the financial transaction
    /// </summary>
    /// <value>The GUID that uniquely identifies this transaction.</value>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the sequential transaction number for human-readable identification
    /// </summary>
    /// <value>The formatted transaction number (FIN-YYYYMMDD-XXXXXX).</value>
    /// <example>FIN-20251108-000123</example>
    public string TransactionNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of financial transaction
    /// </summary>
    /// <value>Transaction type as string (CustomerPayment, SupplierPayment, SaleRevenue, etc.).</value>
    /// <example>CustomerPayment</example>
    public string TransactionType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the monetary amount of the transaction
    /// </summary>
    /// <value>
    /// Transaction amount where positive values represent inflows and negative values represent outflows.
    /// </value>
    /// <example>1299.99</example>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the ISO 4217 three-letter currency code
    /// </summary>
    /// <value>The currency in which the transaction is denominated.</value>
    /// <example>USD</example>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date when the financial transaction occurred
    /// </summary>
    /// <value>The UTC date and time of the transaction.</value>
    /// <example>2025-11-08T14:30:00Z</example>
    public DateTime TransactionDate { get; set; }

    /// <summary>
    /// Gets or sets the description of the transaction
    /// </summary>
    /// <value>Human-readable description providing context.</value>
    /// <example>Payment received for Order #ORD-12345 - Customer John Doe</example>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional related order ID
    /// </summary>
    /// <value>GUID of the associated order, or null if not order-related.</value>
    /// <example>7c9e6679-7425-40de-944b-e07fc1f90ae7</example>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// Gets or sets the optional related payment ID
    /// </summary>
    /// <value>GUID of the associated payment, or null if not payment-related.</value>
    /// <example>8b3c8d7a-9c4e-4f5a-8d3c-9e5f8a7b6c5d</example>
    public Guid? PaymentId { get; set; }

    /// <summary>
    /// Gets or sets the optional related inventory transaction ID
    /// </summary>
    /// <value>GUID of the associated inventory transaction, or null if not applicable.</value>
    /// <example>9d4e5f8b-0d5f-4a6b-9e8d-0f1a2b3c4d5e</example>
    public Guid? InventoryTransactionId { get; set; }

    /// <summary>
    /// Gets or sets the optional related journal entry ID
    /// </summary>
    /// <value>GUID of the associated accounting journal entry, or null if not applicable.</value>
    /// <example>0e1f2a3b-4c5d-6e7f-8a9b-0c1d2e3f4a5b</example>
    public Guid? JournalEntryId { get; set; }

    /// <summary>
    /// Gets or sets the optional related product ID
    /// </summary>
    /// <value>GUID of the associated product, or null if not product-specific.</value>
    /// <example>1f2a3b4c-5d6e-7f8a-9b0c-1d2e3f4a5b6c</example>
    public Guid? ProductId { get; set; }

    /// <summary>
    /// Gets or sets the counterparty in the transaction
    /// </summary>
    /// <value>Name or identifier of the other party (customer, supplier, etc.).</value>
    /// <example>ABC Suppliers Inc.</example>
    public string? Counterparty { get; set; }

    /// <summary>
    /// Gets or sets the external reference number
    /// </summary>
    /// <value>Reference from external documents (invoice, receipt, bank statement).</value>
    /// <example>INV-2025-001234</example>
    public string? ReferenceNumber { get; set; }

    /// <summary>
    /// Gets or sets whether the transaction has been reconciled
    /// </summary>
    /// <value>True if reconciled with bank statements; false otherwise.</value>
    public bool IsReconciled { get; set; }

    /// <summary>
    /// Gets or sets the reconciliation date
    /// </summary>
    /// <value>UTC date when reconciled, or null if not yet reconciled.</value>
    /// <example>2025-11-10T09:15:00Z</example>
    public DateTime? ReconciledAt { get; set; }

    /// <summary>
    /// Gets or sets the payment method used
    /// </summary>
    /// <value>Payment method as string, or null if not applicable.</value>
    /// <example>CreditCard</example>
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// Gets or sets the payment provider
    /// </summary>
    /// <value>Name of the payment processor, or null if not applicable.</value>
    /// <example>Stripe</example>
    public string? PaymentProvider { get; set; }

    /// <summary>
    /// Gets or sets the transaction status
    /// </summary>
    /// <value>Current status: Pending, Completed, Cancelled, Failed.</value>
    /// <example>Completed</example>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional notes
    /// </summary>
    /// <value>Internal notes or additional information.</value>
    /// <example>Reconciled with bank statement dated 2025-11-09</example>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets the tax amount
    /// </summary>
    /// <value>Portion of the transaction representing taxes.</value>
    /// <example>129.99</example>
    public decimal TaxAmount { get; set; }

    /// <summary>
    /// Gets or sets the fee amount
    /// </summary>
    /// <value>Processing fees or commissions charged.</value>
    /// <example>38.99</example>
    public decimal FeeAmount { get; set; }

    /// <summary>
    /// Gets or sets the net amount
    /// </summary>
    /// <value>Net amount after taxes and fees (Amount - TaxAmount - FeeAmount).</value>
    /// <example>1131.01</example>
    public decimal NetAmount { get; set; }

    /// <summary>
    /// Gets or sets the user ID who created the transaction
    /// </summary>
    /// <value>GUID of the user who created this transaction.</value>
    /// <example>2a3b4c5d-6e7f-8a9b-0c1d-2e3f4a5b6c7d</example>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the creation date and time
    /// </summary>
    /// <value>UTC timestamp when the transaction was created.</value>
    /// <example>2025-11-08T14:30:00Z</example>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update date and time
    /// </summary>
    /// <value>UTC timestamp of the most recent modification.</value>
    /// <example>2025-11-10T09:15:00Z</example>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Data Transfer Object (DTO) for cash flow report
/// </summary>
/// <remarks>
/// Provides a summary of cash inflows and outflows for a specific period,
/// categorized by transaction type for comprehensive cash flow analysis.
/// </remarks>
public sealed class CashFlowReportDto
{
    /// <summary>
    /// Gets or sets the start date of the reporting period
    /// </summary>
    /// <value>UTC date of the period start.</value>
    /// <example>2025-11-01T00:00:00Z</example>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Gets or sets the end date of the reporting period
    /// </summary>
    /// <value>UTC date of the period end.</value>
    /// <example>2025-11-30T23:59:59Z</example>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Gets or sets the total cash inflows
    /// </summary>
    /// <value>Sum of all positive cash movements (revenue, payments received).</value>
    /// <example>125000.00</example>
    public decimal TotalInflows { get; set; }

    /// <summary>
    /// Gets or sets the total cash outflows
    /// </summary>
    /// <value>Sum of all negative cash movements (expenses, payments made).</value>
    /// <example>87500.00</example>
    public decimal TotalOutflows { get; set; }

    /// <summary>
    /// Gets or sets the net cash flow
    /// </summary>
    /// <value>Net change in cash position (TotalInflows - TotalOutflows).</value>
    /// <example>37500.00</example>
    public decimal NetCashFlow { get; set; }

    /// <summary>
    /// Gets or sets the customer payments received
    /// </summary>
    /// <value>Total payments received from customers.</value>
    /// <example>120000.00</example>
    public decimal CustomerPayments { get; set; }

    /// <summary>
    /// Gets or sets the sales revenue recognized
    /// </summary>
    /// <value>Total revenue from sales.</value>
    /// <example>125000.00</example>
    public decimal SalesRevenue { get; set; }

    /// <summary>
    /// Gets or sets the supplier payments made
    /// </summary>
    /// <value>Total payments made to suppliers.</value>
    /// <example>65000.00</example>
    public decimal SupplierPayments { get; set; }

    /// <summary>
    /// Gets or sets the purchase expenses
    /// </summary>
    /// <value>Total cost of inventory purchased.</value>
    /// <example>70000.00</example>
    public decimal PurchaseExpenses { get; set; }

    /// <summary>
    /// Gets or sets the customer refunds issued
    /// </summary>
    /// <value>Total refunds given to customers.</value>
    /// <example>2500.00</example>
    public decimal Refunds { get; set; }

    /// <summary>
    /// Gets or sets the operating expenses
    /// </summary>
    /// <value>Total operating costs (excluding inventory and fees).</value>
    /// <example>15000.00</example>
    public decimal OperatingExpenses { get; set; }

    /// <summary>
    /// Gets or sets the payment processing fees
    /// </summary>
    /// <value>Total fees charged by payment processors.</value>
    /// <example>3500.00</example>
    public decimal PaymentFees { get; set; }

    /// <summary>
    /// Gets or sets the shipping costs
    /// </summary>
    /// <value>Total shipping and logistics expenses.</value>
    /// <example>4500.00</example>
    public decimal ShippingCosts { get; set; }

    /// <summary>
    /// Gets or sets the taxes collected or paid
    /// </summary>
    /// <value>Net tax amount (positive if collected, negative if paid).</value>
    /// <example>8000.00</example>
    public decimal Taxes { get; set; }
}

/// <summary>
/// Data Transfer Object (DTO) for accounts receivable summary
/// </summary>
/// <remarks>
/// Provides aging analysis and total outstanding receivables from customers,
/// essential for credit management and cash flow forecasting.
/// </remarks>
public sealed class AccountsReceivableDto
{
    /// <summary>
    /// Gets or sets the total accounts receivable
    /// </summary>
    /// <value>Total amount owed by customers.</value>
    /// <example>45000.00</example>
    public decimal TotalReceivable { get; set; }

    /// <summary>
    /// Gets or sets the current receivables (0-30 days)
    /// </summary>
    /// <value>Amount due within 30 days or less.</value>
    /// <example>32000.00</example>
    public decimal Current_0_30 { get; set; }

    /// <summary>
    /// Gets or sets the aging receivables (31-60 days)
    /// </summary>
    /// <value>Amount overdue by 31-60 days.</value>
    /// <example>8000.00</example>
    public decimal Aging_31_60 { get; set; }

    /// <summary>
    /// Gets or sets the aging receivables (61-90 days)
    /// </summary>
    /// <value>Amount overdue by 61-90 days.</value>
    /// <example>3000.00</example>
    public decimal Aging_61_90 { get; set; }

    /// <summary>
    /// Gets or sets the aging receivables (over 90 days)
    /// </summary>
    /// <value>Amount overdue by more than 90 days (high risk).</value>
    /// <example>2000.00</example>
    public decimal Aging_Over90 { get; set; }

    /// <summary>
    /// Gets or sets the number of outstanding invoices
    /// </summary>
    /// <value>Count of unpaid customer invoices.</value>
    /// <example>15</example>
    public int Count { get; set; }
}

/// <summary>
/// Data Transfer Object (DTO) for accounts payable summary
/// </summary>
/// <remarks>
/// Provides aging analysis and total outstanding payables to suppliers,
/// critical for managing supplier relationships and payment schedules.
/// </remarks>
public sealed class AccountsPayableDto
{
    /// <summary>
    /// Gets or sets the total accounts payable
    /// </summary>
    /// <value>Total amount owed to suppliers.</value>
    /// <example>28000.00</example>
    public decimal TotalPayable { get; set; }

    /// <summary>
    /// Gets or sets the current payables (0-30 days)
    /// </summary>
    /// <value>Amount due within 30 days or less.</value>
    /// <example>22000.00</example>
    public decimal Current_0_30 { get; set; }

    /// <summary>
    /// Gets or sets the aging payables (31-60 days)
    /// </summary>
    /// <value>Amount overdue by 31-60 days.</value>
    /// <example>4000.00</example>
    public decimal Aging_31_60 { get; set; }

    /// <summary>
    /// Gets or sets the aging payables (61-90 days)
    /// </summary>
    /// <value>Amount overdue by 61-90 days.</value>
    /// <example>1500.00</example>
    public decimal Aging_61_90 { get; set; }

    /// <summary>
    /// Gets or sets the aging payables (over 90 days)
    /// </summary>
    /// <value>Amount overdue by more than 90 days.</value>
    /// <example>500.00</example>
    public decimal Aging_Over90 { get; set; }

    /// <summary>
    /// Gets or sets the number of outstanding bills
    /// </summary>
    /// <value>Count of unpaid supplier bills.</value>
    /// <example>8</example>
    public int Count { get; set; }
}

/// <summary>
/// Data Transfer Object (DTO) for financial dashboard summary
/// </summary>
/// <remarks>
/// Provides key financial metrics and indicators for executive dashboards
/// and financial monitoring, offering a quick overview of financial health.
/// </remarks>
public sealed class FinancialDashboardDto
{
    /// <summary>
    /// Gets or sets the cash flow report
    /// </summary>
    /// <value>Detailed cash flow analysis for the period.</value>
    public CashFlowReportDto CashFlow { get; set; } = new();

    /// <summary>
    /// Gets or sets the accounts receivable summary
    /// </summary>
    /// <value>Current AR aging analysis.</value>
    public AccountsReceivableDto AccountsReceivable { get; set; } = new();

    /// <summary>
    /// Gets or sets the accounts payable summary
    /// </summary>
    /// <value>Current AP aging analysis.</value>
    public AccountsPayableDto AccountsPayable { get; set; } = new();

    /// <summary>
    /// Gets or sets the number of unreconciled transactions
    /// </summary>
    /// <value>Count of transactions not yet reconciled with bank statements.</value>
    /// <example>12</example>
    public int UnreconciledTransactions { get; set; }

    /// <summary>
    /// Gets or sets the total transaction count for the period
    /// </summary>
    /// <value>Total number of financial transactions.</value>
    /// <example>456</example>
    public int TotalTransactions { get; set; }

    /// <summary>
    /// Gets or sets the date of the last reconciliation
    /// </summary>
    /// <value>UTC timestamp of the most recent reconciliation, or null if none.</value>
    /// <example>2025-11-07T16:30:00Z</example>
    public DateTime? LastReconciliationDate { get; set; }
}

/// <summary>
/// Data Transfer Object (DTO) for reconciling a financial transaction
/// </summary>
/// <remarks>
/// Request payload for marking a transaction as reconciled after verifying
/// it matches bank statement records.
/// </remarks>
public sealed class ReconcileTransactionRequestDto
{
    /// <summary>
    /// Gets or sets optional reconciliation notes
    /// </summary>
    /// <value>
    /// Notes or comments about the reconciliation process,
    /// such as bank statement reference or discrepancy resolution.
    /// </value>
    /// <example>Reconciled with Chase bank statement #12345 dated 2025-11-08</example>
    public string? Notes { get; set; }
}
