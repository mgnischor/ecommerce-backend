using System.Security.Claims;
using ECommerce.API.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

/// <summary>
/// Inventory transaction management endpoints
/// </summary>
/// <remarks>
/// <para>
/// Provides comprehensive inventory tracking with automatic accounting integration.
/// All inventory movements (receiving, shipping, adjustments, transfers) are automatically
/// posted to the accounting system using double-entry bookkeeping principles.
/// </para>
/// <para>
/// <strong>Transaction Types Supported:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><strong>Receiving:</strong> Stock received from suppliers or production (increases inventory)</description></item>
/// <item><description><strong>Shipping:</strong> Stock shipped to customers or transferred out (decreases inventory)</description></item>
/// <item><description><strong>Adjustment:</strong> Inventory corrections for discrepancies, damage, or theft</description></item>
/// <item><description><strong>Transfer:</strong> Stock movement between warehouse locations (no quantity change)</description></item>
/// </list>
/// <para>
/// <strong>Automatic Accounting Integration:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><strong>Receiving:</strong> Debit Inventory, Credit Accounts Payable or Cash</description></item>
/// <item><description><strong>Shipping:</strong> Debit Cost of Goods Sold, Credit Inventory</description></item>
/// <item><description><strong>Adjustment (Loss):</strong> Debit Inventory Shrinkage Expense, Credit Inventory</description></item>
/// <item><description><strong>Adjustment (Gain):</strong> Debit Inventory, Credit Inventory Gain</description></item>
/// <item><description><strong>Transfer:</strong> Location tracking only, no accounting impact</description></item>
/// </list>
/// <para>
/// <strong>Key Features:</strong>
/// </para>
/// <list type="bullet">
/// <item><description>Real-time inventory tracking across multiple locations</description></item>
/// <item><description>Automatic journal entry generation for financial records</description></item>
/// <item><description>Complete audit trail with transaction numbers and timestamps</description></item>
/// <item><description>Unit cost tracking for accurate cost of goods sold calculations</description></item>
/// <item><description>Integration with orders and purchase orders via document numbers</description></item>
/// <item><description>Historical transaction reporting by product or date range</description></item>
/// </list>
/// <para>
/// <strong>Security:</strong> All endpoints require authentication. Users must be logged in to record
/// or view inventory transactions. Each transaction is associated with the authenticated user for audit purposes.
/// </para>
/// </remarks>
[Tags("Inventory")]
[ApiController]
[Route("api/v1/inventory-transactions")]
[Produces("application/json")]
[Authorize]
public sealed class InventoryTransactionController : ControllerBase
{
    /// <summary>
    /// Service for inventory transaction business logic and accounting integration
    /// </summary>
    /// <remarks>
    /// Handles the creation of inventory transactions, automatic journal entry generation,
    /// inventory quantity updates, and transaction history retrieval.
    /// </remarks>
    private readonly IInventoryTransactionService _transactionService;

