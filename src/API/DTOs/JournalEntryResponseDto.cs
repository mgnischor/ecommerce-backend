namespace ECommerce.API.DTOs;

/// <summary>
/// DTO for journal entry response
/// </summary>
public sealed class JournalEntryResponseDto
{
    /// <summary>
    /// Journal entry identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Entry number
    /// </summary>
    public string EntryNumber { get; set; } = string.Empty;

    /// <summary>
    /// Entry date
    /// </summary>
    public DateTime EntryDate { get; set; }

    /// <summary>
    /// Document type
    /// </summary>
    public string DocumentType { get; set; } = string.Empty;

    /// <summary>
    /// Document number
    /// </summary>
    public string DocumentNumber { get; set; } = string.Empty;

    /// <summary>
    /// Historical description
    /// </summary>
    public string History { get; set; } = string.Empty;

    /// <summary>
    /// Total amount
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Whether the entry is posted
    /// </summary>
    public bool IsPosted { get; set; }

    /// <summary>
    /// Posted timestamp
    /// </summary>
    public DateTime? PostedAt { get; set; }

    /// <summary>
    /// Related product identifier
    /// </summary>
    public Guid? ProductId { get; set; }

    /// <summary>
    /// Related inventory transaction identifier
    /// </summary>
    public Guid? InventoryTransactionId { get; set; }

    /// <summary>
    /// Related order identifier
    /// </summary>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// User who created the entry
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// List of accounting entries (debits and credits)
    /// </summary>
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
