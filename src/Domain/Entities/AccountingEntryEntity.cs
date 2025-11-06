using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents an individual accounting entry (Debit or Credit) in the double-entry bookkeeping system.
/// </summary>
/// <remarks>
/// Each <see cref="JournalEntryEntity"/> contains multiple <see cref="AccountingEntryEntity"/> instances
/// to maintain the accounting equation where total debits must equal total credits.
/// This follows the fundamental principle of double-entry bookkeeping as defined in NBC TG 26.
/// </remarks>
public sealed class AccountingEntryEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for this accounting entry.
    /// </summary>
    /// <value>A <see cref="Guid"/> that uniquely identifies the accounting entry.</value>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the journal entry to which this accounting entry belongs.
    /// </summary>
    /// <value>A <see cref="Guid"/> referencing the parent <see cref="JournalEntryEntity"/>.</value>
    /// <example>7c9e6679-7425-40de-944b-e07fc1f90ae7</example>
    public Guid JournalEntryId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the account being debited or credited.
    /// </summary>
    /// <value>A <see cref="Guid"/> referencing the <see cref="ChartOfAccountsEntity"/>.</value>
    /// <example>8b3c8d7a-9c4e-4f5a-8d3c-9e5f8a7b6c5d</example>
    public Guid AccountId { get; set; }

    /// <summary>
    /// Gets or sets the type of accounting entry.
    /// </summary>
    /// <value>
    /// An <see cref="EntryType"/> enumeration value indicating whether this is a debit or credit entry.
    /// Valid values: <see cref="EntryType.Debit"/>, <see cref="EntryType.Credit"/>
    /// </value>
    public EntryType EntryType { get; set; }

    /// <summary>
    /// Gets or sets the monetary amount of this accounting entry.
    /// </summary>
    /// <value>
    /// A <see cref="decimal"/> representing the amount in the system's base currency.
    /// Must be a positive value; the <see cref="EntryType"/> determines whether it's a debit or credit.
    /// </value>
    /// <example>1500.00</example>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets an optional complementary description for this entry.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> providing additional context or explanation for the accounting entry,
    /// or <c>null</c> if no additional description is needed.
    /// </value>
    /// <example>Payment received from customer order #12345</example>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the optional cost center code for management accounting purposes.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> identifying the cost center or department,
    /// or <c>null</c> if not applicable to this entry.
    /// </value>
    /// <example>DEPT-001, CC-SALES, WAREHOUSE-A</example>
    public string? CostCenter { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this accounting entry was created.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> in UTC format representing the creation timestamp.
    /// Defaults to <see cref="DateTime.UtcNow"/> when the entry is instantiated.
    /// </value>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the navigation property to the parent journal entry.
    /// </summary>
    /// <value>
    /// A <see cref="JournalEntryEntity"/> instance representing the parent journal entry,
    /// or <c>null</c> if not loaded.
    /// </value>
    public JournalEntryEntity? JournalEntry { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the related chart of accounts.
    /// </summary>
    /// <value>
    /// A <see cref="ChartOfAccountsEntity"/> instance representing the account being affected,
    /// or <c>null</c> if not loaded.
    /// </value>
    public ChartOfAccountsEntity? Account { get; set; }
}
