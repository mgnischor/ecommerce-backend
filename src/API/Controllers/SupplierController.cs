using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Controllers;

/// <summary>
/// Supplier management endpoints
/// </summary>
[Tags("Suppliers")]
[ApiController]
[Route("api/v1/suppliers")]
[Produces("application/json")]
[Authorize(Roles = "Admin,Manager")]
public sealed class SupplierController : ControllerBase
{
    private readonly PostgresqlContext _context;
    private readonly ILogger<SupplierController> _logger;

    public SupplierController(PostgresqlContext context, ILogger<SupplierController> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all active suppliers
    /// </summary>
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
    /// Retrieves a specific supplier by ID
    /// </summary>
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
    /// Retrieves a supplier by code
    /// </summary>
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
    /// Searches suppliers by name
    /// </summary>
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
    /// Creates a new supplier
    /// </summary>
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
    /// Updates an existing supplier
    /// </summary>
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
    /// Deletes a supplier (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
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
