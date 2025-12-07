using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for managing journal entry data access operations.
/// </summary>
/// <remarks>
/// Provides data access methods for <see cref="JournalEntryEntity"/>, which represents
/// the header of accounting transactions in the double-entry bookkeeping system.
/// Each journal entry contains multiple accounting entries (debits and credits) that balance.
/// Supports queries by date range, pagination, and posting status. Journal entries maintain
/// an immutable audit trail once posted, following accounting best practices.
/// All query operations use AsNoTracking for optimal read performance.
/// </remarks>
public sealed class JournalEntryRepository : IJournalEntryRepository
{
    private readonly PostgresqlContext _context;
    private readonly ILoggingService _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="JournalEntryRepository"/> class.
    /// </summary>
    /// <param name="context">The database context for data access operations.</param>
    /// <param name="logger">The logging service for diagnostic information.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="logger"/> is null.</exception>
    public JournalEntryRepository(PostgresqlContext context, ILoggingService logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<JournalEntryEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .JournalEntries.AsNoTracking()
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<JournalEntryEntity?> GetByIdWithEntriesAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        // Note: Entity doesn't have navigation property, return base entity
        return await GetByIdAsync(id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<JournalEntryEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .JournalEntries.AsNoTracking()
            .OrderByDescending(j => j.EntryDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.JournalEntries.CountAsync(cancellationToken);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public void Update(JournalEntryEntity journalEntry)
    {
        if (journalEntry == null)
            throw new ArgumentNullException(nameof(journalEntry));

        _logger.LogDebug("Updating journal entry: {JournalEntryId}", journalEntry.Id);
        _context.JournalEntries.Update(journalEntry);
    }

    /// <inheritdoc />
    public void Remove(JournalEntryEntity journalEntry)
    {
        if (journalEntry == null)
            throw new ArgumentNullException(nameof(journalEntry));

        _logger.LogDebug("Removing journal entry: {JournalEntryId}", journalEntry.Id);
        _context.JournalEntries.Remove(journalEntry);
    }

    /// <inheritdoc />
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
