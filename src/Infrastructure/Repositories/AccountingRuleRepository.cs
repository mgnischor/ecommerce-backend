using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for AccountingRuleEntity
/// </summary>
public class AccountingRuleRepository : IAccountingRuleRepository
{
    private readonly Persistence.PostgresqlContext _context;

    public AccountingRuleRepository(Persistence.PostgresqlContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<AccountingRuleEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.AccountingRules.FirstOrDefaultAsync(
            r => r.Id == id,
            cancellationToken
        );
    }

    public async Task<AccountingRuleEntity?> GetByRuleCodeAsync(
        string ruleCode,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.AccountingRules.FirstOrDefaultAsync(
            r => r.RuleCode == ruleCode && r.IsActive,
            cancellationToken
        );
    }

    public async Task<IReadOnlyList<AccountingRuleEntity>> GetByTransactionTypeAsync(
        InventoryTransactionType transactionType,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .AccountingRules.Where(r => r.TransactionType == transactionType && r.IsActive)
            .OrderBy(r => r.Condition == null ? 0 : 1) // Rules without conditions first
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AccountingRuleEntity>> GetActiveRulesAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .AccountingRules.Where(r => r.IsActive)
            .OrderBy(r => r.TransactionType)
            .ThenBy(r => r.RuleCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AccountingRuleEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .AccountingRules.OrderBy(r => r.TransactionType)
            .ThenBy(r => r.RuleCode)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        AccountingRuleEntity rule,
        CancellationToken cancellationToken = default
    )
    {
        await _context.AccountingRules.AddAsync(rule, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public void Update(AccountingRuleEntity rule)
    {
        rule.UpdatedAt = DateTime.UtcNow;
        _context.AccountingRules.Update(rule);
    }

    public void Remove(AccountingRuleEntity rule)
    {
        _context.AccountingRules.Remove(rule);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
