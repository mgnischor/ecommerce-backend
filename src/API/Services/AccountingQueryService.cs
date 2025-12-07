using ECommerce.API.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;

namespace ECommerce.API.Services;

/// <summary>
/// Provides read-only accounting query operations with DTO mapping helpers.
/// </summary>
public sealed class AccountingQueryService : IAccountingQueryService
{
    private readonly IChartOfAccountsRepository _chartOfAccountsRepository;
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IAccountingEntryRepository _accountingEntryRepository;
    private readonly ILoggingService _logger;

    public AccountingQueryService(
        IChartOfAccountsRepository chartOfAccountsRepository,
        IJournalEntryRepository journalEntryRepository,
        IAccountingEntryRepository accountingEntryRepository,
        ILoggingService logger
    )
    {
        _chartOfAccountsRepository =
            chartOfAccountsRepository
            ?? throw new ArgumentNullException(nameof(chartOfAccountsRepository));
        _journalEntryRepository =
            journalEntryRepository
            ?? throw new ArgumentNullException(nameof(journalEntryRepository));
        _accountingEntryRepository =
            accountingEntryRepository
            ?? throw new ArgumentNullException(nameof(accountingEntryRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyList<ChartOfAccountsResponseDto>> GetChartOfAccountsAsync(
        CancellationToken cancellationToken = default
    )
    {
        var accounts = await _chartOfAccountsRepository.GetAllAsync(cancellationToken);
        var response = accounts.Select(MapAccount).ToList();

        _logger.LogInformation(
            "Prepared chart of accounts response: Count={Count}",
            response.Count
        );

        return response;
    }

    public async Task<ChartOfAccountsResponseDto?> GetAccountByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var account = await _chartOfAccountsRepository.GetByIdAsync(id, cancellationToken);
        return account == null ? null : MapAccount(account);
    }

    public async Task<IReadOnlyList<JournalEntryResponseDto>> GetJournalEntriesAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        var journalEntries = await _journalEntryRepository.GetPagedAsync(
            pageNumber,
            pageSize,
            cancellationToken
        );

        return await BuildJournalEntryDtosAsync(journalEntries, cancellationToken);
    }

    public async Task<JournalEntryResponseDto?> GetJournalEntryByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var journalEntry = await _journalEntryRepository.GetByIdAsync(id, cancellationToken);

        if (journalEntry == null)
        {
            return null;
        }

        var results = await BuildJournalEntryDtosAsync(new[] { journalEntry }, cancellationToken);
        return results.FirstOrDefault();
    }

    public async Task<IReadOnlyList<JournalEntryResponseDto>> GetJournalEntriesByProductAsync(
        Guid productId,
        CancellationToken cancellationToken = default
    )
    {
        var journalEntries = await _journalEntryRepository.GetAllAsync(cancellationToken);
        var filteredEntries = journalEntries
            .Where(entry => entry.ProductId == productId)
            .OrderByDescending(entry => entry.EntryDate)
            .ToList();

        return await BuildJournalEntryDtosAsync(filteredEntries, cancellationToken);
    }

    private async Task<IReadOnlyList<JournalEntryResponseDto>> BuildJournalEntryDtosAsync(
        IReadOnlyCollection<JournalEntryEntity> journalEntries,
        CancellationToken cancellationToken
    )
    {
        if (journalEntries.Count == 0)
        {
            return Array.Empty<JournalEntryResponseDto>();
        }

        var accountsById = await GetAccountsDictionaryAsync(cancellationToken);
        var accountingEntriesByJournalEntryId = await GetAccountingEntriesLookupAsync(
            journalEntries,
            cancellationToken
        );

        return journalEntries
            .Select(entry =>
                MapJournalEntry(entry, accountingEntriesByJournalEntryId, accountsById)
            )
            .ToList();
    }

