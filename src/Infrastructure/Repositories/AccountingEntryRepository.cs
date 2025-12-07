using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for managing accounting entry data access operations.
/// </summary>
/// <remarks>
/// Provides data access methods for <see cref="AccountingEntryEntity"/>, which represents
/// individual debit and credit lines within journal entries. Supports queries by journal entry,
/// account, date range, and provides aggregation capabilities for balance calculations.
/// Each accounting entry is part of the double-entry bookkeeping system where debits equal credits.
/// All query operations use AsNoTracking for optimal read performance.
/// </remarks>
public sealed class AccountingEntryRepository : IAccountingEntryRepository
{
    private readonly PostgresqlContext _context;
    private readonly ILoggingService _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountingEntryRepository"/> class.
    /// </summary>
    /// <param name="context">The database context for data access operations.</param>
    /// <param name="logger">The logging service for diagnostic information.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="logger"/> is null.</exception>
    public AccountingEntryRepository(PostgresqlContext context, ILoggingService logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<AccountingEntryEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .AccountingEntries.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AccountingEntryEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .AccountingEntries.AsNoTracking()
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AccountingEntryEntity>> GetByJournalEntryIdAsync(
        Guid journalEntryId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .AccountingEntries.AsNoTracking()
            .Where(a => a.JournalEntryId == journalEntryId)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AccountingEntryEntity>> GetByAccountIdAsync(
        Guid accountId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .AccountingEntries.AsNoTracking()
            .Where(a => a.AccountId == accountId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AccountingEntryEntity>> GetByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .AccountingEntries.AsNoTracking()
            .Where(a => a.CreatedAt >= startDate && a.CreatedAt <= endDate)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AccountingEntries.CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(
        AccountingEntryEntity entry,
        CancellationToken cancellationToken = default
    )
    {
        if (entry == null)
            throw new ArgumentNullException(nameof(entry));

        _logger.LogDebug("Adding new accounting entry: {EntryId}", entry.Id);
        await _context.AccountingEntries.AddAsync(entry, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public void Update(AccountingEntryEntity entry)
    {
        if (entry == null)
            throw new ArgumentNullException(nameof(entry));

        _logger.LogDebug("Updating accounting entry: {EntryId}", entry.Id);
        _context.AccountingEntries.Update(entry);
    }

    /// <inheritdoc />
    public void Remove(AccountingEntryEntity entry)
    {
        if (entry == null)
            throw new ArgumentNullException(nameof(entry));

        _logger.LogDebug("Removing accounting entry: {EntryId}", entry.Id);
        _context.AccountingEntries.Remove(entry);
    }

    /// <inheritdoc />
    public async Task<bool> RemoveByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entry = await _context.AccountingEntries.FirstOrDefaultAsync(
            a => a.Id == id,
            cancellationToken
        );

        if (entry == null)
            return false;

        _context.AccountingEntries.Remove(entry);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
