using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Repositories;

/// <summary>
/// Repository for managing AccountingEntryEntity data access.
/// </summary>
public sealed class AccountingEntryRepository : IAccountingEntryRepository
{
    private readonly PostgresqlContext _context;
    private readonly ILogger<AccountingEntryRepository> _logger;

    public AccountingEntryRepository(
        PostgresqlContext context,
        ILogger<AccountingEntryRepository> logger
    )
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AccountingEntryEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .AccountingEntries.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<AccountingEntryEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .AccountingEntries.AsNoTracking()
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

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

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AccountingEntries.CountAsync(cancellationToken);
    }

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

    public void Update(AccountingEntryEntity entry)
    {
        if (entry == null)
            throw new ArgumentNullException(nameof(entry));

        _logger.LogDebug("Updating accounting entry: {EntryId}", entry.Id);
        _context.AccountingEntries.Update(entry);
    }

    public void Remove(AccountingEntryEntity entry)
    {
        if (entry == null)
            throw new ArgumentNullException(nameof(entry));

        _logger.LogDebug("Removing accounting entry: {EntryId}", entry.Id);
        _context.AccountingEntries.Remove(entry);
    }

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
