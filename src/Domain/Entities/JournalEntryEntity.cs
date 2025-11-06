namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents a journal entry in the double-entry bookkeeping system.
/// </summary>
/// <remarks>
/// A journal entry is a fundamental accounting record that contains a set of <see cref="AccountingEntryEntity"/>
/// instances representing debits and credits. Each journal entry must follow the double-entry bookkeeping principle
/// where the total of all debits equals the total of all credits.
/// Journal entries can be in draft status (not posted) allowing modifications, or posted status where they
/// become immutable and affect account balances. Once posted, entries cannot be modified to maintain audit integrity.
/// This implementation automatically links journal entries to their originating business transactions
/// (orders, inventory movements, payments) for complete traceability and compliance with accounting standards.
/// </remarks>
public sealed class JournalEntryEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for this journal entry.
    /// </summary>
    /// <value>A <see cref="Guid"/> that uniquely identifies the journal entry.</value>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the sequential entry number for human-readable identification.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> containing the sequential journal entry number.
    /// Typically formatted as JE-YYYYMMDD-XXXX where XXXX is the daily sequence.
    /// </value>
    /// <example>JE-20250106-0001, JE-20250106-0002</example>
    public string EntryNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the accounting date for this journal entry.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> representing when the transaction should be recorded in the books.
    /// This may differ from <see cref="CreatedAt"/> for backdated or future-dated entries.
    /// </value>
    public DateTime EntryDate { get; set; }

    /// <summary>
    /// Gets or sets the document type that originated this journal entry.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> identifying the source transaction type.
    /// Used for categorization and reporting purposes.
    /// </value>
    /// <example>Sales Invoice, Purchase Order, Inventory Adjustment, Payment Receipt</example>
    public string DocumentType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional reference number of the originating document.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> containing the document number from the source system,
    /// or <c>null</c> if not applicable.
    /// </value>
    /// <example>INV-2025-001, PO-12345, ADJ-789</example>
    public string? DocumentNumber { get; set; }

    /// <summary>
    /// Gets or sets the narrative description of this journal entry.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> containing the entry history or explanation.
    /// This describes what the transaction represents in business terms.
    /// </value>
    /// <example>Sale of goods to customer ABC123, Receipt of inventory from supplier, Monthly depreciation</example>
    public string History { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total monetary amount of this journal entry.
    /// </summary>
    /// <value>
    /// A <see cref="decimal"/> representing the sum of all debits (which must equal the sum of all credits).
    /// Used for validation and quick reference without querying all accounting entries.
    /// </value>
    /// <example>1500.00, 25000.00</example>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Gets or sets the optional reference to the related order.
    /// </summary>
    /// <value>
    /// A <see cref="Guid"/> referencing the <see cref="OrderEntity"/>,
    /// or <c>null</c> if this entry is not related to an order.
    /// </value>
    /// <example>7c9e6679-7425-40de-944b-e07fc1f90ae7</example>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// Gets or sets the optional reference to the related product.
    /// </summary>
    /// <value>
    /// A <see cref="Guid"/> referencing the <see cref="ProductEntity"/>,
    /// or <c>null</c> if this entry is not related to a specific product.
    /// </value>
    /// <example>8b3c8d7a-9c4e-4f5a-8d3c-9e5f8a7b6c5d</example>
    public Guid? ProductId { get; set; }

    /// <summary>
    /// Gets or sets the optional reference to the related inventory transaction.
    /// </summary>
    /// <value>
    /// A <see cref="Guid"/> referencing the <see cref="InventoryTransactionEntity"/>,
    /// or <c>null</c> if this entry is not related to inventory movement.
    /// </value>
    /// <example>9d4e5f8b-0d5f-4a6b-9e8d-0f1a2b3c4d5e</example>
    public Guid? InventoryTransactionId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this journal entry has been posted.
    /// </summary>
    /// <value>
    /// <c>true</c> if the entry is posted and its accounting entries affect account balances;
    /// <c>false</c> if the entry is in draft status and can still be modified.
    /// Once posted, entries become immutable to maintain accounting integrity.
    /// </value>
    public bool IsPosted { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this journal entry was posted.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> in UTC format representing when the entry was posted,
    /// or <c>null</c> if the entry is still in draft status.
    /// </value>
    public DateTime? PostedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who created this journal entry.
    /// </summary>
    /// <value>
    /// A <see cref="Guid"/> referencing the user or system account that created the entry.
    /// Used for audit trail and accountability.
    /// </value>
    /// <example>0e1f2a3b-4c5d-6e7f-8a9b-0c1d2e3f4a5b</example>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this journal entry was created.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> in UTC format representing the creation timestamp.
    /// Defaults to <see cref="DateTime.UtcNow"/> when the entity is instantiated.
    /// This represents when the entry was first recorded in the system.
    /// </value>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when this journal entry was last updated.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> in UTC format representing the last modification timestamp.
    /// Defaults to <see cref="DateTime.UtcNow"/> when the entity is instantiated.
    /// Only applicable to draft entries; posted entries cannot be modified.
    /// </value>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
