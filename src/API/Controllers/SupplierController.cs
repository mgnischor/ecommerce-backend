using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Controllers;

/// <summary>
/// Provides endpoints for managing supplier entities in the e-commerce system
/// </summary>
/// <remarks>
/// This controller handles CRUD operations for supplier/vendor management including
/// retrieval, search, creation, updates, and soft deletion of supplier records.
/// All endpoints require Admin or Manager role authorization unless otherwise specified.
/// </remarks>
[Tags("Suppliers")]
[ApiController]
[Route("api/v1/suppliers")]
[Produces("application/json")]
[Authorize(Roles = "Admin,Manager,Developer")]
public sealed class SupplierController : ControllerBase
{
    private readonly PostgresqlContext _context;
    private readonly ILoggingService _logger;

    public SupplierController(PostgresqlContext context, LoggingService<SupplierController> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all active suppliers from the system
    /// </summary>
    /// <remarks>
    /// Returns a list of all suppliers that are marked as active and not deleted.
    /// Results are ordered by preferred status (preferred suppliers first) and then alphabetically by company name.
    ///
    /// Sample request:
    ///
    ///     GET /api/v1/suppliers
    ///
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>A list of active supplier entities</returns>
    /// <response code="200">Returns the list of active suppliers</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user does not have Admin or Manager role</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SupplierEntity>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SupplierEntity>>> GetAllSuppliers(
        CancellationToken cancellationToken = default
    )
    {
        var suppliers = await _context
            .Suppliers.Where(s => s.IsActive && !s.IsDeleted)
            .OrderByDescending(s => s.IsPreferred)
            .ThenBy(s => s.CompanyName)
            .ToListAsync(cancellationToken);

        return Ok(suppliers);
    }

