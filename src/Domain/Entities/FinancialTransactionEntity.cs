using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents a financial transaction tracking cash flow and monetary movements in the system
/// </summary>
/// <remarks>
/// <para>
/// Financial transactions extend beyond accounting journal entries to provide a comprehensive
/// view of cash flow, accounts receivable, accounts payable, and payment reconciliation.
/// While <see cref="JournalEntryEntity"/> tracks double-entry bookkeeping for compliance,
/// <see cref="FinancialTransactionEntity"/> focuses on business financial operations and cash management.
/// </para>
/// <para>
/// <strong>Key Differences from Journal Entries:</strong>
/// </para>
/// <list type="bullet">
/// <item><description>Journal entries are accounting-focused (debits/credits for financial statements)</description></item>
/// <item><description>Financial transactions are business-focused (cash flow, payments, revenue recognition)</description></item>
/// <item><description>One business transaction may generate both a journal entry and multiple financial transactions</description></item>
/// </list>
/// <para>
/// <strong>Integration with Other Entities:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><see cref="InventoryTransactionEntity"/>: Links inventory movements to financial impact</description></item>
/// <item><description><see cref="JournalEntryEntity"/>: Provides accounting foundation for financial transactions</description></item>
/// <item><description><see cref="PaymentEntity"/>: Records actual payment processing and provider details</description></item>
/// <item><description><see cref="OrderEntity"/>: Connects sales transactions to customer orders</description></item>
/// </list>
/// <para>
/// <strong>Use Cases:</strong>
/// </para>
/// <list type="bullet">
/// <item><description>Cash flow analysis and forecasting</description></item>
/// <item><description>Accounts receivable and accounts payable management</description></item>
/// <item><description>Payment reconciliation with bank statements</description></item>
/// <item><description>Revenue recognition and expense tracking</description></item>
/// <item><description>Financial reporting and business intelligence</description></item>
/// </list>
/// <para>
/// <strong>Compliance and Audit:</strong>
/// All financial transactions maintain complete audit trails with timestamps,
/// responsible users, and references to source documents for regulatory compliance.
/// </para>
/// </remarks>
/// <example>
/// A sale transaction flow:
/// 1. Inventory transaction (reduces stock)
/// 2. Journal entry (COGS accounting)
/// 3. Financial transaction (revenue recognition)
/// 4. Financial transaction (accounts receivable)
/// 5. Payment entity (payment processing)
/// 6. Financial transaction (customer payment received)
/// </example>
public sealed class FinancialTransactionEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for this financial transaction
    /// </summary>
    /// <value>
    /// A <see cref="Guid"/> that uniquely identifies the financial transaction in the system.
    /// </value>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the sequential transaction number for human-readable identification
    /// </summary>
    /// <value>
    /// A unique string containing the sequential financial transaction number.
    /// Format: FIN-YYYYMMDD-XXXXXX where XXXXXX is a zero-padded 6-digit sequence.
    /// </value>
    /// <remarks>
    /// Used for references in reports, customer communications, and reconciliation.
    /// Generated automatically and guaranteed to be unique and chronological.
    /// </remarks>
    /// <example>FIN-20251108-000123</example>
    public string TransactionNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of financial transaction
    /// </summary>
    /// <value>
    /// The <see cref="FinancialTransactionType"/> enum value indicating the nature of this transaction.
    /// </value>
    /// <remarks>
    /// Transaction type determines the financial impact, accounting treatment,
    /// and which accounts are affected. Critical for cash flow classification and reporting.
    /// </remarks>
    /// <example>FinancialTransactionType.CustomerPayment</example>
    public FinancialTransactionType TransactionType { get; set; }

    /// <summary>
    /// Gets or sets the monetary amount of the transaction
    /// </summary>
    /// <value>
    /// The transaction amount in the system's base currency (typically USD or BRL).
    /// Positive amounts represent inflows (revenue, payments received),
    /// negative amounts represent outflows (expenses, payments made).
    /// </value>
    /// <remarks>
    /// <para>
    /// Amount interpretation by transaction type:
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Positive:</strong> CustomerPayment, SaleRevenue, AccountsReceivable</description></item>
    /// <item><description><strong>Negative:</strong> SupplierPayment, CustomerRefund, OperatingExpense</description></item>
    /// </list>
    /// </remarks>
    /// <example>1299.99</example>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the ISO 4217 three-letter currency code
    /// </summary>
    /// <value>
    /// The currency in which the transaction is denominated.
    /// Default is USD. Must match supported currencies in the system.
    /// </value>
    /// <example>USD</example>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Gets or sets the date when the financial transaction occurred
    /// </summary>
    /// <value>
    /// The UTC date and time when the financial event took place.
    /// May differ from <see cref="CreatedAt"/> for backdated or scheduled transactions.
    /// </value>
    /// <remarks>
    /// Critical for cash flow analysis, period-based reporting, and revenue recognition timing.
    /// Used to determine which accounting period the transaction belongs to.
    /// </remarks>
    /// <example>2025-11-08T14:30:00Z</example>
    public DateTime TransactionDate { get; set; }

    /// <summary>
    /// Gets or sets a descriptive explanation of the financial transaction
    /// </summary>
    /// <value>
    /// A human-readable description providing context about the transaction.
    /// Should be clear enough for financial review and audit purposes.
    /// </value>
    /// <example>Payment received for Order #ORD-12345 - Customer John Doe</example>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional reference to the related order
    /// </summary>
    /// <value>
    /// A <see cref="Guid"/> referencing the associated <see cref="OrderEntity"/>,
    /// or null if this transaction is not order-related.
    /// </value>
    /// <example>7c9e6679-7425-40de-944b-e07fc1f90ae7</example>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// Gets or sets the optional reference to the related payment
    /// </summary>
    /// <value>
    /// A <see cref="Guid"/> referencing the associated <see cref="PaymentEntity"/>,
    /// or null if this transaction is not payment-related.
    /// </value>
    /// <remarks>
    /// Links financial transactions to payment processing records for reconciliation.
    /// Essential for matching bank deposits with payment provider settlements.
    /// </remarks>
    /// <example>8b3c8d7a-9c4e-4f5a-8d3c-9e5f8a7b6c5d</example>
    public Guid? PaymentId { get; set; }

    /// <summary>
    /// Gets or sets the optional reference to the related inventory transaction
    /// </summary>
    /// <value>
    /// A <see cref="Guid"/> referencing the associated <see cref="InventoryTransactionEntity"/>,
    /// or null if this transaction is not inventory-related.
    /// </value>
    /// <remarks>
    /// Creates the link between inventory movements and their financial impact,
    /// enabling complete traceability from physical goods to financial records.
    /// </remarks>
    /// <example>9d4e5f8b-0d5f-4a6b-9e8d-0f1a2b3c4d5e</example>
    public Guid? InventoryTransactionId { get; set; }

    /// <summary>
    /// Gets or sets the optional reference to the related journal entry
    /// </summary>
    /// <value>
    /// A <see cref="Guid"/> referencing the associated <see cref="JournalEntryEntity"/>,
    /// or null if no journal entry is associated.
    /// </value>
    /// <remarks>
    /// Links financial transactions to their accounting foundation.
    /// One journal entry may result in multiple financial transactions
    /// (e.g., a sale creates revenue recognition and accounts receivable transactions).
    /// </remarks>
    /// <example>0e1f2a3b-4c5d-6e7f-8a9b-0c1d2e3f4a5b</example>
    public Guid? JournalEntryId { get; set; }

    /// <summary>
    /// Gets or sets the optional reference to the related product
    /// </summary>
    /// <value>
    /// A <see cref="Guid"/> referencing the associated <see cref="ProductEntity"/>,
    /// or null if this transaction is not product-specific.
    /// </value>
    /// <remarks>
    /// Enables product-level financial analysis, profitability tracking, and cost management.
    /// </remarks>
    /// <example>1f2a3b4c-5d6e-7f8a-9b0c-1d2e3f4a5b6c</example>
    public Guid? ProductId { get; set; }

    /// <summary>
    /// Gets or sets the party involved in the transaction (customer, supplier, etc.)
    /// </summary>
    /// <value>
    /// The name or identifier of the counterparty in the financial transaction.
    /// Could be a customer name, supplier company, payment processor, etc.
    /// </value>
    /// <example>ABC Suppliers Inc.</example>
    public string? Counterparty { get; set; }

    /// <summary>
    /// Gets or sets the external reference number from source documents
    /// </summary>
    /// <value>
    /// Reference number from external documents such as invoices, receipts,
    /// bank statements, or payment confirmations. Optional.
    /// </value>
    /// <remarks>
    /// Used for reconciliation with external systems, bank statements,
    /// and supplier/customer records. Critical for audit trails.
    /// </remarks>
    /// <example>INV-2025-001234</example>
    public string? ReferenceNumber { get; set; }

    /// <summary>
    /// Gets or sets whether the transaction has been reconciled with bank statements
    /// </summary>
    /// <value>
    /// True if the transaction has been matched and verified against bank records;
    /// false if still pending reconciliation. Default is false.
    /// </value>
    /// <remarks>
    /// Reconciliation ensures financial records match actual bank transactions.
    /// Unreconciled transactions require investigation and may indicate errors or fraud.
    /// </remarks>
    public bool IsReconciled { get; set; }

    /// <summary>
    /// Gets or sets the date when the transaction was reconciled
    /// </summary>
    /// <value>
    /// The UTC date and time when reconciliation was completed,
    /// or null if not yet reconciled.
    /// </value>
    /// <example>2025-11-10T09:15:00Z</example>
    public DateTime? ReconciledAt { get; set; }

    /// <summary>
    /// Gets or sets the user ID who reconciled the transaction
    /// </summary>
    /// <value>
    /// A <see cref="Guid"/> referencing the user who performed reconciliation,
    /// or null if not yet reconciled.
    /// </value>
    /// <remarks>
    /// Maintains accountability for reconciliation activities and audit trails.
    /// </remarks>
    public Guid? ReconciledBy { get; set; }

    /// <summary>
    /// Gets or sets the payment method used (for payment-related transactions)
    /// </summary>
    /// <value>
    /// The <see cref="PaymentMethod"/> enum value, or null if not applicable.
    /// </value>
    /// <remarks>
    /// Provides insight into cash flow by payment channel (credit card, bank transfer, etc.).
    /// Useful for analyzing payment processing costs and customer payment preferences.
    /// </remarks>
    /// <example>PaymentMethod.CreditCard</example>
    public PaymentMethod? PaymentMethod { get; set; }

    /// <summary>
    /// Gets or sets the payment provider (Stripe, PayPal, etc.)
    /// </summary>
    /// <value>
    /// The name of the payment processor or gateway,
    /// or null if not applicable or not processed through a provider.
    /// </value>
    /// <example>Stripe</example>
    public string? PaymentProvider { get; set; }

    /// <summary>
    /// Gets or sets the status of the financial transaction
    /// </summary>
    /// <value>
    /// Current status of the transaction: Pending, Completed, Cancelled, Failed.
    /// Default is Pending.
    /// </value>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description><strong>Pending:</strong> Initiated but not yet completed</description></item>
    /// <item><description><strong>Completed:</strong> Successfully processed and recorded</description></item>
    /// <item><description><strong>Cancelled:</strong> Transaction was cancelled before completion</description></item>
    /// <item><description><strong>Failed:</strong> Transaction attempted but failed</description></item>
    /// </list>
    /// </remarks>
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// Gets or sets optional notes or additional information
    /// </summary>
    /// <value>
    /// Internal notes, comments, or additional context about the transaction.
    /// May include reconciliation notes, approval comments, or investigation findings.
    /// </value>
    /// <example>Reconciled with bank statement dated 2025-11-09. Match confirmed.</example>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets the tax amount included in the transaction
    /// </summary>
    /// <value>
    /// The portion of the transaction amount that represents taxes (sales tax, VAT, etc.),
    /// or zero if no tax is applicable.
    /// </value>
    /// <remarks>
    /// Separating tax amounts is crucial for tax reporting, compliance,
    /// and understanding net revenue vs. tax collection obligations.
    /// </remarks>
    /// <example>129.99</example>
    public decimal TaxAmount { get; set; }

    /// <summary>
    /// Gets or sets the fee or commission amount
    /// </summary>
    /// <value>
    /// Fees charged for processing the transaction (payment processing fees,
    /// platform fees, commission, etc.), or zero if no fees apply.
    /// </value>
    /// <remarks>
    /// Tracking fees separately enables accurate profitability analysis
    /// and cost management for different transaction types and payment methods.
    /// </remarks>
    /// <example>38.99</example>
    public decimal FeeAmount { get; set; }

    /// <summary>
    /// Gets or sets the net amount after taxes and fees
    /// </summary>
    /// <value>
    /// The actual amount received or paid after deducting taxes and fees.
    /// Calculated as: Amount - TaxAmount - FeeAmount
    /// </value>
    /// <remarks>
    /// Net amount represents the true financial impact on the business.
    /// Used for cash flow analysis and profitability calculations.
    /// </remarks>
    /// <example>1131.01</example>
    public decimal NetAmount { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who created this transaction
    /// </summary>
    /// <value>
    /// A <see cref="Guid"/> referencing the user or system account that created the transaction.
    /// </value>
    /// <remarks>
    /// Provides accountability and audit trails. May represent an automated system
    /// process or a human user depending on transaction origination.
    /// </remarks>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this transaction was created
    /// </summary>
    /// <value>
    /// The UTC timestamp when the transaction record was first created in the system.
    /// Default value is the current UTC time.
    /// </value>
    /// <remarks>
    /// Represents when the transaction was recorded, which may differ from
    /// <see cref="TransactionDate"/> (the actual transaction occurrence date).
    /// </remarks>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when this transaction was last updated
    /// </summary>
    /// <value>
    /// The UTC timestamp of the most recent modification.
    /// Default value is the current UTC time.
    /// </value>
    /// <remarks>
    /// Updated when transaction status changes, reconciliation occurs,
    /// or any other modification is made to the transaction record.
    /// </remarks>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties

    /// <summary>
    /// Gets or sets the related inventory transaction
    /// </summary>
    /// <value>
    /// The <see cref="InventoryTransactionEntity"/> associated with this financial transaction,
    /// or null if not applicable.
    /// </value>
    public InventoryTransactionEntity? InventoryTransaction { get; set; }

    /// <summary>
    /// Gets or sets the related journal entry
    /// </summary>
    /// <value>
    /// The <see cref="JournalEntryEntity"/> associated with this financial transaction,
    /// or null if not applicable.
    /// </value>
    public JournalEntryEntity? JournalEntry { get; set; }

    /// <summary>
    /// Gets or sets the related payment
    /// </summary>
    /// <value>
    /// The <see cref="PaymentEntity"/> associated with this financial transaction,
    /// or null if not applicable.
    /// </value>
    public PaymentEntity? Payment { get; set; }
}
