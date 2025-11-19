using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.Interfaces;

/// <summary>
/// Interface for inventory transaction service operations
/// </summary>
/// <remarks>
/// Manages inventory movements with automatic accounting and financial integration.
/// Every inventory transaction generates corresponding journal entries and financial records.
/// </remarks>
public interface IInventoryTransactionService
{
    /// <summary>
    /// Records a new inventory transaction with complete traceability
    /// </summary>
    /// <param name="transactionType">Type of inventory transaction</param>
    /// <param name="productId">Product identifier</param>
    /// <param name="productSku">Product SKU</param>
    /// <param name="productName">Product name</param>
    /// <param name="quantity">Quantity being moved (positive or negative based on type)</param>
    /// <param name="unitCost">Cost per unit</param>
    /// <param name="toLocation">Destination location</param>
    /// <param name="createdBy">User ID recording the transaction</param>
    /// <param name="fromLocation">Source location (for transfers)</param>
    /// <param name="orderId">Related order ID (if applicable)</param>
    /// <param name="documentNumber">Document or reference number</param>
    /// <param name="notes">Transaction notes or comments</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created inventory transaction entity</returns>
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
    /// Retrieves all transactions for a specific product
    /// </summary>
    /// <param name="productId">Product identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of inventory transactions for the product</returns>
    Task<IEnumerable<InventoryTransactionEntity>> GetProductTransactionsAsync(
        Guid productId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves transactions within a date range
    /// </summary>
    /// <param name="startDate">Period start date</param>
    /// <param name="endDate">Period end date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of inventory transactions in the period</returns>
    Task<IEnumerable<InventoryTransactionEntity>> GetTransactionsByPeriodAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default
    );
}
