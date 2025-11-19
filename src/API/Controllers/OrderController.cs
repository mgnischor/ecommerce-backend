using System.Security.Claims;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Domain.Aggregates;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Policies;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Controllers;

/// <summary>
/// Order management and processing endpoints
/// </summary>
/// <remarks>
/// <para>
/// Provides comprehensive order management including creation, status tracking, modifications,
/// and cancellations. This controller implements business rules for order processing and
/// integrates with inventory and financial systems.
/// </para>
/// </remarks>
[Tags("Orders")]
[ApiController]
[Route("api/v1/orders")]
[Produces("application/json")]
[Authorize]
public sealed class OrderController : ControllerBase
{
    private readonly PostgresqlContext _context;
    private readonly IInventoryTransactionService _inventoryService;
    private readonly ILoggingService _logger;

    public OrderController(
        PostgresqlContext context,
        IInventoryTransactionService inventoryService,
        LoggingService<OrderController> logger
    )
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _inventoryService =
            inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new order with business rule validation
    /// </summary>
    /// <param name="order">Order entity with items</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created order with ID and status</returns>
    /// <response code="201">Order created successfully</response>
    /// <response code="400">Invalid order data or business rule violation</response>
    /// <response code="401">Authentication required</response>
    [HttpPost]
    [ProducesResponseType(typeof(OrderEntity), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OrderEntity>> CreateOrder(
        [FromBody] OrderEntity order,
        CancellationToken cancellationToken = default
    )
    {
        if (order == null)
            return BadRequest("Order data is required");

        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized(new { Message = "User ID not found in authentication token" });

        // Note: OrderEntity doesn't have Items navigation property
        // Items should be loaded separately as OrderItemEntity collection
        var itemCount = 0; // TODO: Load items from OrderItems table

        // Apply business rules validation
        var (isValid, errorMessage) = OrderProcessingPolicy.CanCreateOrder(
            order.TotalAmount,
            itemCount,
            order.CustomerId
        );

        if (!isValid)
        {
            _logger.LogWarning(
                "Order creation validation failed: {Error}, CustomerId={CustomerId}, Amount={Amount}",
                errorMessage ?? "Unknown error",
                order.CustomerId,
                order.TotalAmount
            );
            return BadRequest(new { Message = errorMessage });
        }

        // TODO: Validate each order item when items are properly loaded from database
        // For now, skip item-level validation

        // Validate order total calculation using SubTotal from entity
        var itemsTotal = order.SubTotal;
        var (totalValid, totalError) = OrderProcessingPolicy.ValidateOrderTotal(
            itemsTotal,
            order.ShippingCost,
            order.TaxAmount,
            order.DiscountAmount,
            order.TotalAmount
        );

        if (!totalValid)
        {
            _logger.LogWarning(
                "Order total validation failed: {Error}",
                totalError ?? "Unknown error"
            );
            return BadRequest(new { Message = totalError });
        }

        // Set initial values
        order.Id = Guid.NewGuid();
        order.Status = OrderStatus.Pending;
        order.CreatedAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        // TODO: Reserve inventory for order items when items are loaded from database
        // This requires loading OrderItemEntity collection separately

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Order created: OrderId={OrderId}, CustomerId={CustomerId}, Total={Total}",
            order.Id,
            order.CustomerId,
            order.TotalAmount
        );

        return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
    }

    /// <summary>
    /// Retrieves an order by its ID
    /// </summary>
    /// <param name="id">Order unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Order details</returns>
    /// <response code="200">Order found and returned</response>
    /// <response code="401">Authentication required</response>
    /// <response code="404">Order not found</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderEntity>> GetOrderById(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order == null)
            return NotFound(new { Message = $"Order with ID '{id}' not found" });

        return Ok(order);
    }

    /// <summary>
    /// Updates order status with business rule validation
    /// </summary>
    /// <param name="id">Order unique identifier</param>
    /// <param name="newStatus">New order status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated order</returns>
    /// <response code="200">Order status updated successfully</response>
    /// <response code="400">Invalid status transition</response>
    /// <response code="401">Authentication required</response>
    /// <response code="403">Insufficient permissions</response>
    /// <response code="404">Order not found</response>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin,Manager,Developer")]
    [ProducesResponseType(typeof(OrderEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderEntity>> UpdateOrderStatus(
        Guid id,
        [FromBody] OrderStatus newStatus,
        CancellationToken cancellationToken = default
    )
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order == null)
            return NotFound(new { Message = $"Order with ID '{id}' not found" });

        var userId = GetCurrentUserId();

        // Validate status transition
        var (isValid, errorMessage) = OrderProcessingPolicy.CanTransitionOrderStatus(
            order.Status,
            newStatus
        );

        if (!isValid)
        {
            _logger.LogWarning(
                "Invalid order status transition: OrderId={OrderId}, From={CurrentStatus}, To={NewStatus}, Error={Error}",
                id,
                order.Status,
                newStatus,
                errorMessage ?? "Unknown error"
            );
            return BadRequest(new { Message = errorMessage });
        }

        var oldStatus = order.Status;
        order.Status = newStatus;
        order.UpdatedAt = DateTime.UtcNow;

        // TODO: Handle status-specific actions (e.g., inventory fulfillment on ship)
        // This requires loading OrderItemEntity collection separately

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Order status updated: OrderId={OrderId}, From={OldStatus}, To={NewStatus}",
            id,
            oldStatus,
            newStatus
        );

        return Ok(order);
    }

    /// <summary>
    /// Cancels an order with business rule validation
    /// </summary>
    /// <param name="id">Order unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cancelled order</returns>
    /// <response code="200">Order cancelled successfully</response>
    /// <response code="400">Order cannot be cancelled</response>
    /// <response code="401">Authentication required</response>
    /// <response code="404">Order not found</response>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(OrderEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderEntity>> CancelOrder(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order == null)
            return NotFound(new { Message = $"Order with ID '{id}' not found" });

        var userId = GetCurrentUserId();

        // Validate cancellation
        var (canCancel, errorMessage) = OrderProcessingPolicy.CanCancelOrder(order.Status);

        if (!canCancel)
        {
            _logger.LogWarning(
                "Order cannot be cancelled: OrderId={OrderId}, Status={Status}, Error={Error}",
                id,
                order.Status,
                errorMessage ?? "Unknown error"
            );
            return BadRequest(new { Message = errorMessage });
        }

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;

        // TODO: Release inventory reservations when items are loaded from database
        // This requires loading OrderItemEntity collection separately

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order cancelled: OrderId={OrderId}", id);

        return Ok(order);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
