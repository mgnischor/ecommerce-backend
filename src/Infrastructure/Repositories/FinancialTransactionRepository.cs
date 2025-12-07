using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for managing financial transaction data access operations.
/// </summary>
/// <remarks>
/// Provides comprehensive data access methods for <see cref="FinancialTransactionEntity"/>
/// which tracks all monetary movements in the system. Includes specialized queries for
/// period-based financial reporting, reconciliation tracking, counterparty filtering,
/// and relationship-based filtering (orders, payments, inventory transactions). Supports
/// financial audit trails, cash flow analysis, and accounting reconciliation processes.
/// Extends the generic repository pattern with financial-specific query methods. Critical
/// for financial reporting, bank reconciliation, and compliance with accounting standards.
/// Transactions are sorted by date for chronological financial record keeping.
/// </remarks>
public class FinancialTransactionRepository
    : Repository<FinancialTransactionEntity>,
        IFinancialTransactionRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FinancialTransactionRepository"/> class.
    /// </summary>
    /// <param name="context">The database context for data access operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null (via base constructor).</exception>
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
