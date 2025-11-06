namespace ECommerce.API.DTOs;

/// <summary>
/// Data Transfer Object (DTO) for accounting journal entry responses
/// </summary>
/// <remarks>
/// <para>
/// Represents a complete accounting journal entry in the general ledger, containing
/// header information and a collection of debit and credit entries that must balance
/// according to double-entry bookkeeping principles.
/// </para>
/// <para>
/// <strong>Double-Entry Bookkeeping:</strong> Every journal entry must have equal
/// total debits and credits. This fundamental accounting principle ensures the
/// accounting equation (Assets = Liabilities + Equity) remains balanced.
/// </para>
/// <para>
/// <strong>Lifecycle:</strong>
/// </para>
/// <list type="number">
/// <item><description><strong>Draft:</strong> Entry is created but not posted (IsPosted = false)</description></item>
/// <item><description><strong>Posted:</strong> Entry is finalized and affects account balances (IsPosted = true)</description></item>
/// <item><description><strong>Reversed:</strong> A correcting entry is created to undo the original (separate entry)</description></item>
/// </list>
/// <para>
/// <strong>Integration:</strong> Journal entries are automatically created from inventory
/// transactions, sales orders, and purchase orders to maintain synchronized financial records.
/// </para>
/// </remarks>
public sealed class JournalEntryResponseDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the journal entry
    /// </summary>
    /// <value>
    /// A globally unique identifier (GUID) that uniquely identifies this journal entry.
    /// </value>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the unique entry number for reference and tracking
    /// </summary>
    /// <value>
    /// A system-generated sequential or formatted identifier for the journal entry
    /// (e.g., "JE-2025-00001").
    /// </value>
    /// <remarks>
    /// Entry numbers are used for searching, auditing, and referencing in financial reports.
    /// They typically follow a chronological sequence.
    /// </remarks>
    /// <example>JE-2025-00001</example>
    public string EntryNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the accounting date of the journal entry
    /// </summary>
    /// <value>
    /// The date this entry should be recorded in the accounting books for financial reporting purposes.
    /// </value>
    /// <remarks>
    /// This date determines which accounting period the entry affects and is used for
    /// period-end closing and financial statement generation. It may differ from
    /// <see cref="CreatedAt"/> which is the system timestamp.
    /// </remarks>
    /// <example>2025-11-06T00:00:00Z</example>
    public DateTime EntryDate { get; set; }

    /// <summary>
    /// Gets or sets the type of source document that generated this entry
    /// </summary>
    /// <value>
    /// The category or type of business transaction (e.g., "Invoice", "Receipt", "Adjustment", "Transfer").
    /// </value>
    /// <remarks>
    /// Helps classify entries by their origin for reporting and audit purposes.
    /// </remarks>
    /// <example>Invoice</example>
    public string DocumentType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the reference number of the source document
    /// </summary>
    /// <value>
    /// The identifier from the originating document such as invoice number, receipt number,
    /// or order number.
    /// </value>
    /// <remarks>
    /// Provides traceability between the journal entry and the source business transaction.
    /// Essential for audit trails and reconciliation.
    /// </remarks>
    /// <example>INV-2025-00123</example>
    public string DocumentNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the historical description or narrative of the transaction
    /// </summary>
    /// <value>
    /// A textual description explaining the business transaction and its purpose.
    /// </value>
    /// <remarks>
    /// Also known as the "memo" or "narrative", this field helps users understand
    /// the nature and reason for the journal entry when reviewing records.
    /// </remarks>
    /// <example>Sale of Blue T-Shirts to Customer ABC Corp</example>
    public string History { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total amount of the journal entry
    /// </summary>
    /// <value>
    /// The sum of all debit entries (which should equal the sum of all credit entries).
    /// </value>
    /// <remarks>
    /// In double-entry bookkeeping, this represents either the total debits or total credits,
    /// as both sides must be equal. This provides a quick reference for the entry's magnitude.
    /// </remarks>
    /// <example>2599.00</example>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Gets or sets whether the journal entry has been posted to the ledger
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the entry is posted and affects account balances;
    /// <see langword="false"/> if the entry is in draft status.
    /// </value>
    /// <remarks>
    /// <para>
    /// <strong>Draft Entries:</strong> Can be modified or deleted without affecting financial reports.
    /// </para>
    /// <para>
    /// <strong>Posted Entries:</strong> Are permanent and affect account balances. They typically
    /// cannot be edited directly and require reversal entries for corrections.
    /// </para>
    /// </remarks>
    /// <example>true</example>
    public bool IsPosted { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the entry was posted
    /// </summary>
    /// <value>
    /// The UTC date and time when the entry was posted to the ledger, or <c>null</c> if not yet posted.
    /// </value>
    /// <remarks>
    /// This timestamp records when the entry became permanent and started affecting account balances
    /// and financial reports. Used for audit trails and period-closing controls.
    /// </remarks>
    /// <example>2025-11-06T14:40:15Z</example>
    public DateTime? PostedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the related product
    /// </summary>
    /// <value>
    /// The GUID of the product associated with this entry, or <c>null</c> if not product-related.
    /// </value>
    /// <remarks>
    /// Used when the entry is related to product costs, inventory valuation, or COGS.
    /// Enables product-level profitability analysis and cost tracking.
    /// </remarks>
    /// <example>2c963f66-afa6-4562-b3fc-3fa85f645717</example>
    public Guid? ProductId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the related inventory transaction
    /// </summary>
    /// <value>
    /// The GUID of the inventory transaction that generated this journal entry,
    /// or <c>null</c> if not inventory-related.
    /// </value>
    /// <remarks>
    /// Links accounting entries to physical inventory movements, ensuring financial
    /// records match inventory operations. Essential for inventory accounting and COGS calculation.
    /// </remarks>
    /// <example>4fa85f64-5717-4562-b3fc-2c963f66afa7</example>
    public Guid? InventoryTransactionId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the related sales or purchase order
    /// </summary>
    /// <value>
    /// The GUID of the order associated with this entry, or <c>null</c> if not order-related.
    /// </value>
    /// <remarks>
    /// Links journal entries to orders for complete transaction tracking from order
    /// to fulfillment to accounting. Useful for revenue recognition and order profitability analysis.
    /// </remarks>
    /// <example>5fa85f64-5717-4562-b3fc-2c963f66afa8</example>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who created the journal entry
    /// </summary>
    /// <value>
    /// The GUID of the user account that created this journal entry.
    /// </value>
    /// <remarks>
    /// Used for audit trails and accountability. Important for financial controls
    /// and segregation of duties compliance.
    /// </remarks>
    /// <example>6fa85f64-5717-4562-b3fc-2c963f66afa9</example>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the journal entry record was created
    /// </summary>
    /// <value>
    /// The UTC date and time when this record was created in the system.
    /// </value>
    /// <remarks>
    /// This represents when the entry was initially created, which may differ from
    /// <see cref="EntryDate"/> (the accounting date) and <see cref="PostedAt"/> (when it was finalized).
    /// </remarks>
    /// <example>2025-11-06T14:35:00Z</example>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the collection of accounting line items (debits and credits)
    /// </summary>
    /// <value>
    /// A list of <see cref="AccountingEntryResponseDto"/> objects representing individual
    /// debit and credit lines that comprise this journal entry.
    /// </value>
    /// <remarks>
    /// <para>
    /// Each entry must specify an account, amount, and whether it's a debit or credit.
    /// The sum of all debit amounts must equal the sum of all credit amounts.
    /// </para>
    /// <para>
    /// <strong>Validation:</strong> At least two entries (one debit and one credit) are
    /// required for a valid journal entry, though complex transactions may have multiple lines.
    /// </para>
    /// </remarks>
    public List<AccountingEntryResponseDto> Entries { get; set; } = new();
}

/// <summary>
/// DTO for accounting entry response
/// </summary>
public sealed class AccountingEntryResponseDto
{
    /// <summary>
    /// Entry identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Account identifier
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    /// Account code
    /// </summary>
    public string AccountCode { get; set; } = string.Empty;

    /// <summary>
    /// Account name
    /// </summary>
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// Entry type (Debit or Credit)
    /// </summary>
    public string EntryType { get; set; } = string.Empty;

    /// <summary>
    /// Amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Cost center
    /// </summary>
    public string? CostCenter { get; set; }
}
