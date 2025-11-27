using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents an inventory transaction record with complete traceability in the e-commerce system.
/// </summary>
/// <remarks>
/// Every inventory movement (receipts, sales, transfers, adjustments) is recorded as a transaction
/// for complete audit trails and inventory valuation. Inventory transactions automatically generate
/// corresponding accounting entries to maintain financial accuracy and compliance.
/// Supports FIFO, LIFO, and weighted average cost methods for inventory valuation.
/// </remarks>
public sealed class InventoryTransactionEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for this inventory transaction.
    /// </summary>
    /// <value>A <see cref="Guid"/> that uniquely identifies the transaction.</value>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the sequential transaction number for human-readable identification.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> containing the formatted transaction number.
    /// Typically follows a pattern like INV-YYYYMMDD-XXXXXX.
    /// </value>
    /// <remarks>
    /// Used for referencing transactions in reports, audits, and communications.
    /// Generated automatically and guaranteed to be unique and chronological.
    /// </remarks>
    /// <example>INV-20251126-000123</example>
    public string TransactionNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when the inventory movement occurred.
    /// </summary>
    /// <value>A <see cref="DateTime"/> in UTC format representing the transaction date.</value>
    /// <remarks>
    /// May differ from <see cref="CreatedAt"/> for backdated transactions or adjustments.
    /// Critical for accurate inventory valuation and period-end reporting.
    /// </remarks>
    public DateTime TransactionDate { get; set; }

    /// <summary>
    /// Gets or sets the type of inventory transaction.
    /// </summary>
    /// <value>An <see cref="InventoryTransactionType"/> enumeration value.</value>
    /// <remarks>
    /// Common transaction types include:
    /// Purchase Receipt, Sale, Transfer, Adjustment, Return, Damage, Theft.
    /// Each type has specific accounting treatment and business logic.
    /// </remarks>
    public InventoryTransactionType TransactionType { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the product being moved.
    /// </summary>
    /// <value>A <see cref="Guid"/> referencing the <see cref="ProductEntity"/>.</value>
    /// <example>7c9e6679-7425-40de-944b-e07fc1f90ae7</example>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Gets or sets the product SKU at the time of transaction.
    /// </summary>
    /// <value>A <see cref="string"/> containing the product's stock keeping unit.</value>
    /// <remarks>
    /// Captured as a snapshot to preserve transaction history even if SKU changes.
    /// Essential for warehouse operations and picking processes.
    /// </remarks>
    /// <example>WH-1000XM4-BLK</example>
    public string ProductSku { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the product name at the time of transaction.
    /// </summary>
    /// <value>A <see cref="string"/> containing the product name.</value>
    /// <remarks>
    /// Snapshot of product name ensures transaction records remain accurate
    /// even if the product is renamed or deleted from the catalog.
    /// </remarks>
    /// <example>Wireless Bluetooth Headphones</example>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source location for transfer transactions.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> identifying the location inventory is moving from,
    /// or <c>null</c> for non-transfer transactions.
    /// </value>
    /// <remarks>
    /// Used for inter-location transfers, tracking inventory movement between:
    /// warehouses, stores, fulfillment centers, or storage areas.
    /// </remarks>
    /// <example>Main Warehouse, Store-NYC-001</example>
    public string? FromLocation { get; set; }

    /// <summary>
    /// Gets or sets the destination location for this transaction.
    /// </summary>
    /// <value>A <see cref="string"/> identifying where inventory is being received or stored.</value>
    /// <remarks>
    /// For receipts: the receiving warehouse or location.
    /// For sales: typically "Customer" or a delivery designation.
    /// For transfers: the destination warehouse or store.
    /// </remarks>
    /// <example>Main Warehouse, Customer, Store-LA-002</example>
    public string ToLocation { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the quantity of units moved in this transaction.
    /// </summary>
    /// <value>
    /// An <see cref="int"/> representing units moved.
    /// Positive for inbound movements (receipts, returns), negative for outbound (sales, adjustments).
    /// </value>
    /// <remarks>
    /// Sign convention:
    /// <list type="bullet">
    /// <item><description>Positive: increases inventory (purchases, returns, adjustments up)</description></item>
    /// <item><description>Negative: decreases inventory (sales, damages, adjustments down)</description></item>
    /// </list>
    /// </remarks>
    /// <example>50, -10</example>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the cost per unit for this transaction.
    /// </summary>
    /// <value>A <see cref="decimal"/> representing the unit cost in the base currency.</value>
    /// <remarks>
    /// Used for inventory valuation and cost of goods sold (COGS) calculations.
    /// For purchases: the acquisition cost per unit.
    /// For sales: the carrying cost per unit (FIFO, LIFO, or weighted average).
    /// </remarks>
    /// <example>45.50</example>
    public decimal UnitCost { get; set; }

    /// <summary>
    /// Gets or sets the total cost for this transaction.
    /// </summary>
    /// <value>
    /// A <see cref="decimal"/> calculated as Quantity × UnitCost.
    /// </value>
    /// <remarks>
    /// Total cost impacts:
    /// <list type="bullet">
    /// <item><description>Inventory asset value on the balance sheet</description></item>
    /// <item><description>Cost of goods sold for sales transactions</description></item>
    /// <item><description>Profit margin calculations</description></item>
    /// </list>
    /// Automatically generates corresponding journal entries for accounting.
    /// </remarks>
    /// <example>2275.00</example>
    public decimal TotalCost { get; set; }

    /// <summary>
    /// Gets or sets the optional reference to the related order.
    /// </summary>
    /// <value>
    /// A <see cref="Guid"/> referencing the <see cref="OrderEntity"/>,
    /// or <c>null</c> if not order-related.
    /// </value>
    /// <remarks>
    /// Links inventory movements to sales orders for complete order fulfillment tracking.
    /// Essential for order-to-cash process visibility.
    /// </remarks>
    /// <example>8b3c8d7a-9c4e-4f5a-8d3c-9e5f8a7b6c5d</example>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// Gets or sets the fiscal or regulatory document number.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> containing the document reference,
    /// or <c>null</c> if not applicable.
    /// </value>
    /// <remarks>
    /// May include:
    /// <list type="bullet">
    /// <item><description>Purchase order numbers</description></item>
    /// <item><description>Supplier invoice numbers</description></item>
    /// <item><description>Tax invoice numbers (NF-e, fiscal notes)</description></item>
    /// <item><description>Return authorization numbers</description></item>
    /// </list>
    /// Critical for audit trails and regulatory compliance.
    /// </remarks>
    /// <example>PO-2025-001234, NFE-12345678</example>
    public string? DocumentNumber { get; set; }

    /// <summary>
    /// Gets or sets additional notes or comments about this transaction.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> containing explanatory notes,
    /// or <c>null</c> if no additional information is needed.
    /// </value>
    /// <remarks>
    /// Used for:
    /// <list type="bullet">
    /// <item><description>Explaining adjustments or corrections</description></item>
    /// <item><description>Recording damage or quality issues</description></item>
    /// <item><description>Documenting unusual circumstances</description></item>
    /// <item><description>Providing context for auditors</description></item>
    /// </list>
    /// </remarks>
    /// <example>Damaged during receiving, Annual physical count adjustment</example>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets the reference to the automatically generated journal entry.
    /// </summary>
    /// <value>
    /// A <see cref="Guid"/> referencing the <see cref="JournalEntryEntity"/>,
    /// or <c>null</c> if the accounting entry hasn't been created yet.
    /// </value>
    /// <remarks>
    /// Inventory transactions automatically generate accounting entries:
    /// <list type="bullet">
    /// <item><description>Purchases: Debit Inventory, Credit Cash/Payables</description></item>
    /// <item><description>Sales: Debit COGS, Credit Inventory</description></item>
    /// <item><description>Adjustments: Adjust inventory value as needed</description></item>
    /// </list>
    /// Links inventory operations to financial accounting for complete traceability.
    /// </remarks>
    public Guid? JournalEntryId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who created this transaction.
    /// </summary>
    /// <value>A <see cref="Guid"/> referencing the responsible user or system account.</value>
    /// <remarks>
    /// Provides accountability and audit trails.
    /// May be a warehouse user, system process, or administrator.
    /// </remarks>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this transaction record was created.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> in UTC format representing the creation timestamp.
    /// Defaults to <see cref="DateTime.UtcNow"/> when the entity is instantiated.
    /// </value>
    /// <remarks>
    /// Represents when the transaction was recorded in the system,
    /// which may differ from <see cref="TransactionDate"/> for backdated entries.
    /// </remarks>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the navigation property to the related product.
    /// </summary>
    /// <value>
    /// A <see cref="ProductEntity"/> instance,
    /// or <c>null</c> if not loaded.
    /// </value>
    public ProductEntity? Product { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the related journal entry.
    /// </summary>
    /// <value>
    /// A <see cref="JournalEntryEntity"/> instance,
    /// or <c>null</c> if not loaded.
    /// </value>
    public JournalEntryEntity? JournalEntry { get; set; }
}
