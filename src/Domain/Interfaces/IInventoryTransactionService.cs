using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Interfaces;

/// <summary>
/// Interface for inventory transaction management
/// </summary>
public interface IInventoryTransactionService
{
    /// <summary>
    /// Records an inventory transaction and generates the corresponding accounting entry
    /// </summary>
    Task<InventoryTransactionEntity> RecordTransactionAsync(
        InventoryTransactionType transactionType,
        Guid productId,
        string productSku,
        string productName,
        int quantity,
        decimal unitCost,
        string toLocation,
        Guid createdBy,
        string? fromLocation = null,
        Guid? orderId = null,
        string? documentNumber = null,
        string? notes = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets transaction history for a product
    /// </summary>
    Task<IEnumerable<InventoryTransactionEntity>> GetProductTransactionsAsync(
        Guid productId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets transactions for a specific period
    /// </summary>
    Task<IEnumerable<InventoryTransactionEntity>> GetTransactionsByPeriodAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default
    );
}
