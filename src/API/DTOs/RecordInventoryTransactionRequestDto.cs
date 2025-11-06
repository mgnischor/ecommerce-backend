using System.ComponentModel.DataAnnotations;
using ECommerce.Domain.Enums;

namespace ECommerce.API.DTOs;

/// <summary>
/// Data Transfer Object (DTO) for recording a new inventory transaction request
/// </summary>
/// <remarks>
/// <para>
/// This DTO captures all required and optional information needed to record an inventory
/// movement in the system. It includes comprehensive DataAnnotations validation to ensure
/// data integrity before processing.
/// </para>
/// <para>
/// <strong>Transaction Types:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><strong>StockEntry:</strong> Receiving inventory (purchases, production, returns from customers)</description></item>
/// <item><description><strong>StockWithdrawal:</strong> Removing inventory (sales, consumption, defects)</description></item>
/// <item><description><strong>StockTransfer:</strong> Moving inventory between locations (requires FromLocation)</description></item>
/// <item><description><strong>StockAdjustment:</strong> Correcting inventory levels (physical counts, corrections)</description></item>
/// <item><description><strong>StockReturn:</strong> Returning inventory to suppliers or receiving from customers</description></item>
/// </list>
/// <para>
/// <strong>Automatic Processing:</strong> Upon successful validation, this transaction will:
/// </para>
/// <list type="number">
/// <item><description>Update inventory levels at the specified locations</description></item>
/// <item><description>Generate a corresponding journal entry in the accounting system</description></item>
/// <item><description>Create a permanent audit trail with transaction number</description></item>
/// <item><description>Update product cost information if applicable</description></item>
/// </list>
/// <para>
/// <strong>Example Request:</strong>
/// </para>
/// <code>
/// POST /api/v1/inventory/transactions
/// {
///   "transactionType": "StockEntry",
///   "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
///   "productSku": "PROD-001",
///   "productName": "Blue T-Shirt",
///   "quantity": 100,
///   "unitCost": 25.99,
///   "toLocation": "Warehouse A - Shelf 12",
///   "documentNumber": "PO-2025-00123",
///   "notes": "Initial stock receipt from supplier"
/// }
/// </code>
/// </remarks>
public sealed class RecordInventoryTransactionRequestDto
{
    /// <summary>
    /// Gets or sets the type of inventory transaction to record
    /// </summary>
    /// <value>
    /// A value from the <see cref="InventoryTransactionType"/> enum specifying the nature
    /// of the inventory movement.
    /// </value>
    /// <remarks>
    /// <para>
    /// The transaction type determines how inventory levels are affected and which
    /// accounting entries are generated. This field is required for all transactions.
    /// </para>
    /// <para>
    /// <strong>Valid Values:</strong> StockEntry, StockWithdrawal, StockTransfer, StockAdjustment, StockReturn
    /// </para>
    /// </remarks>
    /// <example>StockEntry</example>
    [Required(ErrorMessage = "Transaction type is required")]
    public InventoryTransactionType TransactionType { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the product being transacted
    /// </summary>
    /// <value>
    /// The GUID of the product in the product catalog. This field is required.
    /// </value>
    /// <remarks>
    /// The product must exist in the system. This ID is used to update the product's
    /// inventory levels and retrieve product information for accounting entries.
    /// </remarks>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    [Required(ErrorMessage = "Product ID is required")]
    public Guid ProductId { get; set; }

    /// <summary>
    /// Gets or sets the Stock Keeping Unit (SKU) of the product
    /// </summary>
    /// <value>
    /// The unique product identifier, maximum 50 characters. This field is required.
    /// </value>
    /// <remarks>
    /// The SKU should match the product's SKU in the product catalog. It's included
    /// for validation, reference, and convenience in transaction history.
    /// </remarks>
    /// <example>PROD-001-BLU-L</example>
    [Required(ErrorMessage = "Product SKU is required")]
    [StringLength(50, ErrorMessage = "Product SKU cannot exceed 50 characters")]
    public string ProductSku { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the product
    /// </summary>
    /// <value>
    /// The product's descriptive name, maximum 200 characters. This field is required.
    /// </value>
    /// <remarks>
    /// The product name is stored with the transaction for historical reference and
    /// display purposes, even if the product is later renamed or deleted.
    /// </remarks>
    /// <example>Blue T-Shirt - Large</example>
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(200, ErrorMessage = "Product name cannot exceed 200 characters")]
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the quantity of units involved in the transaction
    /// </summary>
    /// <value>
    /// The number of units to move. This field is required.
    /// </value>
    /// <remarks>
    /// <para>
    /// <strong>Interpretation by Transaction Type:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>StockEntry:</strong> Positive quantity increases inventory</description></item>
    /// <item><description><strong>StockWithdrawal:</strong> Positive quantity decreases inventory</description></item>
    /// <item><description><strong>StockTransfer:</strong> Quantity moves from FromLocation to ToLocation</description></item>
    /// <item><description><strong>StockAdjustment:</strong> Can be positive or negative to correct levels</description></item>
    /// <item><description><strong>StockReturn:</strong> Positive quantity for returns received</description></item>
    /// </list>
    /// </remarks>
    /// <example>100</example>
    [Required(ErrorMessage = "Quantity is required")]
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the cost per unit for the transaction
    /// </summary>
    /// <value>
    /// The unit cost in the base currency, must be greater than 0.01. This field is required.
    /// </value>
    /// <remarks>
    /// <para>
    /// Used for inventory valuation and Cost of Goods Sold (COGS) calculation.
    /// The unit cost multiplied by quantity determines the total value of the transaction.
    /// </para>
    /// <para>
    /// <strong>Important:</strong> For withdrawals and sales, this should be the cost at which
    /// the inventory was acquired, not the selling price.
    /// </para>
    /// </remarks>
    /// <example>25.99</example>
    [Required(ErrorMessage = "Unit cost is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Unit cost must be greater than zero")]
    public decimal UnitCost { get; set; }

    /// <summary>
    /// Gets or sets the source location from which inventory is being moved
    /// </summary>
    /// <value>
    /// The origin location/warehouse, maximum 100 characters. Optional for some transaction types.
    /// </value>
    /// <remarks>
    /// <para>
    /// <strong>Required for:</strong> StockTransfer, StockWithdrawal
    /// </para>
    /// <para>
    /// <strong>Optional for:</strong> StockEntry (new inventory has no source), StockAdjustment, StockReturn
    /// </para>
    /// <para>
    /// Examples: "Warehouse A - Shelf 12", "Main Store - Aisle 5", "Production Floor"
    /// </para>
    /// </remarks>
    /// <example>Warehouse A - Shelf 12</example>
    [StringLength(100, ErrorMessage = "From location cannot exceed 100 characters")]
    public string? FromLocation { get; set; }

    /// <summary>
    /// Gets or sets the destination location to which inventory is being moved
    /// </summary>
    /// <value>
    /// The destination location/warehouse, maximum 100 characters. This field is required.
    /// </value>
    /// <remarks>
    /// <para>
    /// Always required. Specifies where the inventory ends up or resides after the transaction.
    /// </para>
    /// <para>
    /// <strong>Special Values:</strong> For withdrawals, this might be "Sold", "Damaged", "Returned to Supplier"
    /// to indicate inventory leaving the system.
    /// </para>
    /// </remarks>
    /// <example>Warehouse B - Shelf 5</example>
    [Required(ErrorMessage = "To location is required")]
    [StringLength(100, ErrorMessage = "To location cannot exceed 100 characters")]
    public string ToLocation { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of a related sales or purchase order
    /// </summary>
    /// <value>
    /// The GUID of the associated order, or <c>null</c> if not order-related.
    /// </value>
    /// <remarks>
    /// <para>
    /// Optional. Links the inventory transaction to a business order for tracking and reconciliation.
    /// </para>
    /// <para>
    /// <strong>Use Cases:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Withdrawals linked to sales orders for order fulfillment tracking</description></item>
    /// <item><description>Entries linked to purchase orders for goods receipt confirmation</description></item>
    /// <item><description>Returns linked to original order for return processing</description></item>
    /// </list>
    /// </remarks>
    /// <example>5fa85f64-5717-4562-b3fc-2c963f66afa8</example>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// Gets or sets the external document number associated with the transaction
    /// </summary>
    /// <value>
    /// Reference number from external documents, maximum 100 characters. Optional.
    /// </value>
    /// <remarks>
    /// <para>
    /// Links to external documents such as:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Purchase order numbers (PO-2025-00123)</description></item>
    /// <item><description>Sales invoice numbers (INV-2025-456789)</description></item>
    /// <item><description>Delivery note numbers (DN-2025-00789)</description></item>
    /// <item><description>Goods receipt numbers (GR-2025-00345)</description></item>
    /// </list>
    /// <para>
    /// Useful for audit trails, reconciliation with external systems, and document tracing.
    /// </para>
    /// </remarks>
    /// <example>PO-2025-00123</example>
    [StringLength(100, ErrorMessage = "Document number cannot exceed 100 characters")]
    public string? DocumentNumber { get; set; }

    /// <summary>
    /// Gets or sets additional notes or comments about the transaction
    /// </summary>
    /// <value>
    /// Free-form text providing context or explanation, maximum 500 characters. Optional.
    /// </value>
    /// <remarks>
    /// <para>
    /// Use this field to record:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Special circumstances or exceptions</description></item>
    /// <item><description>Reasons for adjustments or corrections</description></item>
    /// <item><description>Quality issues or damage descriptions</description></item>
    /// <item><description>Operational instructions or follow-up requirements</description></item>
    /// </list>
    /// <para>
    /// These notes become part of the permanent transaction record and audit trail.
    /// </para>
    /// </remarks>
    /// <example>Initial stock receipt from new supplier - quality checked and approved</example>
    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }
}
