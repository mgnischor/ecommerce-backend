using ECommerce.API.DTOs;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

/// <summary>
/// Controller for managing accounting operations
/// </summary>
[ApiController]
[Route("api/v1/accounting")]
[Produces("application/json")]
[Authorize]
public sealed class AccountingController : ControllerBase
{
    private readonly IRepository<ChartOfAccountsEntity> _chartOfAccountsRepository;
    private readonly IRepository<JournalEntryEntity> _journalEntryRepository;
    private readonly IRepository<AccountingEntryEntity> _accountingEntryRepository;
    private readonly ILogger<AccountingController> _logger;

    public AccountingController(
        IRepository<ChartOfAccountsEntity> chartOfAccountsRepository,
        IRepository<JournalEntryEntity> journalEntryRepository,
        IRepository<AccountingEntryEntity> accountingEntryRepository,
        ILogger<AccountingController> logger
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

    /// <summary>
    /// Gets all accounts from the chart of accounts
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of accounts</returns>
    [HttpGet("chart-of-accounts")]
    [ProducesResponseType(typeof(IEnumerable<ChartOfAccountsResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<ChartOfAccountsResponseDto>>> GetChartOfAccounts(
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Retrieving chart of accounts");

        try
        {
            var accounts = await _chartOfAccountsRepository.GetAllAsync(cancellationToken);

            var response = accounts.Select(a => new ChartOfAccountsResponseDto
            {
                Id = a.Id,
                AccountCode = a.AccountCode,
                AccountName = a.AccountName,
                AccountType = a.AccountType.ToString(),
                ParentAccountId = a.ParentAccountId,
                IsAnalytic = a.IsAnalytic,
                IsActive = a.IsActive,
                Balance = a.Balance,
                Description = a.Description,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt,
            });

            _logger.LogInformation(
                "Retrieved {Count} accounts from chart of accounts",
                response.Count()
            );

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving chart of accounts");
            throw;
        }
    }

    /// <summary>
    /// Gets a specific account by identifier
    /// </summary>
    /// <param name="id">Account identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Account details</returns>
    [HttpGet("chart-of-accounts/{id:guid}")]
    [ProducesResponseType(typeof(ChartOfAccountsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ChartOfAccountsResponseDto>> GetAccountById(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogDebug("Retrieving account: AccountId={AccountId}", id);

        try
        {
            var account = await _chartOfAccountsRepository.GetByIdAsync(id, cancellationToken);

            if (account == null)
            {
                _logger.LogWarning("Account not found: AccountId={AccountId}", id);
                return NotFound(new { Message = $"Account with ID {id} not found" });
            }

            var response = new ChartOfAccountsResponseDto
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

            _logger.LogDebug("Account retrieved successfully: AccountId={AccountId}", id);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving account: AccountId={AccountId}", id);
            throw;
        }
    }

    /// <summary>
    /// Gets all journal entries
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 50, max: 200)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of journal entries</returns>
    [HttpGet("journal-entries")]
    [ProducesResponseType(typeof(IEnumerable<JournalEntryResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<JournalEntryResponseDto>>> GetJournalEntries(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation(
            "Retrieving journal entries: PageNumber={PageNumber}, PageSize={PageSize}",
            pageNumber,
            pageSize
        );

        if (pageNumber < 1)
        {
            _logger.LogWarning("Invalid page number: {PageNumber}", pageNumber);
            return BadRequest(new { Message = "Page number must be greater than or equal to 1" });
        }

        if (pageSize < 1 || pageSize > 200)
        {
            _logger.LogWarning("Invalid page size: {PageSize}", pageSize);
            return BadRequest(new { Message = "Page size must be between 1 and 200" });
        }

        try
        {
            var allEntries = await _journalEntryRepository.GetAllAsync(cancellationToken);
            var entries = allEntries
                .OrderByDescending(je => je.EntryDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            var response = new List<JournalEntryResponseDto>();

            foreach (var entry in entries)
            {
                var accountingEntries = (
                    await _accountingEntryRepository.GetAllAsync(cancellationToken)
                )
                    .Where(ae => ae.JournalEntryId == entry.Id)
                    .ToList();

                var accounts = await _chartOfAccountsRepository.GetAllAsync(cancellationToken);

                var entryDto = new JournalEntryResponseDto
                {
                    Id = entry.Id,
                    EntryNumber = entry.EntryNumber,
                    EntryDate = entry.EntryDate,
                    DocumentType = entry.DocumentType,
                    DocumentNumber = entry.DocumentNumber,
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
                        .Select(ae =>
                        {
                            var account = accounts.FirstOrDefault(a => a.Id == ae.AccountId);
                            return new AccountingEntryResponseDto
                            {
                                Id = ae.Id,
                                AccountId = ae.AccountId,
                                AccountCode = account?.AccountCode ?? "N/A",
                                AccountName = account?.AccountName ?? "N/A",
                                EntryType = ae.EntryType.ToString(),
                                Amount = ae.Amount,
                                Description = ae.Description,
                                CostCenter = ae.CostCenter,
                            };
                        })
                        .ToList(),
                };

                response.Add(entryDto);
            }

            _logger.LogInformation(
                "Retrieved {Count} journal entries (page {PageNumber} of size {PageSize})",
                response.Count,
                pageNumber,
                pageSize
            );

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving journal entries: PageNumber={PageNumber}, PageSize={PageSize}",
                pageNumber,
                pageSize
            );
            throw;
        }
    }

    /// <summary>
    /// Gets a specific journal entry by identifier with all its accounting entries
    /// </summary>
    /// <param name="id">Journal entry identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Journal entry details with accounting entries</returns>
    [HttpGet("journal-entries/{id:guid}")]
    [ProducesResponseType(typeof(JournalEntryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<JournalEntryResponseDto>> GetJournalEntryById(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogDebug("Retrieving journal entry: JournalEntryId={JournalEntryId}", id);

        try
        {
            var entry = await _journalEntryRepository.GetByIdAsync(id, cancellationToken);

            if (entry == null)
            {
                _logger.LogWarning("Journal entry not found: JournalEntryId={JournalEntryId}", id);
                return NotFound(new { Message = $"Journal entry with ID {id} not found" });
            }

            var accountingEntries = (
                await _accountingEntryRepository.GetAllAsync(cancellationToken)
            )
                .Where(ae => ae.JournalEntryId == entry.Id)
                .ToList();

            var accounts = await _chartOfAccountsRepository.GetAllAsync(cancellationToken);

            var response = new JournalEntryResponseDto
            {
                Id = entry.Id,
                EntryNumber = entry.EntryNumber,
                EntryDate = entry.EntryDate,
                DocumentType = entry.DocumentType,
                DocumentNumber = entry.DocumentNumber,
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
                    .Select(ae =>
                    {
                        var account = accounts.FirstOrDefault(a => a.Id == ae.AccountId);
                        return new AccountingEntryResponseDto
                        {
                            Id = ae.Id,
                            AccountId = ae.AccountId,
                            AccountCode = account?.AccountCode ?? "N/A",
                            AccountName = account?.AccountName ?? "N/A",
                            EntryType = ae.EntryType.ToString(),
                            Amount = ae.Amount,
                            Description = ae.Description,
                            CostCenter = ae.CostCenter,
                        };
                    })
                    .ToList(),
            };

            _logger.LogDebug(
                "Journal entry retrieved successfully: JournalEntryId={JournalEntryId}, EntriesCount={EntriesCount}",
                id,
                response.Entries.Count
            );

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving journal entry: JournalEntryId={JournalEntryId}",
                id
            );
            throw;
        }
    }

    /// <summary>
    /// Gets journal entries by product identifier
    /// </summary>
    /// <param name="productId">Product identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of journal entries related to the product</returns>
    [HttpGet("journal-entries/product/{productId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<JournalEntryResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<
        ActionResult<IEnumerable<JournalEntryResponseDto>>
    > GetJournalEntriesByProduct(Guid productId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Retrieving journal entries for product: ProductId={ProductId}",
            productId
        );

        try
        {
            var allEntries = await _journalEntryRepository.GetAllAsync(cancellationToken);
            var entries = allEntries
                .Where(je => je.ProductId == productId)
                .OrderByDescending(je => je.EntryDate);

            var response = new List<JournalEntryResponseDto>();

            foreach (var entry in entries)
            {
                var accountingEntries = (
                    await _accountingEntryRepository.GetAllAsync(cancellationToken)
                )
                    .Where(ae => ae.JournalEntryId == entry.Id)
                    .ToList();

                var accounts = await _chartOfAccountsRepository.GetAllAsync(cancellationToken);

                var entryDto = new JournalEntryResponseDto
                {
                    Id = entry.Id,
                    EntryNumber = entry.EntryNumber,
                    EntryDate = entry.EntryDate,
                    DocumentType = entry.DocumentType,
                    DocumentNumber = entry.DocumentNumber,
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
                        .Select(ae =>
                        {
                            var account = accounts.FirstOrDefault(a => a.Id == ae.AccountId);
                            return new AccountingEntryResponseDto
                            {
                                Id = ae.Id,
                                AccountId = ae.AccountId,
                                AccountCode = account?.AccountCode ?? "N/A",
                                AccountName = account?.AccountName ?? "N/A",
                                EntryType = ae.EntryType.ToString(),
                                Amount = ae.Amount,
                                Description = ae.Description,
                                CostCenter = ae.CostCenter,
                            };
                        })
                        .ToList(),
                };

                response.Add(entryDto);
            }

            _logger.LogInformation(
                "Retrieved {Count} journal entries for product: ProductId={ProductId}",
                response.Count,
                productId
            );

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving journal entries for product: ProductId={ProductId}",
                productId
            );
            throw;
        }
    }
}
