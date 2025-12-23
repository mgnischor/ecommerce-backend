using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;

namespace ECommerce.API.Controllers;

/// <summary>
/// Product variant management endpoints
/// </summary>
/// <remarks>
/// Provides RESTful API endpoints for managing product variants including retrieval, creation,
/// update, and deletion operations. Product variants represent different versions or
/// configurations of a base product (e.g., size, color, material variations).
/// </remarks>
[Tags("ProductVariants")]
[ApiController]
[Route("api/v1/product-variants")]
[Produces("application/json")]
public sealed class ProductVariantController : ControllerBase
{
    /// <summary>
    /// Database context for PostgreSQL operations
    /// </summary>
    private readonly PostgresqlContext _context;

    /// <summary>
    /// Logger instance for controller operations
    /// </summary>
    private readonly ILoggingService _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductVariantController"/> class
    /// </summary>
    /// <param name="context">The PostgreSQL database context for data operations</param>
    /// <param name="logger">The logger instance for logging controller operations</param>
    /// <exception cref="ArgumentNullException">Thrown when context or logger is null</exception>
    public ProductVariantController(
        PostgresqlContext context,
        LoggingService<ProductVariantController> logger
    )
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all variants for a specific product
    /// </summary>
    /// <param name="productId">The unique identifier of the product</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>A collection of product variants ordered by display order and name</returns>
    /// <response code="200">Returns the list of product variants for the specified product</response>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/v1/product-variants/product/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///
    /// This endpoint returns only non-deleted variants, ordered by display order and then by name.
    /// </remarks>
    [HttpGet("product/{productId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<ProductVariantEntity>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductVariantEntity>>> GetVariantsByProduct(
        Guid productId,
        CancellationToken cancellationToken = default
    )
    {
        var variants = await _context
            .ProductVariants.Where(v => v.ProductId == productId && !v.IsDeleted)
            .OrderBy(v => v.DisplayOrder)
            .ThenBy(v => v.Name)
            .ToListAsync(cancellationToken);

        return Ok(variants);
    }

    /// <summary>
    /// Retrieves a specific variant by ID
    /// </summary>
    /// <param name="id">The unique identifier of the product variant</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>The product variant entity if found</returns>
    /// <response code="200">Returns the product variant with the specified ID</response>
    /// <response code="404">If the product variant is not found or has been deleted</response>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/v1/product-variants/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///
    /// </remarks>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductVariantEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductVariantEntity>> GetVariantById(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var variant = await _context.ProductVariants.FirstOrDefaultAsync(
            v => v.Id == id && !v.IsDeleted,
            cancellationToken
        );

        if (variant == null)
            return NotFound(new { Message = $"Product variant with ID '{id}' not found" });

