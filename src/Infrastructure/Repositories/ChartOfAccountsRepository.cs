using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

/// <summary>
/// Repository for managing ChartOfAccountsEntity data access.
/// </summary>
public sealed class ChartOfAccountsRepository : IChartOfAccountsRepository
{
    private readonly PostgresqlContext _context;
    private readonly ILoggingService _logger;

    public ChartOfAccountsRepository(PostgresqlContext context, ILoggingService logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ChartOfAccountsEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .ChartOfAccounts.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<ChartOfAccountsEntity?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .ChartOfAccounts.AsNoTracking()
            .FirstOrDefaultAsync(c => c.AccountCode == code, cancellationToken);
    }

    public async Task<IReadOnlyList<ChartOfAccountsEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .ChartOfAccounts.AsNoTracking()
            .OrderBy(c => c.AccountCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ChartOfAccountsEntity>> GetByTypeAsync(
        int accountType,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .ChartOfAccounts.AsNoTracking()
            .Where(c => (int)c.AccountType == accountType)
            .OrderBy(c => c.AccountCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ChartOfAccountsEntity>> GetActiveAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .ChartOfAccounts.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.AccountCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ChartOfAccounts.CountAsync(cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(
        string code,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.ChartOfAccounts.AnyAsync(
            c => c.AccountCode == code,
            cancellationToken
        );
    }

    public async Task AddAsync(
        ChartOfAccountsEntity account,
        CancellationToken cancellationToken = default
    )
    {
        if (account == null)
            throw new ArgumentNullException(nameof(account));

        _logger.LogDebug("Adding new chart of accounts: {AccountCode}", account.AccountCode);
        await _context.ChartOfAccounts.AddAsync(account, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public void Update(ChartOfAccountsEntity account)
    {
        if (account == null)
            throw new ArgumentNullException(nameof(account));

        _logger.LogDebug("Updating chart of accounts: {AccountId}", account.Id);
        _context.ChartOfAccounts.Update(account);
    }

    public void Remove(ChartOfAccountsEntity account)
    {
        if (account == null)
            throw new ArgumentNullException(nameof(account));

        _logger.LogDebug("Removing chart of accounts: {AccountId}", account.Id);
        _context.ChartOfAccounts.Remove(account);
    }

    public async Task<bool> RemoveByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var account = await _context.ChartOfAccounts.FirstOrDefaultAsync(
            c => c.Id == id,
            cancellationToken
        );

        if (account == null)
            return false;

        _context.ChartOfAccounts.Remove(account);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
