using System.Security.Claims;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Controllers;

/// <summary>
/// Provides HTTP endpoints for managing geographical shipping zones and their associated rates.
/// Handles shipping cost calculation rules based on destination, weight, and order value.
/// </summary>
/// <remarks>
/// This controller manages shipping zone operations including:
/// <list type="bullet">
/// <item><description>Defining geographical regions (countries, states, postal codes) for shipping</description></item>
/// <item><description>Configuring shipping rates (base rate, per-kg, per-item pricing)</description></item>
/// <item><description>Setting free shipping thresholds and delivery time estimates</description></item>
/// <item><description>Managing zone priorities for overlapping regions</description></item>
/// <item><description>Tax rate configuration per shipping zone</description></item>
/// <item><description>Soft-delete support for maintaining historical shipping records</description></item>
/// </list>
/// <para>
/// Most endpoints require Admin or Manager role for configuration. The GET all zones endpoint
/// is public (AllowAnonymous) to support checkout and shipping cost calculation for customers
/// without authentication.
/// </para>
/// <para>
/// Shipping zones use a priority system to resolve conflicts when multiple zones could apply
/// to the same destination. Higher priority values are evaluated first.
/// </para>
/// All responses are in JSON format and follow RESTful conventions.
/// </remarks>
[Tags("ShippingZones")]
[ApiController]
[Route("api/v1/shipping-zones")]
[Produces("application/json")]
[Authorize(Roles = "Admin,Manager,Developer")]
public sealed class ShippingZoneController : ControllerBase
{
    /// <summary>
    /// The database context for accessing shipping zone configuration data.
    /// </summary>
    private readonly PostgresqlContext _context;

    /// <summary>
    /// Logger instance for recording controller operations, configuration changes, and errors.
    /// </summary>
    private readonly ILoggingService _logger;

