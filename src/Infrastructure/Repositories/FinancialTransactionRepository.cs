using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for financial transaction data access
/// </summary>
/// <remarks>
/// Provides specialized queries for financial transactions including period-based queries,
/// reconciliation tracking, and relationship-based filtering (orders, payments, inventory).
/// Extends the generic repository with financial-specific query methods.
/// </remarks>
public class FinancialTransactionRepository
    : Repository<FinancialTransactionEntity>,
        IFinancialTransactionRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FinancialTransactionRepository"/> class
    /// </summary>
    /// <param name="context">Database context for data access operations</param>
    public FinancialTransactionRepository(PostgresqlContext context)
        : base(context) { }

    /// <inheritdoc />
    public async Task<IEnumerable<FinancialTransactionEntity>> GetByPeriodAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .FinancialTransactions.Where(ft =>
                ft.TransactionDate >= startDate && ft.TransactionDate <= endDate
            )
            .OrderByDescending(ft => ft.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<FinancialTransactionEntity>> GetUnreconciledAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .FinancialTransactions.Where(ft => !ft.IsReconciled)
            .OrderBy(ft => ft.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<FinancialTransactionEntity>> GetByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .FinancialTransactions.Where(ft => ft.OrderId == orderId)
            .OrderBy(ft => ft.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<FinancialTransactionEntity>> GetByInventoryTransactionIdAsync(
        Guid inventoryTransactionId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .FinancialTransactions.Where(ft => ft.InventoryTransactionId == inventoryTransactionId)
            .OrderBy(ft => ft.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<FinancialTransactionEntity>> GetByPaymentIdAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .FinancialTransactions.Where(ft => ft.PaymentId == paymentId)
            .OrderBy(ft => ft.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<FinancialTransactionEntity>> GetByCounterpartyAsync(
        string counterparty,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .FinancialTransactions.Where(ft =>
                ft.Counterparty != null && ft.Counterparty.Contains(counterparty)
            )
            .OrderByDescending(ft => ft.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> GetNextTransactionNumberAsync(
        CancellationToken cancellationToken = default
    )
    {
        var count = await _context.FinancialTransactions.CountAsync(cancellationToken);
        return count + 1;
    }
}
