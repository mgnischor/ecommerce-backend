using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Interfaces;

/// <summary>
/// Repository interface for AccountingRuleEntity operations
/// </summary>
public interface IAccountingRuleRepository
{
    Task<Domain.Entities.AccountingRuleEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );

    Task<Domain.Entities.AccountingRuleEntity?> GetByRuleCodeAsync(
        string ruleCode,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<Domain.Entities.AccountingRuleEntity>> GetByTransactionTypeAsync(
        InventoryTransactionType transactionType,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<Domain.Entities.AccountingRuleEntity>> GetActiveRulesAsync(
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<Domain.Entities.AccountingRuleEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    );

    Task AddAsync(
        Domain.Entities.AccountingRuleEntity rule,
        CancellationToken cancellationToken = default
    );

    void Update(Domain.Entities.AccountingRuleEntity rule);

    void Remove(Domain.Entities.AccountingRuleEntity rule);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
