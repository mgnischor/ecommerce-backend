using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Repositories;

/// <summary>
/// Repository for managing JournalEntryEntity data access.
/// </summary>
public sealed class JournalEntryRepository : IJournalEntryRepository
{
    private readonly PostgresqlContext _context;
    private readonly ILogger<JournalEntryRepository> _logger;

    public JournalEntryRepository(PostgresqlContext context, ILogger<JournalEntryRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<JournalEntryEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .JournalEntries.AsNoTracking()
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
    }

    public async Task<JournalEntryEntity?> GetByIdWithEntriesAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        // Note: Entity doesn't have navigation property, return base entity
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<IReadOnlyList<JournalEntryEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .JournalEntries.AsNoTracking()
            .OrderByDescending(j => j.EntryDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<JournalEntryEntity>> GetByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .JournalEntries.AsNoTracking()
            .Where(j => j.EntryDate >= startDate && j.EntryDate <= endDate)
            .OrderByDescending(j => j.EntryDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<JournalEntryEntity>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .JournalEntries.AsNoTracking()
            .OrderByDescending(j => j.EntryDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.JournalEntries.CountAsync(cancellationToken);
    }

    public async Task AddAsync(
        JournalEntryEntity journalEntry,
        CancellationToken cancellationToken = default
    )
    {
        if (journalEntry == null)
            throw new ArgumentNullException(nameof(journalEntry));

        _logger.LogDebug("Adding new journal entry: {JournalEntryId}", journalEntry.Id);
        await _context.JournalEntries.AddAsync(journalEntry, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public void Update(JournalEntryEntity journalEntry)
    {
        if (journalEntry == null)
            throw new ArgumentNullException(nameof(journalEntry));

        _logger.LogDebug("Updating journal entry: {JournalEntryId}", journalEntry.Id);
        _context.JournalEntries.Update(journalEntry);
    }

    public void Remove(JournalEntryEntity journalEntry)
    {
        if (journalEntry == null)
            throw new ArgumentNullException(nameof(journalEntry));

        _logger.LogDebug("Removing journal entry: {JournalEntryId}", journalEntry.Id);
        _context.JournalEntries.Remove(journalEntry);
    }

    public async Task<bool> RemoveByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var journalEntry = await _context.JournalEntries.FirstOrDefaultAsync(
            j => j.Id == id,
            cancellationToken
        );

        if (journalEntry == null)
            return false;

        _context.JournalEntries.Remove(journalEntry);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
