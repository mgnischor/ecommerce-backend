using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Interfaces;

/// <summary>
/// Repository interface for inventory transaction operations
/// </summary>
public interface IInventoryTransactionRepository : IRepository<InventoryTransactionEntity>
{
    /// <summary>
    /// Gets all transactions for a specific product
    /// </summary>
    Task<IEnumerable<InventoryTransactionEntity>> GetByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets transactions by transaction type
    /// </summary>
    Task<IEnumerable<InventoryTransactionEntity>> GetByTypeAsync(
        InventoryTransactionType type,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets transactions within a date range
    /// </summary>
    Task<IEnumerable<InventoryTransactionEntity>> GetByPeriodAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets transactions by location
    /// </summary>
    Task<IEnumerable<InventoryTransactionEntity>> GetByLocationAsync(
        string location,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets transactions by order ID
    /// </summary>
    Task<IEnumerable<InventoryTransactionEntity>> GetByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets transactions by journal entry ID
    /// </summary>
    Task<IEnumerable<InventoryTransactionEntity>> GetByJournalEntryIdAsync(
        Guid journalEntryId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets transactions created by a specific user
    /// </summary>
    Task<IEnumerable<InventoryTransactionEntity>> GetByCreatedByAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
}
