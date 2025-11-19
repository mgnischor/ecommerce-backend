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

        // Load order items from database to validate business rules
        var orderItems = await _context
            .OrderItems.Where(item => order.ItemIds.Contains(item.Id))
            .ToListAsync(cancellationToken);

        var itemCount = orderItems.Count;

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

        // Validate each order item against business rules
        foreach (var item in orderItems)
        {
            if (item.Quantity <= 0)
            {
                _logger.LogWarning(
                    "Invalid item quantity: ProductId={ProductId}, Quantity={Quantity}",
                    item.ProductId,
                    item.Quantity
                );
                return BadRequest(
                    new
                    {
                        Message = $"Item quantity must be positive for product {item.ProductName}",
                    }
                );
            }
        }

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

        // Reserve inventory for each order item
        foreach (var item in orderItems)
        {
            try
            {
                await _inventoryService.RecordTransactionAsync(
                    Domain.Enums.InventoryTransactionType.Reservation,
                    item.ProductId,
                    item.ProductSku,
                    item.ProductName,
                    item.Quantity,
                    item.UnitPrice,
                    "Reserved",
                    userId,
                    null,
                    order.Id,
                    order.OrderNumber,
                    $"Inventory reservation for order {order.OrderNumber}",
                    cancellationToken
                );

                _logger.LogInformation(
                    "Inventory reserved: OrderId={OrderId}, ProductId={ProductId}, Quantity={Quantity}",
                    order.Id,
                    item.ProductId,
                    item.Quantity
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to reserve inventory: ProductId={ProductId}, OrderId={OrderId}",
                    item.ProductId,
                    order.Id
                );
                return BadRequest(
                    new { Message = $"Failed to reserve inventory for {item.ProductName}" }
                );
            }
        }

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

        // Handle status-specific actions based on transition
        if (newStatus == OrderStatus.Shipped || newStatus == OrderStatus.Processing)
        {
            // Load order items for inventory fulfillment
            var orderItems = await _context
                .OrderItems.Where(item => order.ItemIds.Contains(item.Id))
                .ToListAsync(cancellationToken);

            foreach (var item in orderItems)
            {
                try
                {
                    // Record inventory fulfillment transaction
                    await _inventoryService.RecordTransactionAsync(
                        Domain.Enums.InventoryTransactionType.Fulfillment,
                        item.ProductId,
                        item.ProductSku,
                        item.ProductName,
                        item.Quantity,
                        item.UnitPrice,
                        "Shipped",
                        userId,
                        "Reserved",
                        order.Id,
                        order.OrderNumber,
                        $"Inventory fulfillment for order {order.OrderNumber} - Status: {newStatus}",
                        cancellationToken
                    );

                    _logger.LogInformation(
                        "Inventory fulfilled: OrderId={OrderId}, ProductId={ProductId}, Status={Status}",
                        order.Id,
                        item.ProductId,
                        newStatus
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Failed to record inventory fulfillment: ProductId={ProductId}, OrderId={OrderId}",
                        item.ProductId,
                        order.Id
                    );
                    // Continue with other items even if one fails
                }
            }
        }

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

        // Release inventory reservations for cancelled order
        var orderItems = await _context
            .OrderItems.Where(item => order.ItemIds.Contains(item.Id))
            .ToListAsync(cancellationToken);

        foreach (var item in orderItems)
        {
            try
            {
                await _inventoryService.RecordTransactionAsync(
                    Domain.Enums.InventoryTransactionType.ReservationRelease,
                    item.ProductId,
                    item.ProductSku,
                    item.ProductName,
                    item.Quantity,
                    item.UnitPrice,
                    "Available",
                    userId,
                    "Reserved",
                    order.Id,
                    order.OrderNumber,
                    $"Inventory reservation released - Order cancelled: {order.OrderNumber}",
                    cancellationToken
                );

                _logger.LogInformation(
                    "Inventory reservation released: OrderId={OrderId}, ProductId={ProductId}, Quantity={Quantity}",
                    order.Id,
                    item.ProductId,
                    item.Quantity
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to release inventory reservation: ProductId={ProductId}, OrderId={OrderId}",
                    item.ProductId,
                    order.Id
                );
                // Continue with other items even if one fails
            }
        }

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
