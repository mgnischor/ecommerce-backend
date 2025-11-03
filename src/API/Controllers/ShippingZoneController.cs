using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
    private const int MaxShippingZones = 200;

    public ShippingZoneController(PostgresqlContext context, ILogger<ShippingZoneController> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private string? GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    /// <summary>
    /// Retrieves all active shipping zones (Public endpoint)
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<ShippingZoneEntity>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ShippingZoneEntity>>> GetAllShippingZones(
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var zones = await _context.ShippingZones
                .Where(z => z.IsActive && !z.IsDeleted)
                .OrderBy(z => z.Priority)
                .ThenBy(z => z.Name)
                .Take(MaxShippingZones)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Retrieved {Count} shipping zones", zones.Count);
            return Ok(zones);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shipping zones");
            return StatusCode(500, new { Message = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Retrieves a specific shipping zone by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ShippingZoneEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShippingZoneEntity>> GetShippingZoneById(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid shipping zone GUID provided");
            return BadRequest(new { Message = "Invalid shipping zone ID" });
        }

        try
        {
            var zone = await _context.ShippingZones
                .AsNoTracking()
                .FirstOrDefaultAsync(z => z.Id == id && !z.IsDeleted, cancellationToken);

            if (zone == null)
            {
                _logger.LogInformation("Shipping zone not found: {ZoneId}", id);
                return NotFound(new { Message = "Shipping zone not found" });
            }

            return Ok(zone);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shipping zone: {ZoneId}", id);
            return StatusCode(500, new { Message = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Creates a new shipping zone
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ShippingZoneEntity), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ShippingZoneEntity>> CreateShippingZone(
        [FromBody] ShippingZoneEntity zone,
        CancellationToken cancellationToken = default
    )
    {
        if (zone == null)
        {
            _logger.LogWarning("Null shipping zone data received");
            return BadRequest(new { Message = "Shipping zone data is required" });
        }

        // Input validation
        if (string.IsNullOrWhiteSpace(zone.Name) || zone.Name.Length > 200)
        {
            return BadRequest(new { Message = "Valid zone name is required (max 200 characters)" });
        }

        if (zone.BaseRate < 0 || zone.RatePerKg < 0 || zone.RatePerItem < 0)
        {
            return BadRequest(new { Message = "Rates cannot be negative" });
        }

        if (zone.FreeShippingThreshold.HasValue && zone.FreeShippingThreshold < 0)
        {
            return BadRequest(new { Message = "Free shipping threshold cannot be negative" });
        }

        if (zone.Priority < 0)
        {
            return BadRequest(new { Message = "Priority must be a positive number" });
        }

        try
        {
            // Check for duplicate name
            var duplicateName = await _context.ShippingZones
                .AnyAsync(z => z.Name == zone.Name && !z.IsDeleted, cancellationToken);

            if (duplicateName)
            {
                _logger.LogWarning("Duplicate shipping zone name attempt: {Name}", zone.Name);
                return Conflict(new { Message = "Shipping zone name already exists" });
            }

            // Secure assignment
            var newZone = new ShippingZoneEntity
            {
                Id = Guid.NewGuid(),
                Name = zone.Name,
                Description = zone.Description,
                Countries = zone.Countries,
                States = zone.States,
                PostalCodes = zone.PostalCodes,
                BaseRate = zone.BaseRate,
                RatePerKg = zone.RatePerKg,
                RatePerItem = zone.RatePerItem,
                TaxRate = zone.TaxRate,
                FreeShippingThreshold = zone.FreeShippingThreshold,
                EstimatedDeliveryDays = zone.EstimatedDeliveryDays,
                Priority = zone.Priority,
                IsActive = zone.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = Guid.TryParse(GetCurrentUserId(), out var userId) ? userId : Guid.Empty
            };

            _context.ShippingZones.Add(newZone);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Shipping zone created: {ZoneId}, Name: {Name}, User: {UserId}", 
                newZone.Id, newZone.Name, GetCurrentUserId());

            return CreatedAtAction(nameof(GetShippingZoneById), new { id = newZone.Id }, newZone);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating shipping zone");
            return StatusCode(500, new { Message = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Updates an existing shipping zone
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateShippingZone(
        Guid id,
        [FromBody] ShippingZoneEntity zone,
        CancellationToken cancellationToken = default
    )
    {
        if (zone == null)
        {
            _logger.LogWarning("Null shipping zone data received for update");
            return BadRequest(new { Message = "Shipping zone data is required" });
        }

        if (id == Guid.Empty || id != zone.Id)
        {
            _logger.LogWarning("ID mismatch in shipping zone update. Route: {RouteId}, Body: {BodyId}", id, zone.Id);
            return BadRequest(new { Message = "ID mismatch" });
        }

        // Input validation
        if (string.IsNullOrWhiteSpace(zone.Name) || zone.Name.Length > 200)
        {
            return BadRequest(new { Message = "Valid zone name is required (max 200 characters)" });
        }

        if (zone.BaseRate < 0 || zone.RatePerKg < 0 || zone.RatePerItem < 0)
        {
            return BadRequest(new { Message = "Rates cannot be negative" });
        }

        try
        {
            var existingZone = await _context.ShippingZones
                .FirstOrDefaultAsync(z => z.Id == id && !z.IsDeleted, cancellationToken);

            if (existingZone == null)
            {
                _logger.LogWarning("Shipping zone not found for update: {ZoneId}", id);
                return NotFound(new { Message = "Shipping zone not found" });
            }

            // Check for name conflict if name changed
            if (existingZone.Name != zone.Name)
            {
                var duplicateName = await _context.ShippingZones
                    .AnyAsync(z => z.Name == zone.Name && z.Id != id && !z.IsDeleted, cancellationToken);

                if (duplicateName)
                {
                    _logger.LogWarning("Duplicate shipping zone name in update: {Name}", zone.Name);
                    return Conflict(new { Message = "Shipping zone name already exists" });
                }
            }

            // Selective update
            existingZone.Name = zone.Name;
            existingZone.Description = zone.Description;
            existingZone.Countries = zone.Countries;
            existingZone.States = zone.States;
            existingZone.PostalCodes = zone.PostalCodes;
            existingZone.BaseRate = zone.BaseRate;
            existingZone.RatePerKg = zone.RatePerKg;
            existingZone.RatePerItem = zone.RatePerItem;
            existingZone.TaxRate = zone.TaxRate;
            existingZone.FreeShippingThreshold = zone.FreeShippingThreshold;
            existingZone.EstimatedDeliveryDays = zone.EstimatedDeliveryDays;
            existingZone.Priority = zone.Priority;
            existingZone.IsActive = zone.IsActive;
            existingZone.UpdatedAt = DateTime.UtcNow;
            existingZone.UpdatedBy = Guid.TryParse(GetCurrentUserId(), out var userId) ? userId : Guid.Empty;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Shipping zone updated: {ZoneId}, User: {UserId}", id, GetCurrentUserId());
            return NoContent();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict updating shipping zone: {ZoneId}", id);
            return Conflict(new { Message = "The shipping zone was modified by another user. Please refresh and try again" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating shipping zone: {ZoneId}", id);
            return StatusCode(500, new { Message = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Deletes a shipping zone (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteShippingZone(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid shipping zone GUID provided for deletion");
            return BadRequest(new { Message = "Invalid shipping zone ID" });
        }

        try
        {
            var zone = await _context.ShippingZones
                .FirstOrDefaultAsync(z => z.Id == id && !z.IsDeleted, cancellationToken);

            if (zone == null)
            {
                _logger.LogWarning("Shipping zone not found for deletion: {ZoneId}", id);
                return NotFound(new { Message = "Shipping zone not found" });
            }

            zone.IsDeleted = true;
            zone.UpdatedAt = DateTime.UtcNow;
            zone.UpdatedBy = Guid.TryParse(GetCurrentUserId(), out var userId) ? userId : Guid.Empty;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogWarning("Shipping zone deleted: {ZoneId}, Name: {Name}, User: {UserId}", 
                id, zone.Name, GetCurrentUserId());
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting shipping zone: {ZoneId}", id);
            return StatusCode(500, new { Message = "An error occurred while processing your request" });
        }
    }
}
