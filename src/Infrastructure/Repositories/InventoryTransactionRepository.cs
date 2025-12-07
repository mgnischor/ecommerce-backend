using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for managing inventory transaction data access operations.
/// </summary>
/// <remarks>
/// Provides comprehensive data access methods for <see cref="InventoryTransactionEntity"/>
/// which tracks all inventory movements including purchases, sales, returns, adjustments,
/// transfers, and losses. Extends the generic repository pattern with specialized queries
/// for product-based filtering, transaction type filtering, date range queries, location
/// tracking, and relationship queries (orders, journal entries). Essential for inventory
/// audit trails, stock reconciliation, and accounting integration. Supports eager loading
/// of related entities (Product, JournalEntry) for complete transaction context.
/// </remarks>
public class InventoryTransactionRepository
    : Repository<InventoryTransactionEntity>,
        IInventoryTransactionRepository
{
    private readonly ILoggingService _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="InventoryTransactionRepository"/> class.
    /// </summary>
    /// <param name="context">The database context for data access operations.</param>
    /// <param name="logger">The logging service for diagnostic and audit information.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null.</exception>
    public InventoryTransactionRepository(PostgresqlContext context, ILoggingService logger)
        : base(context)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<InventoryTransactionEntity>> GetByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation(
            "Getting inventory transactions for product: {ProductId}",
            productId
        );

        return await _context
            .InventoryTransactions.Where(t => t.ProductId == productId)
            .Include(t => t.Product)
            .Include(t => t.JournalEntry)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<InventoryTransactionEntity>> GetByTypeAsync(
        InventoryTransactionType type,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Getting inventory transactions by type: {Type}", type);

        return await _context
            .InventoryTransactions.Where(t => t.TransactionType == type)
            .Include(t => t.Product)
            .Include(t => t.JournalEntry)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<InventoryTransactionEntity>> GetByPeriodAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation(
            "Getting inventory transactions for period: {StartDate} to {EndDate}",
            startDate,
            endDate
        );

        return await _context
            .InventoryTransactions.Where(t =>
                t.TransactionDate >= startDate && t.TransactionDate <= endDate
            )
            .Include(t => t.Product)
            .Include(t => t.JournalEntry)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<InventoryTransactionEntity>> GetByLocationAsync(
        string location,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Getting inventory transactions for location: {Location}", location);

        return await _context
            .InventoryTransactions.Where(t =>
                t.ToLocation == location || t.FromLocation == location
            )
            .Include(t => t.Product)
            .Include(t => t.JournalEntry)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<InventoryTransactionEntity>> GetByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Getting inventory transactions for order: {OrderId}", orderId);

        return await _context
            .InventoryTransactions.Where(t => t.OrderId == orderId)
            .Include(t => t.Product)
            .Include(t => t.JournalEntry)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<InventoryTransactionEntity>> GetByJournalEntryIdAsync(
        Guid journalEntryId,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation(
            "Getting inventory transactions for journal entry: {JournalEntryId}",
            journalEntryId
        );

        return await _context
            .InventoryTransactions.Where(t => t.JournalEntryId == journalEntryId)
            .Include(t => t.Product)
            .Include(t => t.JournalEntry)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<InventoryTransactionEntity>> GetByCreatedByAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Getting inventory transactions created by user: {UserId}", userId);

        return await _context
            .InventoryTransactions.Where(t => t.CreatedBy == userId)
            .Include(t => t.Product)
            .Include(t => t.JournalEntry)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }
}
