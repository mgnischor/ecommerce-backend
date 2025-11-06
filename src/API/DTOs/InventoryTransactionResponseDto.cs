using ECommerce.Domain.Enums;

namespace ECommerce.API.DTOs;

/// <summary>
/// Data Transfer Object (DTO) for inventory transaction responses
/// </summary>
/// <remarks>
/// <para>
/// Represents a complete inventory movement record with all associated details including
/// product information, locations, costs, and related accounting entries.
/// </para>
/// <para>
/// <strong>Transaction Types:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><strong>StockEntry:</strong> Receiving inventory (purchases, production)</description></item>
/// <item><description><strong>StockWithdrawal:</strong> Removing inventory (sales, consumption)</description></item>
/// <item><description><strong>StockTransfer:</strong> Moving inventory between locations</description></item>
/// <item><description><strong>StockAdjustment:</strong> Correcting inventory levels (counts, damage)</description></item>
/// <item><description><strong>StockReturn:</strong> Returning inventory (customer returns, supplier returns)</description></item>
/// </list>
/// <para>
/// <strong>Integration:</strong> Inventory transactions automatically create corresponding
/// journal entries in the accounting system to maintain accurate COGS and inventory valuation.
/// </para>
/// </remarks>
public sealed class InventoryTransactionResponseDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the inventory transaction
    /// </summary>
    /// <value>
    /// A globally unique identifier (GUID) that uniquely identifies this transaction.
    /// </value>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the unique transaction number for reference and tracking
    /// </summary>
    /// <value>
    /// A system-generated sequential or formatted identifier for the transaction
    /// (e.g., "INV-2025-00001").
    /// </value>
    /// <remarks>
    /// Transaction numbers are typically displayed on reports, documents, and used for
    /// searching and referencing specific inventory movements.
    /// </remarks>
    /// <example>INV-2025-00001</example>
    public string TransactionNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when the transaction occurred
    /// </summary>
    /// <value>
    /// The UTC date and time when the inventory movement was recorded.
    /// </value>
    /// <remarks>
    /// This date is used for inventory valuation calculations, COGS determination,
    /// and historical reporting. It may differ from <see cref="CreatedAt"/> which
    /// represents when the record was created in the system.
    /// </remarks>
    /// <example>2025-11-06T14:30:00Z</example>
    public DateTime TransactionDate { get; set; }

    /// <summary>
    /// Gets or sets the type of inventory transaction
    /// </summary>
    /// <value>
    /// The transaction type as a string representation of <see cref="InventoryTransactionType"/> enum.
    /// Values include: StockEntry, StockWithdrawal, StockTransfer, StockAdjustment, StockReturn.
    /// </value>
    /// <remarks>
    /// The transaction type determines how the inventory affects stock levels and
    /// which accounting entries are generated.
    /// </remarks>
    /// <example>StockEntry</example>
    public string TransactionType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique identifier of the product involved in the transaction
    /// </summary>
    /// <value>
    /// The GUID referencing the product entity in the system.
    /// </value>
    /// <example>2c963f66-afa6-4562-b3fc-3fa85f645717</example>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Gets or sets the Stock Keeping Unit (SKU) of the product
    /// </summary>
    /// <value>
    /// The unique product identifier used for inventory tracking and identification.
    /// </value>
    /// <remarks>
    /// SKUs are used for quick product lookup and are typically more user-friendly
    /// than GUIDs for operational staff.
    /// </remarks>
    /// <example>PROD-001-BLU-L</example>
    public string ProductSku { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the product
    /// </summary>
    /// <value>
    /// The descriptive name of the product for display and reporting purposes.
    /// </value>
    /// <example>Blue T-Shirt - Large</example>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source location for the inventory movement
    /// </summary>
    /// <value>
    /// The location/warehouse from which the inventory is being moved.
    /// Can be <c>null</c> for stock entries (new inventory) or stock adjustments.
    /// </value>
    /// <remarks>
    /// Required for stock withdrawals and transfers. Helps track inventory by location
    /// and enables multi-warehouse management.
    /// </remarks>
    /// <example>Warehouse A - Shelf 12</example>
    public string? FromLocation { get; set; }

    /// <summary>
    /// Gets or sets the destination location for the inventory movement
    /// </summary>
    /// <value>
    /// The location/warehouse to which the inventory is being moved or where it currently resides.
    /// </value>
    /// <remarks>
    /// Always required. For withdrawals, this might be "Sold" or "Damaged".
    /// For entries, this is where the new inventory is stored.
    /// </remarks>
    /// <example>Warehouse B - Shelf 5</example>
    public string ToLocation { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the quantity of units involved in the transaction
    /// </summary>
    /// <value>
    /// The number of units moved. Positive values represent increases in destination location,
    /// negative values (if allowed by business rules) represent decreases.
    /// </value>
    /// <remarks>
    /// This quantity is used to update stock levels and calculate the total cost.
    /// The interpretation depends on the transaction type.
    /// </remarks>
    /// <example>100</example>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the cost per unit of the product
    /// </summary>
    /// <value>
    /// The monetary cost for a single unit of the product in the base currency.
    /// </value>
    /// <remarks>
    /// Used for inventory valuation and Cost of Goods Sold (COGS) calculation.
    /// May represent purchase cost, average cost, or standard cost depending on
    /// the inventory valuation method used.
    /// </remarks>
    /// <example>25.99</example>
    public decimal UnitCost { get; set; }

    /// <summary>
    /// Gets or sets the total cost of the transaction
    /// </summary>
    /// <value>
    /// The total monetary value calculated as <see cref="Quantity"/> × <see cref="UnitCost"/>.
    /// </value>
    /// <remarks>
    /// This amount is used in the corresponding journal entry to update inventory
    /// asset accounts and COGS/expense accounts.
    /// </remarks>
    /// <example>2599.00</example>
    public decimal TotalCost { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the related accounting journal entry
    /// </summary>
    /// <value>
    /// The GUID of the journal entry that records the financial impact of this transaction,
    /// or <c>null</c> if no journal entry has been created yet.
    /// </value>
    /// <remarks>
    /// This links the inventory movement to the accounting system, ensuring
    /// financial records match physical inventory movements.
    /// </remarks>
    /// <example>4fa85f64-5717-4562-b3fc-2c963f66afa7</example>
    public Guid? JournalEntryId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the related sales or purchase order
    /// </summary>
    /// <value>
    /// The GUID of the order that triggered this transaction, or <c>null</c> if not
    /// associated with an order (e.g., adjustments, transfers).
    /// </value>
    /// <remarks>
    /// Links inventory movements to sales orders (withdrawals) or purchase orders (entries)
    /// for complete order fulfillment tracking.
    /// </remarks>
    /// <example>5fa85f64-5717-4562-b3fc-2c963f66afa8</example>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// Gets or sets the external document number associated with the transaction
    /// </summary>
    /// <value>
    /// Reference number from external documents such as invoices, receipts, packing slips,
    /// or delivery notes. Can be <c>null</c> if not applicable.
    /// </value>
    /// <remarks>
    /// Useful for audit trails and reconciliation with external documents and systems.
    /// </remarks>
    /// <example>INV-2025-456789</example>
    public string? DocumentNumber { get; set; }

    /// <summary>
    /// Gets or sets additional notes or comments about the transaction
    /// </summary>
    /// <value>
    /// Free-form text providing additional context, explanations, or special instructions
    /// related to the transaction. Can be <c>null</c> if no notes are needed.
    /// </value>
    /// <example>Emergency restock due to unexpected demand</example>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who created the transaction
    /// </summary>
    /// <value>
    /// The GUID of the user account that recorded this inventory transaction.
    /// </value>
    /// <remarks>
    /// Used for audit trails and accountability. Helps track who performed
    /// inventory operations for security and operational review.
    /// </remarks>
    /// <example>6fa85f64-5717-4562-b3fc-2c963f66afa9</example>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the transaction record was created in the system
    /// </summary>
    /// <value>
    /// The UTC date and time when this record was created.
    /// </value>
    /// <remarks>
    /// This represents when the record was entered into the system, which may differ
    /// from <see cref="TransactionDate"/> (the actual date of the inventory movement).
    /// </remarks>
    /// <example>2025-11-06T14:35:22Z</example>
    public DateTime CreatedAt { get; set; }
}
