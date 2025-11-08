using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Interfaces;

/// <summary>
/// Repository interface for financial transaction data access operations
/// </summary>
/// <remarks>
/// <para>
/// Provides specialized data access methods for financial transactions beyond
/// the generic repository pattern. Includes queries for financial reporting,
/// reconciliation, and cash flow analysis.
/// </para>
/// <para>
/// <strong>Query Capabilities:</strong>
/// </para>
/// <list type="bullet">
/// <item><description>Period-based queries for financial reporting</description></item>
/// <item><description>Transaction type filtering for category analysis</description></item>
/// <item><description>Reconciliation status tracking</description></item>
/// <item><description>Counterparty (customer/supplier) filtering</description></item>
/// <item><description>Payment method and provider analysis</description></item>
/// </list>
/// </remarks>
public interface IFinancialTransactionRepository : IRepository<FinancialTransactionEntity>
{
    /// <summary>
    /// Retrieves financial transactions within a date range
    /// </summary>
    /// <param name="startDate">Start date of the period</param>
    /// <param name="endDate">End date of the period</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of financial transactions in the specified period</returns>
    Task<IEnumerable<FinancialTransactionEntity>> GetByPeriodAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves unreconciled financial transactions
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of transactions that have not been reconciled</returns>
    Task<IEnumerable<FinancialTransactionEntity>> GetUnreconciledAsync(
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves financial transactions by order ID
    /// </summary>
    /// <param name="orderId">Order identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of financial transactions related to the order</returns>
    Task<IEnumerable<FinancialTransactionEntity>> GetByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves financial transactions by inventory transaction ID
    /// </summary>
    /// <param name="inventoryTransactionId">Inventory transaction identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of financial transactions related to the inventory movement</returns>
    Task<IEnumerable<FinancialTransactionEntity>> GetByInventoryTransactionIdAsync(
        Guid inventoryTransactionId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves financial transactions by payment ID
    /// </summary>
    /// <param name="paymentId">Payment identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of financial transactions related to the payment</returns>
    Task<IEnumerable<FinancialTransactionEntity>> GetByPaymentIdAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves financial transactions by counterparty
    /// </summary>
    /// <param name="counterparty">Name or identifier of the counterparty</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of financial transactions with the specified counterparty</returns>
    Task<IEnumerable<FinancialTransactionEntity>> GetByCounterpartyAsync(
        string counterparty,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves the next sequential transaction number
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The next available transaction number in sequence</returns>
    Task<int> GetNextTransactionNumberAsync(CancellationToken cancellationToken = default);
}
