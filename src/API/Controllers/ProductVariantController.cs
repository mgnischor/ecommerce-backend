using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Controllers;

/// <summary>
/// Product variant management endpoints
/// </summary>
[Tags("ProductVariants")]
[ApiController]
[Route("api/v1/product-variants")]
[Produces("application/json")]
public sealed class ProductVariantController : ControllerBase
{
    private readonly PostgresqlContext _context;
    private readonly ILogger<ProductVariantController> _logger;

    public ProductVariantController(
        PostgresqlContext context,
        ILogger<ProductVariantController> logger
    )
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all variants for a specific product
    /// </summary>
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
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
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
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
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

        var existingVariant = await _context.ProductVariants.FindAsync(
            new object[] { id },
            cancellationToken
        );

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
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteVariant(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var variant = await _context.ProductVariants.FindAsync(
            new object[] { id },
            cancellationToken
        );

        if (variant == null || variant.IsDeleted)
            return NotFound(new { Message = $"Product variant with ID '{id}' not found" });

        variant.IsDeleted = true;
        variant.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
