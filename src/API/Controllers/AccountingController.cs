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
/// <para>
/// Provides comprehensive accounting operations including chart of accounts management and journal entries.
/// Supports double-entry bookkeeping with debits and credits. All endpoints require authentication.
/// </para>
/// <para>
/// <strong>Key Features:</strong>
/// </para>
/// <list type="bullet">
/// <item><description>Chart of Accounts management with hierarchical account structures</description></item>
/// <item><description>Journal entry tracking with complete audit trails</description></item>
/// <item><description>Double-entry bookkeeping validation (debits = credits)</description></item>
/// <item><description>Support for multiple account types (Asset, Liability, Equity, Revenue, Expense)</description></item>
/// <item><description>Integration with product, inventory, and order transactions</description></item>
/// </list>
/// <para>
/// <strong>Security:</strong> All endpoints require authenticated users. Administrative operations may require elevated privileges.
/// </para>
/// </remarks>
[Tags("Accounting")]
[ApiController]
[Route("api/v1/accounting")]
[Produces("application/json")]
[Authorize]
public sealed class AccountingController : ControllerBase
{
    /// <summary>
    /// Repository for managing chart of accounts entities
    /// </summary>
    /// <remarks>
    /// Provides data access operations for account definitions including account codes, names, types, and balances.
    /// </remarks>
    private readonly IRepository<ChartOfAccountsEntity> _chartOfAccountsRepository;

    /// <summary>
    /// Repository for managing journal entry entities
    /// </summary>
    /// <remarks>
    /// Provides data access operations for journal entries which represent complete accounting transactions.
    /// </remarks>
    private readonly IRepository<JournalEntryEntity> _journalEntryRepository;

    /// <summary>
    /// Repository for managing accounting entry entities
    /// </summary>
    /// <remarks>
    /// Provides data access operations for individual debit and credit entries within journal entries.
    /// </remarks>
    private readonly IRepository<AccountingEntryEntity> _accountingEntryRepository;

