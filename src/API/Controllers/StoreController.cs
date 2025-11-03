using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Controllers;

/// <summary>
/// Store management endpoints
/// </summary>
[Tags("Stores")]
[ApiController]
[Route("api/v1/stores")]
[Produces("application/json")]
public sealed class StoreController : ControllerBase
{
    private readonly PostgresqlContext _context;
    private readonly ILogger<StoreController> _logger;

    public StoreController(PostgresqlContext context, ILogger<StoreController> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all active stores
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<StoreEntity>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<StoreEntity>>> GetAllStores(
        CancellationToken cancellationToken = default
    )
    {
        var stores = await _context
            .Stores.Where(s => s.IsActive && !s.IsDeleted)
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .ToListAsync(cancellationToken);

        return Ok(stores);
    }

    /// <summary>
    /// Retrieves a specific store by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(StoreEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StoreEntity>> GetStoreById(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var store = await _context.Stores.FirstOrDefaultAsync(
            s => s.Id == id && !s.IsDeleted,
            cancellationToken
        );

        if (store == null)
            return NotFound(new { Message = $"Store with ID '{id}' not found" });

        return Ok(store);
    }

    /// <summary>
    /// Retrieves a store by code
    /// </summary>
    [HttpGet("code/{code}")]
    [ProducesResponseType(typeof(StoreEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StoreEntity>> GetStoreByCode(
        string code,
        CancellationToken cancellationToken = default
    )
    {
        var store = await _context.Stores.FirstOrDefaultAsync(
            s => s.StoreCode == code && !s.IsDeleted,
            cancellationToken
        );

        if (store == null)
            return NotFound(new { Message = $"Store with code '{code}' not found" });

        return Ok(store);
    }

    /// <summary>
    /// Searches stores by city
    /// </summary>
    [HttpGet("search/city/{city}")]
    [ProducesResponseType(typeof(IEnumerable<StoreEntity>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<StoreEntity>>> GetStoresByCity(
        string city,
        CancellationToken cancellationToken = default
    )
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(city))
        {
            return BadRequest(new { Message = "City name is required" });
        }

        if (city.Length > 100)
        {
            return BadRequest(new { Message = "City name must not exceed 100 characters" });
        }

        try
        {
            // Use parameterized query
            var cityLower = city.ToLowerInvariant();

            var stores = await _context
                .Stores.Where(s => s.City.ToLower() == cityLower && s.IsActive && !s.IsDeleted)
                .OrderBy(s => s.DisplayOrder)
                .Take(100) // Limit results
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            _logger.LogInformation(
                "Store search by city completed: '{City}', Results: {Count}",
                city,
                stores.Count
            );

            return Ok(stores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching stores by city: {City}", city);
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }

    /// <summary>
    /// Creates a new store
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(StoreEntity), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<StoreEntity>> CreateStore(
        [FromBody] StoreEntity store,
        CancellationToken cancellationToken = default
    )
    {
        if (store == null)
            return BadRequest("Store data is required");

        store.Id = Guid.NewGuid();
        store.CreatedAt = DateTime.UtcNow;
        store.UpdatedAt = DateTime.UtcNow;

        _context.Stores.Add(store);
        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetStoreById), new { id = store.Id }, store);
    }

    /// <summary>
    /// Updates an existing store
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStore(
        Guid id,
        [FromBody] StoreEntity store,
        CancellationToken cancellationToken = default
    )
    {
        if (store == null)
            return BadRequest("Store data is required");

        if (id != store.Id)
            return BadRequest("ID mismatch");

        var existingStore = await _context.Stores.FindAsync(new object[] { id }, cancellationToken);

        if (existingStore == null || existingStore.IsDeleted)
            return NotFound(new { Message = $"Store with ID '{id}' not found" });

        store.UpdatedAt = DateTime.UtcNow;
        _context.Entry(existingStore).CurrentValues.SetValues(store);

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Deletes a store (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteStore(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var store = await _context.Stores.FindAsync(new object[] { id }, cancellationToken);

        if (store == null || store.IsDeleted)
            return NotFound(new { Message = $"Store with ID '{id}' not found" });

        store.IsDeleted = true;
        store.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
