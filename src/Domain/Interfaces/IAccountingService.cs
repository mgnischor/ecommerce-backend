using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Interfaces;

/// <summary>
/// Interface for inventory accounting operations
/// </summary>
public interface IAccountingService
{
    /// <summary>
    /// Creates accounting entry for inventory receipt (purchase)
    /// Debit: Inventory (Asset)
    /// Credit: Suppliers/Cash (Liability/Asset)
    /// </summary>
    Task<JournalEntryEntity> RecordPurchaseAsync(
        InventoryTransactionEntity transaction,
        Guid createdBy,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Creates accounting entry for inventory withdrawal (sale - COGS)
    /// Debit: Cost of Goods Sold (Expense/Cost)
    /// Credit: Inventory (Asset)
    /// </summary>
    Task<JournalEntryEntity> RecordSaleAsync(
        InventoryTransactionEntity transaction,
        Guid createdBy,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Creates accounting entry for sales return
    /// Debit: Inventory (Asset)
    /// Credit: Cost of Goods Sold (Expense/Cost)
    /// </summary>
    Task<JournalEntryEntity> RecordSaleReturnAsync(
        InventoryTransactionEntity transaction,
        Guid createdBy,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Creates accounting entry for purchase return
    /// Debit: Suppliers/Cash (Liability/Asset)
    /// Credit: Inventory (Asset)
    /// </summary>
    Task<JournalEntryEntity> RecordPurchaseReturnAsync(
        InventoryTransactionEntity transaction,
        Guid createdBy,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Creates accounting entry for inventory adjustment
    /// Positive - Debit: Inventory, Credit: Other Income
    /// Negative - Debit: Inventory Loss, Credit: Inventory
    /// </summary>
    Task<JournalEntryEntity> RecordAdjustmentAsync(
        InventoryTransactionEntity transaction,
        Guid createdBy,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Creates accounting entry for inventory loss/shrinkage
    /// Debit: Inventory Loss (Expense)
    /// Credit: Inventory (Asset)
    /// </summary>
    Task<JournalEntryEntity> RecordLossAsync(
        InventoryTransactionEntity transaction,
        Guid createdBy,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets or creates an accounting account by code
    /// </summary>
    Task<ChartOfAccountsEntity> GetOrCreateAccountAsync(
        string accountCode,
        string accountName,
        AccountType accountType,
        CancellationToken cancellationToken = default
    );
}
