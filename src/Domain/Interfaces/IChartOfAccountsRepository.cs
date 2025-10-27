namespace ECommerce.Domain.Interfaces;

/// <summary>
/// Repository interface for ChartOfAccountsEntity operations
/// </summary>
public interface IChartOfAccountsRepository
{
    Task<Domain.Entities.ChartOfAccountsEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );
    Task<Domain.Entities.ChartOfAccountsEntity?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.ChartOfAccountsEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.ChartOfAccountsEntity>> GetByTypeAsync(
        int accountType,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Domain.Entities.ChartOfAccountsEntity>> GetActiveAsync(
        CancellationToken cancellationToken = default
    );
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task AddAsync(
        Domain.Entities.ChartOfAccountsEntity account,
        CancellationToken cancellationToken = default
    );
    void Update(Domain.Entities.ChartOfAccountsEntity account);
    void Remove(Domain.Entities.ChartOfAccountsEntity account);
    Task<bool> RemoveByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
