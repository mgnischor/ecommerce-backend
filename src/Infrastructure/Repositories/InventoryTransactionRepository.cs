using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for inventory transaction operations
/// </summary>
public class InventoryTransactionRepository
    : Repository<InventoryTransactionEntity>,
        IInventoryTransactionRepository
{
    private readonly ILoggingService _logger;

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
