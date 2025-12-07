using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for managing chart of accounts data access operations.
/// </summary>
/// <remarks>
/// Provides comprehensive data access methods for <see cref="ChartOfAccountsEntity"/>.
/// The chart of accounts defines the structure of all accounting accounts in the system,
/// following NBC TG 26 (Brazilian GAAP) standards. Supports hierarchical account structures,
/// account type filtering, and balance tracking for double-entry bookkeeping.
/// All query operations use AsNoTracking for read performance except where entity tracking is required.
/// </remarks>
public sealed class ChartOfAccountsRepository : IChartOfAccountsRepository
{
    private readonly PostgresqlContext _context;
    private readonly ILoggingService _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChartOfAccountsRepository"/> class.
    /// </summary>
    /// <param name="context">The database context for data access operations.</param>
    /// <param name="logger">The logging service for diagnostic information.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="logger"/> is null.</exception>
    public ChartOfAccountsRepository(PostgresqlContext context, ILoggingService logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<ChartOfAccountsEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .ChartOfAccounts.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ChartOfAccountsEntity?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .ChartOfAccounts.AsNoTracking()
            .FirstOrDefaultAsync(c => c.AccountCode == code, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ChartOfAccountsEntity>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .ChartOfAccounts.AsNoTracking()
            .OrderBy(c => c.AccountCode)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ChartOfAccounts.CountAsync(cancellationToken);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public void Update(ChartOfAccountsEntity account)
    {
        if (account == null)
            throw new ArgumentNullException(nameof(account));

        _logger.LogDebug("Updating chart of accounts: {AccountId}", account.Id);
        _context.ChartOfAccounts.Update(account);
    }

    /// <inheritdoc />
    public void Remove(ChartOfAccountsEntity account)
    {
        if (account == null)
            throw new ArgumentNullException(nameof(account));

        _logger.LogDebug("Removing chart of accounts: {AccountId}", account.Id);
        _context.ChartOfAccounts.Remove(account);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
