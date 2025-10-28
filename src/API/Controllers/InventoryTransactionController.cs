using System.Security.Claims;
using ECommerce.API.DTOs;
using ECommerce.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

/// <summary>
/// Inventory transaction management endpoints
/// </summary>
/// <remarks>
/// Provides inventory tracking with automatic accounting integration. All inventory movements
/// (receiving, shipping, adjustments, transfers) are automatically posted to the accounting system.
/// All endpoints require authentication.
/// </remarks>
[Tags("Inventory")]
[ApiController]
[Route("api/v1/inventory-transactions")]
[Produces("application/json")]
[Authorize]
public sealed class InventoryTransactionController : ControllerBase
{
    private readonly IInventoryTransactionService _transactionService;
    private readonly ILogger<InventoryTransactionController> _logger;

    public InventoryTransactionController(
        IInventoryTransactionService transactionService,
        ILogger<InventoryTransactionController> logger
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
    /// Creates an inventory transaction and automatically generates corresponding journal entries.
    /// The system posts debits and credits based on transaction type (Receiving, Shipping, Adjustment, Transfer).
    ///
    /// Sample request:
    ///
    ///     POST /api/v1/inventory-transactions
    ///     {
    ///        "transactionType": "Receiving",
    ///        "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///        "productSku": "PROD-001",
    ///        "productName": "Gaming Laptop",
    ///        "quantity": 10,
    ///        "unitCost": 1000.00,
    ///        "toLocation": "Warehouse A",
    ///        "documentNumber": "PO-2024-001",
    ///        "notes": "Received from supplier XYZ"
    ///     }
    ///
    /// **Required permissions:** Authenticated user
    ///
    /// **Transaction types:** Receiving, Shipping, Adjustment, Transfer
    ///
    /// **Accounting integration:** Automatically creates journal entries with proper debits/credits
    ///
    /// </remarks>
    /// <param name="request">Transaction details including type, product, quantity, and cost</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Created inventory transaction with generated transaction number and journal entry ID</returns>
    /// <response code="201">Successfully created the transaction. Location header contains the URI of the new resource.</response>
    /// <response code="400">Invalid request. Validation errors in transaction data.</response>
    /// <response code="401">Unauthorized. Authentication required.</response>
    /// <response code="500">Internal server error. Failed to create transaction or accounting entries.</response>
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
    /// Returns detailed information about a single inventory transaction including accounting integration details.
    ///
    /// Sample request:
    ///
    ///     GET /api/v1/inventory-transactions/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///
    /// **Required permissions:** Authenticated user
    ///
    /// **Note:** This endpoint is currently a placeholder and requires service implementation.
    ///
    /// </remarks>
    /// <param name="id">Transaction unique identifier (GUID)</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Inventory transaction details with accounting references</returns>
    /// <response code="200">Successfully retrieved the transaction.</response>
    /// <response code="401">Unauthorized. Authentication required.</response>
    /// <response code="404">Transaction not found with the specified ID.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(InventoryTransactionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InventoryTransactionResponseDto>> GetTransactionById(
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

        return NotFound(new { Message = "Transaction retrieval not yet implemented" });
    }

    /// <summary>
    /// Retrieves all inventory transactions for a specific product
    /// </summary>
    /// <remarks>
    /// Returns the complete transaction history for a product including all movements and adjustments.
    /// Useful for inventory audit trails and stock movement analysis.
    ///
    /// Sample request:
    ///
    ///     GET /api/v1/inventory-transactions/product/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///
    /// **Required permissions:** Authenticated user
    ///
    /// Results include all transaction types: Receiving, Shipping, Adjustments, and Transfers.
    ///
    /// </remarks>
    /// <param name="productId">Product unique identifier (GUID)</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>List of all inventory transactions for the specified product</returns>
    /// <response code="200">Successfully retrieved product transactions. May be empty if no transactions found.</response>
    /// <response code="401">Unauthorized. Authentication required.</response>
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
    /// Returns all inventory transactions that occurred between the start and end dates (inclusive).
    /// Useful for period-end inventory reconciliation and reporting.
    ///
    /// Sample request:
    ///
    ///     GET /api/v1/inventory-transactions/period?startDate=2024-01-01&amp;endDate=2024-01-31
    ///
    /// **Required permissions:** Authenticated user
    ///
    /// **Note:** Both dates are inclusive. Time component is ignored (full day ranges).
    ///
    /// </remarks>
    /// <param name="startDate">Start date of the period (inclusive)</param>
    /// <param name="endDate">End date of the period (inclusive)</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>List of inventory transactions within the specified period</returns>
    /// <response code="200">Successfully retrieved transactions for the period. May be empty if no transactions found.</response>
    /// <response code="400">Invalid date range. Start date must be before or equal to end date.</response>
    /// <response code="401">Unauthorized. Authentication required.</response>
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
    /// <returns>User ID or Guid.Empty if not found</returns>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