        return Ok(variant);
    }

    /// <summary>
    /// Retrieves a variant by SKU
    /// </summary>
    /// <param name="sku">The Stock Keeping Unit (SKU) identifier of the product variant</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>The product variant entity if found</returns>
    /// <response code="200">Returns the product variant with the specified SKU</response>
    /// <response code="400">If the SKU is invalid, empty, exceeds 50 characters, or contains invalid characters</response>
    /// <response code="404">If the product variant is not found or has been deleted</response>
    /// <response code="500">If an internal server error occurs during processing</response>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/v1/product-variants/sku/PROD-VAR-001
    ///
    /// Valid SKU format: alphanumeric characters, hyphens, and underscores only.
    /// Maximum length: 50 characters.
    /// </remarks>
    [HttpGet("sku/{sku}")]
    [ProducesResponseType(typeof(ProductVariantEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductVariantEntity>> GetVariantBySku(
        string sku,
        CancellationToken cancellationToken = default
    )
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(sku))
        {
            return BadRequest(new { Message = "SKU is required" });
        }

        if (sku.Length > 50)
        {
            return BadRequest(new { Message = "SKU must not exceed 50 characters" });
        }

        // Validate SKU format (alphanumeric, hyphens, underscores)
        if (!System.Text.RegularExpressions.Regex.IsMatch(sku, @"^[a-zA-Z0-9\-_]+$"))
        {
            return BadRequest(new { Message = "Invalid SKU format" });
        }

        try
        {
            var variant = await _context
                .ProductVariants.AsNoTracking()
                .FirstOrDefaultAsync(v => v.Sku == sku && !v.IsDeleted, cancellationToken);

            if (variant == null)
            {
                return NotFound(new { Message = "Product variant not found" });
            }

            return Ok(variant);
        }
        catch (Exception)
        {
            // Don't log the SKU to avoid exposing potential attack vectors in logs
            return StatusCode(
                500,
                new { Message = "An error occurred while processing your request" }
            );
        }
    }

    /// <summary>
    /// Creates a new product variant
    /// </summary>
    /// <param name="variant">The product variant entity to create</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>The newly created product variant entity</returns>
    /// <response code="201">Returns the newly created product variant</response>
    /// <response code="400">If the variant data is null or invalid</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user does not have Admin or Manager role</response>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/v1/product-variants
    ///     {
    ///        "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///        "name": "Large - Blue",
    ///        "sku": "PROD-L-BLU",
    ///        "price": 29.99,
    ///        "stockQuantity": 100,
    ///        "displayOrder": 1
    ///     }
    ///
    /// The ID, CreatedAt, and UpdatedAt fields are automatically generated.
    /// Requires Admin or Manager role.
    /// </remarks>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Developer")]
    [ProducesResponseType(typeof(ProductVariantEntity), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductVariantEntity>> CreateVariant(
        [FromBody] ProductVariantEntity variant,
        CancellationToken cancellationToken = default
    )
    {
        if (variant == null)
            return BadRequest("Variant data is required");

        variant.Id = Guid.NewGuid();
        variant.CreatedAt = DateTime.UtcNow;
        variant.UpdatedAt = DateTime.UtcNow;

        _context.ProductVariants.Add(variant);
        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetVariantById), new { id = variant.Id }, variant);
    }

    /// <summary>
    /// Updates an existing product variant
    /// </summary>
    /// <param name="id">The unique identifier of the product variant to update</param>
    /// <param name="variant">The updated product variant entity data</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>No content on successful update</returns>
    /// <response code="204">The product variant was successfully updated</response>
    /// <response code="400">If the variant data is null or the ID in the URL does not match the ID in the body</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user does not have Admin or Manager role</response>
    /// <response code="404">If the product variant is not found or has been deleted</response>
    /// <remarks>
    /// Sample request:
    ///
    ///     PUT /api/v1/product-variants/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///     {
    ///        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///        "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///        "name": "Large - Blue (Updated)",
    ///        "sku": "PROD-L-BLU",
    ///        "price": 34.99,
    ///        "stockQuantity": 150,
    ///        "displayOrder": 1
    ///     }
    ///
    /// The UpdatedAt field is automatically set to the current UTC time.
    /// Requires Admin or Manager role.
    /// </remarks>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,Developer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateVariant(
        Guid id,
        [FromBody] ProductVariantEntity variant,
        CancellationToken cancellationToken = default
    )
    {
        if (variant == null)
            return BadRequest("Variant data is required");

        if (id != variant.Id)
            return BadRequest("ID mismatch");

        var existingVariant = await _context.ProductVariants.FindAsync([id], cancellationToken);

        if (existingVariant == null || existingVariant.IsDeleted)
            return NotFound(new { Message = $"Product variant with ID '{id}' not found" });

        variant.UpdatedAt = DateTime.UtcNow;
        _context.Entry(existingVariant).CurrentValues.SetValues(variant);

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Deletes a product variant (soft delete)
    /// </summary>
    /// <param name="id">The unique identifier of the product variant to delete</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>No content on successful deletion</returns>
    /// <response code="204">The product variant was successfully deleted</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user does not have Admin role</response>
    /// <response code="404">If the product variant is not found or has already been deleted</response>
    /// <remarks>
    /// Sample request:
    ///
    ///     DELETE /api/v1/product-variants/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///
    /// This performs a soft delete by setting the IsDeleted flag to true.
    /// The variant remains in the database but will not be returned by query endpoints.
    /// Requires Admin role.
    /// </remarks>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,Developer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteVariant(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var variant = await _context.ProductVariants.FindAsync([id], cancellationToken);

        if (variant == null || variant.IsDeleted)
            return NotFound(new { Message = $"Product variant with ID '{id}' not found" });

        variant.IsDeleted = true;
        variant.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
