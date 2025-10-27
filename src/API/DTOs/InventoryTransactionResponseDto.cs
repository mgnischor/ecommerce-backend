using ECommerce.Domain.Enums;

namespace ECommerce.API.DTOs;

/// <summary>
/// DTO for inventory transaction response
/// </summary>
public sealed class InventoryTransactionResponseDto
{
    /// <summary>
    /// Transaction identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Transaction number
    /// </summary>
    public string TransactionNumber { get; set; } = string.Empty;

    /// <summary>
    /// Transaction date
    /// </summary>
    public DateTime TransactionDate { get; set; }

    /// <summary>
    /// Type of transaction
    /// </summary>
    public string TransactionType { get; set; } = string.Empty;

    /// <summary>
    /// Product identifier
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
    /// Source location
    /// </summary>
    public string? FromLocation { get; set; }

    /// <summary>
    /// Destination location
    /// </summary>
    public string ToLocation { get; set; } = string.Empty;

    /// <summary>
    /// Quantity moved
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Unit cost
    /// </summary>
    public decimal UnitCost { get; set; }

    /// <summary>
    /// Total cost
    /// </summary>
    public decimal TotalCost { get; set; }

    /// <summary>
    /// Related journal entry identifier
    /// </summary>
    public Guid? JournalEntryId { get; set; }

    /// <summary>
    /// Related order identifier
    /// </summary>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// Document number
    /// </summary>
    public string? DocumentNumber { get; set; }

    /// <summary>
    /// Notes
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// User who created the transaction
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
