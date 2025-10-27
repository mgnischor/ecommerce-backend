namespace ECommerce.Domain.Interfaces;

/// <summary>
/// Repository interface for JournalEntryEntity operations
/// </summary>
public interface IJournalEntryRepository
{
    Task<Domain.Entities.JournalEntryEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );
    Task<Domain.Entities.JournalEntryEntity?> GetByIdWithEntriesAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.JournalEntryEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.JournalEntryEntity>> GetByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.JournalEntryEntity>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    );
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
    Task AddAsync(
        Domain.Entities.JournalEntryEntity journalEntry,
        CancellationToken cancellationToken = default
    );
    void Update(Domain.Entities.JournalEntryEntity journalEntry);
    void Remove(Domain.Entities.JournalEntryEntity journalEntry);
    Task<bool> RemoveByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
