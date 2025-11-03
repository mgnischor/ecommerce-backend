using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Controllers;

/// <summary>
/// Product attribute management endpoints
/// </summary>
[Tags("ProductAttributes")]
[ApiController]
[Route("api/v1/product-attributes")]
[Produces("application/json")]
[Authorize(Roles = "Admin,Manager")]
public sealed class ProductAttributeController : ControllerBase
{
    private readonly PostgresqlContext _context;
    private readonly ILogger<ProductAttributeController> _logger;

    public ProductAttributeController(
        PostgresqlContext context,
        ILogger<ProductAttributeController> logger
    )
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all product attributes
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<ProductAttributeEntity>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductAttributeEntity>>> GetAllAttributes(
        CancellationToken cancellationToken = default
    )
    {
        var attributes = await _context
            .ProductAttributes.Where(a => !a.IsDeleted)
            .OrderBy(a => a.DisplayOrder)
            .ThenBy(a => a.Name)
            .ToListAsync(cancellationToken);

        return Ok(attributes);
    }

    /// <summary>
    /// Retrieves a specific attribute by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProductAttributeEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductAttributeEntity>> GetAttributeById(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var attribute = await _context.ProductAttributes.FirstOrDefaultAsync(
            a => a.Id == id && !a.IsDeleted,
            cancellationToken
        );

        if (attribute == null)
            return NotFound(new { Message = $"Product attribute with ID '{id}' not found" });

        return Ok(attribute);
    }

    /// <summary>
    /// Retrieves an attribute by code
    /// </summary>
    [HttpGet("code/{code}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProductAttributeEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductAttributeEntity>> GetAttributeByCode(
        string code,
        CancellationToken cancellationToken = default
    )
    {
        var attribute = await _context.ProductAttributes.FirstOrDefaultAsync(
            a => a.Code == code && !a.IsDeleted,
            cancellationToken
        );

        if (attribute == null)
            return NotFound(new { Message = $"Product attribute with code '{code}' not found" });

        return Ok(attribute);
    }

    /// <summary>
    /// Retrieves variant attributes only
    /// </summary>
    [HttpGet("variant")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<ProductAttributeEntity>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductAttributeEntity>>> GetVariantAttributes(
        CancellationToken cancellationToken = default
    )
    {
        var attributes = await _context
            .ProductAttributes
            .Where(a => a.IsVariantAttribute && !a.IsDeleted)
            .OrderBy(a => a.DisplayOrder)
            .ToListAsync(cancellationToken);

        return Ok(attributes);
    }

    /// <summary>
    /// Creates a new product attribute
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductAttributeEntity), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductAttributeEntity>> CreateAttribute(
        [FromBody] ProductAttributeEntity attribute,
        CancellationToken cancellationToken = default
    )
    {
        if (attribute == null)
            return BadRequest("Attribute data is required");

        attribute.Id = Guid.NewGuid();
        attribute.CreatedAt = DateTime.UtcNow;
        attribute.UpdatedAt = DateTime.UtcNow;

        _context.ProductAttributes.Add(attribute);
        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetAttributeById), new { id = attribute.Id }, attribute);
    }

    /// <summary>
    /// Updates an existing product attribute
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAttribute(
        Guid id,
        [FromBody] ProductAttributeEntity attribute,
        CancellationToken cancellationToken = default
    )
    {
        if (attribute == null)
            return BadRequest("Attribute data is required");

        if (id != attribute.Id)
            return BadRequest("ID mismatch");

        var existingAttribute = await _context.ProductAttributes.FindAsync(
            new object[] { id },
            cancellationToken
        );

        if (existingAttribute == null || existingAttribute.IsDeleted)
            return NotFound(new { Message = $"Product attribute with ID '{id}' not found" });

        attribute.UpdatedAt = DateTime.UtcNow;
        _context.Entry(existingAttribute).CurrentValues.SetValues(attribute);

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Deletes a product attribute (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAttribute(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var attribute = await _context.ProductAttributes.FindAsync(
            new object[] { id },
            cancellationToken
        );

        if (attribute == null || attribute.IsDeleted)
            return NotFound(new { Message = $"Product attribute with ID '{id}' not found" });

        attribute.IsDeleted = true;
        attribute.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
