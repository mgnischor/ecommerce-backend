using System.Security.Claims;
using System.Text.RegularExpressions;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Controllers;

/// <summary>
/// Shipment management endpoints
/// </summary>
[Tags("Shipments")]
[ApiController]
[Route("api/v1/shipments")]
[Produces("application/json")]
[Authorize]
public sealed class ShipmentController : ControllerBase
{
    private readonly PostgresqlContext _context;
    private readonly ILogger<ShipmentController> _logger;
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 10;
    private const int MaxResultLimit = 1000;

    public ShipmentController(PostgresqlContext context, ILogger<ShipmentController> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private string? GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private bool IsAdmin() => User.IsInRole("Admin");

    private bool IsManager() => User.IsInRole("Manager");

    /// <summary>
    /// Retrieves all shipments with pagination (Admin/Manager only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(IEnumerable<ShipmentEntity>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<ShipmentEntity>>> GetAllShipments(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = DefaultPageSize,
        CancellationToken cancellationToken = default
    )
    {
        // Input validation
        if (pageNumber < 1)
        {
            _logger.LogWarning("Invalid page number requested: {PageNumber}", pageNumber);
            return BadRequest(new { Message = "Page number must be greater than 0" });
        }

        if (pageSize < 1 || pageSize > MaxPageSize)
        {
            _logger.LogWarning("Invalid page size requested: {PageSize}", pageSize);
            return BadRequest(new { Message = $"Page size must be between 1 and {MaxPageSize}" });
        }

        try
        {
            var query = _context.Shipments.Where(s => !s.IsDeleted);

            var totalCount = await query.CountAsync(cancellationToken);

            // Prevent excessive data retrieval
            if (totalCount > MaxResultLimit && pageNumber * pageSize > MaxResultLimit)
            {
                _logger.LogWarning(
                    "Attempt to retrieve data beyond limit. User: {UserId}",
                    GetCurrentUserId()
                );
                return BadRequest(
                    new { Message = "Result set too large. Please refine your query" }
                );
            }

            var shipments = await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            Response.Headers.Append("X-Total-Count", totalCount.ToString());
            Response.Headers.Append("X-Page-Number", pageNumber.ToString());
            Response.Headers.Append("X-Page-Size", pageSize.ToString());
            Response.Headers.Append(
                "X-Total-Pages",
                ((int)Math.Ceiling(totalCount / (double)pageSize)).ToString()
            );

            _logger.LogInformation(
                "Retrieved {Count} shipments. Page: {Page}, User: {UserId}",
                shipments.Count,
                pageNumber,
                GetCurrentUserId()
            );

            return Ok(shipments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shipments. User: {UserId}", GetCurrentUserId());
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }

    /// <summary>
    /// Retrieves a specific shipment by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ShipmentEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShipmentEntity>> GetShipmentById(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid GUID provided for shipment lookup");
            return BadRequest(new { Message = "Invalid shipment ID" });
        }

        try
        {
            var shipment = await _context
                .Shipments.AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);

            if (shipment == null)
            {
                _logger.LogWarning(
                    "Shipment not found: {ShipmentId}, User: {UserId}",
                    id,
                    GetCurrentUserId()
                );
                return NotFound(new { Message = "Shipment not found" });
            }

            // Authorization check - users can only view their own shipments unless admin/manager
            if (!IsAdmin() && !IsManager())
            {
                var order = await _context
                    .Orders.AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Id == shipment.OrderId, cancellationToken);

                var currentUserId = GetCurrentUserId();
                if (order == null || order.CustomerId.ToString() != currentUserId)
                {
                    _logger.LogWarning(
                        "Unauthorized access attempt to shipment: {ShipmentId}, User: {UserId}",
                        id,
                        currentUserId
                    );
                    return Forbid();
                }
            }

            _logger.LogInformation(
                "Shipment retrieved: {ShipmentId}, User: {UserId}",
                id,
                GetCurrentUserId()
            );
            return Ok(shipment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shipment: {ShipmentId}", id);
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }

    /// <summary>
    /// Retrieves shipments for a specific order
    /// </summary>
    [HttpGet("order/{orderId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<ShipmentEntity>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<ShipmentEntity>>> GetShipmentsByOrder(
        Guid orderId,
        CancellationToken cancellationToken = default
    )
    {
        if (orderId == Guid.Empty)
        {
            _logger.LogWarning("Invalid order GUID provided");
            return BadRequest(new { Message = "Invalid order ID" });
        }

        try
        {
            // Verify order exists and user has access
            var order = await _context
                .Orders.AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted, cancellationToken);

            if (order == null)
            {
                _logger.LogWarning(
                    "Order not found: {OrderId}, User: {UserId}",
                    orderId,
                    GetCurrentUserId()
                );
                return NotFound(new { Message = "Order not found" });
            }

            // Authorization check
            var currentUserId = GetCurrentUserId();
            if (!IsAdmin() && !IsManager() && order.CustomerId.ToString() != currentUserId)
            {
                _logger.LogWarning(
                    "Unauthorized access attempt to order shipments: {OrderId}, User: {UserId}",
                    orderId,
                    currentUserId
                );
                return Forbid();
            }

            var shipments = await _context
                .Shipments.Where(s => s.OrderId == orderId && !s.IsDeleted)
                .OrderByDescending(s => s.CreatedAt)
                .Take(100) // Limit results
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            _logger.LogInformation(
                "Retrieved {Count} shipments for order: {OrderId}, User: {UserId}",
                shipments.Count,
                orderId,
                currentUserId
            );

            return Ok(shipments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shipments for order: {OrderId}", orderId);
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }

    /// <summary>
    /// Retrieves a shipment by tracking number (Public endpoint with rate limiting)
    /// </summary>
    [HttpGet("tracking/{trackingNumber}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ShipmentEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ShipmentEntity>> GetShipmentByTrackingNumber(
        string trackingNumber,
        CancellationToken cancellationToken = default
    )
    {
        // Input validation - prevent injection attacks
        if (string.IsNullOrWhiteSpace(trackingNumber))
        {
            _logger.LogWarning("Empty tracking number provided");
            return BadRequest(new { Message = "Tracking number is required" });
        }

        // Validate tracking number format (alphanumeric, hyphens, max 50 chars)
        if (trackingNumber.Length > 50 || !Regex.IsMatch(trackingNumber, @"^[a-zA-Z0-9\-]+$"))
        {
            _logger.LogWarning("Invalid tracking number format: {TrackingNumber}", trackingNumber);
            return BadRequest(new { Message = "Invalid tracking number format" });
        }

        try
        {
            // Use parameterized query to prevent SQL injection
            var shipment = await _context
                .Shipments.AsNoTracking()
                .FirstOrDefaultAsync(
                    s => s.TrackingNumber == trackingNumber && !s.IsDeleted,
                    cancellationToken
                );

            if (shipment == null)
            {
                _logger.LogInformation(
                    "Tracking number not found: {TrackingNumber}",
                    trackingNumber
                );
                return NotFound(new { Message = "Shipment not found" });
            }

            _logger.LogInformation(
                "Shipment tracked successfully: {TrackingNumber}",
                trackingNumber
            );
            return Ok(shipment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking shipment: {TrackingNumber}", trackingNumber);
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }

    /// <summary>
    /// Creates a new shipment
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ShipmentEntity), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ShipmentEntity>> CreateShipment(
        [FromBody] ShipmentEntity shipment,
        CancellationToken cancellationToken = default
    )
    {
        if (shipment == null)
        {
            _logger.LogWarning("Null shipment data received");
            return BadRequest(new { Message = "Shipment data is required" });
        }

        // Input validation
        if (shipment.OrderId == Guid.Empty)
        {
            return BadRequest(new { Message = "Valid Order ID is required" });
        }

        if (
            string.IsNullOrWhiteSpace(shipment.TrackingNumber)
            || shipment.TrackingNumber.Length > 50
        )
        {
            return BadRequest(
                new { Message = "Valid tracking number is required (max 50 characters)" }
            );
        }

        if (string.IsNullOrWhiteSpace(shipment.Carrier) || shipment.Carrier.Length > 100)
        {
            return BadRequest(
                new { Message = "Valid carrier name is required (max 100 characters)" }
            );
        }

        try
        {
            // Verify order exists
            var orderExists = await _context.Orders.AnyAsync(
                o => o.Id == shipment.OrderId && !o.IsDeleted,
                cancellationToken
            );

            if (!orderExists)
            {
                _logger.LogWarning(
                    "Attempt to create shipment for non-existent order: {OrderId}",
                    shipment.OrderId
                );
                return BadRequest(new { Message = "Order not found" });
            }

            // Check for duplicate tracking number
            var duplicateTracking = await _context.Shipments.AnyAsync(
                s => s.TrackingNumber == shipment.TrackingNumber && !s.IsDeleted,
                cancellationToken
            );

            if (duplicateTracking)
            {
                _logger.LogWarning(
                    "Duplicate tracking number attempt: {TrackingNumber}",
                    shipment.TrackingNumber
                );
                return Conflict(new { Message = "Tracking number already exists" });
            }

            // Secure assignment - prevent mass assignment vulnerabilities
            var newShipment = new ShipmentEntity
            {
                Id = Guid.NewGuid(),
                OrderId = shipment.OrderId,
                TrackingNumber = shipment.TrackingNumber,
                Carrier = shipment.Carrier,
                Status = shipment.Status,
                ShippedAt = shipment.ShippedAt,
                ExpectedDeliveryDate = shipment.ExpectedDeliveryDate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = Guid.TryParse(GetCurrentUserId(), out var userId) ? userId : Guid.Empty,
            };

            _context.Shipments.Add(newShipment);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Shipment created: {ShipmentId}, Order: {OrderId}, User: {UserId}",
                newShipment.Id,
                newShipment.OrderId,
                GetCurrentUserId()
            );

            return CreatedAtAction(
                nameof(GetShipmentById),
                new { id = newShipment.Id },
                newShipment
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating shipment for order: {OrderId}", shipment.OrderId);
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }

    /// <summary>
    /// Updates an existing shipment
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateShipment(
        Guid id,
        [FromBody] ShipmentEntity shipment,
        CancellationToken cancellationToken = default
    )
    {
        if (shipment == null)
        {
            _logger.LogWarning("Null shipment data received for update");
            return BadRequest(new { Message = "Shipment data is required" });
        }

        if (id == Guid.Empty || id != shipment.Id)
        {
            _logger.LogWarning(
                "ID mismatch in shipment update. Route: {RouteId}, Body: {BodyId}",
                id,
                shipment.Id
            );
            return BadRequest(new { Message = "ID mismatch" });
        }

        // Input validation
        if (
            string.IsNullOrWhiteSpace(shipment.TrackingNumber)
            || shipment.TrackingNumber.Length > 50
        )
        {
            return BadRequest(
                new { Message = "Valid tracking number is required (max 50 characters)" }
            );
        }

        try
        {
            var existingShipment = await _context.Shipments.FirstOrDefaultAsync(
                s => s.Id == id && !s.IsDeleted,
                cancellationToken
            );

            if (existingShipment == null)
            {
                _logger.LogWarning("Shipment not found for update: {ShipmentId}", id);
                return NotFound(new { Message = "Shipment not found" });
            }

            // Check for tracking number conflict
            if (existingShipment.TrackingNumber != shipment.TrackingNumber)
            {
                var duplicateTracking = await _context.Shipments.AnyAsync(
                    s => s.TrackingNumber == shipment.TrackingNumber && s.Id != id && !s.IsDeleted,
                    cancellationToken
                );

                if (duplicateTracking)
                {
                    _logger.LogWarning(
                        "Duplicate tracking number in update: {TrackingNumber}",
                        shipment.TrackingNumber
                    );
                    return Conflict(new { Message = "Tracking number already exists" });
                }
            }

            // Selective update - prevent mass assignment
            existingShipment.TrackingNumber = shipment.TrackingNumber;
            existingShipment.Carrier = shipment.Carrier;
            existingShipment.Status = shipment.Status;
            existingShipment.ShippedAt = shipment.ShippedAt;
            existingShipment.ExpectedDeliveryDate = shipment.ExpectedDeliveryDate;
            existingShipment.DeliveredAt = shipment.DeliveredAt;
            existingShipment.UpdatedAt = DateTime.UtcNow;
            existingShipment.UpdatedBy = Guid.TryParse(GetCurrentUserId(), out var userId)
                ? userId
                : Guid.Empty;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Shipment updated: {ShipmentId}, User: {UserId}",
                id,
                GetCurrentUserId()
            );
            return NoContent();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict updating shipment: {ShipmentId}", id);
            return Conflict(
                new
                {
                    Message = "The shipment was modified by another user. Please refresh and try again",
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating shipment: {ShipmentId}", id);
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }

    /// <summary>
    /// Deletes a shipment (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteShipment(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid GUID provided for shipment deletion");
            return BadRequest(new { Message = "Invalid shipment ID" });
        }

        try
        {
            var shipment = await _context.Shipments.FirstOrDefaultAsync(
                s => s.Id == id && !s.IsDeleted,
                cancellationToken
            );

            if (shipment == null)
            {
                _logger.LogWarning("Shipment not found for deletion: {ShipmentId}", id);
                return NotFound(new { Message = "Shipment not found" });
            }

            shipment.IsDeleted = true;
            shipment.UpdatedAt = DateTime.UtcNow;
            shipment.UpdatedBy = Guid.TryParse(GetCurrentUserId(), out var userId)
                ? userId
                : Guid.Empty;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogWarning(
                "Shipment deleted: {ShipmentId}, User: {UserId}",
                id,
                GetCurrentUserId()
            );
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting shipment: {ShipmentId}", id);
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }
}
