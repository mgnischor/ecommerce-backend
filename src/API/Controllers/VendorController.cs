using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Controllers;

/// <summary>
/// Vendor management endpoints
/// </summary>
[Tags("Vendors")]
[ApiController]
[Route("api/v1/vendors")]
[Produces("application/json")]
public sealed class VendorController : ControllerBase
{
    private readonly PostgresqlContext _context;
    private readonly ILogger<VendorController> _logger;

    public VendorController(PostgresqlContext context, ILogger<VendorController> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all vendors with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<VendorEntity>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<VendorEntity>>> GetAllVendors(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default
    )
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest("Invalid pagination parameters");

        var vendors = await _context
            .Vendors.Where(v => !v.IsDeleted)
            .OrderByDescending(v => v.Rating)
            .ThenByDescending(v => v.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var totalCount = await _context.Vendors.CountAsync(v => !v.IsDeleted, cancellationToken);

        Response.Headers.Append("X-Total-Count", totalCount.ToString());
        Response.Headers.Append("X-Page-Number", pageNumber.ToString());
        Response.Headers.Append("X-Page-Size", pageSize.ToString());

        return Ok(vendors);
    }

    /// <summary>
    /// Retrieves a specific vendor by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VendorEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VendorEntity>> GetVendorById(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var vendor = await _context.Vendors.FirstOrDefaultAsync(
            v => v.Id == id && !v.IsDeleted,
            cancellationToken
        );

        if (vendor == null)
            return NotFound(new { Message = $"Vendor with ID '{id}' not found" });

        return Ok(vendor);
    }

    /// <summary>
    /// Retrieves featured vendors
    /// </summary>
    [HttpGet("featured")]
    [ProducesResponseType(typeof(IEnumerable<VendorEntity>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<VendorEntity>>> GetFeaturedVendors(
        CancellationToken cancellationToken = default
    )
    {
        var vendors = await _context
            .Vendors.Where(v => v.IsFeatured && !v.IsDeleted && v.Status == Domain.Enums.VendorStatus.Active)
            .OrderByDescending(v => v.Rating)
            .ToListAsync(cancellationToken);

        return Ok(vendors);
    }

    /// <summary>
    /// Searches vendors by name
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<VendorEntity>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<VendorEntity>>> SearchVendors(
        [FromQuery] string searchTerm,
        CancellationToken cancellationToken = default
    )
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return BadRequest(new { Message = "Search term is required" });
        }

        if (searchTerm.Length < 2)
        {
            return BadRequest(new { Message = "Search term must be at least 2 characters" });
        }

        if (searchTerm.Length > 100)
        {
            return BadRequest(new { Message = "Search term must not exceed 100 characters" });
        }

        try
        {
            // Use EF Core parameterized queries (prevents SQL injection)
            var searchTermLower = searchTerm.ToLowerInvariant();
            
            var vendors = await _context.Vendors
                .Where(v => !v.IsDeleted &&
                    (EF.Functions.Like(v.StoreName.ToLower(), $"%{searchTermLower}%") ||
                     EF.Functions.Like(v.BusinessName.ToLower(), $"%{searchTermLower}%")))
                .OrderByDescending(v => v.Rating)
                .Take(50) // Limit results
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Vendor search completed: '{SearchTerm}', Results: {Count}", 
                searchTerm, vendors.Count);

            return Ok(vendors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching vendors with term: {SearchTerm}", searchTerm);
            return StatusCode(500, new { Message = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Creates a new vendor
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(VendorEntity), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VendorEntity>> CreateVendor(
        [FromBody] VendorEntity vendor,
        CancellationToken cancellationToken = default
    )
    {
        if (vendor == null)
            return BadRequest("Vendor data is required");

        vendor.Id = Guid.NewGuid();
        vendor.CreatedAt = DateTime.UtcNow;
        vendor.UpdatedAt = DateTime.UtcNow;

        _context.Vendors.Add(vendor);
        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetVendorById), new { id = vendor.Id }, vendor);
    }

    /// <summary>
    /// Updates an existing vendor
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateVendor(
        Guid id,
        [FromBody] VendorEntity vendor,
        CancellationToken cancellationToken = default
    )
    {
        if (vendor == null)
            return BadRequest("Vendor data is required");

        if (id != vendor.Id)
            return BadRequest("ID mismatch");

        var existingVendor = await _context.Vendors.FindAsync(
            new object[] { id },
            cancellationToken
        );

        if (existingVendor == null || existingVendor.IsDeleted)
            return NotFound(new { Message = $"Vendor with ID '{id}' not found" });

        vendor.UpdatedAt = DateTime.UtcNow;
        _context.Entry(existingVendor).CurrentValues.SetValues(vendor);

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Verifies a vendor
    /// </summary>
    [HttpPatch("{id:guid}/verify")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyVendor(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var vendor = await _context.Vendors.FindAsync(new object[] { id }, cancellationToken);

        if (vendor == null || vendor.IsDeleted)
            return NotFound(new { Message = $"Vendor with ID '{id}' not found" });

        vendor.IsVerified = true;
        vendor.VerifiedAt = DateTime.UtcNow;
        vendor.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Deletes a vendor (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteVendor(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var vendor = await _context.Vendors.FindAsync(new object[] { id }, cancellationToken);

        if (vendor == null || vendor.IsDeleted)
            return NotFound(new { Message = $"Vendor with ID '{id}' not found" });

        vendor.IsDeleted = true;
        vendor.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
