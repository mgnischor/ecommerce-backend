namespace ECommerce.Domain.Interfaces;

/// <summary>
/// Repository interface for AccountingEntryEntity operations
/// </summary>
public interface IAccountingEntryRepository
{
    Task<Domain.Entities.AccountingEntryEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.AccountingEntryEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.AccountingEntryEntity>> GetByJournalEntryIdAsync(
        Guid journalEntryId,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.AccountingEntryEntity>> GetByAccountIdAsync(
        Guid accountId,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.AccountingEntryEntity>> GetByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default
    );
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
    Task AddAsync(
        Domain.Entities.AccountingEntryEntity entry,
        CancellationToken cancellationToken = default
    );
    void Update(Domain.Entities.AccountingEntryEntity entry);
    void Remove(Domain.Entities.AccountingEntryEntity entry);
    Task<bool> RemoveByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