    /// <summary>
    /// Retrieves a specific supplier by its unique identifier
    /// </summary>
    /// <remarks>
    /// Returns detailed information about a single supplier identified by GUID.
    /// Only returns suppliers that are not marked as deleted.
    ///
    /// Sample request:
    ///
    ///     GET /api/v1/suppliers/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///
    /// </remarks>
    /// <param name="id">The unique GUID identifier of the supplier</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>The requested supplier entity</returns>
    /// <response code="200">Returns the requested supplier</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user does not have Admin or Manager role</response>
    /// <response code="404">If the supplier with the specified ID is not found or has been deleted</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SupplierEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SupplierEntity>> GetSupplierById(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var supplier = await _context.Suppliers.FirstOrDefaultAsync(
            s => s.Id == id && !s.IsDeleted,
            cancellationToken
        );

        if (supplier == null)
            return NotFound(new { Message = $"Supplier with ID '{id}' not found" });

        return Ok(supplier);
    }

    /// <summary>
    /// Retrieves a supplier by its unique supplier code
    /// </summary>
    /// <remarks>
    /// Returns a supplier identified by its alphanumeric supplier code.
    /// Supplier codes are typically short, unique identifiers used for business operations and integrations.
    ///
    /// Sample request:
    ///
    ///     GET /api/v1/suppliers/code/SUP001
    ///
    /// </remarks>
    /// <param name="code">The unique supplier code (e.g., "SUP001", "VENDOR-XYZ")</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>The supplier entity matching the provided code</returns>
    /// <response code="200">Returns the supplier with the specified code</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user does not have Admin or Manager role</response>
    /// <response code="404">If no supplier with the specified code is found or has been deleted</response>
    [HttpGet("code/{code}")]
    [ProducesResponseType(typeof(SupplierEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SupplierEntity>> GetSupplierByCode(
        string code,
        CancellationToken cancellationToken = default
    )
    {
        var supplier = await _context.Suppliers.FirstOrDefaultAsync(
            s => s.SupplierCode == code && !s.IsDeleted,
            cancellationToken
        );

        if (supplier == null)
            return NotFound(new { Message = $"Supplier with code '{code}' not found" });

        return Ok(supplier);
    }

    /// <summary>
    /// Searches for suppliers by company name
    /// </summary>
    /// <remarks>
    /// Performs a case-insensitive partial match search on supplier company names.
    /// Returns only non-deleted suppliers ordered alphabetically by company name.
    /// Results are limited to 50 suppliers for performance.
    ///
    /// Search requirements:
    /// - Minimum 2 characters
    /// - Maximum 100 characters
    /// - Case-insensitive partial matching
    ///
    /// Sample request:
    ///
    ///     GET /api/v1/suppliers/search?searchTerm=Acme
    ///
    /// </remarks>
    /// <param name="searchTerm">The search term to match against company names (2-100 characters)</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>A list of suppliers matching the search criteria</returns>
    /// <response code="200">Returns the list of suppliers matching the search term</response>
    /// <response code="400">If the search term is empty, less than 2 characters, or exceeds 100 characters</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user does not have Admin or Manager role</response>
    /// <response code="500">If an internal server error occurs during the search operation</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<SupplierEntity>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<SupplierEntity>>> SearchSuppliers(
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

            var suppliers = await _context
                .Suppliers.Where(s =>
                    !s.IsDeleted
                    && EF.Functions.Like(s.CompanyName.ToLower(), $"%{searchTermLower}%")
                )
                .OrderBy(s => s.CompanyName)
                .Take(50) // Limit results
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            _logger.LogInformation(
                "Supplier search completed: '{SearchTerm}', Results: {Count}",
                searchTerm,
                suppliers.Count
            );

            return Ok(suppliers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching suppliers with term: {SearchTerm}", searchTerm);
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }

    /// <summary>
    /// Creates a new supplier in the system
    /// </summary>
    /// <remarks>
    /// Creates a new supplier entity with the provided information.
    /// The ID, CreatedAt, and UpdatedAt fields will be automatically set by the system.
    /// Requires Admin or Manager role authorization.
    ///
    /// Sample request:
    ///
    ///     POST /api/v1/suppliers
    ///     {
    ///         "companyName": "Acme Corporation",
    ///         "supplierCode": "ACME001",
    ///         "contactName": "John Doe",
    ///         "email": "contact@acme.com",
    ///         "phone": "+1-555-0100",
    ///         "address": "123 Industrial Park",
    ///         "city": "Springfield",
    ///         "state": "IL",
    ///         "zipCode": "62701",
    ///         "country": "USA",
    ///         "isActive": true,
    ///         "isPreferred": false
    ///     }
    ///
    /// </remarks>
    /// <param name="supplier">The supplier entity to create with all required fields</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>The newly created supplier entity with generated ID and timestamps</returns>
    /// <response code="201">Returns the newly created supplier with its assigned ID</response>
    /// <response code="400">If the supplier data is null or invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user does not have Admin or Manager role</response>
    [HttpPost]
    [ProducesResponseType(typeof(SupplierEntity), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SupplierEntity>> CreateSupplier(
        [FromBody] SupplierEntity supplier,
        CancellationToken cancellationToken = default
    )
    {
        if (supplier == null)
            return BadRequest("Supplier data is required");

        supplier.Id = Guid.NewGuid();
        supplier.CreatedAt = DateTime.UtcNow;
        supplier.UpdatedAt = DateTime.UtcNow;

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetSupplierById), new { id = supplier.Id }, supplier);
    }

    /// <summary>
    /// Updates an existing supplier's information
    /// </summary>
    /// <remarks>
    /// Updates all fields of an existing supplier entity.
    /// The supplier ID in the URL must match the ID in the request body.
    /// The UpdatedAt timestamp will be automatically set to the current UTC time.
    /// Requires Admin or Manager role authorization.
    ///
    /// Sample request:
    ///
    ///     PUT /api/v1/suppliers/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///     {
    ///         "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///         "companyName": "Acme Corporation Updated",
    ///         "supplierCode": "ACME001",
    ///         "contactName": "Jane Smith",
    ///         "email": "newcontact@acme.com",
    ///         "phone": "+1-555-0200",
    ///         "address": "456 New Industrial Park",
    ///         "city": "Springfield",
    ///         "state": "IL",
    ///         "zipCode": "62701",
    ///         "country": "USA",
    ///         "isActive": true,
    ///         "isPreferred": true
    ///     }
    ///
    /// </remarks>
    /// <param name="id">The unique GUID identifier of the supplier to update</param>
    /// <param name="supplier">The updated supplier entity with all fields</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>No content on successful update</returns>
    /// <response code="204">The supplier was successfully updated</response>
    /// <response code="400">If the supplier data is null or the ID in the URL doesn't match the ID in the body</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user does not have Admin or Manager role</response>
    /// <response code="404">If the supplier with the specified ID is not found or has been deleted</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSupplier(
        Guid id,
        [FromBody] SupplierEntity supplier,
        CancellationToken cancellationToken = default
    )
    {
        if (supplier == null)
            return BadRequest("Supplier data is required");

        if (id != supplier.Id)
            return BadRequest("ID mismatch");

        var existingSupplier = await _context.Suppliers.FindAsync(
            new object[] { id },
            cancellationToken
        );

        if (existingSupplier == null || existingSupplier.IsDeleted)
            return NotFound(new { Message = $"Supplier with ID '{id}' not found" });

        supplier.UpdatedAt = DateTime.UtcNow;
        _context.Entry(existingSupplier).CurrentValues.SetValues(supplier);

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Deletes a supplier from the system (soft delete)
    /// </summary>
    /// <remarks>
    /// Performs a soft delete by marking the supplier as deleted rather than permanently removing it.
    /// The supplier will no longer appear in standard queries but remains in the database for audit and historical purposes.
    /// The UpdatedAt timestamp will be set to the current UTC time.
    /// Requires Admin role authorization (higher privilege than general supplier management).
    ///
    /// Sample request:
    ///
    ///     DELETE /api/v1/suppliers/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///
    /// </remarks>
    /// <param name="id">The unique GUID identifier of the supplier to delete</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>No content on successful deletion</returns>
    /// <response code="204">The supplier was successfully marked as deleted</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user does not have Admin role</response>
    /// <response code="404">If the supplier with the specified ID is not found or has already been deleted</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,Developer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSupplier(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var supplier = await _context.Suppliers.FindAsync(new object[] { id }, cancellationToken);

        if (supplier == null || supplier.IsDeleted)
            return NotFound(new { Message = $"Supplier with ID '{id}' not found" });

        supplier.IsDeleted = true;
        supplier.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