    /// <summary>
    /// Logger instance for tracking inventory transaction operations and errors
    /// </summary>
    /// <remarks>
    /// Used to log transaction recordings, retrievals, validation errors, and exceptions
    /// for monitoring, debugging, and audit trail purposes.
    /// </remarks>
    private readonly ILoggingService _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="InventoryTransactionController"/> class
    /// </summary>
    /// <param name="transactionService">
    /// Service for inventory transaction operations and accounting integration.
    /// Handles transaction recording, journal entry creation, and inventory updates.
    /// Cannot be null.
    /// </param>
    /// <param name="logger">
    /// Logger instance for recording inventory transaction events and errors.
    /// Used for operational monitoring and audit trails.
    /// Cannot be null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when either transactionService or logger parameter is null.
    /// </exception>
    /// <remarks>
    /// This constructor uses dependency injection to provide all required services.
    /// All parameters are validated for null values to ensure the controller operates correctly.
    /// The controller is instantiated by the ASP.NET Core dependency injection container
    /// when handling inventory transaction requests.
    /// </remarks>
    public InventoryTransactionController(
        IInventoryTransactionService transactionService,
        LoggingService<InventoryTransactionController> logger
    )
    {
        _transactionService =
            transactionService ?? throw new ArgumentNullException(nameof(transactionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Records a new inventory transaction with automatic accounting integration
    /// </summary>
    /// <remarks>
    /// <para>
    /// Creates an inventory transaction and automatically generates corresponding journal entries.
    /// The system posts debits and credits based on transaction type, maintaining accurate inventory
    /// quantities and financial records simultaneously.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// POST /api/v1/inventory-transactions
    /// Authorization: Bearer {token}
    /// Content-Type: application/json
    ///
    /// {
    ///    "transactionType": "Receiving",
    ///    "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///    "productSku": "PROD-001",
    ///    "productName": "Gaming Laptop",
    ///    "quantity": 10,
    ///    "unitCost": 1000.00,
    ///    "toLocation": "Warehouse A",
    ///    "documentNumber": "PO-2024-001",
    ///    "notes": "Received from supplier XYZ"
    /// }
    /// </code>
    /// <para>
    /// <strong>Transaction Processing Flow:</strong>
    /// </para>
    /// <list type="number">
    /// <item><description><strong>Validation:</strong> Verifies all required fields and data formats</description></item>
    /// <item><description><strong>User Authentication:</strong> Extracts user ID from JWT claims for audit trail</description></item>
    /// <item><description><strong>Transaction Creation:</strong> Generates unique transaction number and records details</description></item>
    /// <item><description><strong>Inventory Update:</strong> Updates product quantity in inventory table</description></item>
    /// <item><description><strong>Accounting Integration:</strong> Creates journal entry with appropriate debits and credits</description></item>
    /// <item><description><strong>Response:</strong> Returns transaction details with journal entry reference</description></item>
    /// </list>
    /// <para>
    /// <strong>Transaction Types and Accounting Impact:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Receiving:</strong> Increases inventory quantity. Debits Inventory account, Credits Accounts Payable or Cash</description></item>
    /// <item><description><strong>Shipping:</strong> Decreases inventory quantity. Debits Cost of Goods Sold, Credits Inventory</description></item>
    /// <item><description><strong>Adjustment (Positive):</strong> Increases inventory. Debits Inventory, Credits Inventory Gain</description></item>
    /// <item><description><strong>Adjustment (Negative):</strong> Decreases inventory. Debits Shrinkage/Loss Expense, Credits Inventory</description></item>
    /// <item><description><strong>Transfer:</strong> No quantity change. Updates location only, no accounting entry</description></item>
    /// </list>
    /// <para>
    /// <strong>Required Fields:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>transactionType:</strong> Must be one of: Receiving, Shipping, Adjustment, Transfer</description></item>
    /// <item><description><strong>productId:</strong> Valid GUID of existing product</description></item>
    /// <item><description><strong>productSku:</strong> Product stock keeping unit code</description></item>
    /// <item><description><strong>productName:</strong> Product name for reference</description></item>
    /// <item><description><strong>quantity:</strong> Number of units (must be positive)</description></item>
    /// <item><description><strong>unitCost:</strong> Cost per unit (must be positive)</description></item>
    /// <item><description><strong>toLocation:</strong> Destination location (required for Receiving and Transfer)</description></item>
    /// </list>
    /// <para>
    /// <strong>Optional Fields:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>fromLocation:</strong> Source location (required for Shipping and Transfer)</description></item>
    /// <item><description><strong>orderId:</strong> Associated order ID for tracking</description></item>
    /// <item><description><strong>documentNumber:</strong> Purchase order, invoice, or reference number</description></item>
    /// <item><description><strong>notes:</strong> Additional comments or explanation</description></item>
    /// </list>
    /// <para>
    /// <strong>Required permissions:</strong> Authenticated user (any access level)
    /// </para>
    /// <para>
    /// <strong>Business Rules:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Quantity must be positive (greater than zero)</description></item>
    /// <item><description>Unit cost must be positive for costing accuracy</description></item>
    /// <item><description>Product must exist in the system</description></item>
    /// <item><description>Shipping transactions cannot reduce inventory below zero</description></item>
    /// <item><description>Transfer requires both from and to locations</description></item>
    /// <item><description>Each transaction generates a unique transaction number for audit trails</description></item>
    /// </list>
    /// </remarks>
    /// <param name="request">
    /// Transaction details including type, product information, quantity, cost, and location data.
    /// Must be a valid <see cref="RecordInventoryTransactionRequestDto"/> object with all required fields populated.
    /// The request body must be valid JSON matching the DTO schema.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to cancel the asynchronous operation.
    /// Allows the client to cancel the transaction recording if it's no longer needed,
    /// though partial operations may have been committed.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing an <see cref="InventoryTransactionResponseDto"/> object.
    /// The response includes the generated transaction number, unique transaction ID, journal entry ID,
    /// timestamps, and all transaction details. The Location header contains the URI to retrieve this transaction.
    /// </returns>
    /// <response code="201">
    /// Created. Successfully recorded the inventory transaction and generated accounting entries.
    /// The Location header contains the URI of the newly created transaction resource.
    /// Returns complete transaction details including the generated transaction number and journal entry ID.
    /// </response>
    /// <response code="400">
    /// Bad request. Invalid transaction data or validation errors.
    /// Common causes include: missing required fields, invalid product ID, negative quantity or cost,
    /// invalid transaction type, missing location data, or validation rule violations.
    /// </response>
    /// <response code="401">
    /// Unauthorized. Authentication required or invalid authentication token.
    /// User must be logged in to record inventory transactions. Each transaction is associated
    /// with the authenticated user for audit trail purposes.
    /// </response>
    /// <response code="500">
    /// Internal server error. Failed to create transaction or accounting entries.
    /// This may occur due to database errors, accounting integration failures, or unexpected system errors.
    /// The transaction may have been partially completed. Check server logs for detailed error information.
    /// </response>
    [HttpPost]
    [ProducesResponseType(typeof(InventoryTransactionResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<InventoryTransactionResponseDto>> RecordTransaction(
        [FromBody] RecordInventoryTransactionRequestDto request,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation(
            "Recording inventory transaction: Type={TransactionType}, ProductId={ProductId}, Quantity={Quantity}",
            request.TransactionType,
            request.ProductId,
            request.Quantity
        );

        if (!ModelState.IsValid)
        {
            _logger.LogWarning(
                "Invalid model state for inventory transaction: {ValidationErrors}",
                string.Join(
                    ", ",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                )
            );
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            _logger.LogWarning("Unable to retrieve user ID from claims");
            return Unauthorized(new { Message = "User ID not found in authentication token" });
        }

        try
        {
            var transaction = await _transactionService.RecordTransactionAsync(
                request.TransactionType,
                request.ProductId,
                request.ProductSku,
                request.ProductName,
                request.Quantity,
                request.UnitCost,
                request.ToLocation,
                userId,
                request.FromLocation,
                request.OrderId,
                request.DocumentNumber,
                request.Notes,
                cancellationToken
            );

            _logger.LogInformation(
                "Inventory transaction recorded successfully: TransactionId={TransactionId}, TransactionNumber={TransactionNumber}",
                transaction.Id,
                transaction.TransactionNumber
            );

            var response = new InventoryTransactionResponseDto
            {
                Id = transaction.Id,
                TransactionNumber = transaction.TransactionNumber,
                TransactionDate = transaction.TransactionDate,
                TransactionType = transaction.TransactionType.ToString(),
                ProductId = transaction.ProductId,
                ProductSku = transaction.ProductSku,
                ProductName = transaction.ProductName,
                FromLocation = transaction.FromLocation,
                ToLocation = transaction.ToLocation,
                Quantity = transaction.Quantity,
                UnitCost = transaction.UnitCost,
                TotalCost = transaction.TotalCost,
                JournalEntryId = transaction.JournalEntryId,
                OrderId = transaction.OrderId,
                DocumentNumber = transaction.DocumentNumber,
                Notes = transaction.Notes,
                CreatedBy = transaction.CreatedBy,
                CreatedAt = transaction.CreatedAt,
            };

            return CreatedAtAction(
                nameof(GetTransactionById),
                new { id = transaction.Id },
                response
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error recording inventory transaction: Type={TransactionType}, ProductId={ProductId}",
                request.TransactionType,
                request.ProductId
            );
            throw;
        }
    }

    /// <summary>
    /// Retrieves a specific inventory transaction by its unique identifier
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returns detailed information about a single inventory transaction including accounting integration details.
    /// Use this endpoint to view complete transaction information including associated journal entries and audit data.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// GET /api/v1/inventory-transactions/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// Authorization: Bearer {token}
    /// </code>
    /// <para>
    /// <strong>Required permissions:</strong> Authenticated user (any access level)
    /// </para>
    /// <para>
    /// <strong>Response includes:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Transaction Header:</strong> ID, transaction number, date, type</description></item>
    /// <item><description><strong>Product Information:</strong> Product ID, SKU, and name</description></item>
    /// <item><description><strong>Quantity Data:</strong> Number of units and unit cost</description></item>
    /// <item><description><strong>Location Information:</strong> From and to locations</description></item>
    /// <item><description><strong>Financial Data:</strong> Unit cost, total cost, journal entry ID</description></item>
    /// <item><description><strong>References:</strong> Order ID, document number</description></item>
    /// <item><description><strong>Audit Information:</strong> Creator ID, creation timestamp, notes</description></item>
    /// </list>
    /// <para>
    /// <strong>Use cases:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Viewing detailed transaction information for audit purposes</description></item>
    /// <item><description>Verifying transaction posting and accounting integration</description></item>
    /// <item><description>Investigating inventory discrepancies</description></item>
    /// <item><description>Tracing journal entries back to source transactions</description></item>
    /// </list>
    /// <para>
    /// <strong>Important Note:</strong> This endpoint is currently a placeholder and requires service implementation.
    /// Once implemented, it will retrieve transaction details from the database using the transaction ID.
    /// </para>
    /// </remarks>
    /// <param name="id">
    /// The unique identifier (GUID) of the inventory transaction to retrieve.
    /// Must be a valid GUID format (e.g., 3fa85f64-5717-4562-b3fc-2c963f66afa6).
    /// This ID is returned when creating a transaction via the POST endpoint.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to cancel the asynchronous operation.
    /// Allows the client to cancel the request if it's no longer needed.
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that represents the asynchronous operation.
    /// The task result contains an <see cref="ActionResult{T}"/> with an <see cref="InventoryTransactionResponseDto"/>
    /// object containing complete transaction details including accounting references.
    /// Currently returns 404 NotFound as the service implementation is pending.
    /// </returns>
    /// <response code="200">
    /// Successfully retrieved the inventory transaction.
    /// Returns a JSON object with complete transaction details including product information,
    /// quantities, costs, locations, and accounting integration data.
    /// </response>
    /// <response code="401">
    /// Unauthorized. Authentication required or invalid authentication token.
    /// The request must include a valid authorization token.
    /// </response>
    /// <response code="404">
    /// Transaction not found with the specified ID. The GUID may be invalid or the transaction
    /// may not exist in the system. This status is also returned when the service implementation is pending.
    /// </response>
    /// <response code="500">
    /// Internal server error. An unexpected error occurred while retrieving the transaction.
    /// Check server logs for detailed error information.
    /// </response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(InventoryTransactionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public Task<ActionResult<InventoryTransactionResponseDto>> GetTransactionById(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogDebug("Retrieving inventory transaction: TransactionId={TransactionId}", id);

        // This method requires implementation in the service interface
        // For now, return NotFound as placeholder
        _logger.LogWarning(
            "GetTransactionById not yet implemented in service: TransactionId={TransactionId}",
            id
        );

        return Task.FromResult<ActionResult<InventoryTransactionResponseDto>>(
            NotFound(new { Message = "Transaction retrieval not yet implemented" })
        );
    }

    /// <summary>
    /// Retrieves all inventory transactions for a specific product
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returns the complete transaction history for a product including all movements and adjustments.
    /// This endpoint provides a comprehensive audit trail showing how inventory quantities changed over time.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// GET /api/v1/inventory-transactions/product/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// Authorization: Bearer {token}
    /// </code>
    /// <para>
    /// <strong>Required permissions:</strong> Authenticated user (any access level)
    /// </para>
    /// <para>
    /// <strong>Transaction Types Included:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Receiving:</strong> Stock received from suppliers or production</description></item>
    /// <item><description><strong>Shipping:</strong> Stock shipped to customers or transferred out</description></item>
    /// <item><description><strong>Adjustments:</strong> Inventory corrections, write-offs, or found stock</description></item>
    /// <item><description><strong>Transfers:</strong> Movement between warehouse locations</description></item>
    /// </list>
    /// <para>
    /// <strong>Response includes for each transaction:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Transaction ID, number, date, and type</description></item>
    /// <item><description>Product identification (ID, SKU, name)</description></item>
    /// <item><description>Quantity and unit cost information</description></item>
    /// <item><description>Location data (from/to locations)</description></item>
    /// <item><description>Financial integration (journal entry ID, total cost)</description></item>
    /// <item><description>References (order ID, document number)</description></item>
    /// <item><description>Audit trail (creator, timestamp, notes)</description></item>
    /// </list>
    /// <para>
    /// <strong>Use cases:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Inventory Audits:</strong> Complete transaction history for reconciliation</description></item>
    /// <item><description><strong>Cost Analysis:</strong> Track unit costs and total costs over time</description></item>
    /// <item><description><strong>Movement Tracking:</strong> Trace product flow through locations</description></item>
    /// <item><description><strong>Discrepancy Investigation:</strong> Identify when and why quantities changed</description></item>
    /// <item><description><strong>Financial Reconciliation:</strong> Match inventory changes to accounting entries</description></item>
    /// <item><description><strong>Compliance:</strong> Provide audit trail for regulatory requirements</description></item>
    /// </list>
    /// <para>
    /// <strong>Results ordering:</strong> Transactions are typically returned in chronological order (oldest to newest)
    /// to provide a clear historical progression of inventory movements.
    /// </para>
    /// <para>
    /// <strong>Empty results:</strong> An empty array is returned if no transactions are found for the specified product.
    /// This is a normal response and does not indicate an error.
    /// </para>
    /// </remarks>
    /// <param name="productId">
    /// The unique identifier (GUID) of the product for which to retrieve transaction history.
    /// Must be a valid GUID format (e.g., 3fa85f64-5717-4562-b3fc-2c963f66afa6).
    /// The product should exist in the system, though the endpoint will return an empty array
    /// if no transactions exist for a valid product.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to cancel the asynchronous operation.
    /// Useful for preventing unnecessary processing if the client cancels the request.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing an enumerable collection of <see cref="InventoryTransactionResponseDto"/> objects.
    /// Each DTO represents a complete inventory transaction with all details.
    /// Returns an empty array if no transactions exist for the specified product.
    /// </returns>
    /// <response code="200">
    /// Successfully retrieved product transactions. Returns a JSON array of transaction objects.
    /// The array may be empty if no transactions are found for the specified product ID.
    /// Each transaction includes complete details including accounting integration information.
    /// </response>
    /// <response code="401">
    /// Unauthorized. Authentication required or invalid authentication token.
    /// The request must include a valid authorization token.
    /// </response>
    /// <response code="500">
    /// Internal server error. An unexpected error occurred while retrieving transactions.
    /// This may occur due to database errors or system failures.
    /// Check server logs for detailed error information.
    /// </response>
    [HttpGet("product/{productId:guid}")]
    [ProducesResponseType(
        typeof(IEnumerable<InventoryTransactionResponseDto>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<
        ActionResult<IEnumerable<InventoryTransactionResponseDto>>
    > GetProductTransactions(Guid productId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Retrieving inventory transactions for product: ProductId={ProductId}",
            productId
        );

        try
        {
            var transactions = await _transactionService.GetProductTransactionsAsync(
                productId,
                cancellationToken
            );

            var response = transactions.Select(t => new InventoryTransactionResponseDto
            {
                Id = t.Id,
                TransactionNumber = t.TransactionNumber,
                TransactionDate = t.TransactionDate,
                TransactionType = t.TransactionType.ToString(),
                ProductId = t.ProductId,
                ProductSku = t.ProductSku,
                ProductName = t.ProductName,
                FromLocation = t.FromLocation,
                ToLocation = t.ToLocation,
                Quantity = t.Quantity,
                UnitCost = t.UnitCost,
                TotalCost = t.TotalCost,
                JournalEntryId = t.JournalEntryId,
                OrderId = t.OrderId,
                DocumentNumber = t.DocumentNumber,
                Notes = t.Notes,
                CreatedBy = t.CreatedBy,
                CreatedAt = t.CreatedAt,
            });

            _logger.LogInformation(
                "Retrieved {Count} transactions for product: ProductId={ProductId}",
                response.Count(),
                productId
            );

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving transactions for product: ProductId={ProductId}",
                productId
            );
            throw;
        }
    }

    /// <summary>
    /// Retrieves inventory transactions within a specified date range
    /// </summary>
    /// <remarks>
    /// <para>
    /// Returns all inventory transactions that occurred between the start and end dates (inclusive).
    /// This endpoint is essential for period-end inventory reconciliation, financial reporting,
    /// and audit trail analysis for specific time periods.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// </para>
    /// <code>
    /// GET /api/v1/inventory-transactions/period?startDate=2024-01-01&amp;endDate=2024-01-31
    /// Authorization: Bearer {token}
    /// </code>
    /// <para>
    /// <strong>Sample request (with time):</strong>
    /// </para>
    /// <code>
    /// GET /api/v1/inventory-transactions/period?startDate=2024-01-01T00:00:00Z&amp;endDate=2024-01-31T23:59:59Z
    /// Authorization: Bearer {token}
    /// </code>
    /// <para>
    /// <strong>Required permissions:</strong> Authenticated user (any access level)
    /// </para>
    /// <para>
    /// <strong>Date Range Behavior:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Inclusive:</strong> Both start and end dates are included in the results</description></item>
    /// <item><description><strong>Full Days:</strong> Time component is typically ignored, treating dates as full day ranges</description></item>
    /// <item><description><strong>Validation:</strong> Start date must be before or equal to end date</description></item>
    /// <item><description><strong>Time Zones:</strong> Dates should be provided in UTC or with appropriate timezone offset</description></item>
    /// </list>
    /// <para>
    /// <strong>Use cases:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Period-End Closing:</strong> Reconcile inventory movements for month-end or year-end</description></item>
    /// <item><description><strong>Financial Reporting:</strong> Generate cost of goods sold reports for accounting periods</description></item>
    /// <item><description><strong>Audit Requirements:</strong> Provide transaction history for specific audit periods</description></item>
    /// <item><description><strong>Performance Analysis:</strong> Analyze inventory turnover and movement patterns</description></item>
    /// <item><description><strong>Reconciliation:</strong> Match physical inventory counts with system transactions</description></item>
    /// <item><description><strong>Compliance Reporting:</strong> Generate regulatory reports for specific periods</description></item>
    /// </list>
    /// <para>
    /// <strong>Response includes for each transaction:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Complete transaction details (ID, number, date, type)</description></item>
    /// <item><description>Product information (ID, SKU, name)</description></item>
    /// <item><description>Quantity and cost data</description></item>
    /// <item><description>Location information</description></item>
    /// <item><description>Accounting integration (journal entry ID)</description></item>
    /// <item><description>References and audit trail</description></item>
    /// </list>
    /// <para>
    /// <strong>Results ordering:</strong> Transactions are typically returned in chronological order
    /// within the specified date range for easier period analysis.
    /// </para>
    /// <para>
    /// <strong>Empty results:</strong> An empty array is returned if no transactions occurred during
    /// the specified period. This is a normal response and does not indicate an error.
    /// </para>
    /// <para>
    /// <strong>Performance consideration:</strong> Large date ranges may return many transactions.
    /// Consider using smaller date ranges for better performance in high-volume systems.
    /// </para>
    /// </remarks>
    /// <param name="startDate">
    /// The start date of the period to query (inclusive).
    /// Transactions with dates on or after this date will be included.
    /// Should be provided in ISO 8601 format (e.g., 2024-01-01 or 2024-01-01T00:00:00Z).
    /// Must be before or equal to the endDate parameter.
    /// </param>
    /// <param name="endDate">
    /// The end date of the period to query (inclusive).
    /// Transactions with dates on or before this date will be included.
    /// Should be provided in ISO 8601 format (e.g., 2024-01-31 or 2024-01-31T23:59:59Z).
    /// Must be after or equal to the startDate parameter.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token to cancel the asynchronous operation.
    /// Useful for preventing unnecessary processing if the client cancels the request,
    /// especially important for queries covering large date ranges.
    /// </param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing an enumerable collection of <see cref="InventoryTransactionResponseDto"/> objects.
    /// Each DTO represents a complete inventory transaction that occurred within the specified date range.
    /// Returns an empty array if no transactions occurred during the period.
    /// </returns>
    /// <response code="200">
    /// Successfully retrieved transactions for the specified period. Returns a JSON array of transaction objects.
    /// The array may be empty if no transactions occurred during the date range.
    /// Each transaction includes complete details including accounting integration information.
    /// </response>
    /// <response code="400">
    /// Bad request. Invalid date range parameters provided.
    /// Common causes: start date is after end date, invalid date format, or missing date parameters.
    /// The error message will indicate the specific validation issue.
    /// </response>
    /// <response code="401">
    /// Unauthorized. Authentication required or invalid authentication token.
    /// The request must include a valid authorization token.
    /// </response>
    /// <response code="500">
    /// Internal server error. An unexpected error occurred while retrieving transactions.
    /// This may occur due to database errors, date parsing issues, or system failures.
    /// Check server logs for detailed error information.
    /// </response>
    [HttpGet("period")]
    [ProducesResponseType(
        typeof(IEnumerable<InventoryTransactionResponseDto>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<
        ActionResult<IEnumerable<InventoryTransactionResponseDto>>
    > GetTransactionsByPeriod(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation(
            "Retrieving inventory transactions for period: StartDate={StartDate}, EndDate={EndDate}",
            startDate,
            endDate
        );

        if (startDate > endDate)
        {
            _logger.LogWarning(
                "Invalid date range: StartDate={StartDate} is after EndDate={EndDate}",
                startDate,
                endDate
            );
            return BadRequest(new { Message = "Start date must be before or equal to end date" });
        }

        try
        {
            var transactions = await _transactionService.GetTransactionsByPeriodAsync(
                startDate,
                endDate,
                cancellationToken
            );

            var response = transactions.Select(t => new InventoryTransactionResponseDto
            {
                Id = t.Id,
                TransactionNumber = t.TransactionNumber,
                TransactionDate = t.TransactionDate,
                TransactionType = t.TransactionType.ToString(),
                ProductId = t.ProductId,
                ProductSku = t.ProductSku,
                ProductName = t.ProductName,
                FromLocation = t.FromLocation,
                ToLocation = t.ToLocation,
                Quantity = t.Quantity,
                UnitCost = t.UnitCost,
                TotalCost = t.TotalCost,
                JournalEntryId = t.JournalEntryId,
                OrderId = t.OrderId,
                DocumentNumber = t.DocumentNumber,
                Notes = t.Notes,
                CreatedBy = t.CreatedBy,
                CreatedAt = t.CreatedAt,
            });

            _logger.LogInformation(
                "Retrieved {Count} transactions for period: StartDate={StartDate}, EndDate={EndDate}",
                response.Count(),
                startDate,
                endDate
            );

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving transactions for period: StartDate={StartDate}, EndDate={EndDate}",
                startDate,
                endDate
            );
            throw;
        }
    }

    /// <summary>
    /// Gets the current authenticated user's ID from JWT claims
    /// </summary>
    /// <remarks>
    /// <para>
    /// Extracts the user identifier from the JWT token claims in the current HTTP context.
    /// This ID is used to associate inventory transactions with the user who created them,
    /// providing an essential audit trail for inventory operations.
    /// </para>
    /// <para>
    /// The method looks for the <see cref="ClaimTypes.NameIdentifier"/> claim in the JWT token,
    /// which contains the user's unique identifier (GUID) set during authentication.
    /// </para>
    /// <para>
    /// <strong>Return values:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Valid GUID:</strong> When the claim exists and contains a valid GUID</description></item>
    /// <item><description><strong>Guid.Empty:</strong> When the claim is missing, empty, or contains an invalid GUID format</description></item>
    /// </list>
    /// <para>
    /// <strong>Usage:</strong> This method is called internally before recording inventory transactions
    /// to ensure proper user tracking and audit trail maintenance.
    /// </para>
    /// </remarks>
    /// <returns>
    /// A <see cref="Guid"/> representing the current authenticated user's unique identifier.
    /// Returns <see cref="Guid.Empty"/> if the user ID claim is not found in the token,
    /// contains an invalid value, or if the user is not authenticated.
    /// </returns>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
