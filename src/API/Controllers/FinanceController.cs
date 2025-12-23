using ECommerce.API.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Domain.Interfaces;

namespace ECommerce.API.Controllers;

/// <summary>
/// Financial transaction and cash flow management endpoints
/// </summary>
/// <remarks>
/// <para>
/// Provides comprehensive financial operations including transaction tracking, cash flow analysis,
/// accounts receivable/payable management, payment reconciliation, and financial reporting.
/// All endpoints require authentication and provide real-time financial insights.
/// </para>
/// <para>
/// <strong>Key Features:</strong>
/// </para>
/// <list type="bullet">
/// <item><description>Complete financial transaction history with filtering and search</description></item>
/// <item><description>Real-time cash flow reports and trend analysis</description></item>
/// <item><description>Accounts receivable aging and collection tracking</description></item>
/// <item><description>Accounts payable aging and payment scheduling</description></item>
/// <item><description>Payment reconciliation with bank statements</description></item>
/// <item><description>Financial dashboard with key performance indicators</description></item>
/// <item><description>Integration with inventory and accounting systems</description></item>
/// </list>
/// <para>
/// <strong>Financial Transaction Types:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><strong>Customer Payments:</strong> Cash received from customers</description></item>
/// <item><description><strong>Supplier Payments:</strong> Cash paid to suppliers</description></item>
/// <item><description><strong>Sales Revenue:</strong> Revenue recognition from sales</description></item>
/// <item><description><strong>Purchase Expenses:</strong> Costs of inventory acquisition</description></item>
/// <item><description><strong>Refunds:</strong> Customer refunds and returns</description></item>
/// <item><description><strong>Operating Expenses:</strong> Shipping, fees, marketing, etc.</description></item>
/// <item><description><strong>Accounts Receivable/Payable:</strong> Credit transactions</description></item>
/// </list>
/// <para>
/// <strong>Security:</strong> All endpoints require authenticated users with appropriate permissions.
/// Financial data access may be restricted based on user roles and organizational hierarchy.
/// </para>
/// <para>
/// <strong>Integration:</strong> Financial transactions are automatically created when:
/// </para>
/// <list type="bullet">
/// <item><description>Inventory movements occur (purchases, sales, returns)</description></item>
/// <item><description>Payments are processed through payment providers</description></item>
/// <item><description>Accounting journal entries are posted</description></item>
/// <item><description>Operating expenses are recorded</description></item>
/// </list>
/// </remarks>
[Tags("Finance")]
[ApiController]
[Route("api/v1/finance")]
[Produces("application/json")]
[Authorize]
public sealed class FinanceController : ControllerBase
{
    private readonly IFinancialService _financialService;
    private readonly IFinancialTransactionRepository _financialTransactionRepository;
    private readonly ILoggingService _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FinanceController"/> class
    /// </summary>
    /// <param name="financialService">Financial service for business operations</param>
    /// <param name="financialTransactionRepository">Repository for financial transaction data access</param>
    /// <param name="logger">Logger for tracking operations and errors</param>
    /// <exception cref="ArgumentNullException">Thrown when any required dependency is null</exception>
    public FinanceController(
        IFinancialService financialService,
        IFinancialTransactionRepository financialTransactionRepository,
        LoggingService<FinanceController> logger
    )
    {
        _financialService =
            financialService ?? throw new ArgumentNullException(nameof(financialService));
        _financialTransactionRepository =
            financialTransactionRepository
            ?? throw new ArgumentNullException(nameof(financialTransactionRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all financial transactions with pagination
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returns a paginated list of all financial transactions ordered by date (most recent first).
    /// Use query parameters to filter by date range, transaction type, or reconciliation status.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// GET /api/v1/finance/transactions?pageNumber=1&amp;pageSize=50
    /// Authorization: Bearer {token}
    /// </code>
    /// <para>
    /// <strong>Pagination Guidelines:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Page numbering starts at 1</description></item>
    /// <item><description>Default page size is 50 transactions</description></item>
    /// <item><description>Maximum page size is 200 for performance</description></item>
    /// </list>
    /// </remarks>
    /// <param name="pageNumber">Page number to retrieve (1-based index). Default is 1.</param>
    /// <param name="pageSize">Number of transactions per page. Default is 50.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated collection of financial transaction DTOs</returns>
    /// <response code="200">Successfully retrieved financial transactions</response>
    /// <response code="400">Bad request - invalid pagination parameters</response>
    /// <response code="401">Unauthorized - authentication required</response>
    [HttpGet("transactions")]
    [ProducesResponseType(
        typeof(IEnumerable<FinancialTransactionResponseDto>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<FinancialTransactionResponseDto>>> GetTransactions(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation(
            "Fetching financial transactions: Page={PageNumber}, Size={PageSize}",
            pageNumber,
            pageSize
        );

        if (pageNumber < 1)
            return BadRequest(new { Message = "Page number must be greater than or equal to 1" });

        if (pageSize < 1 || pageSize > 200)
            return BadRequest(new { Message = "Page size must be between 1 and 200" });

        try
        {
            var transactions = (
                await _financialTransactionRepository.GetAllAsync(cancellationToken)
            )
                .OrderByDescending(t => t.TransactionDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToDto)
                .ToList();

            _logger.LogInformation(
                "Retrieved {Count} financial transactions: Page={PageNumber}",
                transactions.Count,
                pageNumber
            );

            return Ok(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving financial transactions");
            return StatusCode(500, new { Message = "An error occurred processing your request" });
        }
    }

    /// <summary>
    /// Retrieves a specific financial transaction by ID
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returns detailed information about a single financial transaction including
    /// all related entities and reconciliation status.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// GET /api/v1/finance/transactions/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// Authorization: Bearer {token}
    /// </code>
    /// </remarks>
    /// <param name="id">Unique identifier of the financial transaction</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Financial transaction DTO with complete details</returns>
    /// <response code="200">Successfully retrieved the financial transaction</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Financial transaction not found</response>
    [HttpGet("transactions/{id:guid}")]
    [ProducesResponseType(typeof(FinancialTransactionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FinancialTransactionResponseDto>> GetTransactionById(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogDebug("Retrieving financial transaction: {TransactionId}", id);

        try
        {
            var transaction = await _financialTransactionRepository.GetByIdAsync(
                id,
                cancellationToken
            );

            if (transaction == null)
            {
                _logger.LogWarning("Financial transaction not found: {TransactionId}", id);
                return NotFound(new { Message = $"Financial transaction with ID {id} not found" });
            }

            return Ok(MapToDto(transaction));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving financial transaction: {TransactionId}", id);
            throw;
        }
    }

    /// <summary>
    /// Retrieves financial transactions for a date range
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returns all financial transactions that occurred within the specified date range.
    /// Essential for generating period-based financial reports and analyzing trends.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// GET /api/v1/finance/transactions/period?startDate=2025-11-01&amp;endDate=2025-11-30
    /// Authorization: Bearer {token}
    /// </code>
    /// <para>
    /// <strong>Use Cases:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Monthly, quarterly, or annual financial reports</description></item>
    /// <item><description>Cash flow analysis for specific periods</description></item>
    /// <item><description>Period comparison and trend analysis</description></item>
    /// <item><description>Tax period reporting and compliance</description></item>
    /// </list>
    /// </remarks>
    /// <param name="startDate">Start date of the period (inclusive)</param>
    /// <param name="endDate">End date of the period (inclusive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of financial transactions within the date range</returns>
    /// <response code="200">Successfully retrieved period transactions</response>
    /// <response code="400">Bad request - invalid date range</response>
    /// <response code="401">Unauthorized - authentication required</response>
    [HttpGet("transactions/period")]
    [ProducesResponseType(
        typeof(IEnumerable<FinancialTransactionResponseDto>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<
        ActionResult<IEnumerable<FinancialTransactionResponseDto>>
    > GetTransactionsByPeriod(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation(
            "Fetching transactions for period: {StartDate} to {EndDate}",
            startDate,
            endDate
        );

        if (startDate > endDate)
            return BadRequest(new { Message = "Start date must be before or equal to end date" });

        try
        {
            var transactions = await _financialService.GetTransactionsByPeriodAsync(
                startDate,
                endDate,
                cancellationToken
            );

            var response = transactions.Select(MapToDto).ToList();

            _logger.LogInformation("Retrieved {Count} transactions for period", response.Count);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving period transactions");
            return StatusCode(500, new { Message = "An error occurred processing your request" });
        }
    }

    /// <summary>
    /// Retrieves unreconciled financial transactions
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returns all financial transactions that have not been reconciled with bank statements.
    /// Critical for financial control, fraud detection, and ensuring accounting accuracy.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// GET /api/v1/finance/transactions/unreconciled
    /// Authorization: Bearer {token}
    /// </code>
    /// <para>
    /// <strong>Reconciliation Importance:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Ensures financial records match bank statements</description></item>
    /// <item><description>Detects errors, fraud, or missing transactions</description></item>
    /// <item><description>Required for accurate financial reporting</description></item>
    /// <item><description>Supports audit trails and compliance</description></item>
    /// </list>
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of unreconciled financial transactions</returns>
    /// <response code="200">Successfully retrieved unreconciled transactions</response>
    /// <response code="401">Unauthorized - authentication required</response>
    [HttpGet("transactions/unreconciled")]
    [ProducesResponseType(
        typeof(IEnumerable<FinancialTransactionResponseDto>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<
        ActionResult<IEnumerable<FinancialTransactionResponseDto>>
    > GetUnreconciledTransactions(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching unreconciled transactions");

        try
        {
            var transactions = await _financialService.GetUnreconciledTransactionsAsync(
                cancellationToken
            );

            var response = transactions.Select(MapToDto).ToList();

            _logger.LogInformation("Retrieved {Count} unreconciled transactions", response.Count);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving unreconciled transactions");
            return StatusCode(500, new { Message = "An error occurred processing your request" });
        }
    }

    /// <summary>
    /// Retrieves financial transactions by order ID
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returns all financial transactions associated with a specific order,
    /// providing complete financial visibility for order processing and fulfillment.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// GET /api/v1/finance/transactions/order/7c9e6679-7425-40de-944b-e07fc1f90ae7
    /// Authorization: Bearer {token}
    /// </code>
    /// <para>
    /// An order may have multiple related transactions:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Revenue recognition when order is placed</description></item>
    /// <item><description>Accounts receivable creation</description></item>
    /// <item><description>Customer payment receipt</description></item>
    /// <item><description>Payment processing fees</description></item>
    /// <item><description>Shipping costs</description></item>
    /// <item><description>Refunds if applicable</description></item>
    /// </list>
    /// </remarks>
    /// <param name="orderId">Unique identifier of the order</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of financial transactions for the order</returns>
    /// <response code="200">Successfully retrieved order transactions</response>
    /// <response code="401">Unauthorized - authentication required</response>
    [HttpGet("transactions/order/{orderId:guid}")]
    [ProducesResponseType(
        typeof(IEnumerable<FinancialTransactionResponseDto>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<
        ActionResult<IEnumerable<FinancialTransactionResponseDto>>
    > GetTransactionsByOrder(Guid orderId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching transactions for order: {OrderId}", orderId);

        try
        {
            var transactions = await _financialTransactionRepository.GetByOrderIdAsync(
                orderId,
                cancellationToken
            );

            var response = transactions.Select(MapToDto).ToList();

            _logger.LogInformation(
                "Retrieved {Count} transactions for order {OrderId}",
                response.Count,
                orderId
            );

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order transactions: {OrderId}", orderId);
            return StatusCode(500, new { Message = "An error occurred processing your request" });
        }
    }

    /// <summary>
    /// Retrieves cash flow report for a period
    /// </summary>
    /// <remarks>
    /// <para>
    /// Generates a comprehensive cash flow report showing inflows, outflows, and net cash movement
    /// categorized by transaction type. Essential for financial planning and liquidity management.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// GET /api/v1/finance/cash-flow?startDate=2025-11-01&amp;endDate=2025-11-30
    /// Authorization: Bearer {token}
    /// </code>
    /// <para>
    /// <strong>Report Contents:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Total cash inflows (revenue, payments received)</description></item>
    /// <item><description>Total cash outflows (expenses, payments made)</description></item>
    /// <item><description>Net cash flow (inflows - outflows)</description></item>
    /// <item><description>Breakdown by category (sales, purchases, operating expenses, etc.)</description></item>
    /// </list>
    /// </remarks>
    /// <param name="startDate">Start date of the reporting period</param>
    /// <param name="endDate">End date of the reporting period</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cash flow report with categorized inflows and outflows</returns>
    /// <response code="200">Successfully generated cash flow report</response>
    /// <response code="400">Bad request - invalid date range</response>
    /// <response code="401">Unauthorized - authentication required</response>
    [HttpGet("cash-flow")]
    [ProducesResponseType(typeof(CashFlowReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CashFlowReportDto>> GetCashFlowReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation(
            "Generating cash flow report: {StartDate} to {EndDate}",
            startDate,
            endDate
        );

        if (startDate > endDate)
            return BadRequest(new { Message = "Start date must be before or equal to end date" });

        try
        {
            var summary = await _financialService.GetCashFlowSummaryAsync(
                startDate,
                endDate,
                cancellationToken
            );

            var report = new CashFlowReportDto
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalInflows = summary["TotalInflows"],
                TotalOutflows = summary["TotalOutflows"],
                NetCashFlow = summary["NetCashFlow"],
                CustomerPayments = summary["CustomerPayments"],
                SalesRevenue = summary["SalesRevenue"],
                SupplierPayments = summary["SupplierPayments"],
                PurchaseExpenses = summary["PurchaseExpenses"],
                Refunds = summary["Refunds"],
                OperatingExpenses = summary["OperatingExpenses"],
                PaymentFees = summary["PaymentFees"],
                ShippingCosts = summary["ShippingCosts"],
                Taxes = summary["Taxes"],
            };

            _logger.LogInformation(
                "Cash flow report generated: NetCashFlow={NetCashFlow}",
                report.NetCashFlow
            );

            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating cash flow report");
            return StatusCode(500, new { Message = "An error occurred processing your request" });
        }
    }

    /// <summary>
    /// Retrieves accounts receivable summary and aging
    /// </summary>
    /// <remarks>
    /// <para>
    /// Provides a comprehensive summary of outstanding accounts receivable with aging analysis.
    /// Critical for credit management, collections, and cash flow forecasting.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// GET /api/v1/finance/accounts-receivable
    /// Authorization: Bearer {token}
    /// </code>
    /// <para>
    /// <strong>Aging Buckets:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Current (0-30 days):</strong> Recently billed, payment expected</description></item>
    /// <item><description><strong>31-60 days:</strong> Slightly overdue, follow-up recommended</description></item>
    /// <item><description><strong>61-90 days:</strong> Overdue, collection efforts needed</description></item>
    /// <item><description><strong>Over 90 days:</strong> Seriously overdue, high risk of non-payment</description></item>
    /// </list>
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Accounts receivable summary with aging analysis</returns>
    /// <response code="200">Successfully retrieved AR summary</response>
    /// <response code="401">Unauthorized - authentication required</response>
    [HttpGet("accounts-receivable")]
    [ProducesResponseType(typeof(AccountsReceivableDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AccountsReceivableDto>> GetAccountsReceivable(
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Fetching accounts receivable summary");

        try
        {
            var summary = await _financialService.GetAccountsReceivableSummaryAsync(
                cancellationToken
            );

            var arDto = new AccountsReceivableDto
            {
                TotalReceivable = summary["TotalReceivable"],
                Current_0_30 = summary["Current_0_30"],
                Aging_31_60 = summary["Aging_31_60"],
                Aging_61_90 = summary["Aging_61_90"],
                Aging_Over90 = summary["Aging_Over90"],
                Count = (int)summary["Count"],
            };

            _logger.LogInformation(
                "AR summary retrieved: Total={Total}, Count={Count}",
                arDto.TotalReceivable,
                arDto.Count
            );

            return Ok(arDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving accounts receivable summary");
            return StatusCode(500, new { Message = "An error occurred processing your request" });
        }
    }

    /// <summary>
    /// Retrieves accounts payable summary and aging
    /// </summary>
    /// <remarks>
    /// <para>
    /// Provides a comprehensive summary of outstanding accounts payable with aging analysis.
    /// Essential for supplier relationship management and payment planning.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// GET /api/v1/finance/accounts-payable
    /// Authorization: Bearer {token}
    /// </code>
    /// <para>
    /// <strong>Payment Priority:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Monitor aging to maintain good supplier relationships</description></item>
    /// <item><description>Plan payments based on due dates and cash availability</description></item>
    /// <item><description>Identify opportunities for early payment discounts</description></item>
    /// <item><description>Avoid late fees and credit issues</description></item>
    /// </list>
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Accounts payable summary with aging analysis</returns>
    /// <response code="200">Successfully retrieved AP summary</response>
    /// <response code="401">Unauthorized - authentication required</response>
    [HttpGet("accounts-payable")]
    [ProducesResponseType(typeof(AccountsPayableDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AccountsPayableDto>> GetAccountsPayable(
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Fetching accounts payable summary");

        try
        {
            var summary = await _financialService.GetAccountsPayableSummaryAsync(cancellationToken);

            var apDto = new AccountsPayableDto
            {
                TotalPayable = summary["TotalPayable"],
                Current_0_30 = summary["Current_0_30"],
                Aging_31_60 = summary["Aging_31_60"],
                Aging_61_90 = summary["Aging_61_90"],
                Aging_Over90 = summary["Aging_Over90"],
                Count = (int)summary["Count"],
            };

            _logger.LogInformation(
                "AP summary retrieved: Total={Total}, Count={Count}",
                apDto.TotalPayable,
                apDto.Count
            );

            return Ok(apDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving accounts payable summary");
            return StatusCode(500, new { Message = "An error occurred processing your request" });
        }
    }

    /// <summary>
    /// Retrieves financial dashboard with key metrics
    /// </summary>
    /// <remarks>
    /// <para>
    /// Provides a comprehensive financial dashboard with key performance indicators,
    /// cash flow summary, AR/AP status, and reconciliation tracking. Perfect for executive
    /// overview and financial monitoring.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// GET /api/v1/finance/dashboard?startDate=2025-11-01&amp;endDate=2025-11-30
    /// Authorization: Bearer {token}
    /// </code>
    /// <para>
    /// <strong>Dashboard Includes:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Cash flow report for the specified period</description></item>
    /// <item><description>Current accounts receivable aging</description></item>
    /// <item><description>Current accounts payable aging</description></item>
    /// <item><description>Unreconciled transaction count</description></item>
    /// <item><description>Total transaction volume</description></item>
    /// <item><description>Last reconciliation timestamp</description></item>
    /// </list>
    /// </remarks>
    /// <param name="startDate">Start date for cash flow period</param>
    /// <param name="endDate">End date for cash flow period</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Financial dashboard with consolidated metrics</returns>
    /// <response code="200">Successfully retrieved dashboard data</response>
    /// <response code="400">Bad request - invalid date range</response>
    /// <response code="401">Unauthorized - authentication required</response>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(FinancialDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<FinancialDashboardDto>> GetDashboard(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Generating financial dashboard");

        if (startDate > endDate)
            return BadRequest(new { Message = "Start date must be before or equal to end date" });

        try
        {
            // Get cash flow summary
            var cashFlowSummary = await _financialService.GetCashFlowSummaryAsync(
                startDate,
                endDate,
                cancellationToken
            );

            // Get AR summary
            var arSummary = await _financialService.GetAccountsReceivableSummaryAsync(
                cancellationToken
            );

            // Get AP summary
            var apSummary = await _financialService.GetAccountsPayableSummaryAsync(
                cancellationToken
            );

            // Get unreconciled transactions
            var unreconciled = await _financialService.GetUnreconciledTransactionsAsync(
                cancellationToken
            );

            // Get total transactions for period
            var allTransactions = await _financialService.GetTransactionsByPeriodAsync(
                startDate,
                endDate,
                cancellationToken
            );

            // Find last reconciliation date
            var lastReconciliation = (
                await _financialTransactionRepository.GetAllAsync(cancellationToken)
            )
                .Where(t => t.IsReconciled && t.ReconciledAt.HasValue)
                .OrderByDescending(t => t.ReconciledAt)
                .FirstOrDefault();

            var dashboard = new FinancialDashboardDto
            {
                CashFlow = new CashFlowReportDto
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalInflows = cashFlowSummary["TotalInflows"],
                    TotalOutflows = cashFlowSummary["TotalOutflows"],
                    NetCashFlow = cashFlowSummary["NetCashFlow"],
                    CustomerPayments = cashFlowSummary["CustomerPayments"],
                    SalesRevenue = cashFlowSummary["SalesRevenue"],
                    SupplierPayments = cashFlowSummary["SupplierPayments"],
                    PurchaseExpenses = cashFlowSummary["PurchaseExpenses"],
                    Refunds = cashFlowSummary["Refunds"],
                    OperatingExpenses = cashFlowSummary["OperatingExpenses"],
                    PaymentFees = cashFlowSummary["PaymentFees"],
                    ShippingCosts = cashFlowSummary["ShippingCosts"],
                    Taxes = cashFlowSummary["Taxes"],
                },
                AccountsReceivable = new AccountsReceivableDto
                {
                    TotalReceivable = arSummary["TotalReceivable"],
                    Current_0_30 = arSummary["Current_0_30"],
                    Aging_31_60 = arSummary["Aging_31_60"],
                    Aging_61_90 = arSummary["Aging_61_90"],
                    Aging_Over90 = arSummary["Aging_Over90"],
                    Count = (int)arSummary["Count"],
                },
                AccountsPayable = new AccountsPayableDto
                {
                    TotalPayable = apSummary["TotalPayable"],
                    Current_0_30 = apSummary["Current_0_30"],
                    Aging_31_60 = apSummary["Aging_31_60"],
                    Aging_61_90 = apSummary["Aging_61_90"],
                    Aging_Over90 = apSummary["Aging_Over90"],
                    Count = (int)apSummary["Count"],
                },
                UnreconciledTransactions = unreconciled.Count(),
                TotalTransactions = allTransactions.Count(),
                LastReconciliationDate = lastReconciliation?.ReconciledAt,
            };

            _logger.LogInformation("Financial dashboard generated successfully");

            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating financial dashboard");
            return StatusCode(500, new { Message = "An error occurred processing your request" });
        }
    }

    /// <summary>
    /// Reconciles a financial transaction with bank statement
    /// </summary>
    /// <remarks>
    /// <para>
    /// Marks a financial transaction as reconciled after verifying it matches bank records.
    /// Reconciliation is a critical control for ensuring financial accuracy and detecting fraud.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// POST /api/v1/finance/transactions/3fa85f64-5717-4562-b3fc-2c963f66afa6/reconcile
    /// Authorization: Bearer {token}
    /// Content-Type: application/json
    ///
    /// {
    ///   "notes": "Reconciled with Chase bank statement #12345 dated 2025-11-08"
    /// }
    /// </code>
    /// <para>
    /// <strong>Reconciliation Process:</strong>
    /// </para>
    /// <list type="number">
    /// <item><description>Verify transaction appears on bank statement with matching amount and date</description></item>
    /// <item><description>Check for any discrepancies in amount, date, or description</description></item>
    /// <item><description>Resolve any differences before marking as reconciled</description></item>
    /// <item><description>Record bank statement reference and reconciliation notes</description></item>
    /// <item><description>Mark transaction as reconciled with current timestamp</description></item>
    /// </list>
    /// </remarks>
    /// <param name="id">Unique identifier of the transaction to reconcile</param>
    /// <param name="request">Reconciliation request with optional notes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated financial transaction with reconciliation details</returns>
    /// <response code="200">Successfully reconciled the transaction</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Transaction not found</response>
    [HttpPost("transactions/{id:guid}/reconcile")]
    [ProducesResponseType(typeof(FinancialTransactionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FinancialTransactionResponseDto>> ReconcileTransaction(
        Guid id,
        [FromBody] ReconcileTransactionRequestDto request,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Reconciling transaction: {TransactionId}", id);

        try
        {
            var userId = GetCurrentUserId();

            var transaction = await _financialService.ReconcileTransactionAsync(
                id,
                userId,
                request.Notes,
                cancellationToken
            );

            _logger.LogInformation("Transaction {TransactionId} reconciled successfully", id);

            return Ok(MapToDto(transaction));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Transaction not found: {TransactionId}", id);
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reconciling transaction: {TransactionId}", id);
            return StatusCode(500, new { Message = "An error occurred processing your request" });
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Maps FinancialTransactionEntity to FinancialTransactionResponseDto
    /// </summary>
    private static FinancialTransactionResponseDto MapToDto(
        Domain.Entities.FinancialTransactionEntity entity
    )
    {
        return new FinancialTransactionResponseDto
        {
            Id = entity.Id,
            TransactionNumber = entity.TransactionNumber,
            TransactionType = entity.TransactionType.ToString(),
            Amount = entity.Amount,
            Currency = entity.Currency,
            TransactionDate = entity.TransactionDate,
            Description = entity.Description,
            OrderId = entity.OrderId,
            PaymentId = entity.PaymentId,
            InventoryTransactionId = entity.InventoryTransactionId,
            JournalEntryId = entity.JournalEntryId,
            ProductId = entity.ProductId,
            Counterparty = entity.Counterparty,
            ReferenceNumber = entity.ReferenceNumber,
            IsReconciled = entity.IsReconciled,
            ReconciledAt = entity.ReconciledAt,
            PaymentMethod = entity.PaymentMethod?.ToString(),
            PaymentProvider = entity.PaymentProvider,
            Status = entity.Status,
            Notes = entity.Notes,
            TaxAmount = entity.TaxAmount,
            FeeAmount = entity.FeeAmount,
            NetAmount = entity.NetAmount,
            CreatedBy = entity.CreatedBy,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
        };
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Gets the current authenticated user's ID from JWT claims
    /// </summary>
    /// <returns>User ID from claims, or Guid.Empty if not found</returns>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    #endregion
}
