using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

/// <summary>
/// Individual accounting entry (Debit or Credit)
/// Each JournalEntry has multiple AccountingEntries (double-entry bookkeeping)
/// </summary>
public sealed class AccountingEntryEntity
{
    /// <summary>
    /// Unique entry identifier
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Journal entry to which this belongs
    /// </summary>
    public Guid JournalEntryId { get; set; }
    
    /// <summary>
    /// Debited or credited account
    /// </summary>
    public Guid AccountId { get; set; }
    
    /// <summary>
    /// Entry type (Debit or Credit)
    /// </summary>
    public EntryType EntryType { get; set; }
    
    /// <summary>
    /// Entry amount
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Complementary entry description
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Cost center (optional)
    /// </summary>
    public string? CostCenter { get; set; }
    
    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Navigation property to journal entry
    /// </summary>
    public JournalEntryEntity? JournalEntry { get; set; }
    
    /// <summary>
    /// Navigation property to account
    /// </summary>
    public ChartOfAccountsEntity? Account { get; set; }
}
