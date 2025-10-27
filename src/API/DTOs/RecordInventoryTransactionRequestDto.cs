using System.ComponentModel.DataAnnotations;
using ECommerce.Domain.Enums;

namespace ECommerce.API.DTOs;

/// <summary>
/// DTO for recording a new inventory transaction
/// </summary>
public sealed class RecordInventoryTransactionRequestDto
{
    /// <summary>
    /// Type of inventory transaction
    /// </summary>
    [Required(ErrorMessage = "Transaction type is required")]
    public InventoryTransactionType TransactionType { get; set; }

    /// <summary>
    /// Product identifier
    /// </summary>
    [Required(ErrorMessage = "Product ID is required")]
    public Guid ProductId { get; set; }

    /// <summary>
    /// Product SKU
    /// </summary>
    [Required(ErrorMessage = "Product SKU is required")]
    [StringLength(50, ErrorMessage = "Product SKU cannot exceed 50 characters")]
    public string ProductSku { get; set; } = string.Empty;

    /// <summary>
    /// Product name
    /// </summary>
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(200, ErrorMessage = "Product name cannot exceed 200 characters")]
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Quantity (positive for entries, negative for withdrawals)
    /// </summary>
    [Required(ErrorMessage = "Quantity is required")]
    public int Quantity { get; set; }

    /// <summary>
    /// Unit cost of the product
    /// </summary>
    [Required(ErrorMessage = "Unit cost is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Unit cost must be greater than zero")]
    public decimal UnitCost { get; set; }

    /// <summary>
    /// Source location (for transfers and withdrawals)
    /// </summary>
    [StringLength(100, ErrorMessage = "From location cannot exceed 100 characters")]
    public string? FromLocation { get; set; }

    /// <summary>
    /// Destination location (for transfers and entries)
    /// </summary>
    [Required(ErrorMessage = "To location is required")]
    [StringLength(100, ErrorMessage = "To location cannot exceed 100 characters")]
    public string ToLocation { get; set; } = string.Empty;

    /// <summary>
    /// Related order identifier (for sales and returns)
    /// </summary>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// Document number (invoice, receipt, etc.)
    /// </summary>
    [StringLength(100, ErrorMessage = "Document number cannot exceed 100 characters")]
    public string? DocumentNumber { get; set; }

    /// <summary>
    /// Additional notes or observations
    /// </summary>
    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }
}
