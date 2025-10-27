using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

/// <summary>
/// Inventory transaction record with complete traceability
/// Every inventory movement automatically generates a corresponding accounting entry
/// </summary>
public sealed class InventoryTransactionEntity
{
    /// <summary>
    /// Unique transaction identifier
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Sequential transaction number
    /// </summary>
    public string TransactionNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Transaction timestamp
    /// </summary>
    public DateTime TransactionDate { get; set; }
    
    /// <summary>
    /// Transaction type
    /// </summary>
    public InventoryTransactionType TransactionType { get; set; }
    
    /// <summary>
    /// Product being moved
    /// </summary>
    public Guid ProductId { get; set; }
    
    /// <summary>
    /// Product SKU
    /// </summary>
    public string ProductSku { get; set; } = string.Empty;
    
    /// <summary>
    /// Product name
    /// </summary>
    public string ProductName { get; set; } = string.Empty;
    
    /// <summary>
    /// Source location (for transfers)
    /// </summary>
    public string? FromLocation { get; set; }
    
    /// <summary>
    /// Destination location
    /// </summary>
    public string ToLocation { get; set; } = string.Empty;
    
    /// <summary>
    /// Quantity moved (positive for inbound, negative for outbound)
    /// </summary>
    public int Quantity { get; set; }
    
    /// <summary>
    /// Unit cost of goods
    /// </summary>
    public decimal UnitCost { get; set; }
    
    /// <summary>
    /// Total cost of transaction (Quantity * UnitCost)
    /// </summary>
    public decimal TotalCost { get; set; }
    
    /// <summary>
    /// Related order reference (if applicable)
    /// </summary>
    public Guid? OrderId { get; set; }
    
    /// <summary>
    /// Fiscal document number
    /// </summary>
    public string? DocumentNumber { get; set; }
    
    /// <summary>
    /// Transaction notes/comments
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Generated journal entry reference
    /// </summary>
    public Guid? JournalEntryId { get; set; }
    
    /// <summary>
    /// User responsible for the transaction
    /// </summary>
    public Guid CreatedBy { get; set; }
    
    /// <summary>
    /// Record creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Navigation property to product
    /// </summary>
    public ProductEntity? Product { get; set; }
    
    /// <summary>
    /// Navigation property to journal entry
    /// </summary>
    public JournalEntryEntity? JournalEntry { get; set; }
}