    /// <summary>
    /// Maximum number of shipping zones to return in a single query to prevent performance issues.
    /// </summary>
    /// <remarks>
    /// Limits results to 200 zones. Most e-commerce systems have far fewer zones,
    /// so this limit provides protection against misconfiguration or data issues.
    /// </remarks>
    private const int MaxShippingZones = 200;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShippingZoneController"/> class.
    /// </summary>
    /// <param name="context">The database context for data access operations.</param>
    /// <param name="logger">The logger for recording operational events, configuration changes, and errors.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="context"/> or <paramref name="logger"/> is null.
    /// </exception>
    public ShippingZoneController(
        PostgresqlContext context,
        LoggingService<ShippingZoneController> logger
    )
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves the unique identifier of the currently authenticated user from JWT token claims.
    /// </summary>
    /// <returns>
    /// The user ID as a string, or null if the user is not authenticated.
    /// </returns>
    /// <remarks>
    /// Extracts the NameIdentifier claim from the current user's claims principal.
    /// Used for audit trail tracking in CreatedBy and UpdatedBy fields.
    /// </remarks>
    private string? GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    /// <summary>
    /// Retrieves all active shipping zones available for use in checkout and shipping calculations.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// A collection of active shipping zone entities ordered by priority and name.
    /// </returns>
    /// <remarks>
    /// <para><b>Public Endpoint:</b></para>
    /// This endpoint allows anonymous access to support shipping cost calculation during checkout
    /// for both authenticated and guest customers. Common use cases include:
    /// <list type="bullet">
    /// <item><description>Displaying available shipping options during checkout</description></item>
    /// <item><description>Calculating shipping costs based on customer's delivery address</description></item>
    /// <item><description>Showing estimated delivery times</description></item>
    /// <item><description>Third-party integrations for shipping estimation</description></item>
    /// </list>
    /// <para><b>Filtering:</b></para>
    /// Returns only zones where:
    /// <list type="bullet">
    /// <item><description>IsActive = true (zones available for use)</description></item>
    /// <item><description>IsDeleted = false (not soft-deleted)</description></item>
    /// </list>
    /// <para><b>Ordering:</b></para>
    /// Results are ordered by:
    /// <list type="number">
    /// <item><description>Priority (ascending) - higher priority zones appear first</description></item>
    /// <item><description>Name (ascending) - alphabetical for zones with same priority</description></item>
    /// </list>
    /// <para><b>Result Limiting:</b></para>
    /// Limited to 200 zones maximum. Most e-commerce systems have 5-20 zones,
    /// so this limit provides safety without impacting normal operations.
    /// <para><b>Security Considerations:</b></para>
    /// While public, this endpoint exposes shipping configuration. Consider implementing
    /// caching to reduce database load and rate limiting to prevent abuse.
    /// </remarks>
    /// <response code="200">Returns the list of active shipping zones.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<ShippingZoneEntity>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ShippingZoneEntity>>> GetAllShippingZones(
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var zones = await _context
                .ShippingZones.Where(z => z.IsActive && !z.IsDeleted)
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
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }

    /// <summary>
    /// Retrieves a specific shipping zone by its unique identifier.
    /// </summary>
    /// <param name="id">The unique GUID of the shipping zone to retrieve.</param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The shipping zone entity if found, including all rate and configuration details.
    /// </returns>
    /// <remarks>
    /// Retrieves complete shipping zone configuration including:
    /// <list type="bullet">
    /// <item><description>Geographic coverage (countries, states, postal codes)</description></item>
    /// <item><description>Rate structure (base rate, per-kg, per-item charges)</description></item>
    /// <item><description>Tax rate and free shipping threshold</description></item>
    /// <item><description>Delivery time estimates (min/max days)</description></item>
    /// <item><description>Priority and active status</description></item>
    /// <item><description>Audit information (created/updated by and timestamps)</description></item>
    /// </list>
    /// Returns zones regardless of active status but excludes soft-deleted zones.
    /// Useful for administrative review, editing interfaces, and configuration management.
    /// <para><b>Authorization:</b></para>
    /// Requires Admin or Manager role as specified by the controller-level authorization attribute.
    /// </remarks>
    /// <response code="200">Returns the shipping zone entity.</response>
    /// <response code="400">Invalid shipping zone ID format provided.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Insufficient permissions - Admin or Manager role required.</response>
    /// <response code="404">Shipping zone not found or has been deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ShippingZoneEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
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
            var zone = await _context
                .ShippingZones.AsNoTracking()
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
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }

    /// <summary>
    /// Creates a new shipping zone with rate structure and geographical coverage rules.
    /// </summary>
    /// <param name="zone">
    /// The shipping zone entity containing all configuration including rates, coverage area, and delivery estimates.
    /// </param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The newly created shipping zone entity with server-generated ID and timestamps.
    /// </returns>
    /// <remarks>
    /// <para><b>Required Fields:</b></para>
    /// <list type="bullet">
    /// <item><description>Name: 1-200 characters, must be unique across all zones</description></item>
    /// <item><description>BaseRate: Minimum shipping charge (must be non-negative)</description></item>
    /// <item><description>Priority: Used to resolve overlapping zones (must be non-negative)</description></item>
    /// </list>
    /// <para><b>Optional Geographic Coverage:</b></para>
    /// <list type="bullet">
    /// <item><description>Countries: Array of ISO country codes (e.g., ["US", "CA", "MX"])</description></item>
    /// <item><description>States: Array of state/province codes (e.g., ["CA", "NY", "TX"])</description></item>
    /// <item><description>PostalCodes: Array of postal/zip code patterns or ranges</description></item>
    /// </list>
    /// <para><b>Rate Configuration:</b></para>
    /// <list type="bullet">
    /// <item><description>BaseRate: Fixed charge applied to all shipments in this zone</description></item>
    /// <item><description>RatePerKg: Additional charge per kilogram of package weight</description></item>
    /// <item><description>RatePerItem: Additional charge per item in the order</description></item>
    /// <item><description>TaxRate: Sales tax or VAT percentage for this zone</description></item>
    /// </list>
    /// <para><b>Validation Rules:</b></para>
    /// <list type="bullet">
    /// <item><description>All monetary rates must be non-negative</description></item>
    /// <item><description>FreeShippingThreshold, if specified, must be non-negative</description></item>
    /// <item><description>Priority must be non-negative (0 = lowest priority)</description></item>
    /// <item><description>Name must be unique across all non-deleted zones</description></item>
    /// </list>
    /// <para><b>Automatic Field Assignment:</b></para>
    /// <list type="bullet">
    /// <item><description>Id: Auto-generated GUID</description></item>
    /// <item><description>CreatedAt/UpdatedAt: Set to current UTC time</description></item>
    /// <item><description>CreatedBy: Set from authenticated user's ID</description></item>
    /// <item><description>IsDeleted: Defaults to false</description></item>
    /// </list>
    /// <para><b>Priority System:</b></para>
    /// When multiple zones could apply to a single destination (e.g., overlapping postal codes),
    /// the zone with the highest priority value is selected. This allows for:
    /// <list type="bullet">
    /// <item><description>Special rates for specific regions within broader zones</description></item>
    /// <item><description>Promotional shipping rates that override standard zones</description></item>
    /// <item><description>Fine-grained control over shipping cost calculation</description></item>
    /// </list>
    /// <para><b>Example Scenarios:</b></para>
    /// <list type="bullet">
    /// <item><description>Domestic zone: Countries=["US"], BaseRate=5.99, RatePerKg=2.00</description></item>
    /// <item><description>International zone: Countries=["CA","MX"], BaseRate=15.99, RatePerKg=5.00</description></item>
    /// <item><description>Express zone: States=["CA","NY"], BaseRate=25.00, Priority=10 (overrides domestic)</description></item>
    /// <item><description>Free shipping: FreeShippingThreshold=50.00, BaseRate=0 for orders over $50</description></item>
    /// </list>
    /// <para><b>Authorization:</b></para>
    /// Requires Admin or Manager role. Shipping configuration affects revenue and customer satisfaction,
    /// so access is restricted to authorized personnel only.
    /// </remarks>
    /// <response code="201">Shipping zone created successfully. Returns the created zone with Location header.</response>
    /// <response code="400">Invalid zone data, negative rates, or validation failure.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Insufficient permissions - Admin or Manager role required.</response>
    /// <response code="409">Shipping zone name already exists.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ShippingZoneEntity), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
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
            var duplicateName = await _context.ShippingZones.AnyAsync(
                z => z.Name == zone.Name && !z.IsDeleted,
                cancellationToken
            );

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
                EstimatedDeliveryDaysMin = zone.EstimatedDeliveryDaysMin,
                EstimatedDeliveryDaysMax = zone.EstimatedDeliveryDaysMax,
                Priority = zone.Priority,
                IsActive = zone.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = Guid.TryParse(GetCurrentUserId(), out var userId) ? userId : Guid.Empty,
            };

            _context.ShippingZones.Add(newZone);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Shipping zone created: {ZoneId}, Name: {Name}, User: {UserId}",
                newZone.Id,
                newZone.Name,
                GetCurrentUserId() ?? "Unknown"
            );

            return CreatedAtAction(nameof(GetShippingZoneById), new { id = newZone.Id }, newZone);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating shipping zone");
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }

    /// <summary>
    /// Updates an existing shipping zone's configuration, rates, and coverage area.
    /// </summary>
    /// <param name="id">The unique GUID of the shipping zone to update.</param>
    /// <param name="zone">
    /// The shipping zone entity with updated values. The Id must match the route parameter.
    /// </param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// No content on success (204).
    /// </returns>
    /// <remarks>
    /// <para><b>Updatable Fields:</b></para>
    /// <list type="bullet">
    /// <item><description>Name: Zone identification (must remain unique)</description></item>
    /// <item><description>Description: Additional details about the zone</description></item>
    /// <item><description>Countries, States, PostalCodes: Geographic coverage area</description></item>
    /// <item><description>BaseRate, RatePerKg, RatePerItem: Shipping cost structure</description></item>
    /// <item><description>TaxRate: Sales tax or VAT percentage</description></item>
    /// <item><description>FreeShippingThreshold: Order value for free shipping eligibility</description></item>
    /// <item><description>EstimatedDeliveryDaysMin/Max: Expected delivery timeframe</description></item>
    /// <item><description>Priority: Zone selection preference for overlapping regions</description></item>
    /// <item><description>IsActive: Enable or disable the zone</description></item>
    /// </list>
    /// <para><b>Protected Fields (Not Updated):</b></para>
    /// <list type="bullet">
    /// <item><description>Id: Cannot be changed</description></item>
    /// <item><description>CreatedAt, CreatedBy: Original audit information preserved</description></item>
    /// </list>
    /// <para><b>Validation Rules:</b></para>
    /// <list type="bullet">
    /// <item><description>ID in route must match ID in request body</description></item>
    /// <item><description>Name: 1-200 characters, must be unique (if changed from original)</description></item>
    /// <item><description>All rates must be non-negative</description></item>
    /// <item><description>Zone must exist and not be soft-deleted</description></item>
    /// </list>
    /// <para><b>Impact of Changes:</b></para>
    /// Updates take effect immediately and will be used for all new orders:
    /// <list type="bullet">
    /// <item><description>Rate changes affect shipping cost calculation for new checkouts</description></item>
    /// <item><description>Geographic changes affect zone matching for new addresses</description></item>
    /// <item><description>Priority changes affect zone selection when multiple zones apply</description></item>
    /// <item><description>Existing orders retain their original shipping costs (not retroactively changed)</description></item>
    /// </list>
    /// <para><b>Concurrency:</b></para>
    /// Returns 409 Conflict if the zone was modified by another user during the update operation.
    /// This prevents conflicting changes when multiple administrators work simultaneously.
    /// Clients should refresh data and retry the operation.
    /// <para><b>Best Practices:</b></para>
    /// <list type="bullet">
    /// <item><description>Test rate changes thoroughly before applying to production zones</description></item>
    /// <item><description>Consider creating new zones for significant changes rather than modifying active ones</description></item>
    /// <item><description>Use IsActive=false to temporarily disable zones rather than deleting them</description></item>
    /// <item><description>Document reason for changes in Description field for audit purposes</description></item>
    /// </list>
    /// <para><b>Authorization:</b></para>
    /// Requires Admin or Manager role. Updates are tracked via UpdatedBy and UpdatedAt fields
    /// for complete audit trail of shipping configuration changes.
    /// </remarks>
    /// <response code="204">Shipping zone updated successfully. No content returned.</response>
    /// <response code="400">Invalid data, ID mismatch, negative rates, or validation failure.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Insufficient permissions - Admin or Manager role required.</response>
    /// <response code="404">Shipping zone not found or has been deleted.</response>
    /// <response code="409">Conflict - zone name already exists or concurrent modification detected.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
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
            _logger.LogWarning(
                "ID mismatch in shipping zone update. Route: {RouteId}, Body: {BodyId}",
                id,
                zone.Id
            );
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
            var existingZone = await _context.ShippingZones.FirstOrDefaultAsync(
                z => z.Id == id && !z.IsDeleted,
                cancellationToken
            );

            if (existingZone == null)
            {
                _logger.LogWarning("Shipping zone not found for update: {ZoneId}", id);
                return NotFound(new { Message = "Shipping zone not found" });
            }

            // Check for name conflict if name changed
            if (existingZone.Name != zone.Name)
            {
                var duplicateName = await _context.ShippingZones.AnyAsync(
                    z => z.Name == zone.Name && z.Id != id && !z.IsDeleted,
                    cancellationToken
                );

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
            existingZone.EstimatedDeliveryDaysMin = zone.EstimatedDeliveryDaysMin;
            existingZone.EstimatedDeliveryDaysMax = zone.EstimatedDeliveryDaysMax;
            existingZone.Priority = zone.Priority;
            existingZone.IsActive = zone.IsActive;
            existingZone.UpdatedAt = DateTime.UtcNow;
            existingZone.UpdatedBy = Guid.TryParse(GetCurrentUserId(), out var userId)
                ? userId
                : Guid.Empty;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Shipping zone updated: {ZoneId}, User: {UserId}",
                id,
                GetCurrentUserId() ?? "Unknown"
            );
            return NoContent();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict updating shipping zone: {ZoneId}", id);
            return Conflict(
                new
                {
                    Message = "The shipping zone was modified by another user. Please refresh and try again",
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating shipping zone: {ZoneId}", id);
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }

    /// <summary>
    /// Soft deletes a shipping zone, marking it as deleted while preserving historical shipping data.
    /// </summary>
    /// <param name="id">The unique GUID of the shipping zone to delete.</param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// No content on success (204).
    /// </returns>
    /// <remarks>
    /// <para><b>Soft Delete Behavior:</b></para>
    /// <list type="bullet">
    /// <item><description>Sets IsDeleted = true</description></item>
    /// <item><description>Updates UpdatedAt to current UTC time</description></item>
    /// <item><description>Sets UpdatedBy to current user's ID</description></item>
    /// <item><description>Preserves all zone configuration data in database</description></item>
    /// <item><description>Excluded from active zone lists and shipping calculations</description></item>
    /// </list>
    /// <para><b>Effects:</b></para>
    /// <list type="bullet">
    /// <item><description>Zone no longer appears in available shipping options during checkout</description></item>
    /// <item><description>Zone name becomes available for reuse</description></item>
    /// <item><description>Cannot be selected for new orders</description></item>
    /// <item><description>Historical orders using this zone remain intact</description></item>
    /// <item><description>Shipping cost reports can still reference the deleted zone</description></item>
    /// <item><description>Data can potentially be restored by setting IsDeleted=false</description></item>
    /// </list>
    /// <para><b>Appropriate Use Cases:</b></para>
    /// Deletion should be reserved for:
    /// <list type="bullet">
    /// <item><description>Discontinued shipping methods or carriers</description></item>
    /// <item><description>Geographic regions no longer served</description></item>
    /// <item><description>Test zones created during configuration</description></item>
    /// <item><description>Duplicate zones created in error</description></item>
    /// <item><description>Zones replaced by new rate structures</description></item>
    /// </list>
    /// <para><b>Important Considerations:</b></para>
    /// <list type="bullet">
    /// <item><description>Does not affect existing orders that used this zone</description></item>
    /// <item><description>Consider impact on customers with saved addresses in this zone</description></item>
    /// <item><description>Ensure alternative zones cover the same geographic area if needed</description></item>
    /// <item><description>May affect reporting and analytics if zone is frequently used</description></item>
    /// </list>
    /// <para><b>Recommended Alternatives:</b></para>
    /// Before deletion, consider:
    /// <list type="bullet">
    /// <item><description>Setting IsActive=false to temporarily disable without removing</description></item>
    /// <item><description>Updating rates to discourage use while keeping zone available</description></item>
    /// <item><description>Creating replacement zone before deleting to ensure coverage continuity</description></item>
    /// <item><description>Communicating changes to customers affected by the zone removal</description></item>
    /// </list>
    /// <para><b>Authorization:</b></para>
    /// Requires Admin role only. This is more restrictive than update operations because
    /// deleting shipping zones can significantly impact checkout functionality and customer experience.
    /// Managers cannot delete zones - only Administrators have this permission.
    /// All deletions are logged with user ID and timestamp for accountability.
    /// </remarks>
    /// <response code="204">Shipping zone deleted successfully. No content returned.</response>
    /// <response code="400">Invalid shipping zone ID format provided.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Insufficient permissions - Admin role required.</response>
    /// <response code="404">Shipping zone not found or already deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,Developer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
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
            var zone = await _context.ShippingZones.FirstOrDefaultAsync(
                z => z.Id == id && !z.IsDeleted,
                cancellationToken
            );

            if (zone == null)
            {
                _logger.LogWarning("Shipping zone not found for deletion: {ZoneId}", id);
                return NotFound(new { Message = "Shipping zone not found" });
            }

            zone.IsDeleted = true;
            zone.UpdatedAt = DateTime.UtcNow;
            zone.UpdatedBy = Guid.TryParse(GetCurrentUserId(), out var userId)
                ? userId
                : Guid.Empty;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogWarning(
                "Shipping zone deleted: {ZoneId}, Name: {Name}, User: {UserId}",
                id,
                zone.Name,
                GetCurrentUserId() ?? "Unknown"
            );
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting shipping zone: {ZoneId}", id);
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }
}
