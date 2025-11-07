using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Controllers;

/// <summary>
/// Provides endpoints for managing store entities in the e-commerce system
/// </summary>
/// <remarks>
/// This controller handles CRUD operations for physical store locations including
/// retrieval, creation, updates, and soft deletion of store records.
/// </remarks>
[Tags("Stores")]
[ApiController]
[Route("api/v1/stores")]
[Produces("application/json")]
public sealed class StoreController : ControllerBase
{
    private readonly PostgresqlContext _context;
    private readonly ILoggingService _logger;

    public StoreController(PostgresqlContext context, LoggingService<StoreController> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all active stores from the system
    /// </summary>
    /// <remarks>
    /// Returns a list of all stores that are marked as active and not deleted,
    /// ordered by display order and then alphabetically by name.
    ///
    /// Sample request:
    ///
    ///     GET /api/v1/stores
    ///
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>A list of active store entities</returns>
    /// <response code="200">Returns the list of active stores</response>
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
    /// Retrieves a specific store by its unique identifier
    /// </summary>
    /// <remarks>
    /// Returns detailed information about a single store identified by GUID.
    /// Only returns stores that are not marked as deleted.
    ///
    /// Sample request:
    ///
    ///     GET /api/v1/stores/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///
    /// </remarks>
    /// <param name="id">The unique GUID identifier of the store</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>The requested store entity</returns>
    /// <response code="200">Returns the requested store</response>
    /// <response code="404">If the store with the specified ID is not found or has been deleted</response>
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
    /// Retrieves a store by its unique store code
    /// </summary>
    /// <remarks>
    /// Returns a store identified by its alphanumeric store code.
    /// Store codes are typically short, unique identifiers used for business operations.
    ///
    /// Sample request:
    ///
    ///     GET /api/v1/stores/code/ST001
    ///
    /// </remarks>
    /// <param name="code">The unique store code (e.g., "ST001", "NYC-MAIN")</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>The store entity matching the provided code</returns>
    /// <response code="200">Returns the store with the specified code</response>
    /// <response code="404">If no store with the specified code is found or has been deleted</response>
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
    /// Searches for stores located in a specific city
    /// </summary>
    /// <remarks>
    /// Performs a case-insensitive search for stores in the specified city.
    /// Returns only active, non-deleted stores ordered by display order.
    /// Results are limited to 100 stores for performance.
    ///
    /// Sample request:
    ///
    ///     GET /api/v1/stores/search/city/New York
    ///
    /// </remarks>
    /// <param name="city">The name of the city to search for (1-100 characters)</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>A list of stores located in the specified city</returns>
    /// <response code="200">Returns the list of stores in the specified city</response>
    /// <response code="400">If the city name is empty, whitespace only, or exceeds 100 characters</response>
    /// <response code="500">If an internal server error occurs during the search operation</response>
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
    /// Creates a new store in the system
    /// </summary>
    /// <remarks>
    /// Creates a new store entity with the provided information.
    /// The ID, CreatedAt, and UpdatedAt fields will be automatically set.
    /// Requires Admin role authorization.
    ///
    /// Sample request:
    ///
    ///     POST /api/v1/stores
    ///     {
    ///         "name": "Downtown Store",
    ///         "storeCode": "DT001",
    ///         "address": "123 Main St",
    ///         "city": "New York",
    ///         "state": "NY",
    ///         "zipCode": "10001",
    ///         "isActive": true,
    ///         "displayOrder": 1
    ///     }
    ///
    /// </remarks>
    /// <param name="store">The store entity to create with all required fields</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>The newly created store entity with generated ID and timestamps</returns>
    /// <response code="201">Returns the newly created store with its assigned ID</response>
    /// <response code="400">If the store data is null or invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user does not have Admin role</response>
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
    /// Updates an existing store's information
    /// </summary>
    /// <remarks>
    /// Updates all fields of an existing store entity.
    /// The store ID in the URL must match the ID in the request body.
    /// The UpdatedAt timestamp will be automatically set to the current UTC time.
    /// Requires Admin or Manager role authorization.
    ///
    /// Sample request:
    ///
    ///     PUT /api/v1/stores/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///     {
    ///         "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///         "name": "Updated Store Name",
    ///         "storeCode": "DT001",
    ///         "address": "456 New Address",
    ///         "city": "New York",
    ///         "state": "NY",
    ///         "zipCode": "10001",
    ///         "isActive": true,
    ///         "displayOrder": 1
    ///     }
    ///
    /// </remarks>
    /// <param name="id">The unique GUID identifier of the store to update</param>
    /// <param name="store">The updated store entity with all fields</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>No content on successful update</returns>
    /// <response code="204">The store was successfully updated</response>
    /// <response code="400">If the store data is null or the ID in the URL doesn't match the ID in the body</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user does not have Admin or Manager role</response>
    /// <response code="404">If the store with the specified ID is not found or has been deleted</response>
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
    /// Deletes a store from the system (soft delete)
    /// </summary>
    /// <remarks>
    /// Performs a soft delete by marking the store as deleted rather than permanently removing it.
    /// The store will no longer appear in standard queries but remains in the database for audit purposes.
    /// The UpdatedAt timestamp will be set to the current UTC time.
    /// Requires Admin role authorization.
    ///
    /// Sample request:
    ///
    ///     DELETE /api/v1/stores/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///
    /// </remarks>
    /// <param name="id">The unique GUID identifier of the store to delete</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>No content on successful deletion</returns>
    /// <response code="204">The store was successfully marked as deleted</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user does not have Admin role</response>
    /// <response code="404">If the store with the specified ID is not found or has already been deleted</response>
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