    /// <summary>
    /// Logger instance for tracking controller operations and errors
    /// </summary>
    /// <remarks>
    /// Used to log information, warnings, and errors throughout the accounting controller lifecycle.
    /// </remarks>
    private readonly ILogger<AccountingController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountingController"/> class
    /// </summary>
    /// <param name="chartOfAccountsRepository">Repository for chart of accounts operations. Cannot be null.</param>
    /// <param name="journalEntryRepository">Repository for journal entry operations. Cannot be null.</param>
    /// <param name="accountingEntryRepository">Repository for accounting entry operations. Cannot be null.</param>
    /// <param name="logger">Logger instance for recording controller activity. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any of the required dependencies (repositories or logger) are null.
    /// </exception>
    /// <remarks>
    /// This constructor uses dependency injection to provide all required services.
    /// All parameters are validated for null values to ensure the controller operates correctly.
    /// </remarks>
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
    /// <para>
    /// Returns all accounts from the chart of accounts including their hierarchy and current balances.
    /// This endpoint provides a complete view of the organization's accounting structure.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// GET /api/v1/accounting/chart-of-accounts
    /// Authorization: Bearer {token}
    /// </code>
    /// <para>
    /// <strong>Account Types:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Asset:</strong> Resources owned by the business (e.g., Cash, Inventory, Accounts Receivable)</description></item>
    /// <item><description><strong>Liability:</strong> Obligations owed by the business (e.g., Accounts Payable, Loans)</description></item>
    /// <item><description><strong>Equity:</strong> Owner's stake in the business (e.g., Capital, Retained Earnings)</description></item>
    /// <item><description><strong>Revenue:</strong> Income from business operations (e.g., Sales Revenue, Service Income)</description></item>
    /// <item><description><strong>Expense:</strong> Costs of doing business (e.g., Cost of Goods Sold, Operating Expenses)</description></item>
    /// </list>
    /// <para>
    /// <strong>Required permissions:</strong> Authenticated user
    /// </para>
    /// <para>
    /// <strong>Response includes:</strong> Account ID, Code, Name, Type, Parent Account (for hierarchical structure),
    /// Analytic flag, Active status, Current Balance, Description, and timestamps.
    /// </para>
    /// </remarks>
    /// <param name="cancellationToken">
    /// Optional cancellation token to cancel the asynchronous operation.
    /// Useful for preventing unnecessary database queries if the request is cancelled by the client.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing an enumerable collection of <see cref="ChartOfAccountsResponseDto"/> objects.
    /// Each DTO represents a single account with its complete configuration and current balance.
    /// </returns>
    /// <response code="200">
    /// Successfully retrieved the chart of accounts. Returns a JSON array of account objects.
    /// </response>
    /// <response code="401">
    /// Unauthorized. Authentication required. The request must include a valid authorization token.
    /// </response>
    /// <response code="500">
    /// Internal server error. An unexpected error occurred while processing the request.
    /// Check server logs for detailed error information.
    /// </response>
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
    /// <para>
    /// Returns detailed information about a single account including its current balance and configuration.
    /// Use this endpoint to get complete details about a specific account when you know its unique identifier.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// GET /api/v1/accounting/chart-of-accounts/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// Authorization: Bearer {token}
    /// </code>
    /// <para>
    /// <strong>Required permissions:</strong> Authenticated user
    /// </para>
    /// <para>
    /// <strong>Use cases:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Viewing detailed account information for auditing</description></item>
    /// <item><description>Verifying account configuration before posting journal entries</description></item>
    /// <item><description>Checking current account balances</description></item>
    /// <item><description>Reviewing account hierarchy and relationships</description></item>
    /// </list>
    /// </remarks>
    /// <param name="id">
    /// The unique identifier (GUID) of the account to retrieve.
    /// Must be a valid GUID format (e.g., 3fa85f64-5717-4562-b3fc-2c963f66afa6).
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to cancel the asynchronous operation.
    /// Allows the client to cancel the request if it's no longer needed.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing a <see cref="ChartOfAccountsResponseDto"/> object
    /// with the account's complete details including ID, code, name, type, balance, and configuration.
    /// </returns>
    /// <response code="200">
    /// Successfully retrieved the account. Returns a JSON object with complete account details.
    /// </response>
    /// <response code="401">
    /// Unauthorized. Authentication required. The request must include a valid authorization token.
    /// </response>
    /// <response code="404">
    /// Account not found with the specified ID. The GUID may be invalid or the account may not exist in the system.
    /// </response>
    /// <response code="500">
    /// Internal server error. An unexpected error occurred while processing the request.
    /// Check server logs for detailed error information.
    /// </response>
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
    /// <para>
    /// Returns a paginated list of journal entries including all accounting entries (debits and credits).
    /// Entries are ordered by date (most recent first) to provide a chronological view of transactions.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// GET /api/v1/accounting/journal-entries?pageNumber=1&amp;pageSize=50
    /// Authorization: Bearer {token}
    /// </code>
    /// <para>
    /// <strong>Double-Entry Bookkeeping:</strong>
    /// Each journal entry follows the fundamental accounting equation where total debits must equal total credits.
    /// This ensures the accounting records remain in balance and maintains financial data integrity.
    /// </para>
    /// <para>
    /// <strong>Pagination Guidelines:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Page numbering starts at 1 (not 0)</description></item>
    /// <item><description>Default page size is 50 entries per page</description></item>
    /// <item><description>Maximum page size is 200 to prevent performance issues</description></item>
    /// <item><description>Use smaller page sizes for better performance on large datasets</description></item>
    /// </list>
    /// <para>
    /// <strong>Required permissions:</strong> Authenticated user
    /// </para>
    /// <para>
    /// <strong>Response includes:</strong> Journal entry header (entry number, date, document info) and
    /// detailed accounting entries with account codes, names, amounts, and debit/credit designations.
    /// </para>
    /// </remarks>
    /// <param name="pageNumber">
    /// The page number to retrieve (1-based index).
    /// Default value is 1. Must be greater than or equal to 1.
    /// Use this parameter to navigate through multiple pages of results.
    /// </param>
    /// <param name="pageSize">
    /// The number of journal entries to return per page.
    /// Default value is 50. Valid range is 1 to 200.
    /// Adjust based on your application's performance requirements and user experience needs.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to cancel the asynchronous operation.
    /// Useful for preventing unnecessary processing if the client cancels the request.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing an enumerable collection of <see cref="JournalEntryResponseDto"/> objects.
    /// Each DTO includes the journal entry header and all associated accounting entries (debits and credits).
    /// The collection is limited to the specified page size and represents the requested page.
    /// </returns>
    /// <response code="200">
    /// Successfully retrieved the journal entries. Returns a JSON array of journal entry objects with their accounting entries.
    /// </response>
    /// <response code="400">
    /// Bad request. Invalid pagination parameters provided.
    /// Page number must be greater than or equal to 1, and page size must be between 1 and 200.
    /// </response>
    /// <response code="401">
    /// Unauthorized. Authentication required. The request must include a valid authorization token.
    /// </response>
    /// <response code="500">
    /// Internal server error. An unexpected error occurred while processing the request.
    /// Check server logs for detailed error information.
    /// </response>
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
    /// <para>
    /// Returns detailed information about a single journal entry including all debit and credit entries.
    /// This endpoint provides complete visibility into a specific accounting transaction with all its components.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// GET /api/v1/accounting/journal-entries/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// Authorization: Bearer {token}
    /// </code>
    /// <para>
    /// <strong>Required permissions:</strong> Authenticated user
    /// </para>
    /// <para>
    /// <strong>Response includes:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Journal Entry Header:</strong> Entry number, date, document type and number, history/description, total amount, posting status</description></item>
    /// <item><description><strong>Accounting Entries:</strong> All debit and credit lines with account codes, names, amounts, entry types, descriptions, and cost centers</description></item>
    /// <item><description><strong>Related Entities:</strong> Links to associated products, inventory transactions, and orders (if applicable)</description></item>
    /// <item><description><strong>Audit Information:</strong> Creator ID and timestamps for tracking and compliance</description></item>
    /// </list>
    /// <para>
    /// <strong>Use cases:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Auditing specific transactions</description></item>
    /// <item><description>Reviewing transaction details before approval/posting</description></item>
    /// <item><description>Investigating accounting discrepancies</description></item>
    /// <item><description>Generating detailed transaction reports</description></item>
    /// </list>
    /// </remarks>
    /// <param name="id">
    /// The unique identifier (GUID) of the journal entry to retrieve.
    /// Must be a valid GUID format (e.g., 3fa85f64-5717-4562-b3fc-2c963f66afa6).
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to cancel the asynchronous operation.
    /// Allows the client to cancel the request if it's no longer needed.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing a <see cref="JournalEntryResponseDto"/> object
    /// with the complete journal entry including all accounting entries (debits and credits),
    /// related entity references, and audit information.
    /// </returns>
    /// <response code="200">
    /// Successfully retrieved the journal entry. Returns a JSON object with complete transaction details.
    /// </response>
    /// <response code="401">
    /// Unauthorized. Authentication required. The request must include a valid authorization token.
    /// </response>
    /// <response code="404">
    /// Journal entry not found with the specified ID. The GUID may be invalid or the entry may not exist in the system.
    /// </response>
    /// <response code="500">
    /// Internal server error. An unexpected error occurred while processing the request.
    /// Check server logs for detailed error information.
    /// </response>
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
    /// <para>
    /// Returns journal entries that are associated with a specific product ID.
    /// This endpoint is useful for tracking the complete financial impact and history of product-related transactions.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// GET /api/v1/accounting/journal-entries/product/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// Authorization: Bearer {token}
    /// </code>
    /// <para>
    /// <strong>Required permissions:</strong> Authenticated user
    /// </para>
    /// <para>
    /// <strong>Results are ordered by:</strong> Entry date (most recent first) for chronological analysis.
    /// </para>
    /// <para>
    /// <strong>Common product-related transactions include:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Purchases:</strong> When inventory is purchased (Debit: Inventory, Credit: Accounts Payable)</description></item>
    /// <item><description><strong>Sales:</strong> When products are sold (Debit: Cost of Goods Sold, Credit: Inventory)</description></item>
    /// <item><description><strong>Adjustments:</strong> Inventory write-offs, shrinkage, or corrections</description></item>
    /// <item><description><strong>Returns:</strong> Customer returns or vendor returns affecting inventory</description></item>
    /// <item><description><strong>Revaluations:</strong> Changes in inventory valuation or pricing</description></item>
    /// </list>
    /// <para>
    /// <strong>Use cases:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Product cost analysis and profitability tracking</description></item>
    /// <item><description>Inventory transaction history and audit trails</description></item>
    /// <item><description>Financial reconciliation for specific products</description></item>
    /// <item><description>Investigating discrepancies in product accounting</description></item>
    /// </list>
    /// <para>
    /// <strong>Note:</strong> An empty array is returned if no journal entries are found for the specified product.
    /// This is a normal response and does not indicate an error.
    /// </para>
    /// </remarks>
    /// <param name="productId">
    /// The unique identifier (GUID) of the product for which to retrieve journal entries.
    /// Must be a valid GUID format (e.g., 3fa85f64-5717-4562-b3fc-2c963f66afa6).
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to cancel the asynchronous operation.
    /// Useful for preventing unnecessary processing if the client cancels the request.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing an enumerable collection of <see cref="JournalEntryResponseDto"/> objects.
    /// Each DTO includes the complete journal entry with all accounting entries related to the specified product.
    /// Returns an empty array if no entries are found for the product.
    /// </returns>
    /// <response code="200">
    /// Successfully retrieved the journal entries. Returns a JSON array of journal entry objects.
    /// The array may be empty if no entries are associated with the specified product ID.
    /// </response>
    /// <response code="401">
    /// Unauthorized. Authentication required. The request must include a valid authorization token.
    /// </response>
    /// <response code="500">
    /// Internal server error. An unexpected error occurred while processing the request.
    /// Check server logs for detailed error information.
    /// </response>
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
