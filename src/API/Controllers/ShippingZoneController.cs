using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Controllers;

/// <summary>
/// Shipping zone management endpoints
/// </summary>
[Tags("ShippingZones")]
[ApiController]
[Route("api/v1/shipping-zones")]
[Produces("application/json")]
[Authorize(Roles = "Admin,Manager")]
public sealed class ShippingZoneController : ControllerBase
{
    private readonly PostgresqlContext _context;
    private readonly ILogger<ShippingZoneController> _logger;

    public ShippingZoneController(PostgresqlContext context, ILogger<ShippingZoneController> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all active shipping zones
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<ShippingZoneEntity>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ShippingZoneEntity>>> GetAllShippingZones(
        CancellationToken cancellationToken = default
    )
    {
        var zones = await _context
            .ShippingZones
            .Where(z => z.IsActive && !z.IsDeleted)
            .OrderBy(z => z.Priority)
            .ThenBy(z => z.Name)
            .ToListAsync(cancellationToken);

        return Ok(zones);
    }

    /// <summary>
    /// Retrieves a specific shipping zone by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ShippingZoneEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShippingZoneEntity>> GetShippingZoneById(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var zone = await _context.ShippingZones.FirstOrDefaultAsync(
            z => z.Id == id && !z.IsDeleted,
            cancellationToken
        );

        if (zone == null)
            return NotFound(new { Message = $"Shipping zone with ID '{id}' not found" });

        return Ok(zone);
    }

    /// <summary>
    /// Creates a new shipping zone
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ShippingZoneEntity), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ShippingZoneEntity>> CreateShippingZone(
        [FromBody] ShippingZoneEntity zone,
        CancellationToken cancellationToken = default
    )
    {
        if (zone == null)
            return BadRequest("Shipping zone data is required");

        zone.Id = Guid.NewGuid();
        zone.CreatedAt = DateTime.UtcNow;
        zone.UpdatedAt = DateTime.UtcNow;

        _context.ShippingZones.Add(zone);
        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetShippingZoneById), new { id = zone.Id }, zone);
    }

    /// <summary>
    /// Updates an existing shipping zone
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateShippingZone(
        Guid id,
        [FromBody] ShippingZoneEntity zone,
        CancellationToken cancellationToken = default
    )
    {
        if (zone == null)
            return BadRequest("Shipping zone data is required");

        if (id != zone.Id)
            return BadRequest("ID mismatch");

        var existingZone = await _context.ShippingZones.FindAsync(
            new object[] { id },
            cancellationToken
        );

        if (existingZone == null || existingZone.IsDeleted)
            return NotFound(new { Message = $"Shipping zone with ID '{id}' not found" });

        zone.UpdatedAt = DateTime.UtcNow;
        _context.Entry(existingZone).CurrentValues.SetValues(zone);

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Deletes a shipping zone (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteShippingZone(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var zone = await _context.ShippingZones.FindAsync(
            new object[] { id },
            cancellationToken
        );

        if (zone == null || zone.IsDeleted)
            return NotFound(new { Message = $"Shipping zone with ID '{id}' not found" });

        zone.IsDeleted = true;
        zone.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
