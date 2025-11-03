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

    public ShipmentController(PostgresqlContext context, ILogger<ShipmentController> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all shipments with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ShipmentEntity>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ShipmentEntity>>> GetAllShipments(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default
    )
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest("Invalid pagination parameters");

        var shipments = await _context
            .Shipments.Where(s => !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var totalCount = await _context.Shipments.CountAsync(s => !s.IsDeleted, cancellationToken);

        Response.Headers.Append("X-Total-Count", totalCount.ToString());
        Response.Headers.Append("X-Page-Number", pageNumber.ToString());
        Response.Headers.Append("X-Page-Size", pageSize.ToString());

        return Ok(shipments);
    }

    /// <summary>
    /// Retrieves a specific shipment by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ShipmentEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShipmentEntity>> GetShipmentById(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var shipment = await _context.Shipments.FirstOrDefaultAsync(
            s => s.Id == id && !s.IsDeleted,
            cancellationToken
        );

        if (shipment == null)
            return NotFound(new { Message = $"Shipment with ID '{id}' not found" });

        return Ok(shipment);
    }

    /// <summary>
    /// Retrieves shipments for a specific order
    /// </summary>
    [HttpGet("order/{orderId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<ShipmentEntity>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ShipmentEntity>>> GetShipmentsByOrder(
        Guid orderId,
        CancellationToken cancellationToken = default
    )
    {
        var shipments = await _context
            .Shipments.Where(s => s.OrderId == orderId && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(shipments);
    }

    /// <summary>
    /// Retrieves a shipment by tracking number
    /// </summary>
    [HttpGet("tracking/{trackingNumber}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ShipmentEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShipmentEntity>> GetShipmentByTrackingNumber(
        string trackingNumber,
        CancellationToken cancellationToken = default
    )
    {
        var shipment = await _context.Shipments.FirstOrDefaultAsync(
            s => s.TrackingNumber == trackingNumber && !s.IsDeleted,
            cancellationToken
        );

        if (shipment == null)
            return NotFound(
                new { Message = $"Shipment with tracking number '{trackingNumber}' not found" }
            );

        return Ok(shipment);
    }

    /// <summary>
    /// Creates a new shipment
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ShipmentEntity), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ShipmentEntity>> CreateShipment(
        [FromBody] ShipmentEntity shipment,
        CancellationToken cancellationToken = default
    )
    {
        if (shipment == null)
            return BadRequest("Shipment data is required");

        shipment.Id = Guid.NewGuid();
        shipment.CreatedAt = DateTime.UtcNow;
        shipment.UpdatedAt = DateTime.UtcNow;

        _context.Shipments.Add(shipment);
        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetShipmentById), new { id = shipment.Id }, shipment);
    }

    /// <summary>
    /// Updates an existing shipment
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateShipment(
        Guid id,
        [FromBody] ShipmentEntity shipment,
        CancellationToken cancellationToken = default
    )
    {
        if (shipment == null)
            return BadRequest("Shipment data is required");

        if (id != shipment.Id)
            return BadRequest("ID mismatch");

        var existingShipment = await _context.Shipments.FindAsync(
            new object[] { id },
            cancellationToken
        );

        if (existingShipment == null || existingShipment.IsDeleted)
            return NotFound(new { Message = $"Shipment with ID '{id}' not found" });

        shipment.UpdatedAt = DateTime.UtcNow;
        _context.Entry(existingShipment).CurrentValues.SetValues(shipment);

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Deletes a shipment (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteShipment(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var shipment = await _context.Shipments.FindAsync(
            new object[] { id },
            cancellationToken
        );

        if (shipment == null || shipment.IsDeleted)
            return NotFound(new { Message = $"Shipment with ID '{id}' not found" });

        shipment.IsDeleted = true;
        shipment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
