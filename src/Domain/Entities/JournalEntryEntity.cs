namespace ECommerce.Domain.Entities;

/// <summary>
/// Journal Entry - Represents a set of double-entry bookkeeping transactions
/// Follows the accounting principle that every debit must have a corresponding credit of equal value
/// </summary>
public sealed class JournalEntryEntity
{
    /// <summary>
    /// Unique entry identifier
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Sequential entry number
    /// </summary>
    public string EntryNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Entry date
    /// </summary>
    public DateTime EntryDate { get; set; }
    
    /// <summary>
    /// Document/transaction type that originated the entry
    /// </summary>
    public string DocumentType { get; set; } = string.Empty;
    
    /// <summary>
    /// Reference document number
    /// </summary>
    public string? DocumentNumber { get; set; }
    
    /// <summary>
    /// Entry history (description)
    /// </summary>
    public string History { get; set; } = string.Empty;
    
    /// <summary>
    /// Total entry amount
    /// </summary>
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// Related order reference
    /// </summary>
    public Guid? OrderId { get; set; }
    
    /// <summary>
    /// Related product reference
    /// </summary>
    public Guid? ProductId { get; set; }
    
    /// <summary>
    /// Related inventory transaction reference
    /// </summary>
    public Guid? InventoryTransactionId { get; set; }
    
    /// <summary>
    /// Indicates if the entry is posted (cannot be modified)
    /// </summary>
    public bool IsPosted { get; set; }
    
    /// <summary>
    /// Posted timestamp
    /// </summary>
    public DateTime? PostedAt { get; set; }
    
    /// <summary>
    /// User who created the entry
    /// </summary>
    public Guid CreatedBy { get; set; }
    
    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
