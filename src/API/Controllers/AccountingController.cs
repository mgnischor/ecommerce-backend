using ECommerce.API.DTOs;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

/// <summary>
/// Accounting and financial management endpoints
/// </summary>
/// <remarks>
/// Provides comprehensive accounting operations including chart of accounts management and journal entries.
/// Supports double-entry bookkeeping with debits and credits. All endpoints require authentication.
/// </remarks>
[Tags("Accounting")]
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
    /// Retrieves the complete chart of accounts
    /// </summary>
    /// <remarks>
    /// Returns all accounts from the chart of accounts including their hierarchy and current balances.
    ///
    /// Sample request:
    ///
    ///     GET /api/v1/accounting/chart-of-accounts
    ///
    /// **Required permissions:** Authenticated user
    ///
    /// Account types include: Asset, Liability, Equity, Revenue, Expense
    ///
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>List of all accounts with their details and balances</returns>
    /// <response code="200">Successfully retrieved the chart of accounts.</response>
    /// <response code="401">Unauthorized. Authentication required.</response>
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
    /// Retrieves a specific account from the chart of accounts by its identifier
    /// </summary>
    /// <remarks>
    /// Returns detailed information about a single account including its current balance and configuration.
    ///
    /// Sample request:
    ///
    ///     GET /api/v1/accounting/chart-of-accounts/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///
    /// **Required permissions:** Authenticated user
    ///
    /// </remarks>
    /// <param name="id">Account unique identifier (GUID)</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Account details including balance and configuration</returns>
    /// <response code="200">Successfully retrieved the account.</response>
    /// <response code="401">Unauthorized. Authentication required.</response>
    /// <response code="404">Account not found with the specified ID.</response>
    [HttpGet("chart-of-accounts/{id:guid}")]
    [ProducesResponseType(typeof(ChartOfAccountsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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
    /// Retrieves journal entries with pagination support
    /// </summary>
    /// <remarks>
    /// Returns a paginated list of journal entries including all accounting entries (debits and credits).
    /// Entries are ordered by date (most recent first).
    ///
    /// Sample request:
    ///
    ///     GET /api/v1/accounting/journal-entries?pageNumber=1&amp;pageSize=50
    ///
    /// **Required permissions:** Authenticated user
    ///
    /// Each journal entry follows double-entry bookkeeping rules where total debits must equal total credits.
    ///
    /// </remarks>
    /// <param name="pageNumber">Page number (1-based, default: 1, minimum: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 50, range: 1-200)</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Paginated list of journal entries with their accounting entries</returns>
    /// <response code="200">Successfully retrieved the journal entries.</response>
    /// <response code="400">Invalid pagination parameters. Page number must be >= 1, page size must be between 1 and 200.</response>
    /// <response code="401">Unauthorized. Authentication required.</response>
    [HttpGet("journal-entries")]
    [ProducesResponseType(typeof(IEnumerable<JournalEntryResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<JournalEntryResponseDto>>> GetJournalEntries(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation(
            "Fetching journal entries: Page={PageNumber}, PageSize={PageSize}",
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
            var entries = (await _journalEntryRepository.GetAllAsync(cancellationToken))
                .OrderByDescending(e => e.EntryDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = new List<JournalEntryResponseDto>();

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
                        .Select(ae =>
                        {
                            var account = accounts.FirstOrDefault(a => a.Id == ae.AccountId);
                            return new AccountingEntryResponseDto
                            {
                                Id = ae.Id,
                                AccountId = ae.AccountId,
                                AccountCode = account?.AccountCode ?? string.Empty,
                                AccountName = account?.AccountName ?? string.Empty,
                                Amount = ae.Amount,
                                EntryType = ae.EntryType.ToString(),
                                Description = ae.Description,
                            };
                        })
                        .ToList(),
                };

                result.Add(entryDto);
            }

            _logger.LogInformation(
                "Retrieved {Count} journal entries: Page={PageNumber}",
                result.Count,
                pageNumber
            );

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving journal entries");
            return StatusCode(500, new { Message = "An error occurred processing your request" });
        }
    }

    /// <summary>
    /// Retrieves a specific journal entry with all its accounting entries
    /// </summary>
    /// <remarks>
    /// Returns detailed information about a single journal entry including all debit and credit entries.
    ///
    /// Sample request:
    ///
    ///     GET /api/v1/accounting/journal-entries/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///
    /// **Required permissions:** Authenticated user
    ///
    /// The response includes all associated accounting entries with their accounts, amounts, and entry types.
    ///
    /// </remarks>
    /// <param name="id">Journal entry unique identifier (GUID)</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Journal entry details with all accounting entries</returns>
    /// <response code="200">Successfully retrieved the journal entry.</response>
    /// <response code="401">Unauthorized. Authentication required.</response>
    /// <response code="404">Journal entry not found with the specified ID.</response>
    [HttpGet("journal-entries/{id:guid}")]
    [ProducesResponseType(typeof(JournalEntryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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
    /// Retrieves all journal entries related to a specific product
    /// </summary>
    /// <remarks>
    /// Returns journal entries that are associated with a specific product ID.
    /// Useful for tracking the financial impact of product transactions.
    ///
    /// Sample request:
    ///
    ///     GET /api/v1/accounting/journal-entries/product/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///
    /// **Required permissions:** Authenticated user
    ///
    /// Results are ordered by entry date (most recent first).
    ///
    /// </remarks>
    /// <param name="productId">Product unique identifier (GUID)</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>List of journal entries related to the specified product</returns>
    /// <response code="200">Successfully retrieved the journal entries. May be empty if no entries found for this product.</response>
    /// <response code="401">Unauthorized. Authentication required.</response>
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