    private async Task<IReadOnlyDictionary<Guid, ChartOfAccountsEntity>> GetAccountsDictionaryAsync(
        CancellationToken cancellationToken
    )
    {
        var accounts = await _chartOfAccountsRepository.GetAllAsync(cancellationToken);
        return accounts.ToDictionary(account => account.Id);
    }

    private async Task<
        IReadOnlyDictionary<Guid, List<AccountingEntryEntity>>
    > GetAccountingEntriesLookupAsync(
        IEnumerable<JournalEntryEntity> journalEntries,
        CancellationToken cancellationToken
    )
    {
        var journalEntryIds = journalEntries.Select(entry => entry.Id).ToHashSet();

        if (journalEntryIds.Count == 0)
        {
            return new Dictionary<Guid, List<AccountingEntryEntity>>();
        }

        var accountingEntries = await _accountingEntryRepository.GetAllAsync(cancellationToken);

        var lookup = new Dictionary<Guid, List<AccountingEntryEntity>>();

        foreach (
            var entry in accountingEntries.Where(ae => journalEntryIds.Contains(ae.JournalEntryId))
        )
        {
            if (!lookup.TryGetValue(entry.JournalEntryId, out var entries))
            {
                entries = new List<AccountingEntryEntity>();
                lookup[entry.JournalEntryId] = entries;
            }

            entries.Add(entry);
        }

        return lookup;
    }

    private static ChartOfAccountsResponseDto MapAccount(ChartOfAccountsEntity account)
    {
        return new ChartOfAccountsResponseDto
        {
            Id = account.Id,
            AccountCode = account.AccountCode,
            AccountName = account.AccountName,
            AccountType = account.AccountType.ToString(),
            ParentAccountId = account.ParentAccountId,
            IsAnalytic = account.IsAnalytic,
            IsActive = account.IsActive,
            Balance = account.Balance,
            Description = account.Description,
            CreatedAt = account.CreatedAt,
            UpdatedAt = account.UpdatedAt,
        };
    }

    private JournalEntryResponseDto MapJournalEntry(
        JournalEntryEntity entry,
        IReadOnlyDictionary<Guid, List<AccountingEntryEntity>> accountingEntriesByJournalEntryId,
        IReadOnlyDictionary<Guid, ChartOfAccountsEntity> accountsById
    )
    {
        var accountingEntries = accountingEntriesByJournalEntryId.TryGetValue(
            entry.Id,
            out var entries
        )
            ? entries
            : new List<AccountingEntryEntity>();

        return new JournalEntryResponseDto
        {
            Id = entry.Id,
            EntryNumber = entry.EntryNumber,
            EntryDate = entry.EntryDate,
            DocumentType = entry.DocumentType,
            DocumentNumber = entry.DocumentNumber ?? string.Empty,
            History = entry.History,
            TotalAmount = entry.TotalAmount,
            IsPosted = entry.IsPosted,
            PostedAt = entry.PostedAt,
            ProductId = entry.ProductId,
            InventoryTransactionId = entry.InventoryTransactionId,
            OrderId = entry.OrderId,
            CreatedBy = entry.CreatedBy,
            CreatedAt = entry.CreatedAt,
            Entries = accountingEntries
                .Select(accountingEntry => MapAccountingEntry(accountingEntry, accountsById))
                .ToList(),
        };
    }

    private AccountingEntryResponseDto MapAccountingEntry(
        AccountingEntryEntity entry,
        IReadOnlyDictionary<Guid, ChartOfAccountsEntity> accountsById
    )
    {
        var hasAccount = accountsById.TryGetValue(entry.AccountId, out var account);

        return new AccountingEntryResponseDto
        {
            Id = entry.Id,
            AccountId = entry.AccountId,
            AccountCode = hasAccount ? account!.AccountCode : string.Empty,
            AccountName = hasAccount ? account!.AccountName : string.Empty,
            EntryType = entry.EntryType.ToString(),
            Amount = entry.Amount,
            Description = entry.Description,
            CostCenter = entry.CostCenter,
        };
    }
}
