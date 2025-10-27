using System.Security.Claims;
using ECommerce.API.DTOs;
using ECommerce.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

/// <summary>
/// Controller for managing inventory transactions
/// </summary>
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
    /// <param name="request">Transaction details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created inventory transaction</returns>
    [HttpPost]
    [ProducesResponseType(typeof(InventoryTransactionResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// Gets an inventory transaction by identifier
    /// </summary>
    /// <param name="id">Transaction identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Inventory transaction details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(InventoryTransactionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
    /// Gets all inventory transactions for a specific product
    /// </summary>
    /// <param name="productId">Product identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of inventory transactions</returns>
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
    /// Gets inventory transactions within a date range
    /// </summary>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of inventory transactions</returns>
    [HttpGet("period")]
    [ProducesResponseType(
        typeof(IEnumerable<InventoryTransactionResponseDto>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
