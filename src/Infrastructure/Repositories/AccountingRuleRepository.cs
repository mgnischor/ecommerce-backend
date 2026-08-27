using Comex.Application.Interfaces;
using Comex.Domain.Entities;
using Comex.Domain.Enums;
using Comex.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Comex.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for managing accounting rule data access operations.
/// </summary>
/// <remarks>
/// Provides data access methods for <see cref="AccountingRuleEntity"/> including
/// queries by transaction type, rule code, and condition-based rule selection.
/// Accounting rules define the chart of accounts mapping for double-entry bookkeeping
/// based on inventory transaction types.
/// </remarks>
public class AccountingRuleRepository : IAccountingRuleRepository
{
    private readonly Persistence.PostgresqlContext _context;
    private readonly ILoggingService _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountingRuleRepository"/> class.
    /// </summary>
    /// <param name="context">The database context for data access operations.</param>
    /// <param name="logger">The logging service for diagnostic and audit information.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="logger"/> is null.</exception>
    public AccountingRuleRepository(Persistence.PostgresqlContext context, ILoggingService logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<AccountingRuleEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Getting accounting rule: {RuleId}", id);
        return await _context.AccountingRules.FirstOrDefaultAsync(
            r => r.Id == id,
            cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<AccountingRuleEntity?> GetByRuleCodeAsync(
        string ruleCode,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Getting accounting rule by code: {RuleCode}", ruleCode);
        return await _context.AccountingRules.FirstOrDefaultAsync(
            r => r.RuleCode == ruleCode && r.IsActive,
            cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AccountingRuleEntity>> GetByTransactionTypeAsync(
        InventoryTransactionType transactionType,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation(
            "Getting accounting rules by transaction type: {TransactionType}",
            transactionType
        );
        return await _context
            .AccountingRules.Where(r => r.TransactionType == transactionType && r.IsActive)
            .OrderBy(r => r.Condition == null ? 0 : 1) // Rules without conditions first
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AccountingRuleEntity>> GetActiveRulesAsync(
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Getting active accounting rules");
        return await _context
            .AccountingRules.Where(r => r.IsActive)
            .OrderBy(r => r.TransactionType)
            .ThenBy(r => r.RuleCode)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AccountingRuleEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Getting all accounting rules");
        return await _context
            .AccountingRules.OrderBy(r => r.TransactionType)
            .ThenBy(r => r.RuleCode)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(
        AccountingRuleEntity rule,
        CancellationToken cancellationToken = default
    )
    {
        await _context.AccountingRules.AddAsync(rule, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public void Update(AccountingRuleEntity rule)
    {
        rule.UpdatedAt = DateTime.UtcNow;
        _context.AccountingRules.Update(rule);
    }

    /// <inheritdoc />
    public void Remove(AccountingRuleEntity rule)
    {
        _context.AccountingRules.Remove(rule);
    }

    /// <inheritdoc />
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
