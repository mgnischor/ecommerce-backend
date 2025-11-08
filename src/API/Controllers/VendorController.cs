using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Controllers;

/// <summary>
/// Provides endpoints for managing vendor entities in the e-commerce marketplace
/// </summary>
/// <remarks>
/// This controller handles operations for vendor/merchant management including
/// retrieval with pagination, search, verification, creation, updates, and soft deletion.
/// Vendors are third-party sellers who list and sell products on the platform.
/// </remarks>
[Tags("Vendors")]
[ApiController]
[Route("api/v1/vendors")]
[Produces("application/json")]
public sealed class VendorController : ControllerBase
{
    private readonly PostgresqlContext _context;
    private readonly ILoggingService _logger;

    public VendorController(PostgresqlContext context, ILoggingService logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all vendors with pagination support
    /// </summary>
    /// <remarks>
    /// Returns a paginated list of vendors that are not deleted.
    /// Results are ordered by rating (highest first) and then by creation date (newest first).
    /// Pagination metadata is returned in response headers.
    ///
    /// Response Headers:
    /// - X-Total-Count: Total number of vendors
    /// - X-Page-Number: Current page number
    /// - X-Page-Size: Number of items per page
    ///
    /// Sample request:
    ///
    ///     GET /api/v1/vendors?pageNumber=1&amp;pageSize=10
    ///
    /// </remarks>
    /// <param name="pageNumber">The page number to retrieve (default: 1, minimum: 1)</param>
    /// <param name="pageSize">The number of vendors per page (default: 10, range: 1-100)</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>A paginated list of vendor entities</returns>
    /// <response code="200">Returns the paginated list of vendors with metadata in response headers</response>
    /// <response code="400">If pagination parameters are invalid (pageNumber &lt; 1, pageSize &lt; 1 or pageSize &gt; 100)</response>
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
    /// Retrieves a specific vendor by its unique identifier
    /// </summary>
    /// <remarks>
    /// Returns detailed information about a single vendor identified by GUID.
    /// Only returns vendors that are not marked as deleted.
    ///
    /// Sample request:
    ///
    ///     GET /api/v1/vendors/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///
    /// </remarks>
    /// <param name="id">The unique GUID identifier of the vendor</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>The requested vendor entity</returns>
    /// <response code="200">Returns the requested vendor</response>
    /// <response code="404">If the vendor with the specified ID is not found or has been deleted</response>
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
    /// Retrieves all featured vendors
    /// </summary>
    /// <remarks>
    /// Returns a list of vendors marked as featured with active status.
    /// Featured vendors are highlighted on the platform for promotional purposes.
    /// Results are ordered by rating (highest first) and exclude deleted vendors.
    ///
    /// Sample request:
    ///
    ///     GET /api/v1/vendors/featured
    ///
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>A list of featured vendor entities</returns>
    /// <response code="200">Returns the list of featured vendors ordered by rating</response>
    [HttpGet("featured")]
    [ProducesResponseType(typeof(IEnumerable<VendorEntity>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<VendorEntity>>> GetFeaturedVendors(
        CancellationToken cancellationToken = default
    )
    {
        var vendors = await _context
            .Vendors.Where(v =>
                v.IsFeatured && !v.IsDeleted && v.Status == Domain.Enums.VendorStatus.Active
            )
            .OrderByDescending(v => v.Rating)
            .ToListAsync(cancellationToken);

        return Ok(vendors);
    }

    /// <summary>
    /// Searches for vendors by store name or business name
    /// </summary>
    /// <remarks>
    /// Performs a case-insensitive partial match search on both store names and business names.
    /// Returns only non-deleted vendors ordered by rating (highest first).
    /// Results are limited to 50 vendors for performance.
    ///
    /// Search requirements:
    /// - Minimum 2 characters
    /// - Maximum 100 characters
    /// - Case-insensitive partial matching
    /// - Searches both storeName and businessName fields
    ///
    /// Sample request:
    ///
    ///     GET /api/v1/vendors/search?searchTerm=Fashion
    ///
    /// </remarks>
    /// <param name="searchTerm">The search term to match against store and business names (2-100 characters)</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>A list of vendors matching the search criteria</returns>
    /// <response code="200">Returns the list of vendors matching the search term</response>
    /// <response code="400">If the search term is empty, less than 2 characters, or exceeds 100 characters</response>
    /// <response code="500">If an internal server error occurs during the search operation</response>
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

            var vendors = await _context
                .Vendors.Where(v =>
                    !v.IsDeleted
                    && (
                        EF.Functions.Like(v.StoreName.ToLower(), $"%{searchTermLower}%")
                        || EF.Functions.Like(v.BusinessName.ToLower(), $"%{searchTermLower}%")
                    )
                )
                .OrderByDescending(v => v.Rating)
                .Take(50) // Limit results
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            _logger.LogInformation(
                "Vendor search completed: '{SearchTerm}', Results: {Count}",
                searchTerm,
                vendors.Count
            );

            return Ok(vendors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching vendors with term: {SearchTerm}", searchTerm);
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }

    /// <summary>
    /// Creates a new vendor in the system
    /// </summary>
    /// <remarks>
    /// Registers a new vendor/merchant on the platform.
    /// The ID, CreatedAt, and UpdatedAt fields will be automatically set by the system.
    /// Requires authentication but no specific role.
    ///
    /// Sample request:
    ///
    ///     POST /api/v1/vendors
    ///     {
    ///         "storeName": "Fashion Boutique",
    ///         "businessName": "Fashion Inc.",
    ///         "email": "contact@fashionboutique.com",
    ///         "phone": "+1-555-0100",
    ///         "address": "123 Commerce St",
    ///         "city": "New York",
    ///         "state": "NY",
    ///         "zipCode": "10001",
    ///         "country": "USA",
    ///         "description": "Premium fashion clothing",
    ///         "rating": 0.0,
    ///         "isFeatured": false,
    ///         "isVerified": false,
    ///         "status": "Pending"
    ///     }
    ///
    /// </remarks>
    /// <param name="vendor">The vendor entity to create with all required fields</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>The newly created vendor entity with generated ID and timestamps</returns>
    /// <response code="201">Returns the newly created vendor with its assigned ID</response>
    /// <response code="400">If the vendor data is null or invalid</response>
    /// <response code="401">If the user is not authenticated</response>
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
            return BadRequest(new { Message = "Vendor data is required" });

        // Set system-managed fields
        vendor.Id = Guid.NewGuid();
        vendor.CreatedAt = DateTime.UtcNow;
        vendor.UpdatedAt = DateTime.UtcNow;
        vendor.IsDeleted = false;

        _context.Vendors.Add(vendor);
        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetVendorById), new { id = vendor.Id }, vendor);
    }
}
